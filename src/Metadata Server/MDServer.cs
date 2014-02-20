using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization.Formatters;
using System.Collections;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Timers;

namespace Metadata_Server
{
	public interface MetadataSynchronize
	{
		bool RegisterMetadata(int id, string address);
		Dictionary<string, PadiFile> GetFiles();
		Dictionary<string, int> GetReplicate();
		Dictionary<string, Dictionary<int, RemoteInfo>> GetOpen();
		List<RemoteInfo> GetDataServers();
		Dictionary<int, DSLoad> GetDataServersLoad();
		Dictionary<string, PadiFile> GetToCreate();
		Dictionary<string, PadiFile> GetToDelete();
		Dictionary<string, PadiFile> GetToCreateWaiting();
		Dictionary<string, PadiFile> GetToDeleteWaiting();
		List<Migration> GetToMigrate();

		int ImAlive(int id);

		void OpenSynch(string filename, int id, string address);
		void CloseSynch(string filename, int id);
		void DeleteSynch(string filename);
		void CreateSynch(string filename, int numDS, int readQ, int writeQ, int id, string address, List<RemoteInfo> list);
		void ToCreateSynch(FileId r);
		void ToCreateWaitingSynch(FileId r);
		void ToDeleteSynch(FileId r);
		void ToDeleteWaitingSynch(FileId r);
		void MigrateSynch(Dictionary<int, DSLoad> newLoads, Dictionary<string, PadiFile> newFiles);

	}

	public enum State { Online, Offline };

	class MDServer : MarshalByRefObject, MetadataServerInterface, MetadataSynchronize
	{
		private const int GET_STATS_TIMEOUT = 3000;
		private Timer _getStatsTimer;
		private bool _getStatsTimedOut = false;
		private const int TIME_GET_LOAD = 5000;
		private const int TIME_CREATEDELETE = 500;
		private Timer _timer;
		private Timer _timerCreate;
		private State _state = State.Online;
		private static Dictionary<string, PadiFile> _files = new Dictionary<string, PadiFile>();
		private Dictionary<string, int> _replicate = new Dictionary<string, int>();
		private Dictionary<string, Dictionary<int, RemoteInfo>> _open = new Dictionary<string, Dictionary<int, RemoteInfo>>();
		private static Dictionary<string, PadiFile> _toCreate = new Dictionary<string, PadiFile>();
		private static Dictionary<string, PadiFile> _toCreateWaiting = new Dictionary<string, PadiFile>();
		private static Dictionary<string, PadiFile> _toDelete = new Dictionary<string, PadiFile>();
		private static Dictionary<string, PadiFile> _toDeleteWaiting = new Dictionary<string, PadiFile>();
		private static List<Migration> _toMigrate = new List<Migration>();
		private Dictionary<int, DataServerInterface> _dataServers = new Dictionary<int, DataServerInterface>();
		private static Dictionary<int, DSLoad> _dataServersLoad = new Dictionary<int, DSLoad>();
		private List<RemoteInfo> _dataServersInfo = new List<RemoteInfo>();
		private static List<MetadataSynchronize> _metadataServers = new List<MetadataSynchronize>();
		private static List<string> _metadataServersAddresses = new List<string>();
		private static int _port;
		private static int _id;
		private static bool _primary;
		//private StreamWriter log;

		static void Main(string[] args)
		{
			_port = Convert.ToInt32(args[0]);
			_id = Convert.ToInt32(args[1]);
			if (_id == 0)
			{
				_primary = true;
			}
			else
			{
				_primary = false;
			}
			BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
			provider.TypeFilterLevel = TypeFilterLevel.Full;
			IDictionary props = new Hashtable();
			props["port"] = _port;
			TcpChannel channel = new TcpChannel(props, null, provider);
			ChannelServices.RegisterChannel(channel, false);
			MDServer mo = new MDServer();
			RemotingServices.Marshal(mo, "MetadataServer" + _id, typeof(MDServer));
			for (int i = 2; i < args.Length; ++i)
			{
				_metadataServersAddresses.Add(args[i]);
			}
			foreach (string address in _metadataServersAddresses)
			{
				MetadataSynchronize metadataServer = (MetadataSynchronize)Activator.GetObject(typeof(MetadataSynchronize), address);
				if (metadataServer == null)
				{
					Console.WriteLine("Could not connect to Metadata Server in " + address);
				}
				else
				{
					_metadataServers.Add(metadataServer);
					Console.WriteLine("Successfully connected to Metadata Server in " + address);
					string dsAddress = "tcp://localhost:" + _port + "/MetadataServer" + _id;
					metadataServer.RegisterMetadata(_id, dsAddress);
				}
			}
			Console.WriteLine("METADATA SERVER: " + _id);
			Console.WriteLine("Using Port: " + _port);
			Console.WriteLine("Primary: " + _primary);
			Console.WriteLine("<enter> to exit...");
			Console.ReadLine();
		}

		private void _timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			GetDSLoads();
			CreateFiles();
			DeleteFiles();
			//MigrateFiles();
		}
		private void _timercreate_Elapsed(object sender, ElapsedEventArgs e)
		{
			CreateFiles();
			DeleteFiles();
		}

		private void GetDSLoads()
		{
			foreach (DataServerInterface ds in _dataServers.Values)
			{
				try
				{
					// Alternative 2: asynchronous call with callback
					// Create delegate to remote method
					GetStatsDelegate RemoteDel = new GetStatsDelegate(ds.GetStats);
					// Create delegate to local callback
					AsyncCallback RemoteCallback = new AsyncCallback(MDServer.GetStatsCallBack);
					// Call remote method
					IAsyncResult RemAr = RemoteDel.BeginInvoke(RemoteCallback, null);
				}
				catch (SocketException)
				{
					Console.WriteLine("Could not locate server");
				}
				catch (RemotingException) { }
			}
		}

		//      CLIENT METHODS

		public PadiFile Open(String filename, int id, string address)
		{
			if (_state == State.Offline)
			{
				throw new RemotingException("MetaData Server is down");
			}
			if (!_primary)
			{
				GetDSLoads();
				_primary = true;
				_timer.Enabled = true;
				_timerCreate.Enabled = true;
				Console.WriteLine("Primary: " + _primary);
			}
			if (_files == null)
			{
				InitializeMDS();
			}
			Console.WriteLine("Open(" + filename + ")");
			lock (_open)
			{
				if (_open.ContainsKey(filename))
				{
					if (!_open[filename].ContainsKey(id))
					{
						_open[filename][id] = new RemoteInfo(address, id);
					}
				}
				else
				{
					Dictionary<int, RemoteInfo> list = new Dictionary<int, RemoteInfo>();
					list[id] = new RemoteInfo(address, id);
					_open[filename] = list;
				}
			}
			if (_primary)
			{
				foreach (MetadataSynchronize md in _metadataServers)
				{
					try
					{
						//md.OpenSynch(filename, id, address);
						// Alternative 2: asynchronous call with callback
						// Create delegate to remote method
						OpenAsyncDelegate RemoteDel = new OpenAsyncDelegate(md.OpenSynch);
						// Create delegate to local callback
						AsyncCallback RemoteCallback = new AsyncCallback(MDServer.EmptyAsyncCallBack);
						// Call remote method
						IAsyncResult RemAr = RemoteDel.BeginInvoke(filename, id, address, RemoteCallback, null);
					}
					catch (SocketException)
					{
						Console.WriteLine("Could not locate server");
					}
					catch (RemotingException) { }
				}
			}
			SaveState();
			return _files[filename];
		}

		public void Close(String filename, int id)
		{
			if (_state == State.Offline)
			{
				throw new RemotingException("MetaData Server is down");
			}
			if (!_primary)
			{
				GetDSLoads();
				_primary = true;
				_timer.Enabled = true;
				_timerCreate.Enabled = true;
				Console.WriteLine("Primary: " + _primary);
			}
			if (_files == null)
			{
				InitializeMDS();
			}
			//log.WriteLine("Close - file " + filename + "-------------------START------------------------- id ->" + id);
			Console.WriteLine("Close(" + filename + ")");
			//log.WriteLine("CLOSE - file " + filename + " B Open id ->" + id);
			lock (_open)
			{
				if (_open.ContainsKey(filename))
				{
					_open[filename].Remove(id);
					if (_open[filename].Count == 0)
					{
						_open.Remove(filename);
					}
				}
			}
			//log.WriteLine("CLOSE - file " + filename + " A Open id ->" + id);
			//log.WriteLine("CLOSE - file " + filename + " B Synch id ->" + id);
			if (_primary)
			{
				foreach (MetadataSynchronize md in _metadataServers)
				{
					try
					{
						//md.CloseSynch(filename, id);
						// Alternative 2: asynchronous call with callback
						// Create delegate to remote method
						CloseAsyncDelegate RemoteDel = new CloseAsyncDelegate(md.CloseSynch);
						// Create delegate to local callback
						AsyncCallback RemoteCallback = new AsyncCallback(MDServer.EmptyAsyncCallBack);
						// Call remote method
						IAsyncResult RemAr = RemoteDel.BeginInvoke(filename, id, RemoteCallback, null);
					}
					catch (SocketException)
					{
						Console.WriteLine("Could not locate server");
					}
					catch (RemotingException) { }
				}
			}
			//log.WriteLine("CLOSE - file " + filename + " A Synch id ->" + id);
			//log.WriteLine("CLOSE - file " + filename + " B SaveState id ->" + id);
			SaveState();
			//log.WriteLine("CLOSE - file " + filename + " A SaveState id ->" + id);
			//log.WriteLine("CLOSE - file " + filename + " ------------------------END------------------------------ id ->" + id);
		}

