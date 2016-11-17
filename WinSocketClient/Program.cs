using System;
using System.IO;
using System.Net.Sockets;

namespace WinSocketClient
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient tcpClient;
            NetworkStream networkStream;
            StreamReader streamReader;
            StreamWriter streamWriter;
            try
            {
                tcpClient = new TcpClient();
                tcpClient.NoDelay = true;
                Console.WriteLine("Port:");
                int port = int.Parse(Console.ReadLine());
                tcpClient.Connect(System.Net.IPAddress.Loopback, port);
                Console.WriteLine("Соединение с 127.0.0.1:" + port + " установлено");
                networkStream = tcpClient.GetStream();
                streamWriter = new StreamWriter(networkStream);
                while (true)
                {
                    string message = Console.ReadLine();
                    if (!tcpClient.Connected)
                    {
                        break;
                    }
                    streamWriter.WriteLine(message);
                    try
                    {
                        streamWriter.Flush();
                    }
                    catch (IOException)
                    {
                        Console.WriteLine("Соединение с сервером потеряно");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadLine();
        }
    }
}
