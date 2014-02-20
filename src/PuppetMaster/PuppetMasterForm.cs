using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Net.Sockets;
using System.Runtime.Remoting;

namespace PuppetMaster
{
	public partial class PuppetMasterForm : Form
	{
		private TextReader tr;
		private ScriptInterpreter SI;
		private TcpChannel _channel;
		private int _portCounter = 8086;
		private int _clientIDCounter = 1;
		private int _dataServerIDCounter = 1;
		private int _metadataServerIDCounter = 0;
		private static Dictionary<string, MDSInfo> _metadataServers = new Dictionary<string, MDSInfo>();
		private static Dictionary<string, ClientInfo> _clients = new Dictionary<string, ClientInfo>();
		private static Dictionary<string, DSInfo> _dataServers = new Dictionary<string, DSInfo>();
		private List<string> _stringRegisters = new List<string>(10);
		List<PadiFile> _fileRegisters = new List<PadiFile>(10);

		public PuppetMasterForm()
		{
			InitializeComponent();
			_channel = new TcpChannel();
			ChannelServices.RegisterChannel(_channel, false);
		}

		/*GUI HANDLING FUNCTIONS*/

		private void LoadScript_Click(object sender, EventArgs e)
		{
			SI = new ScriptInterpreter();
			String location = file_location.Text;
			tr = new StreamReader(@location);
			//Display.Text = "Script Loaded!";
			DisplayTB.AppendText("Script Loaded!" + Environment.NewLine);
		}

		private void Run_Click(object sender, EventArgs e)
		{
			while (tr.Peek() != -1)
			{
				String command = tr.ReadLine();
				SI.execute(command, this, -1);
			}
			//Display.Text = "Script Ended!";
			DisplayTB.AppendText("Script Ended!" + Environment.NewLine);
		}

		private void Next_Click(object sender, EventArgs e)
		{
			if (tr.Peek() == -1)
			{
				//Display.Text = "There are no more steps in the script!";
				DisplayTB.AppendText("There are no more steps in the script!" + Environment.NewLine);
				return;
			}
			String command = tr.ReadLine();
			//Display.Text = command;
			DisplayTB.AppendText(command + Environment.NewLine);
			SI.execute(command, this, -1);
		}

		private void LaunchClient_Click(object sender, EventArgs e)
		{
			CreateClient();
		}

		private void LaunchMD_Click(object sender, EventArgs e)
		{
			if (_metadataServerIDCounter > 2)
				return;
			CreateMDS();
			string serverId = "m-" + (_metadataServerIDCounter - 1);
			RecoverCommand(serverId);
		}

		private void LaunchDS_Click(object sender, EventArgs e)
		{
			CreateDS();
		}

		private void Write_Click(object sender, EventArgs e)
		{
			string text = textToWrite.Text;
			WriteCommand(ClientsComboBox.SelectedItem.ToString(), Int32.Parse(FileRegistersComboBox.SelectedItem.ToString()), text);
		}

		private void WriteReg_Click(object sender, EventArgs e)
		{
			WriteCommand(ClientsComboBox.SelectedItem.ToString(), Int32.Parse(FileRegistersComboBox.SelectedItem.ToString()), Int32.Parse(StringRegistersComboBox.SelectedItem.ToString()));
		}

		private void Read_Click(object sender, EventArgs e)
		{
			ReadCommand(ClientsComboBox.SelectedItem.ToString(), Int32.Parse(FileRegistersComboBox.SelectedItem.ToString()), "", Int32.Parse(StringRegistersComboBox.SelectedItem.ToString()));
		}

		private void DSFreeze_Click(object sender, EventArgs e)
		{
			if (ServerComboBox.SelectedItem.ToString()[0] == 'm')
			{
				//Display.Text = "Select a Data Server";
				DisplayTB.AppendText("Select a Data Server" + Environment.NewLine);
			}
			else
				FreezeCommand(ServerComboBox.SelectedItem.ToString());
		}

		private void DSUnfreeze_Click(object sender, EventArgs e)
		{
			if (ServerComboBox.SelectedItem.ToString()[0] == 'm')
			{
				//Display.Text = "Select a Data Server";
				DisplayTB.AppendText("Select a Data Server" + Environment.NewLine);
			}
			else
				UnfreezeCommand(ServerComboBox.SelectedItem.ToString());
		}

		private void FailButton_Click(object sender, EventArgs e)
		{
			FailCommand(ServerComboBox.SelectedItem.ToString());
		}

		private void RecoverButton_Click(object sender, EventArgs e)
		{
			RecoverCommand(ServerComboBox.SelectedItem.ToString());
		}

		private void migration_button_Click(object sender, EventArgs e)
		{
			MigrationCommand(ServerComboBox.SelectedItem.ToString());
		}

