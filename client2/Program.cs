using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TcpChatClient
{
    private static TcpClient client;
    private static NetworkStream stream;

    public static void Main(string[] args)
    {
        Console.WriteLine("Starting TCP chat client...");

        // Kết nối với server
        client = new TcpClient("localhost", 8080);
        stream = client.GetStream();

        // Khởi tạo một thread mới để nhận tin nhắn từ server
        Thread receiveThread = new Thread(ReceiveMessages);
        receiveThread.Start();

        // Nhập và gửi tin nhắn
        while (true)
        {
            string message = Console.ReadLine();
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
    }

    private static void ReceiveMessages()
    {
        byte[] buffer = new byte[1024];
        int bytesRead;

        while (true)
        {
            try
            {
                // Đọc tin nhắn từ server
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received message: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving message: {ex.Message}");
                break;
            }
        }
    }
}