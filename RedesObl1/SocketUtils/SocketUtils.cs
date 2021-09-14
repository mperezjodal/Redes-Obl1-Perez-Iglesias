using System;
using System.Net.Sockets;
using System.Text;

namespace SocketUtils
{
    public static class Utils
    {
        private const int ProtocolFixedSize = 4;
        public static string ReciveMessageData(Socket socket)
        {
            while (true)
            {
                var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                   HeaderConstants.DataLength;
                var buffer = new byte[headerLength];
                try
                {
                    ReceiveData(socket, headerLength, ref buffer);
                    var header = new Header();
                    header.DecodeData(buffer);

                    var bufferData = new byte[header.IDataLength];  
                    ReceiveData(socket, header.IDataLength, ref bufferData);
                    return Encoding.UTF8.GetString(bufferData);
                }
                catch (SocketException e)
                {
                    Console.WriteLine($"Error: {e.Message}..");    
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}..");    
                }
            }
        }

        public static void ReceiveData(Socket socket,  int Length, ref byte[] buffer)
        {
            var iRecv = 0;
            while (iRecv < Length)
            {
                try
                {
                    var localRecv = socket.Receive(buffer, iRecv, Length - iRecv, SocketFlags.None);
                    // if (localRecv == 0) // Si recieve retorna 0 -> la conexion se cerro desde el endpoint remoto
                    // {
                    //     if (!_exit)
                    //     {
                    //         socket.Shutdown(SocketShutdown.Both);
                    //         socket.Close();
                    //     }
                    //     else
                    //     {
                    //         throw new Exception("Server is closing");
                    //     }
                    // }

                    iRecv += localRecv;
                }
                catch (SocketException se)
                {
                    Console.WriteLine(se.Message);
                    return;
                }
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

        public static void Send(Socket socket, byte[] data, string mensaje) {
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