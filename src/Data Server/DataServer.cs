using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Timers;


namespace Data_Server
{
	[Serializable]
	public enum State { Online, Frozen, Offline };

	[Serializable]
	class DataServer : DataServerInterface
	{
		private const int READ_LOAD = 1;
		private const int WRITE_LOAD = 1;
		private const int SIZE_LOAD = 0;
		private State _state = State.Online;
		private static int _id = -1;
		private static int _port = -1;
		private static List<string> _metadataServersAddresses = new List<string>();
		private static List<MetadataServerInterface> _metadataServers = new List<MetadataServerInterface>();
		private Dictionary<string, FileDescriptor> _files = new Dictionary<string, FileDescriptor>();
		private ManualResetEvent _frozen = new ManualResetEvent(true);
		private static string _address = null;
		private static DSLoad _load = null;

		static void Main(string[] args)
		{
			_port = Convert.ToInt32(args[0]);
			_id = Convert.ToInt32(args[1]);
			_address = "tcp://localhost:" + _port + "/DataServer" + _id;
			_load = new DSLoad(_id, _address);
			BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
			provider.TypeFilterLevel = TypeFilterLevel.Full;
			IDictionary props = new Hashtable();
			props["port"] = _port;
			TcpChannel channel = new TcpChannel(props, null, provider);
			ChannelServices.RegisterChannel(channel, false);
			DataServer mo = new DataServer();
			RemotingServices.Marshal(mo, "DataServer" + _id, typeof(DataServer));
			for (int i = 2; i < args.Length; ++i)
			{
				_metadataServersAddresses.Add(args[i]);
			}
			foreach (string address in _metadataServersAddresses)
			{
				MetadataServerInterface metadataServer = (MetadataServerInterface)Activator.GetObject(typeof(MetadataServerInterface), address);
				if (metadataServer == null)
				{
					Console.WriteLine("Could not connect to Metadata Server in " + address);
				}
				else
				{
					_metadataServers.Add(metadataServer);
					Console.WriteLine("Successfully connected to Metadata Server in " + address);
					try
					{
						metadataServer.RegisterDataServer(_id, _address, _load);
					}
					catch (RemotingException) { }
				}
			}


			System.Console.WriteLine("DATA SERVER: " + _id);
			System.Console.WriteLine("Using Port: " + _port);
			System.Console.WriteLine("<enter> para sair...");
			System.Console.ReadLine();
		}

		public override FileId Create(string local_file_name)
		{
			Console.WriteLine("Create(" + local_file_name + ")");
			if (_state == State.Offline)
			{
				Console.WriteLine("Server down: Ignoring Create");
				RemotingException r = new RemotingException("Data Server is down");
				r.Data.Add("data", new FileId(_address, local_file_name));
				throw r;
			}
			if (_state == State.Frozen)
			{
				Console.WriteLine("Server frozen: Queueing Create");
				_frozen.WaitOne();
			}
			lock (_files)
			{
			Stream stream = File.Open(@local_file_name, FileMode.OpenOrCreate);
			BinaryFormatter bFormatter = new BinaryFormatter();
			FileDescriptor f = new FileDescriptor(new UniqueVersion(-1, DateTime.Now), lib.GetBytes(""));
			bFormatter.Serialize(stream, f);
			stream.Close();
			_files[local_file_name] = f;
			}
			lock (_load)
			{
				_load.IncLoad(local_file_name, 0);
			}
			return new FileId(_address, local_file_name);
		}

		public override FileId Delete(string local_file_name)
		{
			Console.WriteLine("Delete(" + local_file_name + ")");
			if (_state == State.Offline)
			{
				Console.WriteLine("Server down: Ignoring Delete");
				RemotingException r = new RemotingException("Data Server is down");
				r.Data.Add(_id, new FileId(_address, local_file_name));
				throw r;
			}
			if (_state == State.Frozen)
			{
				Console.WriteLine("Server frozen: Queueing Delete");
				_frozen.WaitOne();
			}
			lock (_files) {
				_files.Remove(local_file_name);
				File.Delete(local_file_name);
			}
			lock (_load) {
				_load.RemoveFileLoad(local_file_name);
			}
			return new FileId(_address, local_file_name);
		}

