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
        public static async Task<string> ClientReceiveMessageDataAsync(NetworkStream networkStream)
        {
            try
            {
                return await ReceiveMessageDataAsync(networkStream);
            }
            catch (Exception)
            {
                Console.WriteLine("Se ha cerrado la conexión con el servidor.");
                return null;
            }
        }

        private static async Task<string> ReceiveMessageDataAsync(NetworkStream networkStream)
        {
            while (true)
            {
                var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                   HeaderConstants.DataLength;
                var buffer = new byte[headerLength];

                buffer = await ReceiveDataAsync(networkStream, headerLength, buffer);
                var header = new Header();
                header.DecodeData(buffer);

                var bufferData = new byte[header.IDataLength];
                bufferData = await ReceiveDataAsync(networkStream, header.IDataLength, bufferData);
                return Encoding.UTF8.GetString(bufferData);
            }
        }

        public static async Task<List<string>> ClientReceiveCommandAndMessageAsync(NetworkStream networkStream)
        {
            try
            {
                return await ReceiveCommandAndMessageAsync(networkStream);
            }
            catch (Exception)
            {
                Console.WriteLine("Se ha cerrado la conexión con el servidor.");
                return null;
            }
        }

        public static async Task<List<string>> ReceiveCommandAndMessageAsync(NetworkStream networkStream)
        {
            List<string> commandAndMessage = new List<string>();

            var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                HeaderConstants.DataLength;
            var buffer = new byte[headerLength];

            buffer = await ReceiveDataAsync(networkStream, headerLength, buffer);
            var header = new Header();
            header.DecodeData(buffer);
            var bufferData = new byte[header.IDataLength];

            bufferData = await ReceiveDataAsync(networkStream, header.IDataLength, bufferData);
            string message = Encoding.UTF8.GetString(bufferData);

            commandAndMessage.Add(header.ICommand.ToString());
            commandAndMessage.Add(message);

            return commandAndMessage;
        }

        public static async Task<byte[]> ClientReceiveDataAsync(NetworkStream networkStream, int Length, byte[] buffer)
        {
            try
            {
                return await ReceiveDataAsync(networkStream, Length, buffer);
            }
            catch (Exception)
            {
                Console.WriteLine("Se ha cerrado la conexión con el servidor.");
            }
            return buffer;
        }

        public static async Task<byte[]> ServerReceiveDataAsync(NetworkStream networkStream, int Length, byte[] buffer)
        {
            try
            {
                return await ReceiveDataAsync(networkStream, Length, buffer);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return buffer;
        }

        private static async Task<byte[]> ReceiveDataAsync(NetworkStream networkStream, int Length, byte[] buffer)
        {
            var iRecv = 0;
            while (iRecv < Length)
            {
                var localRecv = await networkStream.ReadAsync(buffer, iRecv, Length - iRecv);
                iRecv += localRecv;
            }
            return buffer;
        }

        public static async Task ClientSendDataAsync(NetworkStream networkStream, Header header, string message)
        {
            try
            {
                await SendDataAsync(networkStream, header, message);
            }
            catch (Exception)
            {
                Console.WriteLine("Se ha cerrado la conexión con el servidor.");
            }
        }

        public static async Task ServerSendDataAsync(NetworkStream networkStream, Header header, string message)
        {
            try
            {
                await SendDataAsync(networkStream, header, message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static async Task SendDataAsync(NetworkStream networkStream, Header header, string message)
        {
            byte[] headerData = header.GetRequest();
            byte[] data = Encoding.UTF8.GetBytes(message);
            await networkStream.WriteAsync(header.GetRequest(), 0, headerData.Length);
            await networkStream.WriteAsync(data, 0, data.Length);
        }
    }
}