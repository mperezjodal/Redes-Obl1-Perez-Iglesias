using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using ProtocolLibrary;

namespace SocketUtils
{
    public static class Utils
    {
        public static string ReceiveMessageData(NetworkStream networkStream)
        {
            while (true)
            {
                var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                   HeaderConstants.DataLength;
                var buffer = new byte[headerLength];
                try
                {
                    ReceiveData(networkStream, headerLength, ref buffer);
                    var header = new Header();
                    header.DecodeData(buffer);

                    var bufferData = new byte[header.IDataLength];
                    ReceiveData(networkStream, header.IDataLength, ref bufferData);
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

        public static List<string> ReceiveCommandAndMessage(NetworkStream networkStream)
        {
            List<string> commandAndMessage = new List<string>();

            var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                HeaderConstants.DataLength;
            var buffer = new byte[headerLength];
            try
            {
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
            catch (SocketException)
            {
                return commandAndMessage;
            }
            catch (Exception)
            {
                return commandAndMessage;
            }
        }

        public static void ReceiveData(NetworkStream networkStream, int Length, ref byte[] buffer)
        {
            var iRecv = 0;
            while (iRecv < Length)
            {
                try
                {
                    var localRecv = networkStream.Read(buffer, iRecv, Length - iRecv);
                    iRecv += localRecv;
                }
                catch (SocketException se)
                {
                    Console.WriteLine(se.Message);
                    return;
                }
            }
        }

        public static void SendData(NetworkStream networkStream, Header header, string message)
        {
            try
            {
                byte[] headerData = header.GetRequest();
                byte[] data = Encoding.UTF8.GetBytes(message);
                networkStream.Write(header.GetRequest(), 0, headerData.Length);
                networkStream.Write(data, 0, data.Length);
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
                return;
            }
        }
    }
}