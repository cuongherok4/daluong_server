using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TcpChatServer
{
    private static TcpListener server;
    private static ConcurrentBag<TcpClient> clients = new ConcurrentBag<TcpClient>();
    private static bool isRunning = true;// khưởi

    public static void Main(string[] args)
    {
        Console.WriteLine("Starting TCP chat server...");
        server = new TcpListener(IPAddress.Any, 8080);
        server.Start();
        Thread serverInputThread = new Thread(HandleServerInput);// khởi tạo một luồng để gửi dữ liệu 
        serverInputThread.Start(); // chạy luồng , bắt đầu gửi dữ liệu 
        while (isRunning)
        {
            TcpClient client = server.AcceptTcpClient();
            clients.Add(client);
            Console.WriteLine($"New client connected: {client.Client.RemoteEndPoint}");
            Thread thread = new Thread(() => HandleClient(client));
            thread.Start();
        }
    }

    private static void HandleClient(TcpClient client)
    {
        using (NetworkStream stream = client.GetStream())
        {
            byte[] buffer = new byte[1024];
            int bytesRead;

            while (true)
            {
                try
                {
                  
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received message from {client.Client.RemoteEndPoint}: {message}");
                    BroadcastMessage(message, client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling client: {ex.Message}");
                    break;
                }
            }
        }

        // Remove client from the list upon disconnection
        clients = new ConcurrentBag<TcpClient>(clients.Except(new[] { client }));
        client.Close();
        Console.WriteLine($"Client disconnected: {client.Client.RemoteEndPoint}");
    }

    private static void BroadcastMessage(string message, TcpClient sender = null)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        foreach (TcpClient client in clients)
        {
            if (client != sender)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    stream.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending message to {client.Client.RemoteEndPoint}: {ex.Message}");
                }
            }
        }
    }

    private static void HandleServerInput()// input của server
    {
        while (isRunning)// vòng vô tận 
        {
            string message = Console.ReadLine();// input
           
            BroadcastMessage($"Server gửi : {message}");// gửi tới tất cả client trong luồng
        }
    }
}
