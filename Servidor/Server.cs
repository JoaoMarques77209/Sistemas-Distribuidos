using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

class SimpleTcpServer
{
    public static void Main()
    {
        TcpListener server = null;
        try
        {
            Int32 port = 13000;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            server = new TcpListener(localAddr, port);
            server.Start();
            Console.WriteLine("Server started...");

            while (true)
            {
                using TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Client connected...");

                NetworkStream stream = client.GetStream();

                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string clientId = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received ID: " + clientId);

                byte[] initialResponse = Encoding.ASCII.GetBytes("100 OK");
                stream.Write(initialResponse, 0, initialResponse.Length);
                Console.WriteLine("Sent: 100 OK");

                while (true)
                {
                    buffer = new byte[1024];
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Received: " + message);

                    if (message.Trim().ToUpper() == "QUIT")
                    {
                        byte[] responseMessage = Encoding.ASCII.GetBytes("400 BYE");
                        stream.Write(responseMessage, 0, responseMessage.Length);
                        Console.WriteLine("Sent: 400 BYE");
                        break;
                    }
                }
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }
        finally
        {
            server.Stop();
        }

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}
