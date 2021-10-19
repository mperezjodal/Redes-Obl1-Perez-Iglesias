using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ProtocolLibrary;

namespace ConnectionUtils
{
    public static class Utils
    {
        public static string ClientReceiveMessageData(NetworkStream networkStream)
        {
            try
            {
                return ReceiveMessageData(networkStream);
            }
            catch (Exception)
            {
                Console.WriteLine("Se ha cerrado la conexión con el servidor.");
                return null;
            }
        }

        private static string ReceiveMessageData(NetworkStream networkStream)
        {
            while (true)
            {
                var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                   HeaderConstants.DataLength;
                var buffer = new byte[headerLength];

                ReceiveData(networkStream, headerLength, ref buffer);
                var header = new Header();
                header.DecodeData(buffer);

                var bufferData = new byte[header.IDataLength];
                ReceiveData(networkStream, header.IDataLength, ref bufferData);
                return Encoding.UTF8.GetString(bufferData);
            }
        }

        public static List<string> ClientReceiveCommandAndMessage(NetworkStream networkStream)
        {
            try
            {
                return ReceiveCommandAndMessage(networkStream);
            }
            catch (Exception)
            {
                Console.WriteLine("Se ha cerrado la conexión con el servidor.");
                return null;
            }
        }

        public static List<string> ReceiveCommandAndMessage(NetworkStream networkStream)
        {
            List<string> commandAndMessage = new List<string>();

            var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                HeaderConstants.DataLength;
            var buffer = new byte[headerLength];

            Utils.ReceiveData(networkStream, headerLength, ref buffer);
            var header = new Header();
            header.DecodeData(buffer);
            var bufferData = new byte[header.IDataLength];

            Utils.ReceiveData(networkStream, header.IDataLength, ref bufferData);
            string message = Encoding.UTF8.GetString(bufferData);

            commandAndMessage.Add(header.ICommand.ToString());
            commandAndMessage.Add(message);

            return commandAndMessage;
        }

        public static void ClientReceiveData(NetworkStream networkStream, int Length, ref byte[] buffer)
        {
            try
            {
                ReceiveData(networkStream, Length, ref buffer);
            }
            catch (Exception)
            {
                Console.WriteLine("Se ha cerrado la conexión con el servidor.");
            }
        }

        public static void ServerReceiveData(NetworkStream networkStream, int Length, ref byte[] buffer)
        {
            try
            {
                ReceiveData(networkStream, Length, ref buffer);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void ReceiveData(NetworkStream networkStream, int Length, ref byte[] buffer)
        {
            var iRecv = 0;
            while (iRecv < Length)
            {
                var localRecv = networkStream.ReadAsync(buffer, iRecv, Length - iRecv);
                iRecv += localRecv.Result;
            }
        }

        public static async Task ClientSendData(NetworkStream networkStream, Header header, string message)
        {
            try
            {
                await SendData(networkStream, header, message);
            }
            catch (Exception)
            {
                Console.WriteLine("Se ha cerrado la conexión con el servidor.");
            }
        }

        public static async Task ServerSendData(NetworkStream networkStream, Header header, string message)
        {
            try
            {
                await SendData(networkStream, header, message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static async Task SendData(NetworkStream networkStream, Header header, string message)
        {
            byte[] headerData = header.GetRequest();
            byte[] data = Encoding.UTF8.GetBytes(message);
            await networkStream.WriteAsync(header.GetRequest(), 0, headerData.Length);
            await networkStream.WriteAsync(data, 0, data.Length);
        }
    }
}