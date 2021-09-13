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
        
        public static void SendData(Socket socket, String msg)
        {
            string message = msg;
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

        public static void SendCommand(Socket socket, int command) {
            var mensaje = Console.ReadLine();
            var header = new Header(HeaderConstants.Request, command, 0);
            var data = header.GetRequest();
            var sentBytes = 0;
            while (sentBytes < data.Length)
            {
                sentBytes += socket.Send(data, sentBytes, data.Length - sentBytes, SocketFlags.None);
            }
            sentBytes = 0;
            var bytesMessage = Encoding.UTF8.GetBytes(mensaje);
            while (sentBytes < bytesMessage.Length)
            {
                sentBytes += socket.Send(bytesMessage, sentBytes, bytesMessage.Length - sentBytes,
                    SocketFlags.None);
            }
        }

        public static void SendCommand(Socket socket, byte[] data, string mensaje) {
            var sentBytes = 0;
            while (sentBytes < data.Length)
            {
                sentBytes += socket.Send(data, sentBytes, data.Length - sentBytes, SocketFlags.None);
            }
            sentBytes = 0;
            var bytesMessage = Encoding.UTF8.GetBytes(mensaje);
            while (sentBytes < bytesMessage.Length)
            {
                sentBytes += socket.Send(bytesMessage, sentBytes, bytesMessage.Length - sentBytes,
                    SocketFlags.None);
            }
        }
    }
}