		public void Delete(String filename, int id)
		{
			if (_state == State.Offline)
			{
				throw new RemotingException("MetaData Server is down");
			}
			if (!_primary)
			{
				GetDSLoads();
				_primary = true;
				_timer.Enabled = true;
				_timerCreate.Enabled = true;
				Console.WriteLine("Primary: " + _primary);
			}
			if (_files == null)
			{
				InitializeMDS();
			}
			//log.WriteLine("DELETE - file " + filename + " ---------------------Start--------------------- id ->" + id);
			Console.WriteLine("Delete(" + filename + ")");
			if (_files.ContainsKey(filename))
			{
				//log.WriteLine("DELETE - file " + filename + " A FileExists id ->" + id);
				if (!_open.ContainsKey(filename))
				{
					PadiFile p = null;
					//log.WriteLine("DELETE - file " + filename + " B Files id ->" + id);
					lock (_files)
					{
						if (_files.ContainsKey(filename))
						{
							p = _files[filename];
							_files.Remove(filename);
						}
					}
					//log.WriteLine("DELETE - file " + filename + " A Files id ->" + id);
					//log.WriteLine("DELETE - file " + filename + " B ToDel id ->" + id);
					lock (_toDelete)
					{
						if (p != null)
						{
							_toDelete[filename] = p;
						}
					}
					//log.WriteLine("DELETE - file " + filename + " A ToDEL id ->" + id);
					//log.WriteLine("DELETE - file " + filename + " B DF id ->" + id);
					//DeleteFiles();
					//log.WriteLine("DELETE - file " + filename + " A DF id ->" + id);
					Console.WriteLine("File : " + filename + " deleted sucessfully.");
					//log.WriteLine("DELETE - file " + filename + " B Synch id ->" + id);
					if (_primary)
					{
						foreach (MetadataSynchronize md in _metadataServers)
						{
							try
							{
								//md.DeleteSynch(filename);
								// Alternative 2: asynchronous call with callback
								// Create delegate to remote method
								DeleteAsyncDelegate RemoteDel = new DeleteAsyncDelegate(md.DeleteSynch);
								// Create delegate to local callback
								AsyncCallback RemoteCallback = new AsyncCallback(MDServer.EmptyAsyncCallBack);
								// Call remote method
								IAsyncResult RemAr = RemoteDel.BeginInvoke(filename, RemoteCallback, null);
							}
							catch (SocketException)
							{
								Console.WriteLine("Could not locate server");
							}
							catch (RemotingException) { }
						}
					}
					//log.WriteLine("DELETE - file " + filename + " A Synch id ->" + id);
				}
				else
				{
					//log.WriteLine("DELETE - file " + filename + " FileIsOpen id ->" + id);
					Console.WriteLine("File : " + filename + " is open.");
				}
			}
			else
			{
				//log.WriteLine("DELETE - file " + filename + " NOT EXIST id ->" + id);
				Console.WriteLine("File : " + filename + " does not exist.");
			}
			//log.WriteLine("DELETE - file " + filename + " B SaveState id ->" + id);
			SaveState();
			//log.WriteLine("DELETE - file " + filename + " A SaveState id ->" + id);
			//log.WriteLine("DELETE - file " + filename + " ---------------------END--------------------------------- id ->" + id);
		}

		private void DeleteFiles()
		{
			//Console.WriteLine("DELETEFILES");
			lock (_toDelete)
			{
				foreach (PadiFile p in _toDelete.Values)
				{
					lock (_toDeleteWaiting)
					{
						if (!_toDeleteWaiting.ContainsKey(p.Filename))
						{
							PadiFile file = new PadiFile();
							file.Filename = p.Filename;
							_toDeleteWaiting[p.Filename] = file;
						}
						foreach (Pair ds in p.DataServers)
						{
							DataServerInterface dataServer = (DataServerInterface)Activator.GetObject(typeof(DataServerInterface), ds.DataServerAddress);
							// Create delegate to remote method
							DeleteDelegate RemoteDel = new DeleteDelegate(dataServer.Delete);
							// Create delegate to local callback
							AsyncCallback RemoteCallback = new AsyncCallback(MDServer.DeleteCallBack);
							// Call remote method
							IAsyncResult RemAr = RemoteDel.BeginInvoke(ds.LocalName, RemoteCallback, null);
							_toDeleteWaiting[p.Filename].addDataServer(ds.DataServerAddress, ds.LocalName);
						}
					}
				}
				_toDelete = new Dictionary<string, PadiFile>();
			}
		}

		public PadiFile Create(String filename, int numDS, int readQ, int writeQ, int id, string address)
		{
			if (_state == State.Offline)
			{
				throw new RemotingException("MetaData Server is down");
			}
			GetDSLoads();
			if (!_primary)
			{
				_primary = true;
				_timer.Enabled = true;
				_timerCreate.Enabled = true;
				Console.WriteLine("Primary: " + _primary);
			}
			if (_files == null)
			{
				InitializeMDS();
			}
			Console.WriteLine("Create(" + filename + ", " + numDS + ", " + readQ + ", " + writeQ + ")");
			//log.WriteLine("Create(" + filename + ", " + numDS + ", " + readQ + ", " + writeQ + ") -----------------START----------------- id ->" + id);
			if (_files.ContainsKey(filename))
			{
				Console.WriteLine("The file " + filename + " already exists id ->" + id);
				//log.WriteLine("The file " + filename + " already exists id ->" + id);
				return null;
			}
			PadiFile f = new PadiFile(filename, numDS, readQ, writeQ);
			PadiFile create = new PadiFile(filename, numDS, readQ, writeQ);
			/*
             
			 *  Check Balancing and assign DS
             
			 */
			// TO DO
			//log.WriteLine("CREATE - file " + filename + " B LessUsed id ->" + id);
			List<RemoteInfo> list = GetLessUsedDS(filename, numDS, id);//Just gives the first numDS
			//log.WriteLine("CREATE - file " + filename + " A LessUsed id ->" + id);
			if (list.Count < numDS)
			{
				lock (_replicate)
				{
					_replicate[filename] = numDS - list.Count;
				}
				Console.WriteLine("File " + filename + " created in " + list.Count + " DataServers out of " + numDS);
			}
			//log.WriteLine("CREATE - file " + filename + " A _replicate id ->" + id);
			List<Pair> dataservers = new List<Pair>();
			List<Pair> dataserverscopy = new List<Pair>();
			foreach (RemoteInfo ds in list)
			{
				string local_name = filename + "_" + ds.Id;
				dataservers.Add(new Pair(ds.Address, local_name));
				dataserverscopy.Add(new Pair(ds.Address, local_name));
			}
			f.addDataServer(dataservers);
			create.addDataServer(dataserverscopy);
			//log.WriteLine("CREATE - file " + filename + " B adding files id ->" + id);
			lock (_files)
			{
				_files[filename] = f;
			}
			//log.WriteLine("CREATE - file " + filename + " B adding tocreate id ->" + id);
			lock (_toCreate)
			{
				_toCreate[filename] = create;
			}
			//log.WriteLine("CREATE - file " + filename + " B calling CF id ->" + id);
			//CreateFiles();
			//log.WriteLine("CREATE - file " + filename + " A CF id ->" + id);
			/*Dictionary<int, RemoteInfo> l = new Dictionary<int, RemoteInfo>();
			l.Add(id, new RemoteInfo(address, id));
			lock (_open)
			{
				_open[filename] = l;
			}*/
			//log.WriteLine("CREATE - file " + filename + " A open id ->" + id);
			//log.WriteLine("CREATE - file " + filename + " B synch id ->" + id);
			if (_primary)
			{
				foreach (MetadataSynchronize md in _metadataServers)
				{
					try
					{
						//md.CreateSynch(filename, numDS, readQ, writeQ, id, address, list);
						// Alternative 2: asynchronous call with callback
						// Create delegate to remote method
						CreateAsyncDelegate RemoteDel = new CreateAsyncDelegate(md.CreateSynch);
						// Create delegate to local callback
						AsyncCallback RemoteCallback = new AsyncCallback(MDServer.EmptyAsyncCallBack);
						// Call remote method
						IAsyncResult RemAr = RemoteDel.BeginInvoke(filename, numDS, readQ, writeQ, id, address, list, RemoteCallback, null);
					}
					catch (SocketException)
					{
						Console.WriteLine("Could not locate server");
					}
					catch (RemotingException) { }
				}
			}
			//log.WriteLine("CREATE - file " + filename + " A synch id ->" + id);
			//log.WriteLine("CREATE - file " + filename + " B SaveState id ->" + id);
			SaveState();
			//log.WriteLine("CREATE - file " + filename + " A SaveState id ->" + id);
			//log.WriteLine("CREATE - file " + filename + " -------------------END--------------------------------- id ->" + id);
			Console.WriteLine("File Created (" + filename + ", " + numDS + ", " + readQ + ", " + writeQ + ")");
			return f;
		}