		private void dump_button_Click(object sender, EventArgs e)
		{
			DumpCommand(ServerComboBox.SelectedItem.ToString());
		}

		private void DumpClient_Click(object sender, EventArgs e)
		{
			DumpCommand(ClientsComboBox.SelectedItem.ToString());
		}

		private void Create_Click(object sender, EventArgs e)
		{
			int numds = Convert.ToInt32(NumDSTextBox.Text);
			int rq = Convert.ToInt32(ReadQTextBox.Text);
			int wq = Convert.ToInt32(WriteQTextBox.Text);
			CreateCommand(ClientsComboBox.SelectedItem.ToString(), FileToWRTextBox.Text, numds, rq, wq);
		}

		private void CloseButton_Click(object sender, EventArgs e)
		{
			CloseCommand(ClientsComboBox.SelectedItem.ToString(), FileToWRTextBox.Text);
		}

		private void Delete_Click(object sender, EventArgs e)
		{
			DeleteCommand(ClientsComboBox.SelectedItem.ToString(), FileToWRTextBox.Text);
		}

		private void Open_Click(object sender, EventArgs e)
		{
			OpenCommand(ClientsComboBox.SelectedItem.ToString(), FileToWRTextBox.Text);
		}

		/*SERVER LAUNCHING FUNCTIONS*/

		private void CreateDS()
		{
			string args = _portCounter + " " + _dataServerIDCounter;
			foreach (MDSInfo temp in _metadataServers.Values)
			{
				args += " " + temp.Address;
			}
			Process.Start("Data_Server.exe", args);
			DSInfo info = new DSInfo(_dataServerIDCounter, "tcp://localhost:" + _portCounter + "/DataServer" + _dataServerIDCounter);
			_dataServers.Add("d-" + _dataServerIDCounter, info);
			ServerComboBox.Items.Add("d-" + info.Id);
			//Display.Text = "d-" + _dataServerIDCounter + " launched";
			DisplayTB.AppendText("d-" + _dataServerIDCounter + " launched" + Environment.NewLine);
			_portCounter++;
			_dataServerIDCounter++;
		}

		private void CreateMDS()
		{
			string args = _portCounter + " " + _metadataServerIDCounter;
			foreach (MDSInfo temp in _metadataServers.Values)
			{
				args += " " + temp.Address;
			}
			Process.Start("Metadata_server.exe", args);
			MDSInfo info = new MDSInfo(_metadataServerIDCounter, "tcp://localhost:" + _portCounter + "/MetadataServer" + _metadataServerIDCounter);
			_metadataServers.Add("m-" + _metadataServerIDCounter, info);
			ServerComboBox.Items.Add("m-" + info.Id);
			foreach (ClientInfo clientInfo in _clients.Values)
			{
				clientInfo.RemoteReference.AddMetadataServer("tcp://localhost:" + _portCounter + "/MetadataServer" + _metadataServerIDCounter);
			}
			//Display.Text = "m-" + _metadataServerIDCounter + " launched";
			DisplayTB.AppendText("m-" + _metadataServerIDCounter + " launched" + Environment.NewLine);
			_portCounter++;
			_metadataServerIDCounter++;
		}

		private void CreateClient()
		{
			string args = _portCounter + " " + _clientIDCounter;
			foreach (MDSInfo temp in _metadataServers.Values)
			{
				args += " " + temp.Address;
			}
			Process.Start("Client.exe", args);
			ClientInfo info = new ClientInfo(_clientIDCounter, "tcp://localhost:" + _portCounter + "/Client" + _clientIDCounter);
			_clients.Add("c-" + _clientIDCounter, info);
			ClientsComboBox.Items.Add("c-" + info.Id);
			//Display.Text = "c-" + _clientIDCounter + " launched";
			DisplayTB.AppendText("c-" + _clientIDCounter + " launched" + Environment.NewLine);
			_portCounter++;
			_clientIDCounter++;
		}

		/*COMMAND FUNCTIONS*/

		public void FailCommand(string serverID)
		{
			switch (serverID[0])
			{
				case 'm':
					if (!_metadataServers.ContainsKey(serverID))
						CreateMDS();
					_metadataServers[serverID].RemoteReference.Fail();
					break;
				case 'd':
					if (!_dataServers.ContainsKey(serverID))
						CreateDS();
					_dataServers[serverID].RemoteReference.Fail();
					break;
			}
			//Display.Text = "Server " + serverID + " failed";
			DisplayTB.AppendText("Server " + serverID + " failed" + Environment.NewLine);
		}

