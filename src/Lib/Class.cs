using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
public abstract class DataServerInterface : MarshalByRefObject
{
	public abstract FileId Create(string local_file_name);
	public abstract FileId Delete(string local_file_name);
	public abstract void Write(string local_file_name, Byte[] byte_array, UniqueVersion version);
	public abstract FileDescriptor Read(string local_file_name);
	public abstract void Freeze();
	public abstract void Unfreeze();
	public abstract void Fail();
	public abstract void Recover();
	public abstract void Dump();
	public abstract void AddMetadataServer(string address);
	// just to MD
	// public abstract int GetID();
	// public abstract string GetAddress();
	public abstract DSLoad GetStats();
	public abstract Migration MigrateFile(MigrationInfo migrationInfo, Migration migration);
	public abstract void MigrateWrite(string local_name, Byte[] byte_array, UniqueVersion version);
}

public interface MetadataServerInterface
{
	PadiFile Open(string filename, int id, string address);
	void Close(string filename, int id);
	void Delete(string filename, int id);
	PadiFile Create(string filename, int numDS, int readQ, int writeQ, int id, string address);
	bool RegisterDataServer(int id, string address, DSLoad load);
	void Fail();
	void Recover();
	List<PadiFile> Dump();
	void Migrate();
}

public interface ClientInterface
{
	int Open(string filename);
	void Close(string filename);
	PadiFile Create(string filename, int numDS, int readQ, int writeQ);
	void Delete(string filename);
	string Write(int fileReg, string contents);
	string Write(int fileReg, int byteArrayReg);
	string Read(int fileReg, string semantics, int byteArrayReg);
	string Copy(int fileReg1, string semantics, int fileReg2, string salt);
	void Dump();
	void AddMetadataServer(string address);
	void ExeScript(List<string> commands);
	//To MD
	void AddServer(string file,string address, string localname);
}

public class lib
{
	public static byte[] GetBytes(string str)
	{
		byte[] bytes = new byte[str.Length * sizeof(char)];
		System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
		return bytes;
	}

	public static string GetString(byte[] bytes)
	{
		char[] chars = new char[bytes.Length / sizeof(char)];
		System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
		return new string(chars);
	}
}

[Serializable]
public class PadiFile
{
	private string _filename;
	private int _numDS;
	private int _readQ;
	private int _writeQ;
	private List<Pair> _dataServers;

	public PadiFile(string filename, int numDS, int readQ, int writeQ)
	{
		_filename = filename;
		_numDS = numDS;
		_readQ = readQ;
		_writeQ = writeQ;
		_dataServers = new List<Pair>();
	}

	public PadiFile() { _dataServers = new List<Pair>(); }

	public Pair GetDataServer(int i)
	{
		return _dataServers[i];
	}

	public void SetDataServer(int i, Pair pair)
	{
		_dataServers[i] = pair;
	}

	public int GetDataServersCount()
	{
		return _dataServers.Count;
	}

	public string Filename
	{
		get { return _filename; }
		set { _filename = value; }
	}

	public int ReadQuorum
	{
		get { return _readQ; }
	}

	public int WriteQuorum
	{
		get { return _writeQ; }
	}

	public List<Pair> DataServers
	{
		get { return _dataServers; }
	}

	public bool addDataServer(string ds, string local_file_name)
	{
		Pair p = new Pair(ds, local_file_name);
		if (!_dataServers.Contains(p))
		{
			_dataServers.Add(new Pair(ds, local_file_name));
			return true;
		}
		return false;
	}

	public void addDataServer(List<Pair> list)
	{
		_dataServers = list;
	}