		private void CreateFiles()
		{
			//Console.WriteLine("CREATEFILES");
			lock (_toCreate)
			{
				foreach (PadiFile p in _toCreate.Values)
				{
					lock (_toCreateWaiting)
					{
						if (!_toCreateWaiting.ContainsKey(p.Filename))
						{
							PadiFile file = new PadiFile();
							file.Filename = p.Filename;
							_toCreateWaiting[p.Filename] = file;
						}
						foreach (Pair ds in p.DataServers)
						{
							DataServerInterface dataServer = (DataServerInterface)Activator.GetObject(typeof(DataServerInterface), ds.DataServerAddress);
							// Create delegate to remote method
							CreateDelegate RemoteDel = new CreateDelegate(dataServer.Create);
							// Create delegate to local callback
							AsyncCallback RemoteCallback = new AsyncCallback(MDServer.CreateCallBack);
							// Call remote method
							IAsyncResult RemAr = RemoteDel.BeginInvoke(ds.LocalName, RemoteCallback, null);
							_toCreateWaiting[p.Filename].addDataServer(ds.DataServerAddress, ds.LocalName);
						}
					}
				}
				_toCreate = new Dictionary<string, PadiFile>();
			}
		}

		private List<RemoteInfo> GetLessUsedDS(string filename, int numDS, int id)
		{
			List<RemoteInfo> l = new List<RemoteInfo>();
			lock (_dataServersLoad)
			{
				List<KeyValuePair<int, DSLoad>> myList = _dataServersLoad.ToList();
				if (_dataServersLoad.Count > numDS)
				{
					myList.Sort((firstPair, nextPair) => { return firstPair.Value.CompareTo(nextPair.Value); });
				}
				for (int i = 0; i < numDS && i < myList.Count; i++)
				{
					l.Add(myList[i].Value.Info);
				}
			}
			return l;
		}

		//      PUPPET MASTER METHODS

		public void Fail()
		{
			Console.WriteLine("Fail()");
			_primary = false;
			_timer.Enabled = false;
			_timerCreate.Enabled = false;
			_state = State.Offline;
		}
		public void Recover()
		{
			Console.WriteLine("Recover()");
			lock (this)
			{
				//log = new StreamWriter(@"log" + _id + ".txt");
				if (_timer == null)
				{
					_timer = new Timer(TIME_GET_LOAD);
					_timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
					_timerCreate = new Timer(TIME_CREATEDELETE);
					_timerCreate.Elapsed += new ElapsedEventHandler(_timercreate_Elapsed);
				}
				if (_metadataServersAddresses.Count == 0)
				{
					Directory.CreateDirectory("md");
					InitializeMDS();
					_timer.Enabled = true;
					_timerCreate.Enabled = true;
				}
				else
				{
					ConnectMetadaServers();
					foreach (MetadataSynchronize md in _metadataServers)
					{
						try
						{
							_files = md.GetFiles();
							_replicate = md.GetReplicate();
							_open = md.GetOpen();
							_dataServersInfo = md.GetDataServers();
							_dataServersLoad = md.GetDataServersLoad();
							_toCreate = md.GetToCreate();
							_toDelete = md.GetToDelete();
							_toCreateWaiting = md.GetToCreateWaiting();
							_toDeleteWaiting = md.GetToDeleteWaiting();
							_toMigrate = md.GetToMigrate();
							int id = md.ImAlive(_id);
							if (_id < id)
							{
								_primary = true;
								_timer.Enabled = true;
								_timerCreate.Enabled = true;
							}
							ConnectDataServers();
							break;
						}
						catch (RemotingException e)
						{
							Console.WriteLine(e.Message);
						}
					}
				}
			}
			if (_files == null)
			{
				Console.WriteLine("MD could not synchronize");
			}
			_state = State.Online;
			Console.WriteLine("Primary: " + _primary);
			SaveState();
		}

		private void FindMetadataAddresses()
		{
			try
			{
				BinaryFormatter bFormatter = new BinaryFormatter();
				Stream stream = File.Open(@"MetadataMDAddresses" + _id, FileMode.Open);
				_metadataServersAddresses = (List<string>)bFormatter.Deserialize(stream);
				stream.Close();
				Console.WriteLine("MDS Addresses recovered sucessfully.");
			}
			catch (System.IO.FileNotFoundException) { }
		}

		private void ConnectMetadaServers()
		{
			_metadataServers = new List<MetadataSynchronize>();
			FindMetadataAddresses();
			foreach (string address in _metadataServersAddresses)
			{
				MetadataSynchronize md = (MetadataSynchronize)Activator.GetObject(typeof(MetadataSynchronize), address);
				if (md == null)
				{
					Console.WriteLine("Could not register MetaData");

				}
				else
				{
					_metadataServers.Add(md);
				}
			}
		}

		private void ConnectDataServers()
		{
			_dataServers = new Dictionary<int, DataServerInterface>();
			foreach (RemoteInfo dsi in _dataServersInfo)
			{
				DataServerInterface dataServer = (DataServerInterface)Activator.GetObject(typeof(DataServerInterface), dsi.Address);
				if (dataServer == null)
				{
					Console.WriteLine("Could not register Data Server " + dsi.Id);

				}
				else
				{
					_dataServers[dsi.Id] = dataServer;
				}
			}
		}

		public List<PadiFile> Dump()
		{
			Console.WriteLine("Dump()"); //fazer dump localmente tb
			lock (_files)
			{
				foreach (PadiFile temp in _files.Values.ToList<PadiFile>())
				{
					Console.Write(temp);
				}
			}
			Console.WriteLine("-----------------------------------------------------");
			return _files.Values.ToList<PadiFile>();
		}

		//			DATA SERVER REGISTERING
		public bool RegisterDataServer(int id, string address, DSLoad load)
		{
			if (_state == State.Offline)
			{
				throw new RemotingException("MetaData Server is down");
			}
			if (_files == null)
			{
				InitializeMDS();
			}
			Console.WriteLine("RegisterDataServer(" + id + ", " + address + ")");
			DataServerInterface dataServer = (DataServerInterface)Activator.GetObject(typeof(DataServerInterface), address);
			if (dataServer == null)
			{
				Console.WriteLine("Could not register Data Server " + id);
				return false;
			}
			_dataServers[id] = dataServer;
			_dataServersInfo.Add(new RemoteInfo(address, id));
			lock (_dataServersLoad)
			{
				_dataServersLoad[id] = load;
			}
			Console.WriteLine("Data Server " + id + " registered successfully");
			ReplicateFiles(id, address);
			SaveState();
			return true;
		}

		private void ReplicateFiles(int id, string address)
		{
			lock (_replicate)
			{
				Console.WriteLine("ReplicatingFilesinDataServers");
				List<string> toRemove = new List<string>();
				List<string> toDecrement = new List<string>();
				foreach (string file in _replicate.Keys)
				{
					lock (_open)
					{
						if (_open.ContainsKey(file))
						{
							foreach (RemoteInfo r in _open[file].Values)
							{
								ClientInterface client = (ClientInterface)Activator.GetObject(typeof(ClientInterface), r.Address);
								client.AddServer(file, address, file + "_" + id);
							}
						}
					}
					if (_files[file].addDataServer(address, file + "_" + id))
					{
						Console.WriteLine("Added DataServer to File: " + file);
					}
					lock (_toCreate)
					{
						if (!_toCreate.ContainsKey(file))
						{
							PadiFile p = new PadiFile();
							p.Filename = file;
							_toCreate[file] = p;
						}
						_toCreate[file].addDataServer(address, file + "_" + id);
					}
					if (_replicate[file] == 1)
					{
						toRemove.Add(file);
						Console.WriteLine("File " + file + " replicated in new DataServer. No more Replications Needed");
					}
					else
					{
						toDecrement.Add(file);
						Console.WriteLine("File " + file + " replicated in DataServer " + id + ".");
					}
				}
				foreach (string fileToDec in toDecrement)
				{
					_replicate[fileToDec] -= 1;
				}
				foreach (string fileToRemove in toRemove)
				{
					_replicate.Remove(fileToRemove);
				}
			}
			CreateFiles();
		}