		public void RecoverCommand(string serverID)
		{
			switch (serverID[0])
			{
				case 'm':
					if (!_metadataServers.ContainsKey(serverID))
						CreateMDS();
					_metadataServers[serverID].RemoteReference.Recover();
					break;
				case 'd':
					if (!_dataServers.ContainsKey(serverID))
						CreateDS();
					_dataServers[serverID].RemoteReference.Recover();
					break;
			}
			//Display.Text = "Server " + serverID + " recovered";
			DisplayTB.AppendText("Server " + serverID + " recovered" + Environment.NewLine);
		}

		public void FreezeCommand(string dataServerID)
		{
			if (!_dataServers.ContainsKey(dataServerID))
				CreateDS();
			_dataServers[dataServerID].RemoteReference.Freeze();
			//Display.Text = "Data Server " + dataServerID + " frozen";
			DisplayTB.AppendText("Data Server " + dataServerID + " frozen" + Environment.NewLine);
		}

		public void UnfreezeCommand(string dataServerID)
		{
			if (!_dataServers.ContainsKey(dataServerID))
				CreateDS();
			_dataServers[dataServerID].RemoteReference.Unfreeze();
			//Display.Text = "Data Server " + dataServerID + " unfrozen";
			DisplayTB.AppendText("Data Server " + dataServerID + " unfrozen" + Environment.NewLine);
		}

		public void MigrationCommand(string metadataServerID)
		{
			_metadataServers[metadataServerID].RemoteReference.Migrate();
			//Display.Text = "Metadata Server " + metadataServerID + " executed migration";
			DisplayTB.AppendText("Metadata Server " + metadataServerID + " executed migration" + Environment.NewLine);
		}

		public void DumpCommand(string serverID)
		{
			switch (serverID[0])
			{
				case 'm':
					if (!_metadataServers.ContainsKey(serverID))
					{
						CreateMDS();
					}
					List<PadiFile> toDump = _metadataServers[serverID].RemoteReference.Dump();
					//Display.Text = "Metadata Server " + serverID + " dumped"; //fazer dump localmente tb
					DisplayTB.AppendText("Metadata Server " + serverID + " dumped" + Environment.NewLine);
					String dump = "";
					foreach (PadiFile temp in toDump)
					{
						dump += temp + Environment.NewLine;
					}
					DumpTextBox.Text = dump;
					break;
				case 'd':
					if (!_dataServers.ContainsKey(serverID))
						CreateDS();
					_dataServers[serverID].RemoteReference.Dump();
					//Display.Text = "Data Server " + serverID + " dumped";
					DisplayTB.AppendText("Data Server " + serverID + " dumped" + Environment.NewLine);
					break;
				case 'c':
					if (!_clients.ContainsKey(serverID))
						CreateClient();
					_clients[serverID].RemoteReference.Dump();
					//Display.Text = "Client " + serverID + " dumped";
					DisplayTB.AppendText("Client " + serverID + " dumped" + Environment.NewLine);
					break;
			}
		}

		public void CreateCommand(string clientID, string filename, int numDS, int readQ, int writeQ)
		{
			if (!_clients.ContainsKey(clientID))
				CreateClient();
			PadiFile metadata = _clients[clientID].RemoteReference.Create(filename, numDS, readQ, writeQ);
			//Display.Text = "File " + filename + " created";
			DisplayTB.AppendText("File " + filename + " created" + Environment.NewLine);
		}

		public void DeleteCommand(string clientID, string filename)
		{
			if (!_clients.ContainsKey(clientID))
				CreateClient();
			_clients[clientID].RemoteReference.Delete(filename);
			//Display.Text = "File " + filename + " deleted";
			DisplayTB.AppendText("File " + filename + " deleted" + Environment.NewLine);
		}

		public void OpenCommand(string clientID, string filename)
		{
			if (!_clients.ContainsKey(clientID))
				CreateClient();
			int fileReg = _clients[clientID].RemoteReference.Open(filename);
			//Display.Text = "File " + filename + " opened in client file register " + fileReg;
			DisplayTB.AppendText("File " + filename + " opened in client file register " + fileReg + Environment.NewLine);
		}

		public void CloseCommand(string clientID, string filename)
		{
			if (!_clients.ContainsKey(clientID))
				CreateClient();
			_clients[clientID].RemoteReference.Close(filename);
			//Display.Text = "File " + filename + " closed";
			DisplayTB.AppendText("File " + filename + " closed" + Environment.NewLine);
		}

		public void ReadCommand(string clientID, int fileReg, string semantics, int byteArrayReg)
		{
			if (!_clients.ContainsKey(clientID))
				CreateClient();
			try{
			string contentRead = _clients[clientID].RemoteReference.Read(fileReg, semantics, byteArrayReg);
			//Display.Text = "File in file register " + fileReg + " read and contents stored in byte array register " + byteArrayReg + "\nContent read: " + contentRead;
			DisplayTB.AppendText("File in file register " + fileReg + " read and contents stored in byte array register " + byteArrayReg + "\nContent read: " + contentRead +Environment.NewLine);
			}
			catch (RemotingException e)
			{
				//Display.Text = "READ - " + e.Message;
				DisplayTB.AppendText("READ - " + e.Message + Environment.NewLine);
			}
		}

