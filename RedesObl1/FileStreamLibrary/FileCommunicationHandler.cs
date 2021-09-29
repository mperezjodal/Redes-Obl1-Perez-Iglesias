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

            _socketStreamHandler.SendData(fileNameLengthData);

            _socketStreamHandler.SendData(fileNameData);

            long fileSize = fileInfo.Length;
            byte[] fileSizeDataLength = BitConverter.GetBytes(fileSize);

            _socketStreamHandler.SendData(fileSizeDataLength);

            SendFile(fileSize, path);
        }

        public string ReceiveFile()
        {

            byte[] fileNameLengthData = _socketStreamHandler.ReceiveData(Specification.FixedFileNameLength);
            int fileNameLength = BitConverter.ToInt32(fileNameLengthData);

            byte[] fileNameData = _socketStreamHandler.ReceiveData(fileNameLength);
            string fileName = Encoding.UTF8.GetString(fileNameData);


            byte[] fileSizeDataLength = _socketStreamHandler.ReceiveData(Specification.FixedFileSizeLength);
            long fileSize = BitConverter.ToInt64(fileSizeDataLength);

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