		//       METADATA SYNCH

		public bool RegisterMetadata(int id, string address)
		{
			Console.WriteLine("RegisterMetadata(" + id + ", " + address + ")");
			MetadataSynchronize metadata = (MetadataSynchronize)Activator.GetObject(typeof(MetadataSynchronize), address);
			if (metadata == null)
			{
				Console.WriteLine("Could not register Metadata " + id);
				return false;
			}
			_metadataServersAddresses.Add(address);
			_metadataServers.Add(metadata);
			Console.WriteLine("Metadata " + id + " registered successfully");
			SaveState();
			return true;
		}

		public Dictionary<string, PadiFile> GetFiles()
		{
			if (_state == State.Offline)
			{
				throw new RemotingException("MetaData Server is down");
			}
			return _files;
		}
		public Dictionary<string, int> GetReplicate()
		{
			if (_state == State.Offline)
			{
				throw new RemotingException("MetaData Server is down");
			}
			return _replicate;
		}
		public Dictionary<string, Dictionary<int, RemoteInfo>> GetOpen()
		{
			if (_state == State.Offline)
			{
				throw new RemotingException("MetaData Server is down");
			}
			return _open;
		}
		public List<RemoteInfo> GetDataServers()
		{
			if (_state == State.Offline)
			{
				throw new RemotingException("MetaData Server is down");
			}
			return _dataServersInfo;
		}
		public Dictionary<int, DSLoad> GetDataServersLoad()
		{
			if (_state == State.Offline)
			{
				throw new RemotingException("MetaData Server is down");
			}
			return _dataServersLoad;
		}
		public Dictionary<string, PadiFile> GetToCreate()
		{
			if (_state == State.Offline)
			{
				throw new RemotingException("MetaData Server is down");
			}
			return _toCreate;
		}
		public Dictionary<string, PadiFile> GetToCreateWaiting()
		{
			if (_state == State.Offline)
			{
				throw new RemotingException("MetaData Server is down");
			}
			return _toCreateWaiting;
		}
		public Dictionary<string, PadiFile> GetToDelete()
		{
			if (_state == State.Offline)
			{
				throw new RemotingException("MetaData Server is down");
			}
			return _toDelete;
		}
		public Dictionary<string, PadiFile> GetToDeleteWaiting()
		{
			if (_state == State.Offline)
			{
				throw new RemotingException("MetaData Server is down");
			}
			return _toDeleteWaiting;
		}
		public List<Migration> GetToMigrate()
		{
			if (_state == State.Offline)
			{
				throw new RemotingException("MetaData Server is down");
			}
			return _toMigrate;
		}

		public void OpenSynch(string filename, int id, string address)
		{
			if (_state == State.Offline)
			{
				throw new RemotingException("MetaData Server is down");
			}
			Console.WriteLine("Open(" + filename + ")");
			lock (_open)
			{
				if (_open.ContainsKey(filename))
				{
					if (!_open[filename].ContainsKey(id))
					{
						_open[filename].Add(id, new RemoteInfo(address, id));
					}
				}
				else
				{
					Dictionary<int, RemoteInfo> list = new Dictionary<int, RemoteInfo>();
					list.Add(id, new RemoteInfo(address, id));
					_open[filename] = list;
				}
			}
			SaveState();
		}
		public void CloseSynch(string filename, int id)
		{
			if (_state == State.Offline)
			{
				throw new RemotingException("MetaData Server is down");
			}
			Console.WriteLine("Close(" + filename + ")");
			lock (_open)
			{
				_open[filename].Remove(id);
				if (_open[filename].Count == 0)
				{
					_open.Remove(filename);
				}
			}
			SaveState();
		}
		public void DeleteSynch(string filename)
		{

			if (_state == State.Offline)
			{
				throw new RemotingException("MetaData Server is down");
			}
			Console.WriteLine("Delete(" + filename + ")");
			PadiFile p = null;
			if (!_open.ContainsKey(filename))
			{
				lock (_files)
				{
					p = _files[filename];
					_files.Remove(filename);
				}
			}
			if (p != null)
			{
				lock (_toDelete)
				{
					_toDelete[filename] = p;
				}
			}
			SaveState();
		}
		public void CreateSynch(string filename, int numDS, int readQ, int writeQ, int id, string address, List<RemoteInfo> list)
		{
			if (_state == State.Offline)
			{
				throw new RemotingException("MetaData Server is down");
			}
			Console.WriteLine("Create(" + filename + ", " + numDS + ", " + readQ + ", " + writeQ + ")");
			PadiFile f = new PadiFile(filename, numDS, readQ, writeQ);
			PadiFile create = new PadiFile(filename, numDS, readQ, writeQ);
			lock (_replicate)
			{
				if (list.Count < numDS)
				{
					_replicate[filename] = numDS - list.Count;
					Console.WriteLine("File " + filename + " created in " + list.Count + " DataServers out of " + numDS);
				}
			}
			List<Pair> dataservers = new List<Pair>();
			List<Pair> dataserverscopy = new List<Pair>();
			foreach (RemoteInfo ds in list)
			{
				string local_name = filename + "_" + ds.Id;
				dataservers.Add(new Pair(ds.Address, local_name));
				dataserverscopy.Add(new Pair(ds.Address, local_name));
			}
			f.addDataServer(dataservers);
			create.addDataServer(dataserverscopy);
			lock (_files)
			{
				_files[filename] = f;
			}
			lock (_toCreate)
			{
				_toCreate[filename] = create;
			}
			/*Dictionary<int, RemoteInfo> l = new Dictionary<int, RemoteInfo>();
			l.Add(id, new RemoteInfo(address, id));
			lock (_open)
			{
				_open[filename] = l;
			}*/
			SaveState();
		}
		public void ToCreateSynch(FileId r)
		{
			if (_toCreate.ContainsKey(r.Filename.Substring(0, r.Filename.Length - 2)))
			{
				lock (_toCreate)
				{
					PadiFile p = _toCreate[r.Filename.Substring(0, r.Filename.Length - 2)];
					p.DataServers.Remove(new Pair(r.Address, r.Filename));
					if (p.DataServers.Count == 0)
					{
						_toCreate.Remove(r.Filename.Substring(0, r.Filename.Length - 2));
					}
				}
			}
		}
		public void ToCreateWaitingSynch(FileId r)
		{
			PadiFile p;
			if (_toCreateWaiting.ContainsKey(r.Filename.Substring(0, r.Filename.Length - 2)))
			{
				lock (_toCreateWaiting)
				{
					p = _toCreateWaiting[r.Filename.Substring(0, r.Filename.Length - 2)];
					p.DataServers.Remove(new Pair(r.Address, r.Filename));
					if (p.DataServers.Count == 0)
					{
						_toCreateWaiting.Remove(r.Filename.Substring(0, r.Filename.Length - 2));
					}
				}
				lock (_toCreate)
				{
					if (!_toCreate.ContainsKey(p.Filename))
					{
						PadiFile file = new PadiFile();
						file.Filename = p.Filename;
						_toCreate[p.Filename] = file;
					}
					_toCreate[p.Filename].addDataServer(r.Address, r.Filename);
				}
			}
		}
		public void ToDeleteSynch(FileId r)
		{
			if (_toDelete.ContainsKey(r.Filename.Substring(0, r.Filename.Length - 2)))
			{
				lock (_toDelete)
				{
					PadiFile p = _toDelete[r.Filename.Substring(0, r.Filename.Length - 2)];
					p.DataServers.Remove(new Pair(r.Address, r.Filename));
					if (p.DataServers.Count == 0)
					{
						_toDelete.Remove(r.Filename.Substring(0, r.Filename.Length - 2));
					}
				}
			}
		}
		public void ToDeleteWaitingSynch(FileId r)
		{
			PadiFile p;
			if (_toDeleteWaiting.ContainsKey(r.Filename.Substring(0, r.Filename.Length - 2)))
			{
				lock (_toDeleteWaiting)
				{
					p = _toDeleteWaiting[r.Filename.Substring(0, r.Filename.Length - 2)];
					p.DataServers.Remove(new Pair(r.Address, r.Filename));
					if (p.DataServers.Count == 0)
					{
						_toDeleteWaiting.Remove(r.Filename.Substring(0, r.Filename.Length - 2));
					}
				}
				lock (_toDelete)
				{
					if (!_toDelete.ContainsKey(p.Filename))
					{
						PadiFile file = new PadiFile();
						file.Filename = p.Filename;
						_toDelete[p.Filename] = file;
					}
					_toDelete[p.Filename].addDataServer(r.Address, r.Filename);
				}
			}
		}

