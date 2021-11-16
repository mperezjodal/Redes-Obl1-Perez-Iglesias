using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FileStreamLibrary.Protocol;
using Google.Protobuf;
using GRPCLibrary;

namespace FileStreamLibrary
{
    public class FileCommunicationHandler
    {
        private readonly FileStreamHandler _fileStreamHandler;
        private readonly NetworkStreamHandler _networkStreamHandler;

        public FileCommunicationHandler(NetworkStream networkStream)
        {
            _networkStreamHandler = new NetworkStreamHandler(networkStream);
            _fileStreamHandler = new FileStreamHandler();
        }

        public async Task SendFileAsync(string path)
        {
            var fileInfo = new FileInfo(path);
            string fileName = fileInfo.Name;
            byte[] fileNameData = Encoding.UTF8.GetBytes(fileName);
            int fileNameLength = fileNameData.Length;
            byte[] fileNameLengthData = BitConverter.GetBytes(fileNameLength);

            await _networkStreamHandler.SendDataAsync(fileNameLengthData);
            await _networkStreamHandler.SendDataAsync(fileNameData);

            long fileSize = fileInfo.Length;
            byte[] fileSizeDataLength = BitConverter.GetBytes(fileSize);

            await _networkStreamHandler.SendDataAsync(fileSizeDataLength);

            await SendFileAsync(fileSize, path);
        }

        public async Task TransferFileAsync(GameSystemModel.GameSystemModelClient grpcClient)
        {
            byte[] fileNameLengthData = await _networkStreamHandler.ReadDataAsync(Specification.FixedFileNameLength);
            int fileNameLength = BitConverter.ToInt32(fileNameLengthData);

            byte[] fileNameData = await _networkStreamHandler.ReadDataAsync(fileNameLength);
            string fileName = Encoding.UTF8.GetString(fileNameData);

            byte[] fileSizeDataLength = await _networkStreamHandler.ReadDataAsync(Specification.FixedFileSizeLength);
            long fileSize = BitConverter.ToInt64(fileSizeDataLength);

            await TransferFileAsync(grpcClient, fileSize, fileName);
        }

        public async Task ReceiveFileAsync()
        {
            byte[] fileNameLengthData = await _networkStreamHandler.ReadDataAsync(Specification.FixedFileNameLength);
            int fileNameLength = BitConverter.ToInt32(fileNameLengthData);

            byte[] fileNameData = await _networkStreamHandler.ReadDataAsync(fileNameLength);
            string fileName = Encoding.UTF8.GetString(fileNameData);

            byte[] fileSizeDataLength = await _networkStreamHandler.ReadDataAsync(Specification.FixedFileSizeLength);
            long fileSize = BitConverter.ToInt64(fileSizeDataLength);


            await ReceiveFileAsync(fileSize, fileName);
        }

        public static async Task GrpcSendFileAsync(GameSystemModel.GameSystemModelClient grpcClient, string path)
        {
            var _fileStreamHandler = new FileStreamHandler();

            var fileInfo = new FileInfo(path);
            long fileLength = fileInfo.Length;
            byte[] fileSizeDataLength = BitConverter.GetBytes(fileLength);
            long fileSize = BitConverter.ToInt64(fileSizeDataLength);

            string fileName = path.Split("/").Last();

            long fileParts = Specification.GetParts(fileSize);
            long offset = 0;
            long currentPart = 1;

            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart != fileParts)
                {
                    data = await _fileStreamHandler.ReadDataAsync(path, Specification.MaxPacketSize, offset);
                    offset += Specification.MaxPacketSize;
                }
                else
                {
                    int lastPartSize = (int)(fileSize - offset);
                    data = await _fileStreamHandler.ReadDataAsync(path, lastPartSize, offset);
                    offset += lastPartSize;
                }

                await grpcClient.PostCoverAsync(new CoverModel() { FileName = fileName, Data = ByteString.CopyFrom(data) });
                currentPart++;
            }
        }

        public async static Task<byte[]> GetFilePartBytes(string path, long part, long offset)
        {
            var _fileStreamHandler = new FileStreamHandler();
            var fileInfo = new FileInfo(path);
            long fileLength = fileInfo.Length;
            byte[] fileSizeDataLength = BitConverter.GetBytes(fileLength);
            long fileSize = BitConverter.ToInt64(fileSizeDataLength);

            string fileName = path.Split("/").Last();

            long fileParts = Specification.GetParts(fileSize);


            byte[] data;
            if (part != fileParts)
            {
                data = await _fileStreamHandler.ReadDataAsync(path, Specification.MaxPacketSize, offset);
            }
            else
            {
                int lastPartSize = (int)(fileSize - offset);
                data = await _fileStreamHandler.ReadDataAsync(path, lastPartSize, offset);
            }

            return data;
        }

        private async Task SendFileAsync(long fileSize, string path)
        {
            long fileParts = Specification.GetParts(fileSize);
            long offset = 0;
            long currentPart = 1;

            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart != fileParts)
                {
                    data = await _fileStreamHandler.ReadDataAsync(path, Specification.MaxPacketSize, offset);
                    offset += Specification.MaxPacketSize;
                }
                else
                {
                    int lastPartSize = (int)(fileSize - offset);
                    
                    data = await _fileStreamHandler.ReadDataAsync(path, lastPartSize, offset);
                    offset += lastPartSize;
                }

                await _networkStreamHandler.SendDataAsync(data);
                currentPart++;
            }
        }

        private async Task TransferFileAsync(GameSystemModel.GameSystemModelClient grpcClient, long fileSize, string fileName)
        {
            long fileParts = Specification.GetParts(fileSize);
            long offset = 0;
            long currentPart = 1;
            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart != fileParts)
                {
                    data = await _networkStreamHandler.ReadDataAsync(Specification.MaxPacketSize);
                    offset += Specification.MaxPacketSize;
                }
                else
                {
                    int lastPartSize = (int)(fileSize - offset);
                    data = await _networkStreamHandler.ReadDataAsync(lastPartSize);
                    offset += lastPartSize;
                }
                await grpcClient.PostCoverAsync(new CoverModel() { FileName = fileName, Data = ByteString.CopyFrom(data) });
                currentPart++;
            }
        }

        private async Task ReceiveFileAsync(long fileSize, string fileName)
        {
            long fileParts = Specification.GetParts(fileSize);
            long offset = 0;
            long currentPart = 1;
            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart != fileParts)
                {
                    data = await _networkStreamHandler.ReadDataAsync(Specification.MaxPacketSize);
                    offset += Specification.MaxPacketSize;
                }
                else
                {
                    int lastPartSize = (int)(fileSize - offset);
                    data = await _networkStreamHandler.ReadDataAsync(lastPartSize);
                    offset += lastPartSize;
                }
                await _fileStreamHandler.WriteDataAsync(fileName, data);
                currentPart++;
            }
        }
    }
}