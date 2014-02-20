using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PuppetMaster
{
	class ScriptInterpreter
	{

		//public void execute(String command, PuppetMasterForm puppetMaster, int clientID)
		//{
		//  string[] commandSplited = command.Split(' ');
		//  string[] commandWithCommaSplited = command.Split(',');
		//  string serverID;
		//  string filename;
		//  string numDS;
		//  string readQ;
		//  string writeQ;
		//  string fileReg1;
		//  string fileReg2;
		//  string stringReg;
		//  string contents;
		//  string semantics;
		//  string salt;
		//  switch (commandSplited[0])
		//  {
		//    case "FAIL":
		//      serverID = commandSplited[1];
		//      puppetMaster.FailCommand(serverID);
		//      break;
		//    case "RECOVER":
		//      serverID = commandSplited[1];
		//      puppetMaster.RecoverCommand(serverID);
		//      break;
		//    case "FREEZE":
		//      serverID = commandSplited[1];
		//      puppetMaster.FreezeCommand(serverID);
		//      break;
		//    case "UNFREEZE":
		//      serverID = commandSplited[1];
		//      puppetMaster.UnfreezeCommand(serverID);
		//      break;
		//    case "DUMP":
		//      serverID = commandSplited[1];
		//      puppetMaster.DumpCommand(serverID);
		//      break;
		//    case "CREATE":
		//      serverID = commandSplited[1].Substring(0, commandSplited[1].Length - 1);
		//      filename = commandSplited[2].Substring(0, commandSplited[2].Length - 1);
		//      numDS = commandSplited[3].Substring(0, commandSplited[3].Length - 1);
		//      readQ = commandSplited[4].Substring(0, commandSplited[4].Length - 1);
		//      writeQ = commandSplited[5];
		//      puppetMaster.CreateCommand(serverID, filename, Int32.Parse(numDS), Int32.Parse(readQ), Int32.Parse(writeQ));
		//      break;
		//    case "DELETE":
		//      serverID = commandSplited[1].Substring(0, commandSplited[1].Length - 1);
		//      filename = commandSplited[2];
		//      puppetMaster.DeleteCommand(serverID, filename);
		//      break;
		//    case "OPEN":
		//      serverID = commandSplited[1].Substring(0, commandSplited[1].Length - 1);
		//      filename = commandSplited[2];
		//      puppetMaster.OpenCommand(serverID, filename);
		//      break;
		//    case "CLOSE":
		//      serverID = commandSplited[1].Substring(0, commandSplited[1].Length - 1);
		//      filename = commandSplited[2];
		//      puppetMaster.CloseCommand(serverID, filename);
		//      break;
		//    case "READ":
		//      serverID = commandSplited[1].Substring(0, commandSplited[1].Length - 1);
		//      fileReg1 = commandSplited[2].Substring(0, commandSplited[2].Length - 1);
		//      semantics = commandSplited[3].Substring(0, commandSplited[3].Length - 1);
		//      stringReg = commandSplited[4];
		//      puppetMaster.ReadCommand(serverID, Int32.Parse(fileReg1), semantics, Int32.Parse(stringReg));
		//      break;
		//    case "WRITE":
		//      serverID = commandSplited[1].Substring(0, commandSplited[1].Length - 1);
		//      fileReg1 = commandSplited[2].Substring(0, commandSplited[2].Length - 1);
		//      if (command.Contains(""))
		//      {
		//        contents = command.Substring(command.IndexOf('"') + 1, command.Length - command.IndexOf('"') - 2);
		//        puppetMaster.WriteCommand(serverID, Int32.Parse(fileReg1), contents);
		//      }
		//      else
		//      {
		//        stringReg = commandSplited[3];
		//        puppetMaster.WriteCommand(serverID, Int32.Parse(fileReg1), Int32.Parse(stringReg));
		//      }
		//      break;
		//    case "COPY":
		//      serverID = commandSplited[1].Substring(0, commandSplited[1].Length - 1);
		//      fileReg1 = commandSplited[2].Substring(0, commandSplited[2].Length - 1);
		//      semantics = commandSplited[3].Substring(0, commandSplited[3].Length - 1);
		//      fileReg2 = commandSplited[4].Substring(0, commandSplited[4].Length - 1);
		//      salt = command.Substring(command.IndexOf('"') + 1, command.Length - command.IndexOf('"') - 2);
		//      puppetMaster.CopyCommand(serverID, Int32.Parse(fileReg1), semantics, Int32.Parse(fileReg2), salt);
		//      break;
		//    case "EXESCRIPT":
		//      serverID = commandSplited[1];
		//      filename = commandSplited[2];
		//      //TextWriter tr = new StreamWriter(@"batata.txt");

		//      TextReader tr = new StreamReader(@filename);
		//      List<string> commands = new List<string>();
		//      while (tr.Peek() != -1)
		//      {
		//        string comm = tr.ReadLine();
		//        commands.Add(comm);
		//      }

		//      puppetMaster.ExeScriptCommand(serverID, commands);
		//      //TextReader tr = new StreamReader(@filename);
		//      //executeScript(tr, puppetMaster, Int32.Parse(serverID));
		//      break;
		//    default:
		//      return;
		//  }
		//}

		public void execute(String command, PuppetMasterForm puppetMaster, int clientID)
		{
			string rest = null;
			string serverID = null;
			string com;
			if (command.IndexOf(' ') != -1)
			{
				com = command.Substring(0, command.IndexOf(' '));
			}
			else
			{
				com = command;
			}
			string[] commandWithCommaSplited;// = command.Split(',');
			string filename;
			string numDS;
			string readQ;
			string writeQ;
			string fileReg1;
			string fileReg2;
			string stringReg;
			string contents;
			string semantics;
			string salt;
			switch (com)
			{
				case "FAIL":
					serverID = command.Substring(command.IndexOf(' ') + 1);
					puppetMaster.FailCommand(serverID);
					break;
				case "RECOVER":
					serverID = command.Substring(command.IndexOf(' ') + 1);
					puppetMaster.RecoverCommand(serverID);
					break;
				case "FREEZE":
					serverID = command.Substring(command.IndexOf(' ') + 1);
					puppetMaster.FreezeCommand(serverID);
					break;
				case "UNFREEZE":
					serverID = command.Substring(command.IndexOf(' ') + 1);
					puppetMaster.UnfreezeCommand(serverID);
					break;
				case "DUMP":
					serverID = command.Substring(command.IndexOf(' ') + 1);
					puppetMaster.DumpCommand(serverID);
					break;
				case "CREATE":
					rest = command.Substring(command.IndexOf(' ') + 1);
					commandWithCommaSplited = rest.Split(',');
					serverID = commandWithCommaSplited[0].Trim();
					filename = commandWithCommaSplited[1].Trim();
					numDS = commandWithCommaSplited[2].Trim();
					readQ = commandWithCommaSplited[3].Trim();
					writeQ = commandWithCommaSplited[4].Trim();
					puppetMaster.CreateCommand(serverID, filename, Int32.Parse(numDS), Int32.Parse(readQ), Int32.Parse(writeQ));
					break;
				case "DELETE":
					rest = command.Substring(command.IndexOf(' ') + 1);
					commandWithCommaSplited = rest.Split(',');
					serverID = commandWithCommaSplited[0].Trim();
					filename = commandWithCommaSplited[1].Trim();
					puppetMaster.DeleteCommand(serverID, filename);
					break;
				case "OPEN":
					rest = command.Substring(command.IndexOf(' ') + 1);
					commandWithCommaSplited = rest.Split(',');
					serverID = commandWithCommaSplited[0].Trim();
					filename = commandWithCommaSplited[1].Trim();
					puppetMaster.OpenCommand(serverID, filename);
					break;
				case "CLOSE":
					rest = command.Substring(command.IndexOf(' ') + 1);
					commandWithCommaSplited = rest.Split(',');
					serverID = commandWithCommaSplited[0].Trim();
					filename = commandWithCommaSplited[1].Trim();
					puppetMaster.CloseCommand(serverID, filename);
					break;
				case "READ":
					rest = command.Substring(command.IndexOf(' ') + 1);
					commandWithCommaSplited = rest.Split(',');
					serverID = commandWithCommaSplited[0].Trim();
					fileReg1 = commandWithCommaSplited[1].Trim();
					semantics = commandWithCommaSplited[2].Trim();
					stringReg = commandWithCommaSplited[3].Trim();
					puppetMaster.ReadCommand(serverID, Int32.Parse(fileReg1), semantics, Int32.Parse(stringReg));
					break;
				case "WRITE":
					rest = command.Substring(command.IndexOf(' ') + 1);
					commandWithCommaSplited = rest.Split(',');
					serverID = commandWithCommaSplited[0].Trim();
					fileReg1 = commandWithCommaSplited[1].Trim();
					if (command.Contains(""))
					{
						contents = command.Substring(command.IndexOf('"') + 1, command.Length - command.IndexOf('"') - 2);
						puppetMaster.WriteCommand(serverID, Int32.Parse(fileReg1), contents);
					}
					else
					{
						stringReg = commandWithCommaSplited[2].Trim();
						puppetMaster.WriteCommand(serverID, Int32.Parse(fileReg1), Int32.Parse(stringReg));
					}
					break;
				case "COPY":
					rest = command.Substring(command.IndexOf(' ') + 1);
					commandWithCommaSplited = rest.Split(',');
					serverID = commandWithCommaSplited[0].Trim();
					fileReg1 = commandWithCommaSplited[1].Trim();
					semantics = commandWithCommaSplited[2].Trim();
					fileReg2 = commandWithCommaSplited[3].Trim();
					salt = commandWithCommaSplited[4].Trim();
					puppetMaster.CopyCommand(serverID, Int32.Parse(fileReg1), semantics, Int32.Parse(fileReg2), salt);
					break;
				case "EXESCRIPT":
					rest = command.Substring(command.IndexOf(' ') + 1);
					serverID = rest.Substring(0, rest.IndexOf(' '));
					filename = rest.Substring(rest.IndexOf(' ') + 1); ;
					//TextWriter tr = new StreamWriter(@"batata.txt");

					TextReader tr = new StreamReader(@filename);
					List<string> commands = new List<string>();
					while (tr.Peek() != -1)
					{
						string comm = tr.ReadLine();
						commands.Add(comm);
					}

					puppetMaster.ExeScriptCommand(serverID, commands);
					//TextReader tr = new StreamReader(@filename);
					//executeScript(tr, puppetMaster, Int32.Parse(serverID));
					break;
				default:
					return;
			}
		}
	}
}