		public void MigrateSynch(Dictionary<int, DSLoad> newLoads, Dictionary<string, PadiFile> newFiles)
		{
			/*_dataServersLoad update*/
			Console.WriteLine("MigrateSynch()");
			lock (_dataServersLoad)
			{
				foreach (KeyValuePair<int, DSLoad> pair in newLoads)
				{
					_dataServersLoad[pair.Key] = pair.Value;
				}
			}
			lock (_files)
			{
				foreach (KeyValuePair<string, PadiFile> pair in newFiles)
				{
					_files[pair.Key] = pair.Value;
				}
			}
			Console.WriteLine("Migrations Completed");
		}

		public int ImAlive(int id)
		{
			if (_state == State.Offline)
			{
				throw new RemotingException("MetaData Server is down");
			}
			Console.WriteLine("ImAlive(" + id + ")");
			if (id < _id)
			{
				_primary = false;
				_timer.Enabled = false;
				_timerCreate.Enabled = false;
			}
			Console.WriteLine("Primary: " + _primary);
			return _id;
		}

		//      SAVE/RECOVER STATE

		void SaveState()
		{
			Console.WriteLine("SaveState()");
			Stream stream;
			BinaryFormatter bFormatter;
			lock (_files)
			{
				stream = File.Open(@"md/MetadataFiles" + _id, FileMode.OpenOrCreate);
				bFormatter = new BinaryFormatter();
				bFormatter.Serialize(stream, _files);
				stream.Close();
			}
			lock (_open)
			{
				stream = File.Open(@"md/MetadataOpen" + _id, FileMode.OpenOrCreate);
				bFormatter = new BinaryFormatter();
				bFormatter.Serialize(stream, _open);
				stream.Close();
			}
			lock (_dataServersInfo)
			{
				stream = File.Open(@"md/MetadataDataServers" + _id, FileMode.OpenOrCreate);
				bFormatter = new BinaryFormatter();
				bFormatter.Serialize(stream, _dataServersInfo);
				stream.Close();
			}
			lock (_replicate)
			{
				stream = File.Open(@"md/MetadataReplicate" + _id, FileMode.OpenOrCreate);
				bFormatter = new BinaryFormatter();
				bFormatter.Serialize(stream, _replicate);
				stream.Close();
			}
			lock (_metadataServersAddresses)
			{
				stream = File.Open(@"md/MetadataMDAddresses" + _id, FileMode.OpenOrCreate);
				bFormatter = new BinaryFormatter();
				bFormatter.Serialize(stream, _metadataServersAddresses);
				stream.Close();
			}
			lock (_dataServersLoad)
			{
				stream = File.Open(@"md/MetadataDataServersLoad" + _id, FileMode.OpenOrCreate);
				bFormatter = new BinaryFormatter();
				bFormatter.Serialize(stream, _dataServersLoad);
				stream.Close();
			}
			lock (_toCreate)
			{
				stream = File.Open(@"md/MetadataToCreate" + _id, FileMode.OpenOrCreate);
				bFormatter = new BinaryFormatter();
				bFormatter.Serialize(stream, _toCreate);
				stream.Close();
			}
			lock (_toCreateWaiting)
			{
				stream = File.Open(@"md/MetadataToCreateWaiting" + _id, FileMode.OpenOrCreate);
				bFormatter = new BinaryFormatter();
				bFormatter.Serialize(stream, _toCreateWaiting);
				stream.Close();
			}
			lock (_toDelete)
			{
				stream = File.Open(@"md/MetadataToDelete" + _id, FileMode.OpenOrCreate);
				bFormatter = new BinaryFormatter();
				bFormatter.Serialize(stream, _toDelete);
				stream.Close();
			}
			lock (_toDeleteWaiting)
			{
				stream = File.Open(@"md/MetadataToDeleteWaiting" + _id, FileMode.OpenOrCreate);
				bFormatter = new BinaryFormatter();
				bFormatter.Serialize(stream, _toDeleteWaiting);
				stream.Close();
			}
			lock (_toMigrate)
			{
				stream = File.Open(@"md/MetadataToMigrate" + _id, FileMode.OpenOrCreate);
				bFormatter = new BinaryFormatter();
				bFormatter.Serialize(stream, _toMigrate);
				stream.Close();
			}
			Console.WriteLine("Saved MDS state");
		}

		private void InitializeMDS()
		{
			Console.WriteLine("InitializeMDS()");
			BinaryFormatter bFormatter = new BinaryFormatter();
			try
			{
				Stream stream = File.Open(@"md/MetadataFiles" + _id, FileMode.Open);
				_files = (Dictionary<string, PadiFile>)bFormatter.Deserialize(stream);
				stream.Close();
				Console.WriteLine("MDS Files recovered sucessfully.");
			}
			catch (System.IO.FileNotFoundException)
			{
				Console.WriteLine("No previous state found for files, starting from scratch.");
				_files = new Dictionary<string, PadiFile>();
			}
			try
			{
				Stream stream = File.Open(@"md/MetadataOpen" + _id, FileMode.Open);
				_open = (Dictionary<string, Dictionary<int, RemoteInfo>>)bFormatter.Deserialize(stream);
				stream.Close();
				Console.WriteLine("MDS Open Files recovered sucessfully.");
			}
			catch (System.IO.FileNotFoundException)
			{
				Console.WriteLine("No previous state found for open files, starting from scratch.");
				_open = new Dictionary<string, Dictionary<int, RemoteInfo>>();
			}
			try
			{
				Stream stream = File.Open(@"md/MetadataDataServers" + _id, FileMode.Open);
				_dataServersInfo = (List<RemoteInfo>)bFormatter.Deserialize(stream);
				stream.Close();
				foreach (RemoteInfo info in _dataServersInfo)
				{
					_dataServers[info.Id] = (DataServerInterface)Activator.GetObject(typeof(DataServerInterface), info.Address);
				}
				Console.WriteLine("MDS _DataServers recovered sucessfully.");
			}
			catch (System.IO.FileNotFoundException)
			{
				Console.WriteLine("No previous state found for data servers, starting from scratch.");
				_dataServers = new Dictionary<int, DataServerInterface>();
			}

			try
			{
				Stream stream = File.Open(@"md/MetadataReplicate" + _id, FileMode.Open);
				_replicate = (Dictionary<string, int>)bFormatter.Deserialize(stream);
				stream.Close();
				Console.WriteLine("MDS Replicate recovered sucessfully.");
			}
			catch (System.IO.FileNotFoundException)
			{
				Console.WriteLine("No previous state found for Replicate, starting from scratch.");
				_replicate = new Dictionary<string, int>();
			}
			try
			{
				Stream stream = File.Open(@"md/MetadataMDAddresses" + _id, FileMode.Open);
				_metadataServersAddresses = (List<string>)bFormatter.Deserialize(stream);
				stream.Close();
				Console.WriteLine("MDS Addresses recovered sucessfully.");
			}
			catch (System.IO.FileNotFoundException)
			{
				Console.WriteLine("No previous state found for MD addresses, starting from scratch.");
				_metadataServersAddresses = new List<string>();
			}
			try
			{
				Stream stream = File.Open(@"md/MetadataDataServersLoad" + _id, FileMode.Open);
				_dataServersLoad = (Dictionary<int, DSLoad>)bFormatter.Deserialize(stream);
				stream.Close();
				Console.WriteLine("MDS DataServersLoad recovered sucessfully.");
			}
			catch (System.IO.FileNotFoundException)
			{
				Console.WriteLine("No previous state found for DataServersLoad, starting from scratch.");
				_dataServersLoad = new Dictionary<int, DSLoad>();
			}
			try
			{
				Stream stream = File.Open(@"md/MetadataToCreate" + _id, FileMode.Open);
				_toCreate = (Dictionary<string, PadiFile>)bFormatter.Deserialize(stream);
				stream.Close();
				Console.WriteLine("MDS ToCreate recovered sucessfully.");
			}
			catch (System.IO.FileNotFoundException)
			{
				Console.WriteLine("No previous state found for ToCreate, starting from scratch.");
				_toCreate = new Dictionary<string, PadiFile>();
			}
			try
			{
				Stream stream = File.Open(@"md/MetadataToCreateWaiting" + _id, FileMode.Open);
				_toCreateWaiting = (Dictionary<string, PadiFile>)bFormatter.Deserialize(stream);
				stream.Close();
				Console.WriteLine("MDS ToCreateWaiting recovered sucessfully.");
			}
			catch (System.IO.FileNotFoundException)
			{
				Console.WriteLine("No previous state found for ToCreateWaiting, starting from scratch.");
				_toCreateWaiting = new Dictionary<string, PadiFile>();
			}
			try
			{
				Stream stream = File.Open(@"md/MetadataToDelete" + _id, FileMode.Open);
				_toDelete = (Dictionary<string, PadiFile>)bFormatter.Deserialize(stream);
				stream.Close();
				Console.WriteLine("MDS ToDelete recovered sucessfully.");
			}
			catch (System.IO.FileNotFoundException)
			{
				Console.WriteLine("No previous state found for ToDelete, starting from scratch.");
				_toDelete = new Dictionary<string, PadiFile>();
			}
			try
			{
				Stream stream = File.Open(@"md/MetadataToDeleteWaiting" + _id, FileMode.Open);
				_toDeleteWaiting = (Dictionary<string, PadiFile>)bFormatter.Deserialize(stream);
				stream.Close();
				Console.WriteLine("MDS ToDeleteWaiting recovered sucessfully.");
			}
			catch (System.IO.FileNotFoundException)
			{
				Console.WriteLine("No previous state found for ToDeleteWaiting, starting from scratch.");
				_toDeleteWaiting = new Dictionary<string, PadiFile>();
			}
			try
			{
				Stream stream = File.Open(@"md/MetadataToMigrate" + _id, FileMode.Open);
				_toMigrate = (List<Migration>)bFormatter.Deserialize(stream);
				stream.Close();
				Console.WriteLine("MDS ToMigrate recovered sucessfully.");
			}
			catch (System.IO.FileNotFoundException)
			{
				Console.WriteLine("No previous state found for ToMigrate, starting from scratch.");
				_toMigrate = new List<Migration>();
			}

		}