		public override FileDescriptor Read(String local_file_name)
		{
			Console.WriteLine("Read(" + local_file_name + ")");
			if (_state == State.Offline)
			{
				Console.WriteLine("Server down: Ignoring Read");
				throw new RemotingException("Data Server is down");
			}
			if (_state == State.Frozen)
			{
				Console.WriteLine("Server frozen: Queueing Read");
				_frozen.WaitOne();
			}
			FileDescriptor result;
			if (_files.ContainsKey(local_file_name))
			{
				lock (_files[local_file_name])
				{
					try
					{
						Stream stream = File.Open(@local_file_name, FileMode.Open);
						BinaryFormatter bFormatter = new BinaryFormatter();
						result = (FileDescriptor)bFormatter.Deserialize(stream);
						_load.IncLoad(local_file_name, READ_LOAD + result.Data.Length * SIZE_LOAD);
						stream.Close();
					}
					catch (System.IO.FileNotFoundException)
					{
						InternalWrite(local_file_name);
						result = InternalRead(local_file_name);
						_load.addFile(new FileLoad(local_file_name, READ_LOAD + result.Data.Length * SIZE_LOAD));
					}
					Console.WriteLine("File " + local_file_name + " " + result.Version + " read");
					Console.WriteLine("Content read: " + lib.GetString(result.Data));
				}
				return result;
			}
			else
			{
				Console.WriteLine("File " + local_file_name + " Doesn't Exist");
				throw new RemotingException("File " + local_file_name + " Doesn't Exist");
				//InternalWrite(local_file_name);
				//result = InternalRead(local_file_name);
				//_load.addFile(new FileLoad(local_file_name, READ_LOAD + result.Data.Length * SIZE_LOAD));
				//Console.WriteLine("File " + local_file_name + " version " + result.Version + " timestamp " + result.Version.Timestamp + " read");
				//Console.WriteLine("Content read: " + lib.GetString(result.Data));
				//return result;
			}
		}

		private FileDescriptor InternalRead(string local_file_name)
		{
			FileDescriptor result;
			Stream stream = File.Open(@local_file_name, FileMode.Open);
			BinaryFormatter bFormatter = new BinaryFormatter();
			result = (FileDescriptor)bFormatter.Deserialize(stream);
			stream.Close();
			return result;
		}

		private void InternalWrite(string local_file_name)
		{
			Console.WriteLine("File needs to be created - InternalWrite()");
			Stream stream = File.Open(@local_file_name, FileMode.OpenOrCreate);
			BinaryFormatter bFormatter = new BinaryFormatter();
			FileDescriptor f = new FileDescriptor(new UniqueVersion(0, DateTime.Now), lib.GetBytes(""));
			bFormatter.Serialize(stream, f);
			_files[local_file_name] = f;
			stream.Close();
		}

		public override void Write(String local_file_name, Byte[] byte_array, UniqueVersion version)
		{
			Console.WriteLine("Write(" + local_file_name + ", " + lib.GetString(byte_array) + ")");
			if (_state == State.Offline)
			{
				Console.WriteLine("Server down: Ignoring Write");
				throw new RemotingException("Data Server is down");
			}
			if (_state == State.Frozen)
			{
				Console.WriteLine("Server frozen: Queueing Write");
				_frozen.WaitOne();
			}
			if (_files.ContainsKey(local_file_name))
			{
				lock (_files[local_file_name])
				{
					Stream stream = File.Open(@local_file_name, FileMode.OpenOrCreate);
					BinaryFormatter bFormatter = new BinaryFormatter();
					FileDescriptor f = new FileDescriptor(version, byte_array);
					UniqueVersion currentVersion = _files[local_file_name].Version;
					if (currentVersion.CompareTo(version) < 0)
						_files[local_file_name] = f;
					else
					{
						stream.Close();
						Monitor.Exit(_files[local_file_name]);
						throw new RemotingException("Version is older than current");
					}
					bFormatter.Serialize(stream, f);
					_load.IncLoad(local_file_name, WRITE_LOAD + byte_array.Length * SIZE_LOAD);
					stream.Close();
				}
			}
			else
			{
				Console.WriteLine("File " + local_file_name + " Doesn't Exist");
				throw new RemotingException("File " + local_file_name + " Doesn't Exist");
			}
			Console.WriteLine("File " + local_file_name + " " + version + " written");
			Console.WriteLine("Content written: " + lib.GetString(byte_array));
			return;
		}

