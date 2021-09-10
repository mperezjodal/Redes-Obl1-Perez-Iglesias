using System;
using System.Net.Sockets;
using System.Text;

namespace SocketUtils
{
    public static class Utils
    {
        private const int ProtocolFixedSize = 4;
        public static void ReciveData(Socket socket)
        {
            while (true)
            {
                byte[] dataLength = new byte[ProtocolFixedSize];
                socket.Receive(dataLength);
                int length = BitConverter.ToInt32(dataLength);
                byte[] data = new byte[length];
                socket.Receive(data);
                string message = Encoding.UTF8.GetString(data);
                Console.WriteLine(message);
            }
        }

        public static void SendData(Socket socket)
        {
            while (true)
            {
                string message = Console.ReadLine();
                byte[] data = Encoding.UTF8.GetBytes(message);
                int length = data.Length;
                byte[] dataLength = BitConverter.GetBytes(length);
                int totalDataSent = 0;
                while (totalDataSent < dataLength.Length)
                {
                    int sent = socket.Send(dataLength, totalDataSent, dataLength.Length-totalDataSent, SocketFlags.None);
                    if (sent == 0)
                    {
                        throw new SocketException();
                    }
                    totalDataSent += sent;
                }
                
                totalDataSent = 0;
                while (totalDataSent < data.Length)
                {
                    int sent = socket.Send(data, totalDataSent, data.Length- totalDataSent, SocketFlags.None);
                    if (sent == 0)
                    {
                        throw new SocketException();
                    }
                    totalDataSent += sent;
                }
            }
        }
    }
}