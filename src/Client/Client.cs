using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters;
using System.IO;

namespace Client
{
	class Client : MarshalByRefObject, ClientInterface
	{
		private static List<MetadataServerInterface> _metadataServers = new List<MetadataServerInterface>();
		private static List<string> _metadataServersAddresses = new List<string>();
		private static int _id;
		private static int _port;
		private static string _address;
		private static List<PadiFile> _fileRegisters = new List<PadiFile>();
		private static List<byte[]> _byteArrayRegisters = new List<byte[]>();
		private Dictionary<string, UniqueVersion> _latestVersions = new Dictionary<string, UniqueVersion>();
		private ScriptInterpreterClient SI;


		static void Main(string[] args)
		{
			_port = Convert.ToInt32(args[0]);
			_id = Convert.ToInt32(args[1]);
			BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
			provider.TypeFilterLevel = TypeFilterLevel.Full;
			IDictionary props = new Hashtable();
			props["port"] = _port;
			TcpChannel channel = new TcpChannel(props, null, provider);
			ChannelServices.RegisterChannel(channel, false);
			Client mo = new Client();
			RemotingServices.Marshal(mo, "Client" + _id, typeof(Client));
			_address = "tcp://localhost:" + _port + "/Client" + _id;
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
				}
			}
			for (int i = 0; i < 10; ++i)
			{
				_fileRegisters.Add(null);
				_byteArrayRegisters.Add(null);
			}
			System.Console.WriteLine("CLIENT SERVER: " + _id);
			System.Console.WriteLine("Using Port: " + _port);
			Console.WriteLine("<enter> para sair");
			Console.ReadLine();
		}

		// METADATA SERVER METHODS

		public int Open(string filename)
		{
			Console.WriteLine("Open(" + filename + ")");
			foreach (MetadataServerInterface mds in _metadataServers)
			{
				try
				{
					int fileReg = _fileRegisters.FindIndex(file => file == null);
					if (fileReg == -1)
					{
						Console.WriteLine("NULL");
						fileReg = 0;
					}
					_fileRegisters[fileReg] = mds.Open(filename, _id, _address);
					return fileReg;
				}
				catch (RemotingException) { }
			}
			throw new RemotingException("All Metadatas are Down");
		}
		public void Close(string filename)
		{
			Console.WriteLine("Close(" + filename + ")");
			bool _executed = false;
			foreach (MetadataServerInterface mds in _metadataServers)
			{
				try
				{
					mds.Close(filename, _id);
					_executed = true;
					break;
				}
				catch (RemotingException) { }
			}
			if (!_executed)
			{
				throw new RemotingException("All Metadatas are Down");
			}
		}
		public void Delete(string filename)
		{
			Console.WriteLine("Delete(" + filename + ")");
			bool _executed = false;
			foreach (MetadataServerInterface mds in _metadataServers)
			{
				try
				{
					mds.Delete(filename, _id);
					_executed = true;
					break;
				}
				catch (RemotingException) { }
			}
			if (!_executed)
			{
				throw new RemotingException("All Metadatas are Down");
			}
		}
		public PadiFile Create(string filename, int numDS, int readQ, int writeQ)
		{
			Console.WriteLine("Create(" + filename + ", " + numDS + ", " + readQ + ", " + writeQ + ")");
			foreach (MetadataServerInterface mds in _metadataServers)
			{
				try
				{
					return mds.Create(filename, numDS, readQ, writeQ, _id, _address);
				}
				catch (RemotingException) { }
			}
			throw new RemotingException("All Metadatas are Down");
		}

		//  DATASERVER METHODS

		public delegate FileDescriptor RemoteAsyncDelegate(string filename);
		public delegate void RemoteAsyncDelegateW(string filename, byte[] byte_array, UniqueVersion version);

		public string Write(int fileReg, byte[] bytes)
		{
			/*READING THE LATEST VERSION*/
			UniqueVersion currentVersion;
			try
			{
				FileDescriptor file = ReadAux(fileReg, "monotonic", -1);
				currentVersion = file.Version;
			}
			catch (RemotingException e)
			{
				Console.WriteLine(e.Message);
				throw new RemotingException(e.Message);
			}
			/*WRITING*/
			List<RemoteAsyncDelegateW> delegates = new List<RemoteAsyncDelegateW>();
			List<IAsyncResult> results = new List<IAsyncResult>();
			UniqueVersion newVersion = new UniqueVersion(currentVersion.Version + 1, DateTime.Now);
			List<int> failed = new List<int>();
			if (_fileRegisters[fileReg].WriteQuorum > _fileRegisters[fileReg].DataServers.Count - failed.Count)
			{
				Console.WriteLine("There are not enough Servers to reply the Quorum");
				throw new RemotingException("There are not enough Servers to reply the Quorum");
			}
			foreach (Pair ds in _fileRegisters[fileReg].DataServers)
			{
				DataServerInterface dataServer = (DataServerInterface)Activator.GetObject(typeof(DataServerInterface), ds.DataServerAddress);
				RemoteAsyncDelegateW remoteDel = new RemoteAsyncDelegateW(dataServer.Write);
				IAsyncResult remoteAsyncR = remoteDel.BeginInvoke(ds.LocalName, bytes, newVersion, null, null);
				delegates.Add(remoteDel);
				results.Add(remoteAsyncR);
			}

			int writtenDS = 0;
			while (writtenDS < _fileRegisters[fileReg].WriteQuorum)
			{
				if (_fileRegisters[fileReg].WriteQuorum > _fileRegisters[fileReg].DataServers.Count - failed.Count)
				{
					Console.WriteLine("There are not enough 'Alive' Servers to reply the Quorum");
					throw new RemotingException("There are not enough 'Alive' Servers to reply the Quorum");
				}
				for (int i = 0; i < results.Count; ++i)
				{
					try
					{
						IAsyncResult remoteAsyncR = results[i];
						if (remoteAsyncR.IsCompleted)
						{
							delegates[i].EndInvoke(remoteAsyncR);
							++writtenDS;
							delegates.Remove(delegates[i]);
							results.Remove(remoteAsyncR);
							break;
						}
					}
					catch (RemotingException e)
					{
						Console.WriteLine(e.Message);
						failed.Add(i);
						throw new RemotingException(e.Message);
					}
				}
				System.Threading.Thread.Sleep(100);
			}
			_latestVersions[_fileRegisters[fileReg].Filename] = newVersion;
			return lib.GetString(bytes);
		}

		public string Write(int fileReg, int byteArrayReg)
		{
			Console.WriteLine("Write(" + fileReg + ", " + _byteArrayRegisters[byteArrayReg] + ")");
			return Write(fileReg, _byteArrayRegisters[byteArrayReg]);
		}

		public string Write(int fileReg, string contents)
		{
			byte[] byte_array = lib.GetBytes(contents);
			Console.WriteLine("Write(" + fileReg + ", " + byte_array + ")");
			return Write(fileReg, byte_array);
		}

		public string Read(int fileReg, string semantics, int byteArrayReg)
		{
			Console.WriteLine("Read(" + fileReg + ", " + semantics + ", " + byteArrayReg + ")");
			try
			{
				FileDescriptor file = ReadAux(fileReg, semantics, byteArrayReg);
				return lib.GetString(file.Data);
			}
			catch (RemotingException e)
			{
				Console.WriteLine(e.Message);
				throw new RemotingException(e.Message);
			}
		}

		private FileDescriptor ReadAux(int fileReg, string semantics, int byteArrayReg)
		{
			List<FileDescriptor> replies = new List<FileDescriptor>();
			List<RemoteAsyncDelegate> delegates = new List<RemoteAsyncDelegate>();
			List<IAsyncResult> results = new List<IAsyncResult>();
			foreach (Pair ds in _fileRegisters[fileReg].DataServers)
			{
				DataServerInterface dataServer = (DataServerInterface)Activator.GetObject(typeof(DataServerInterface), ds.DataServerAddress);
				RemoteAsyncDelegate remoteDel = new RemoteAsyncDelegate(dataServer.Read);
				IAsyncResult remoteAsyncR = remoteDel.BeginInvoke(ds.LocalName, null, null);
				delegates.Add(remoteDel);
				results.Add(remoteAsyncR);
			}
			UniqueVersion latestV = new UniqueVersion(-1, DateTime.Now);
			/*when "default" semantics is used there is no need to check against the latest version*/
			if (semantics.Equals("default"))
				latestV = new UniqueVersion(int.MaxValue, DateTime.Now);
			UniqueVersion latestVersion;
			if (_latestVersions.ContainsKey(_fileRegisters[fileReg].Filename))
				latestVersion = _latestVersions[_fileRegisters[fileReg].Filename];
			/*when there is no entry (first time the file is read) there is also no need to check against latest version*/
			else
				latestVersion = new UniqueVersion(-2, DateTime.Now);
			List<int> executed = new List<int>();
			List<int> failed = new List<int>();
			while (replies.Count < _fileRegisters[fileReg].ReadQuorum || latestV.CompareTo(latestVersion) < 0)
			{
				if (_fileRegisters[fileReg].ReadQuorum > _fileRegisters[fileReg].DataServers.Count - failed.Count)
				{
					throw new RemotingException("There are not enough 'Alive' Servers to reply the Quorum");
				}
				for (int i = 0; i < results.Count; ++i)
				{
					if (!executed.Contains(i))
					{
						IAsyncResult remoteAsyncR = results[i];
						if (remoteAsyncR.IsCompleted)
						{
							try
							{
								FileDescriptor file = delegates[i].EndInvoke(remoteAsyncR);
								if (file.Version.CompareTo(latestV) > 0)
								{
									latestV = file.Version;
								}
								replies.Add(file);
								executed.Add(i);
							}
							catch (RemotingException)
							{
								executed.Add(i);
								failed.Add(i);
							}
						}
					}
				}
				System.Threading.Thread.Sleep(100);
			}
			FileDescriptor result = replies[0];
			UniqueVersion highestV = replies[0].Version;
			/*escolhe a versao mais alta*/
			foreach (FileDescriptor reply in replies)
			{
				if (reply.Version.CompareTo(highestV) > 0)
				{
					highestV = reply.Version;
					result = reply;
				}
			}
			if (byteArrayReg != -1)
			{
				_byteArrayRegisters[byteArrayReg] = result.Data;
				_latestVersions[_fileRegisters[fileReg].Filename] = result.Version;
			}
			return result;
		}

		public string Copy(int fileReg1, string semantics, int fileReg2, string salt)
		{
			Console.WriteLine("Copy(" + fileReg1 + ", " + semantics + ", " + fileReg2 + ", " + salt + ")");
			try
			{
				string contents = CopyRead(fileReg1, semantics, -1);
				contents = String.Concat(contents, salt);
				return CopyWrite(fileReg2, contents);
			}
			catch (RemotingException e)
			{
				//Console.WriteLine(e.Message);
				throw new RemotingException(e.Message);
			}
		}

		private string CopyRead(int fileReg, string semantics, int byteArrayReg)
		{
			Console.WriteLine("Copy -> Read(" + fileReg + ", " + semantics + ", " + byteArrayReg + ")");
			try
			{
				FileDescriptor file = ReadAux(fileReg, semantics, byteArrayReg);
				return lib.GetString(file.Data);
			}
			catch (RemotingException e)
			{
				Console.WriteLine(e.Message);
				throw new RemotingException(e.Message);
			}
		}

		private string CopyWrite(int fileReg, string contents)
		{
			byte[] byte_array = lib.GetBytes(contents);
			Console.WriteLine("Copy -> Write(" + fileReg + ", " + byte_array + ")");
			return Write(fileReg, byte_array);
		}

		public void Dump()
		{
			Console.WriteLine("Dump()");
			int i = 0;
			Console.WriteLine(">>>>>>>>>>>>>>>> File Registry <<<<<<<<<<<<<<<");
			foreach (PadiFile p in _fileRegisters)
			{
				Console.Write("Registry " + i++ + ": ");
				if (p != null)
				{
					Console.Write(p);
				}
				else
				{
					Console.WriteLine("Empty");
				}
			}
			i = 0;
			Console.WriteLine(">>>>>>>>>>>>>>>> Byte[] Registry <<<<<<<<<<<<<<<");
			foreach (byte[] b in _byteArrayRegisters)
			{
				Console.Write("Registry " + i++ + ":");
				if (b != null)
				{
					Console.Write(lib.GetString(b));
				}
				else
				{
					Console.WriteLine("Empty");
				}
			}
			Console.WriteLine("-------------------------------------------------");
		}

		public void ExeScript(List<string> commands)
		{
			SI = new ScriptInterpreterClient();
			Console.WriteLine("Script Started");
			foreach (string command in commands)
			{
				SI.execute(command, this);
			}
			Console.WriteLine("Script Ended!");
		}

		public void AddMetadataServer(string address)
		{
			_metadataServersAddresses.Add(address);
			MetadataServerInterface metadataServer = (MetadataServerInterface)Activator.GetObject(typeof(MetadataServerInterface), address);
			if (metadataServer != null)
				_metadataServers.Add(metadataServer);
		}

		public void AddServer(string file, string address, string localname)
		{
			foreach (PadiFile p in _fileRegisters)
			{
				if (p != null && p.Filename.Equals(file))
				{
					if (p.addDataServer(address, localname))
					{
						Console.WriteLine("Added DataServer to File: " + file);
					}
					break;
				}
			}
		}

		public override object InitializeLifetimeService()
		{

			return null;

		}
	}
}