	public override string ToString()
	{
		string returnvalue = "filename: " + _filename + "; numDS: " + _numDS + "; readQ: " + _readQ + "; writeQ: " + _writeQ + Environment.NewLine; ;
		foreach (Pair p in _dataServers)
			returnvalue += p + Environment.NewLine; 
		return returnvalue;
	}

}

	public abstract class ServerInfo
	{
		protected int _id;
		public int Id
		{
			get { return _id; }
		}
		protected string _address;
		public string Address
		{
			get { return _address; }
		}
		public ServerInfo(int id, string address) { _id = id; _address = address; }
	}

	public class MDSInfo : ServerInfo
	{
		private MetadataServerInterface _remoteReference;
		public MetadataServerInterface RemoteReference
		{
			get { return _remoteReference; }
			set { _remoteReference = value; }
		}
		public MDSInfo(int id, string address) : base(id, address) { connect(); }
		public void connect()
		{
			_remoteReference = (MetadataServerInterface)Activator.GetObject(typeof(MetadataServerInterface), _address);
			if (_remoteReference == null)
				Console.WriteLine("Could not connect to MDS at " + _address);
		}
	}

	public class DSInfo : ServerInfo
	{
		private DataServerInterface _remoteReference;

		public DataServerInterface RemoteReference
		{
			get { return _remoteReference; }
			set { _remoteReference = value; }
		}
		public DSInfo(int id, string address)
			: base(id, address)
		{
			connect();
		}

		public void connect()
		{
			_remoteReference = (DataServerInterface)Activator.GetObject(typeof(DataServerInterface), _address);
			if (_remoteReference == null)
				Console.WriteLine("Could not connect to DS at " + _address);
		}
	}

	public class ClientInfo : ServerInfo
	{
		private ClientInterface _remoteReference;
		public ClientInterface RemoteReference
		{
			get { return _remoteReference; }
			set { _remoteReference = value; }
		}
		public ClientInfo(int id, string address) : base(id, address) { connect(); }
		public void connect()
		{
			_remoteReference = (ClientInterface)Activator.GetObject(typeof(ClientInterface), _address);
			if (_remoteReference == null)
				Console.WriteLine("Could not connect to Client at " + _address);
		}
	}

	[Serializable]
	public class UniqueVersion
	{
		private int _version;
		private DateTime _timestamp;

		public UniqueVersion(int version, DateTime timestamp)
		{
			_version = version;
			_timestamp = timestamp;
		}

		public int CompareTo(UniqueVersion version)
		{
			if (_version < version.Version)
				return -1;
			if (_version > version.Version)
				return 1;
			if (_timestamp.CompareTo(version.Timestamp) < 0)
				return -1;
			if (_timestamp.CompareTo(version.Timestamp) > 0)
				return 1;
			return 0;
		}

		public int Version
		{
			get { return _version; }
		}

		public DateTime Timestamp
		{
			get { return _timestamp; }
		}

		public override string ToString()
		{
			return "version: " + _version + " timestamp: " + _timestamp;
		}
	}

	[Serializable]
	public class FileDescriptor
	{

		private UniqueVersion _version;
		private byte[] _data;

		public FileDescriptor() { }

		public FileDescriptor(UniqueVersion v, byte[] d)
		{
			_version = v;
			_data = d;
		}

		public UniqueVersion Version
		{
			get { return _version; }
			set { _version = value; }
		}

		public byte[] Data
		{
			get { return _data; }
			set { _data = value; }
		}

		public override string ToString()
		{
			 return _version + " data:" + lib.GetString(_data);
		}

		public override bool Equals(System.Object obj)
		{
			if (obj == null)
				return false;
			FileDescriptor file = obj as FileDescriptor;
			if ((System.Object)file == null)
				return false;
			return (_version.Version == file.Version.Version) && (_data.Equals(file.Data));
		}

		public bool Equals(FileDescriptor file)
		{
			if ((object)file == null)
				return false;
			return (_version.Version == file.Version.Version) && (_data.Equals(file.Data));
		}
	}

	[Serializable]
	public class Pair
	{
		private string _dataserverAddress;
		private string _localName;

		public Pair() { }
		public Pair(string address, string localname)
		{
			_dataserverAddress = address;
			_localName = localname;
		}

		public string DataServerAddress
		{
			get { return _dataserverAddress; }
		}
		public string LocalName
		{
			get { return _localName; }
		}

		public override string ToString()
		{
			return _dataserverAddress + " localname = " + _localName;
		}
		public override bool Equals(object pair)
		{
			if ((object)pair == null)
				return false;
			Pair p = (Pair)pair;
			return (_dataserverAddress == p._dataserverAddress) && ( _localName.Equals(p._localName));
		}
	}

