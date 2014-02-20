using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client
{
	class ScriptInterpreterClient
	{
		//public void execute(String command, Client client)
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
		//    case "DUMP":
		//      serverID = commandSplited[1];
		//      client.Dump();
		//      break;
		//    case "CREATE":
		//      serverID = commandSplited[1].Substring(0, commandSplited[1].Length - 1);
		//      filename = commandSplited[2].Substring(0, commandSplited[2].Length - 1);
		//      numDS = commandSplited[3].Substring(0, commandSplited[3].Length - 1);
		//      readQ = commandSplited[4].Substring(0, commandSplited[4].Length - 1);
		//      writeQ = commandSplited[5];
		//      client.Create(filename, Int32.Parse(numDS), Int32.Parse(readQ), Int32.Parse(writeQ));
		//      break;
		//    case "DELETE":
		//      serverID = commandSplited[1].Substring(0, commandSplited[1].Length - 1);
		//      filename = commandSplited[2];
		//      client.Delete(filename);
		//      break;
		//    case "OPEN":
		//      serverID = commandSplited[1].Substring(0, commandSplited[1].Length - 1);
		//      filename = commandSplited[2];
		//      client.Open(filename);
		//      break;
		//    case "CLOSE":
		//      serverID = commandSplited[1].Substring(0, commandSplited[1].Length - 1);
		//      filename = commandSplited[2];
		//      client.Close(filename);
		//      break;
		//    case "READ":
		//      serverID = commandSplited[1].Substring(0, commandSplited[1].Length - 1);
		//      fileReg1 = commandSplited[2].Substring(0, commandSplited[2].Length - 1);
		//      semantics = commandSplited[3].Substring(0, commandSplited[3].Length - 1);
		//      stringReg = commandSplited[4];
		//      client.Read(Int32.Parse(fileReg1), semantics, Int32.Parse(stringReg));
		//      break;
		//    case "WRITE":
		//      serverID = commandSplited[1].Substring(0, commandSplited[1].Length - 1);
		//      fileReg1 = commandSplited[2].Substring(0, commandSplited[2].Length - 1);
		//      if (command.Contains(""))
		//      {
		//        contents = command.Substring(command.IndexOf('"') + 1, command.Length - command.IndexOf('"') - 2);
		//        client.Write(Int32.Parse(fileReg1), contents);
		//      }
		//      else
		//      {
		//        stringReg = commandSplited[3];
		//        client.Write(Int32.Parse(fileReg1), Int32.Parse(stringReg));
		//      }
		//      break;
		//    case "COPY":
		//      serverID = commandSplited[1].Substring(0, commandSplited[1].Length - 1);
		//      fileReg1 = commandSplited[2].Substring(0, commandSplited[2].Length - 1);
		//      semantics = commandSplited[3].Substring(0, commandSplited[3].Length - 1);
		//      fileReg2 = commandSplited[4].Substring(0, commandSplited[4].Length - 1);
		//      salt = command.Substring(command.IndexOf('"') + 1, command.Length - command.IndexOf('"') - 2);
		//      client.Copy(Int32.Parse(fileReg1), semantics, Int32.Parse(fileReg2), salt);
		//      break;
		//    default:
		//      return;
		//  }
		//}

		public void execute(String command, Client client)
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
			string[] commandWithCommaSplited;
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
				case "DUMP":
					serverID = command.Substring(command.IndexOf(' ') + 1);
					client.Dump();
					break;
				case "CREATE":
					rest = command.Substring(command.IndexOf(' ') + 1);
					commandWithCommaSplited = rest.Split(',');
					serverID = commandWithCommaSplited[0].Trim();
					filename = commandWithCommaSplited[1].Trim();
					numDS = commandWithCommaSplited[2].Trim();
					readQ = commandWithCommaSplited[3].Trim();
					writeQ = commandWithCommaSplited[4].Trim();
					client.Create(filename, Int32.Parse(numDS), Int32.Parse(readQ), Int32.Parse(writeQ));
					break;
				case "DELETE":
					rest = command.Substring(command.IndexOf(' ') + 1);
					commandWithCommaSplited = rest.Split(',');
					serverID = commandWithCommaSplited[0].Trim();
					filename = commandWithCommaSplited[1].Trim();
					client.Delete(filename);
					break;
				case "OPEN":
					rest = command.Substring(command.IndexOf(' ') + 1);
					commandWithCommaSplited = rest.Split(',');
					serverID = commandWithCommaSplited[0].Trim();
					filename = commandWithCommaSplited[1].Trim();
					client.Open(filename);
					break;
				case "CLOSE":
					rest = command.Substring(command.IndexOf(' ') + 1);
					commandWithCommaSplited = rest.Split(',');
					serverID = commandWithCommaSplited[0].Trim();
					filename = commandWithCommaSplited[1].Trim();
					client.Close(filename);
					break;
				case "READ":
					rest = command.Substring(command.IndexOf(' ') + 1);
					commandWithCommaSplited = rest.Split(',');
					serverID = commandWithCommaSplited[0].Trim();
					fileReg1 = commandWithCommaSplited[1].Trim();
					semantics = commandWithCommaSplited[2].Trim();
					stringReg = commandWithCommaSplited[3].Trim();
					client.Read(Int32.Parse(fileReg1), semantics, Int32.Parse(stringReg));
					break;
				case "WRITE":
					rest = command.Substring(command.IndexOf(' ') + 1);
					commandWithCommaSplited = rest.Split(',');
					serverID = commandWithCommaSplited[0].Trim();
					fileReg1 = commandWithCommaSplited[1].Trim();
					if (command.Contains(""))
					{
						contents = command.Substring(command.IndexOf('"') + 1, command.Length - command.IndexOf('"') - 2);
						client.Write(Int32.Parse(fileReg1), contents);
					}
					else
					{
						stringReg = commandWithCommaSplited[2].Trim();
						client.Write(Int32.Parse(fileReg1), Int32.Parse(stringReg));
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
					Console.WriteLine(fileReg1 + " " + semantics + " " + fileReg2 + " " + salt);
					client.Copy(Int32.Parse(fileReg1), semantics, Int32.Parse(fileReg2), salt);
					break;
				default:
					return;
			}
		}
	}
}