		public override void Freeze()
		{
			Console.WriteLine("Freeze()");
			_state = State.Frozen;
			_frozen.Reset();
		}

		public override void Unfreeze()
		{
			Console.WriteLine("Unfreeze()");
			_state = State.Online;
			_frozen.Set();
		}

		public override void Fail()
		{
			Console.WriteLine("Fail()");
			_state = State.Offline;
		}

		public override void Recover()
		{
			Console.WriteLine("Recover()");
			_state = State.Online;
		}

		public override void Dump()
		{
			Console.WriteLine("Dump()");
			lock (_files)
			{
				foreach (string file in _files.Keys)
				{
					Console.WriteLine(file + ": " + _files[file]);
				}
			}
			Console.WriteLine("-----------------------------------------------------");
		}

		public override void AddMetadataServer(string address)
		{
			_metadataServersAddresses.Add(address);
			MetadataServerInterface metadataServer = (MetadataServerInterface)Activator.GetObject(typeof(MetadataServerInterface), address);
			if (metadataServer != null)
			{
				_metadataServers.Add(metadataServer);
				string myAddress = "tcp://localhost:" + _port + "/DataServer" + _id;
				metadataServer.RegisterDataServer(_id, myAddress, _load);
			}
		}

		public override string ToString()
		{
			return "[Data Server " + _id + "]";
		}

		public override DSLoad GetStats()
		{
			if (_state == State.Offline)
			{
				throw new RemotingException("Data Server is down");
			}
			if (_state == State.Frozen)
			{
				_frozen.WaitOne();
			}
			return _load;
		}

		public override Migration MigrateFile(MigrationInfo migrationInfo, Migration migration)
		{
			if (_state == State.Offline)
			{
				Console.WriteLine("Server down: Ignoring Write");
				throw new RemotingException("Data Server is down");
			}
			if (_state == State.Frozen)
			{
				Console.WriteLine("Server frozen: Queueing Write");
				_frozen.WaitOne();
			}
			DataServerInterface ds = (DataServerInterface)Activator.GetObject(typeof(DataServerInterface), migrationInfo.Address);
			if (ds != null)
			{
				try
				{
					FileDescriptor file = Read(migrationInfo.FilenameOrigin);
					ds.MigrateWrite(migrationInfo.FilenameDestination, file.Data, file.Version);
					lock(_load){
						_load.DecLoad(migrationInfo.FilenameOrigin);
					}
					lock (_files)
					{
						_files.Remove(migrationInfo.FilenameOrigin);
					}
					File.Delete(migrationInfo.FilenameOrigin);				
				}
				catch (RemotingException e) { throw e; }
			}
			return migration;
		}

		public override void MigrateWrite(string local_name, byte[] byte_array, UniqueVersion version)
		{
			Console.WriteLine("MigrateWrite(" + local_name + ", " + lib.GetString(byte_array) + ")");
			if (_state == State.Offline)
			{
				Console.WriteLine("Server down: Ignoring Write");
				throw new RemotingException("Data Server is down");
			}
			if (_state == State.Frozen)
			{
				Console.WriteLine("Server frozen: Queueing Write");
				_frozen.WaitOne();
			}
			Stream stream = File.Open(@local_name, FileMode.OpenOrCreate);
			BinaryFormatter bFormatter = new BinaryFormatter();
			FileDescriptor f = new FileDescriptor(version, byte_array);
			_files[local_name] = f;
			bFormatter.Serialize(stream, f);
			_load.addFile(new FileLoad(local_name, WRITE_LOAD + byte_array.Length * SIZE_LOAD));
			stream.Close();
			Console.WriteLine("File " + local_name + " " + version + " migrated");
			Console.WriteLine("Content written: " + lib.GetString(byte_array));
		}

		public override object InitializeLifetimeService()
		{

			return null;

		}
	}
}