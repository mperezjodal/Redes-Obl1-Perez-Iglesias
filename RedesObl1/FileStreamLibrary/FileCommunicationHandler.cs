using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using FileStreamLibrary.Protocol;

namespace FileStreamLibrary
{
    public class FileCommunicationHandler
    {
        private readonly FileStreamHandler _fileStreamHandler;
        private readonly SocketStreamHandler _socketStreamHandler;

        public FileCommunicationHandler(Socket socket)
        {
            _socketStreamHandler = new SocketStreamHandler(socket);
            _fileStreamHandler = new FileStreamHandler();
        }

        public void SendFile(string path)
        {
            var fileInfo = new FileInfo(path);
            string fileName = fileInfo.Name;
            byte[] fileNameData = Encoding.UTF8.GetBytes(fileName);
            int fileNameLength = fileNameData.Length;
            byte[] fileNameLengthData = BitConverter.GetBytes(fileNameLength);
            // 1.- Envío el largo del nombre del archivo
            _socketStreamHandler.SendData(fileNameLengthData);
            // 2.- Envío el nombre del archivo
            _socketStreamHandler.SendData(fileNameData);

            long fileSize = fileInfo.Length;
            byte[] fileSizeDataLength = BitConverter.GetBytes(fileSize);
            // 3.- Envío el largo del archivo
            _socketStreamHandler.SendData(fileSizeDataLength);
            // 4.- Envío los datos del archivo
            SendFile(fileSize, path);
        }

        public string ReceiveFile()
        {
            // 1.- Recibir el largo del nombre del archivo
            byte[] fileNameLengthData = _socketStreamHandler.ReceiveData(Specification.FixedFileNameLength);
            int fileNameLength = BitConverter.ToInt32(fileNameLengthData);
            // 2.- Recibir el nombre del archivo
            byte[] fileNameData = _socketStreamHandler.ReceiveData(fileNameLength);
            string fileName = Encoding.UTF8.GetString(fileNameData);

            // 3.- Recibo el largo del archivo
            byte[] fileSizeDataLength = _socketStreamHandler.ReceiveData(Specification.FixedFileSizeLength);
            long fileSize = BitConverter.ToInt64(fileSizeDataLength);
            // 4.- Recibo los datos del archivo
            ReceiveFile(fileSize, fileName);
            return fileName;
        }

        private void SendFile(long fileSize, string path)
        {
            long fileParts = Specification.GetParts(fileSize);
            long offset = 0;
            long currentPart = 1;

            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart != fileParts)
                {
                    data = _fileStreamHandler.ReadData(path, Specification.MaxPacketSize, offset);
                    offset += Specification.MaxPacketSize;
                }
                else
                {
                    int lastPartSize = (int)(fileSize - offset);
                    data = _fileStreamHandler.ReadData(path, lastPartSize, offset);
                    offset += lastPartSize;
                }

                _socketStreamHandler.SendData(data);
                currentPart++;
            }
        }

        private void ReceiveFile(long fileSize, string fileName)
        {
            long fileParts = Specification.GetParts(fileSize);
            long offset = 0;
            long currentPart = 1;
            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart != fileParts)
                {
                    data = _socketStreamHandler.ReceiveData(Specification.MaxPacketSize);
                    offset += Specification.MaxPacketSize;
                }
                else
                {
                    int lastPartSize = (int)(fileSize - offset);
                    data = _socketStreamHandler.ReceiveData(lastPartSize);
                    offset += lastPartSize;
                }
                _fileStreamHandler.WriteData(fileName, data);
                currentPart++;
            }
        }
    }
}