using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace WinSocketServer
{
    class Program
    {
        static List<Socket> connectionList = new List<Socket>();
        static void Main(string[] args)
        {
            TcpListener tcpListener = new TcpListener(System.Net.IPAddress.Loopback, 5555);
            tcpListener.Start();
            Console.WriteLine("The Server has started on port 5555");
            while (true)
            {
                Socket socket = tcpListener.AcceptSocket();
                connectionList.Add(socket);
                Thread thread = new Thread(Listening);
                thread.Start(socket);
            }
        }

        public static void Listening(object socket)
        {
            Socket serverSocket = (Socket)socket;
            int id = connectionList.IndexOf(serverSocket);
            StreamWriter streamWriter;
            StreamReader streamReader;
            NetworkStream networkStream;
            try
            {
                if (serverSocket.Connected)
                {
                    Console.WriteLine("Client connected, connection id: " + id);
                    networkStream = new NetworkStream(serverSocket);
                    streamWriter = new StreamWriter(networkStream);
                    streamReader = new StreamReader(networkStream);
                    while (true)
                    {
                        try
                        {
                            Console.WriteLine(id + ": " + streamReader.ReadLine());
                        }
                        catch (IOException)
                        {
                            Console.WriteLine("Connection lost for id:" + id);
                            connectionList.Remove(serverSocket);
                            break;
                        }
                    }
                }
                if (serverSocket.Connected)
                    serverSocket.Close();
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
