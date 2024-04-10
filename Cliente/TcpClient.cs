using System;
using System.Net.Sockets;
using System.Text;

class SimpleTcpClient
{
    static void Main()
    {
        Console.Write("Enter server IP address: ");
        string serverIp = Console.ReadLine();

        using (TcpClient client = new TcpClient(serverIp, 13000))
        {
            Console.WriteLine("Connected to server...");

            NetworkStream stream = client.GetStream();

            string clientId;
            do
            {
                Console.Write("Enter client ID (numbers only): ");
                clientId = Console.ReadLine();
            } while (!IsNumeric(clientId));

            byte[] data = Encoding.ASCII.GetBytes(clientId);
            stream.Write(data, 0, data.Length);
            Console.WriteLine("Sent ID: " + clientId);

            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Received: " + response);

            Console.WriteLine("You can now send messages. Type 'QUIT' to exit.");

            string message;
            do
            {
                message = Console.ReadLine();
                data = Encoding.ASCII.GetBytes(message);
                stream.Write(data, 0, data.Length);

                buffer = new byte[1024];
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received: " + response);
            } while (message.ToUpper() != "QUIT");
        }

        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }

    static bool IsNumeric(string value)
    {
        foreach (char c in value)
        {
            if (!char.IsDigit(c))
                return false;
        }
        return true;
    }
}