		// MIGRATIONS

		public void MigrateFiles()
		{
			lock (_toMigrate)
			{
				for (int i = 0; i < _toMigrate.Count; ++i)
				{
					Migration migration = _toMigrate[i];
					DataServerInterface dataServer = _dataServers[migration.Source];
					MigrationInfo info = new MigrationInfo(_dataServersLoad[migration.Destination].Info.Address, migration.Filename, migration.Filename.Substring(0, migration.Filename.LastIndexOf('_')) + "_" + migration.Destination);
					try
					{
						MigrationDelegate remoteDel = new MigrationDelegate(dataServer.MigrateFile);
						AsyncCallback remoteCallback = new AsyncCallback(MDServer.MigrateCallback);
						IAsyncResult remoteAsyncR = remoteDel.BeginInvoke(info, migration, remoteCallback, null);
					}
					catch (SocketException)
					{
						Console.WriteLine("Could not locate server, Migration not successful:");
						Console.WriteLine("DS" + migration.Source + " ==== " + migration.Filename + " ===> DS" + migration.Destination);
						_toMigrate.Remove(migration);
					}
					catch (RemotingException)
					{
						Console.WriteLine("Migration not successfull:");
						Console.WriteLine("DS" + migration.Source + " ==== " + migration.Filename + " ===> DS" + migration.Destination);
						_toMigrate.RemoveAt(i--);
					}
					//replicar _dataServersLoad (para outros MDS)
					//replicar _files (para outros MDS)
				}
			}
		}

		public void Migrate()
		{
			List<GetStatsDelegate> delegates = new List<GetStatsDelegate>();
			List<IAsyncResult> results = new List<IAsyncResult>();
			List<int> ids = new List<int>();
			foreach (KeyValuePair<int, DataServerInterface> pair in _dataServers.ToList())
			{
				DataServerInterface dataServer = pair.Value;
				GetStatsDelegate remoteDel = new GetStatsDelegate(dataServer.GetStats);
				IAsyncResult remoteAsyncR = remoteDel.BeginInvoke(null, null);
				delegates.Add(remoteDel);
				results.Add(remoteAsyncR);
				ids.Add(pair.Key);
			}
			_dataServersLoad = new Dictionary<int, DSLoad>();
			_getStatsTimer = new Timer(GET_STATS_TIMEOUT);
			_getStatsTimer.Elapsed += new ElapsedEventHandler(getStatsTimeoutHandler);
			_getStatsTimer.Enabled = true;
			_getStatsTimedOut = false;
			//efectuar migracao
			//actualizar _dataServersLoad (para o create ter dados actualizados) e replicar (para outros MDS)
			//actualizar _files e replicar (para outros MDS)
			//TODO
			while (!_getStatsTimedOut && results.Count > 0)
			{
				for (int i = 0; i < results.Count; ++i)
				{
					try
					{
						IAsyncResult remoteAsyncR = results[i];
						if (remoteAsyncR.IsCompleted)
						{
							DSLoad load = delegates[i].EndInvoke(remoteAsyncR);
							_dataServersLoad[load.Info.Id] = load;
							results.RemoveAt(i);
							delegates.RemoveAt(i);
							ids.RemoveAt(i);
						}
					}
					catch (RemotingException e)
					{
						Console.WriteLine("DS" + ids[i] + " did not respond (threw an exception):");
						Console.WriteLine(e.Message);
						results.RemoveAt(i);
						delegates.RemoveAt(i);
						ids.RemoveAt(i);
					}
				}
				System.Threading.Thread.Sleep(100);
			}
			/*DEBUG*/
			Console.WriteLine("Loads report:");
			foreach (KeyValuePair<int, DSLoad> server in _dataServersLoad.ToList())
			{
				Console.WriteLine("  DS" + server.Key + ": " + server.Value.Load);
				foreach (KeyValuePair<string, FileLoad> file in server.Value.Loads.ToList())
				{
					Console.WriteLine("    " + file.Key + ": " + file.Value.Load);
				}
			}
			/*DEBUG*/
			Dictionary<int, DSLoad> filteredLoads = removeOpenFiles(_dataServersLoad);
			filteredLoads = removeDeletedFiles(filteredLoads);
			List<Migration> migrations = CalculateMigrations(filteredLoads.ToList(), 0.15f);
			migrations = OptimizeMigrations(migrations);
			_toMigrate.AddRange(migrations);
			MigrateFiles();
		}