		public void WriteCommand(string clientID, int fileReg, int byteArrayReg)
		{
			if (!_clients.ContainsKey(clientID))
				CreateClient();
			try
			{
				string contentWritten = _clients[clientID].RemoteReference.Write(fileReg, byteArrayReg);
				//Display.Text = "File in file register " + fileReg + " written with contents stored in byte array register " + byteArrayReg + "\nContent written: " + contentWritten;
				DisplayTB.AppendText("File in file register " + fileReg + " written with contents stored in byte array register " + byteArrayReg + "\nContent written: " + contentWritten + Environment.NewLine);
			}
			catch (RemotingException e)
			{
				//Display.Text = "WRITE - " + e.Message;
				DisplayTB.AppendText("WRITE - " + e.Message + Environment.NewLine);
			}
		}

		public void WriteCommand(string clientID, int fileReg, string contents)
		{
			if (!_clients.ContainsKey(clientID))
				CreateClient();
			try{
			string contentWritten = _clients[clientID].RemoteReference.Write(fileReg, contents);
			//Display.Text = "File in file register " + fileReg + " written with " + contents;
			DisplayTB.AppendText("File in file register " + fileReg + " written with " + contents + Environment.NewLine);
			}
			catch (RemotingException e)
			{
				//Display.Text = "WRITE - " + e.Message;
				DisplayTB.AppendText("WRITE - " + e.Message + Environment.NewLine);
			}
		}

		public void CopyCommand(string clientID, int fileReg1, string semantics, int fileReg2, string salt)
		{
			if (!_clients.ContainsKey(clientID))
				CreateClient();
			try{
			string contentWritten = _clients[clientID].RemoteReference.Copy(fileReg1, semantics, fileReg2, salt);
			//Display.Text = "Contents of file in file register " + fileReg2 + " appended with " + salt + " and written in file in file register " + fileReg2 + "\nContent written: " + contentWritten;
			DisplayTB.AppendText("Contents of file in file register " + fileReg2 + " appended with " + salt + " and written in file in file register " + fileReg2 + "\nContent written: " + contentWritten + Environment.NewLine);
			}
			catch (RemotingException e)
			{
				//Display.Text = "Copy - " + e.Message;
				DisplayTB.AppendText("Copy - " + e.Message + Environment.NewLine);
			}
		}

		public delegate void RemoteAsyncDelegate(List<string> commands);

		// This is the call that the AsyncCallBack delegate will reference.
		public static void OurRemoteAsyncCallBack(IAsyncResult ar)
		{
			// Alternative 2: Use the callback to get the return value
			RemoteAsyncDelegate del = (RemoteAsyncDelegate)((AsyncResult)ar).AsyncDelegate;
			/*Console.WriteLine("\r\n**SUCCESS**: Result of the remote AsyncCallBack: " + */
			del.EndInvoke(ar);//);

			return;
		}

		public void ExeScriptCommand(string clientID, List<string> commands)
		{
			if (!_clients.ContainsKey(clientID))
				CreateClient();
			try
			{
				// change this to true to use the callback (alt.2)
				bool useCallback = true;

				if (!useCallback)
				{
					// Alternative 1: asynchronous call without callback
					// Create delegate to remote method
					RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(_clients[clientID].RemoteReference.ExeScript);
					// Call delegate to remote method
					IAsyncResult RemAr = RemoteDel.BeginInvoke(commands, null, null);
					// Wait for the end of the call and then explictly call EndInvoke
					RemAr.AsyncWaitHandle.WaitOne();
					/*Console.WriteLine(*/
					RemoteDel.EndInvoke(RemAr);//);
					//Display.Text = "Exescript command sent";
					DisplayTB.AppendText("Exescript command sent" + Environment.NewLine);

				}
				else
				{
					// Alternative 2: asynchronous call with callback
					// Create delegate to remote method
					RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(_clients[clientID].RemoteReference.ExeScript);
					// Create delegate to local callback
					AsyncCallback RemoteCallback = new AsyncCallback(PuppetMasterForm.OurRemoteAsyncCallBack);
					// Call remote method
					IAsyncResult RemAr = RemoteDel.BeginInvoke(commands, RemoteCallback, null);
				}
			}
			catch (SocketException)
			{
				System.Console.WriteLine("Could not locate server");
			}
		}

		public override object InitializeLifetimeService()
		{

			return null;

		}

		
	}

}