[Serializable]
public class DSLoad
{
	private RemoteInfo _info;
	private Dictionary<string, FileLoad> _files;
	private float _totalLoad;

	public DSLoad(int id, string address) {
		_info = new RemoteInfo(address, id);
		_files = new Dictionary<string,FileLoad>();
		_totalLoad = 0;
	}

	public void addFile(FileLoad fl){
		_files[fl.FileName]= fl;
		_totalLoad += fl.Load;
	}

	public void IncLoad(string filename, float val) {
		if (!_files.ContainsKey(filename))
		{
			_files[filename] = new FileLoad(filename, 0);
		}
		_files[filename].incLoad(val);
		_totalLoad += val;
	}

	public void DecLoad(string filename) { 
		_totalLoad -= _files[filename].Load;
		_files.Remove(filename);
	}

	public FileLoad GetFileLoad(string filename)
	{
		return _files[filename];
	}

	public void RemoveFileLoad(string filename)
	{
		_files.Remove(filename);
	}

	public void SetFileLoad(string filename, FileLoad fileLoad)
	{
		_files[filename] = fileLoad;
	}

	public float Load
	{
		get { return _totalLoad; }
		set { _totalLoad = value; }
	}
	public RemoteInfo Info {
		get { return _info; }
	}

	public Dictionary<string, FileLoad> Loads
	{
		get { return _files; }
	}

	public int CompareTo(DSLoad dSLoad)
	{
		if (_totalLoad < dSLoad.Load)
		{
			return -1;
		}
		else {
			return 1;
		}
	}
}

[Serializable]
public class FileLoad
{
	private string _filename;
	private float _load;

	public FileLoad(string name, float load) {
		_filename = name;
		_load = load;
	}

	public string FileName{
		get { return _filename; }
	}

	public float Load {
		get { return _load; }
		set { _load = value;  }
	}

	internal void incLoad(float val)
	{
		_load += val;
	}

	public int CompareTo(FileLoad dSLoad)
	{
		if (_load < dSLoad.Load)
		{
			return -1;
		}
		else
		{
			return 1;
		}
	}
}

[Serializable]
public class MigrationInfo
{
	private string _address;
	private string _filenameOrigin;
	private string _filenameDestination;

	public MigrationInfo(string address, string origin, string destination)
	{
		_address = address;
		_filenameOrigin = origin;
		_filenameDestination = destination;
	}

	public string Address
	{
		get { return _address; }
		set { _address = value; }
	}
	public string FilenameOrigin
	{
		get { return _filenameOrigin; }
		set { _filenameOrigin = value; }
	}
	public string FilenameDestination
	{
		get { return _filenameDestination; }
		set { _filenameDestination = value; }
	}
}

[Serializable]
public class Migration
{
	private int _sourceID;
	private int _destinationID;
	private string _filename;

	public Migration(int sourceID, int destionationID, string filename)
	{
		_sourceID = sourceID;
		_destinationID = destionationID;
		_filename = filename;
	}

	public bool Equals(Migration migration)
	{
		return _sourceID == migration.Source && _destinationID == migration.Destination && _filename.Equals(migration.Filename);
	}

	public int Source
	{
		get { return _sourceID; }
	}

	public int Destination
	{
		get { return _destinationID; }
	}

	public string Filename
	{
		get { return _filename; }
	}
}

[Serializable]
public class RemoteInfo
{
	private string _address;
	private int _id;

	public RemoteInfo(string address, int id)
	{
		_address = address;
		_id = id;
	}

	public String Address
	{
		get { return _address; }
	}

	public int Id
	{
		get { return _id; }
	}
}

[Serializable]
public class FileId { 
	private string _address;
	private string _filename;

	public FileId(string address, string filename)
	{
		_address = address;
		_filename = filename;
	}

	public String Address
	{
		get { return _address; }
	}

	public string Filename
	{
		get { return _filename; }
	}
}