		private List<Migration> CalculateMigrations(List<KeyValuePair<int, DSLoad>> loads, float threshold)
		{
			Console.WriteLine("\nMigrations calculated:");
			List<Migration> migrations = new List<Migration>();
			while (loads.Count > 1)
			{
				//sort crescente por DS load
				loads.Sort((firstPair, nextPair) => { return firstPair.Value.CompareTo(nextPair.Value); });
				DSLoad smallestLoad = null;
				DSLoad biggestLoad = loads[loads.Count - 1].Value;
				if (biggestLoad.Loads.Count < 2) //DS com maior load so tem 1 ficheiro
				{
					//Console.WriteLine("DS " + biggestLoad.ID + " Load has only 1 file. Removing from list..."); //DEBUG
					loads.RemoveAt(loads.Count - 1);
					continue;
				}
				float biggestScore = biggestLoad.Load;
				List<KeyValuePair<string, FileLoad>> smallestList = null;
				List<KeyValuePair<string, FileLoad>> biggestList = biggestLoad.Loads.ToList();
				//sort crescente por File load
				biggestList.Sort((firstPair, nextPair) => { return firstPair.Value.CompareTo(nextPair.Value); });
				FileLoad highestFile = null;
				bool validMigration = false;
				for (int k = 0; k < loads.Count - 1; ++k)
				{
					smallestLoad = loads[k].Value;
					smallestList = smallestLoad.Loads.ToList();
					int i = biggestList.Count - 1;
					while (!validMigration && i >= 0)
					{
						highestFile = biggestList[i].Value;
						string global_filename = highestFile.FileName.Substring(0, highestFile.FileName.LastIndexOf('_'));
						bool is_replica = false;
						for (int j = 0; j < smallestList.Count; ++j)
						{
							FileLoad comparing_file = smallestList[j].Value;
							string comparing_name = comparing_file.FileName.Substring(0, comparing_file.FileName.LastIndexOf('_'));
							if (global_filename.Equals(comparing_name))
							{
								is_replica = true;
							}
						}
						if (!is_replica)
						{
							validMigration = true;
						}
						--i;
					}
					if (validMigration)
					{
						break;
					}
				}
				if (!validMigration) //não há nenhum DS que não tenha nenhum ficheiro existente no DS com maior load
				{
					loads.RemoveAt(loads.Count - 1);
					continue;
				}
				float smallestScore = smallestLoad.Load;
				float highestScore = highestFile.Load;
				float previousRatio = smallestScore / biggestScore;
				float tempScore1 = smallestScore + highestScore;
				float tempScore2 = biggestScore - highestScore;
				//Console.WriteLine("Biggest Score: " + biggestScore + "  Smallest Score: " + smallestScore); //DEBUG
				//Console.WriteLine("File score to consider: " + highestScore);
				if (Math.Abs(smallestScore - biggestScore) > Math.Abs(tempScore1 - tempScore2)) //distancia menor
				{
					float tempRatio;
					if (tempScore1 < tempScore2)
					{
						tempRatio = tempScore1 / tempScore2;
					}
					else
					{
						tempRatio = tempScore2 / tempScore1;
					}
					if (Math.Abs(previousRatio - tempRatio) > threshold) //ratio maior threshold
					{
						//Console.WriteLine("DS " + biggestLoad.ID + " will migrate file " + highestFile.FileName + " to DS " + smallestLoad.ID); //DEBUG
						//remover highestFile de biggestLoad e actualizar valor de Load
						biggestLoad.Loads.Remove(highestFile.FileName);
						biggestLoad.Load -= highestScore;
						//adicionar highestFile a smallestLoad e actualizar valor de Load
						smallestLoad.Loads.Add(highestFile.FileName, highestFile);
						smallestLoad.Load += highestScore;
						//add migration
						Console.WriteLine("DS" + biggestLoad.Info.Id + " ==== " + highestFile.FileName + " ===> DS" + smallestLoad.Info.Id);
						migrations.Add(new Migration(biggestLoad.Info.Id, smallestLoad.Info.Id, highestFile.FileName));
					}
					else //ratio menor que threshold
					{
						//Console.WriteLine("Previous Ratio (" + previousRatio + ") - New Ratio (" + tempRatio + ") smaller than threshold (" + Math.Abs(previousRatio - tempRatio) + ")"); //DEBUG
						//remover highestFile de biggestLoad e NAO actualizar valor de Load
						biggestLoad.Loads.Remove(highestFile.FileName);
					}
				}
				else //distancia maior
				{
					//Console.WriteLine("New Distance (" + Math.Abs(tempScore1 - tempScore2) + ") bigger than Previous Distance (" + Math.Abs(smallestScore - biggestScore) + ")"); //DEBUG
					//remover highestFile de biggestLoad e NAO actualizar valor de Load
					biggestLoad.Loads.Remove(highestFile.FileName);
				}
			}
			return migrations;
		}

		private List<Migration> OptimizeMigrations(List<Migration> migrations)
		{
			List<Migration> optimized = new List<Migration>();
			Dictionary<string, int> sources = new Dictionary<string, int>();
			Dictionary<string, int> destinations = new Dictionary<string, int>();
			foreach (Migration migration in migrations)
			{
				if (sources.ContainsKey(migration.Filename))
				{
					destinations[migration.Filename] = migration.Destination;
				}
				else
				{
					sources[migration.Filename] = migration.Source;
					destinations[migration.Filename] = migration.Destination;
				}
			}
			Console.WriteLine("\nMigrations optimized:");
			foreach (KeyValuePair<string, int> pair in destinations)
			{
				optimized.Add(new Migration(sources[pair.Key], pair.Value, pair.Key));
				Console.WriteLine("DS" + sources[pair.Key] + " ==== " + pair.Key + " ===> DS" + pair.Value);
			}
			return optimized;
		}

		private Dictionary<int, DSLoad> removeOpenFiles(Dictionary<int, DSLoad> loads)
		{
			Dictionary<int, DSLoad> filteredLoads = new Dictionary<int, DSLoad>();
			foreach (KeyValuePair<int, DSLoad> server in loads)
			{
				DSLoad dsLoad = server.Value;
				DSLoad filteredLoad = new DSLoad(dsLoad.Info.Id, dsLoad.Info.Address);
				foreach (KeyValuePair<string, FileLoad> file in dsLoad.Loads)
				{
					string filename = file.Key.Substring(0, file.Key.LastIndexOf('_'));
					if (!_open.ContainsKey(filename))
					{
						filteredLoad.addFile(new FileLoad(file.Value.FileName, file.Value.Load));
					}
				}
				filteredLoad.Load = dsLoad.Load;
				filteredLoads[filteredLoad.Info.Id] = filteredLoad;
			}
			return filteredLoads;
		}

		private Dictionary<int, DSLoad> removeDeletedFiles(Dictionary<int, DSLoad> loads)
		{
			Dictionary<int, DSLoad> filteredLoads = new Dictionary<int, DSLoad>();
			foreach (KeyValuePair<int, DSLoad> server in loads)
			{
				DSLoad dsLoad = server.Value;
				DSLoad filteredLoad = new DSLoad(dsLoad.Info.Id, dsLoad.Info.Address);
				float extraLoad = 0;
				foreach (KeyValuePair<string, FileLoad> file in dsLoad.Loads)
				{
					string filename = file.Key.Substring(0, file.Key.LastIndexOf('_'));
					if (_files.ContainsKey(filename))
					{
						filteredLoad.addFile(new FileLoad(file.Value.FileName, file.Value.Load));
					}
					else
					{
						extraLoad += file.Value.Load;
					}
				}
				filteredLoad.Load = dsLoad.Load - extraLoad;
				filteredLoads[filteredLoad.Info.Id] = filteredLoad;
			}
			return filteredLoads;
		}

		private void getStatsTimeoutHandler(object sender, ElapsedEventArgs e)
		{
			_getStatsTimedOut = true;
			_getStatsTimer.Enabled = false;
		}

		//    ASYNCH DELEGATES

		public delegate FileId DeleteDelegate(string local_file_name);
		public delegate void ToDeleteAsyncDelegate(FileId r);
		public delegate void ToDeleteWaitingAsyncDelegate(FileId r);
		public delegate FileId CreateDelegate(string local_file_name);
		public delegate void ToCreateAsyncDelegate(FileId r);
		public delegate void ToCreateWaitingAsyncDelegate(FileId r);
		public delegate DSLoad GetStatsDelegate();
		public delegate Migration MigrationDelegate(MigrationInfo migrationInfo, Migration migration);
		public delegate void OpenAsyncDelegate(string filename, int id, string address);
		public delegate void CloseAsyncDelegate(string filename, int id);
		public delegate void DeleteAsyncDelegate(string filename);
		public delegate void CreateAsyncDelegate(string filename, int numDS, int readQ, int writeQ, int id, string address, List<RemoteInfo> list);
		public delegate void MigrateAsyncDelegate(Dictionary<int, DSLoad> newLoads, Dictionary<string, PadiFile> newFiles);

		//    ASYNCH CALLBACKS
		public static void MigrateCallback(IAsyncResult asyncR)
		{
			MigrationDelegate del = (MigrationDelegate)((AsyncResult)asyncR).AsyncDelegate;
			Migration migration = del.EndInvoke(asyncR);
			string filename = migration.Filename;
			string destinationAddress;
			Console.WriteLine("MigrateCallback(" + migration.Source + " --> " + migration.Filename + " --> " + migration.Destination + ")");
			/*_dataServersLoad update*/
			Dictionary<int, DSLoad> newLoads = new Dictionary<int, DSLoad>();
			Dictionary<string, PadiFile> newFiles = new Dictionary<string, PadiFile>();
			lock (_dataServersLoad)
			{
				DSLoad dsLoad = _dataServersLoad[migration.Source];
				FileLoad fileLoad = dsLoad.GetFileLoad(filename);
				float score = fileLoad.Load;
				dsLoad.RemoveFileLoad(filename);
				dsLoad.Load = dsLoad.Load - score;
				_dataServersLoad[migration.Source] = dsLoad;
				newLoads[migration.Source] = dsLoad;
				dsLoad = _dataServersLoad[migration.Destination];
				destinationAddress = dsLoad.Info.Address;
				filename = migration.Filename;
				filename = filename.Substring(0, filename.LastIndexOf('_'));
				filename = filename + "_" + migration.Destination;
				fileLoad = new FileLoad(filename, 0f);
				dsLoad.SetFileLoad(filename, fileLoad);
				_dataServersLoad[migration.Destination] = dsLoad;
				newLoads[migration.Destination] = dsLoad;
			}
			/*_files update*/
			lock (_files)
			{
				filename = filename.Substring(0, filename.LastIndexOf('_'));
				PadiFile pFile = _files[filename];
				filename = filename + "_" + migration.Destination;
				for (int i = 0; i < pFile.GetDataServersCount(); ++i)
				{
					Pair pair = pFile.GetDataServer(i);
					string local_name = pair.LocalName;
					int id = Int32.Parse(local_name.Substring(filename.LastIndexOf('_') + 1, filename.Length - filename.LastIndexOf('_') - 1));
					if (id == migration.Source)
					{
						Pair newPair = new Pair(filename, destinationAddress);
						pFile.SetDataServer(i, newPair);
					}
				}
				newFiles[filename.Substring(0, filename.LastIndexOf('_'))] = pFile;
			}
			Console.WriteLine("Migration successful:");
			Console.WriteLine("DS" + migration.Source + " ==== " + migration.Filename + " ===> DS" + migration.Destination);
			lock (_toMigrate)
			{
				for (int i = 0; i < _toMigrate.Count; ++i)
				{
					if (_toMigrate[i].Equals(migration))
					{
						_toMigrate.RemoveAt(i);
					}
				}
			}
			foreach (MetadataSynchronize md in _metadataServers)
			{
				Console.WriteLine("Trying to replicate migration metadata to MDS");
				try
				{

					MigrateAsyncDelegate RemoteDel = new MigrateAsyncDelegate(md.MigrateSynch);
					AsyncCallback RemoteCallback = new AsyncCallback(MDServer.EmptyAsyncCallBack);
					IAsyncResult RemAr = RemoteDel.BeginInvoke(newLoads, newFiles, RemoteCallback, null);
				}
				catch (SocketException)
				{
					Console.WriteLine("Could not locate server");
				}
				catch (RemotingException) { }
			}
		}

