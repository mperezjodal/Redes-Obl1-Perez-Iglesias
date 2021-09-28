using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using ProtocolLibrary;

namespace SocketUtils
{
    public static class Utils
    {
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

        public static List<string> ReceiveCommandAndMessage(Socket socket)
        {
            List<string> commandAndMessage = new List<string>();

            var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                HeaderConstants.DataLength;
            var buffer = new byte[headerLength];
            Utils.ReceiveData(socket, headerLength, ref buffer);
            var header = new Header();
            header.DecodeData(buffer);
            var bufferData = new byte[header.IDataLength];

            Utils.ReceiveData(socket, header.IDataLength, ref bufferData);
            string message = Encoding.UTF8.GetString(bufferData);

            commandAndMessage.Add(header.ICommand.ToString());
            commandAndMessage.Add(message);

            return commandAndMessage;
        }

        public static void ReceiveData(Socket socket, int Length, ref byte[] buffer)
        {
            var iRecv = 0;
            while (iRecv < Length)
            {
                try
                {
                    var localRecv = socket.Receive(buffer, iRecv, Length - iRecv, SocketFlags.None);
                    iRecv += localRecv;
                }
                catch (SocketException se)
                {
                    Console.WriteLine(se.Message);
                    return;
                }
            }
        }

        public static void SendData(Socket socket, Header header, string mensaje)
        {
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
    }
}