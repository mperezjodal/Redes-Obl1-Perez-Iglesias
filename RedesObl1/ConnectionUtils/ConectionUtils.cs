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
        public static async Task<string> ClientReceiveMessageData(NetworkStream networkStream)
        {
            try
            {
                return await ReceiveMessageData(networkStream);
            }
            catch (Exception)
            {
                Console.WriteLine("Se ha cerrado la conexión con el servidor.");
                return null;
            }
        }

        private static async Task<string> ReceiveMessageData(NetworkStream networkStream)
        {
            while (true)
            {
                var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                   HeaderConstants.DataLength;
                var buffer = new byte[headerLength];

                buffer = await ReceiveData(networkStream, headerLength, buffer);
                var header = new Header();
                header.DecodeData(buffer);

                var bufferData = new byte[header.IDataLength];
                bufferData = await ReceiveData(networkStream, header.IDataLength, bufferData);
                return Encoding.UTF8.GetString(bufferData);
            }
        }

        public static async Task<List<string>> ClientReceiveCommandAndMessage(NetworkStream networkStream)
        {
            try
            {
                return await ReceiveCommandAndMessage(networkStream);
            }
            catch (Exception)
            {
                Console.WriteLine("Se ha cerrado la conexión con el servidor.");
                return null;
            }
        }

        public static async Task<List<string>> ReceiveCommandAndMessage(NetworkStream networkStream)
        {
            List<string> commandAndMessage = new List<string>();

            var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                HeaderConstants.DataLength;
            var buffer = new byte[headerLength];

            buffer = await ReceiveData(networkStream, headerLength, buffer);
            var header = new Header();
            header.DecodeData(buffer);
            var bufferData = new byte[header.IDataLength];

            bufferData = await ReceiveData(networkStream, header.IDataLength, bufferData);
            string message = Encoding.UTF8.GetString(bufferData);

            commandAndMessage.Add(header.ICommand.ToString());
            commandAndMessage.Add(message);

            return commandAndMessage;
        }

        public static async Task<byte[]> ClientReceiveData(NetworkStream networkStream, int Length, byte[] buffer)
        {
            try
            {
                return await ReceiveData(networkStream, Length, buffer);
            }
            catch (Exception)
            {
                Console.WriteLine("Se ha cerrado la conexión con el servidor.");
            }
            return buffer;
        }

        public static async Task<byte[]> ServerReceiveData(NetworkStream networkStream, int Length, byte[] buffer)
        {
            try
            {
                return await ReceiveData(networkStream, Length, buffer);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return buffer;
        }

        private static async Task<byte[]> ReceiveData(NetworkStream networkStream, int Length, byte[] buffer)
        {
            var iRecv = 0;
            while (iRecv < Length)
            {
                var localRecv = await networkStream.ReadAsync(buffer, iRecv, Length - iRecv);
                iRecv += localRecv;
            }
            return buffer;
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