		public static void DeleteCallBack(IAsyncResult ar)
		{
			try
			{
				// Alternative 2: Use the callback to get the return value
				DeleteDelegate del = (DeleteDelegate)((AsyncResult)ar).AsyncDelegate;
				/*Console.WriteLine("\r\n**SUCCESS**: Result of the remote AsyncCallBack: " + */

				FileId r = del.EndInvoke(ar);//);
				//Console.WriteLine("CR CB - Obtaining Lock: TOCREATE");
				lock (_toDeleteWaiting)
				{
					PadiFile p = _toDeleteWaiting[r.Filename.Substring(0, r.Filename.Length - 2)];
					p.DataServers.Remove(new Pair(r.Address, r.Filename));
					if (p.DataServers.Count == 0)
					{
						_toDeleteWaiting.Remove(r.Filename.Substring(0, r.Filename.Length - 2));
					}
				}
				foreach (MetadataSynchronize md in _metadataServers)
				{
					try
					{
						//md.OpenSynch(filename, id, address);
						// Alternative 2: asynchronous call with callback
						// Create delegate to remote method
						ToDeleteAsyncDelegate RemoteDel = new ToDeleteAsyncDelegate(md.ToDeleteSynch);
						// Create delegate to local callback
						AsyncCallback RemoteCallback = new AsyncCallback(MDServer.EmptyAsyncCallBack);
						// Call remote method
						IAsyncResult RemAr = RemoteDel.BeginInvoke(r, RemoteCallback, null);
					}
					catch (SocketException)
					{
						Console.WriteLine("Could not locate server");
					}
					catch (RemotingException) { }
				}
			}
			catch (RemotingException e)
			{
				FileId r = (FileId)e.Data["data"];
				PadiFile p;
				lock (_toDeleteWaiting)
				{
					p = _toDeleteWaiting[r.Filename.Substring(0, r.Filename.Length - 2)];
					p.DataServers.Remove(new Pair(r.Address, r.Filename));
					if (p.DataServers.Count == 0)
					{
						_toDeleteWaiting.Remove(r.Filename.Substring(0, r.Filename.Length - 2));
					}
				}
				lock (_toDelete)
				{
					if (!_toDelete.ContainsKey(p.Filename))
					{
						PadiFile file = new PadiFile();
						file.Filename = p.Filename;
						_toDelete[p.Filename] = file;
					}
					_toDelete[p.Filename].addDataServer(r.Address, r.Filename);
				}
				foreach (MetadataSynchronize md in _metadataServers)
				{
					try
					{
						//md.OpenSynch(filename, id, address);
						// Alternative 2: asynchronous call with callback
						// Create delegate to remote method
						ToDeleteWaitingAsyncDelegate RemoteDel = new ToDeleteWaitingAsyncDelegate(md.ToDeleteWaitingSynch);
						// Create delegate to local callback
						AsyncCallback RemoteCallback = new AsyncCallback(MDServer.EmptyAsyncCallBack);
						// Call remote method
						IAsyncResult RemAr = RemoteDel.BeginInvoke(r, RemoteCallback, null);
					}
					catch (SocketException)
					{
						Console.WriteLine("Could not locate server");
					}
					catch (RemotingException) { }
				}
			}
		}
		public static void CreateCallBack(IAsyncResult ar)
		{
			try
			{
				// Alternative 2: Use the callback to get the return value
				CreateDelegate del = (CreateDelegate)((AsyncResult)ar).AsyncDelegate;
				/*Console.WriteLine("\r\n**SUCCESS**: Result of the remote AsyncCallBack: " + */

				FileId r = del.EndInvoke(ar);//);
				//Console.WriteLine("CR CB - Obtaining Lock: TOCREATE");
				lock (_toCreateWaiting)
				{
					PadiFile p = _toCreateWaiting[r.Filename.Substring(0, r.Filename.Length - 2)];
					p.DataServers.Remove(new Pair(r.Address, r.Filename));
					if (p.DataServers.Count == 0)
					{
						_toCreateWaiting.Remove(r.Filename.Substring(0, r.Filename.Length - 2));
					}
				}
				foreach (MetadataSynchronize md in _metadataServers)
				{
					try
					{
						//md.OpenSynch(filename, id, address);
						// Alternative 2: asynchronous call with callback
						// Create delegate to remote method
						ToCreateAsyncDelegate RemoteDel = new ToCreateAsyncDelegate(md.ToCreateSynch);
						// Create delegate to local callback
						AsyncCallback RemoteCallback = new AsyncCallback(MDServer.EmptyAsyncCallBack);
						// Call remote method
						IAsyncResult RemAr = RemoteDel.BeginInvoke(r, RemoteCallback, null);
					}
					catch (SocketException)
					{
						Console.WriteLine("Could not locate server");
					}
					catch (RemotingException) { }
				}
			}
			catch (RemotingException e)
			{
				FileId r = (FileId)e.Data["data"];
				PadiFile p;
				lock (_toCreateWaiting)
				{
					p = _toCreateWaiting[r.Filename.Substring(0, r.Filename.Length - 2)];
					p.DataServers.Remove(new Pair(r.Address, r.Filename));
					if (p.DataServers.Count == 0)
					{
						_toCreateWaiting.Remove(r.Filename.Substring(0, r.Filename.Length - 2));
					}
				}
				lock (_toCreate)
				{
					if (!_toCreate.ContainsKey(p.Filename))
					{
						PadiFile file = new PadiFile();
						file.Filename = p.Filename;
						_toCreate[p.Filename] = file;
					}
					_toCreate[p.Filename].addDataServer(r.Address, r.Filename);
				}
				foreach (MetadataSynchronize md in _metadataServers)
				{
					try
					{
						//md.OpenSynch(filename, id, address);
						// Alternative 2: asynchronous call with callback
						// Create delegate to remote method
						ToCreateWaitingAsyncDelegate RemoteDel = new ToCreateWaitingAsyncDelegate(md.ToCreateWaitingSynch);
						// Create delegate to local callback
						AsyncCallback RemoteCallback = new AsyncCallback(MDServer.EmptyAsyncCallBack);
						// Call remote method
						IAsyncResult RemAr = RemoteDel.BeginInvoke(r, RemoteCallback, null);
					}
					catch (SocketException)
					{
						Console.WriteLine("Could not locate server");
					}
					catch (RemotingException) { }
				}
			}
		}
		public static void GetStatsCallBack(IAsyncResult ar)
		{
			// Alternative 2: Use the callback to get the return value
			GetStatsDelegate del = (GetStatsDelegate)((AsyncResult)ar).AsyncDelegate;
			/*Console.WriteLine("\r\n**SUCCESS**: Result of the remote AsyncCallBack: " + */
			DSLoad load = del.EndInvoke(ar);//);
			lock (_dataServersLoad)
			{
				_dataServersLoad[load.Info.Id] = load;
			}
			return;
		}
		public static void EmptyAsyncCallBack(IAsyncResult ar)
		{
			try
			{
				ToCreateAsyncDelegate del = (ToCreateAsyncDelegate)((AsyncResult)ar).AsyncDelegate;
				del.EndInvoke(ar);
			}
			catch (RemotingException e) { Console.WriteLine(e.StackTrace); }
			return;
		}

		public override object InitializeLifetimeService()
		{

			return null;

		}
	}
}