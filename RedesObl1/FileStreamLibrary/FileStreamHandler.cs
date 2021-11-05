using System;
using System.IO;
using System.Threading.Tasks;

namespace FileStreamLibrary
{
    public class FileStreamHandler
    {
        public async Task<byte[]> ReadDataAsync(string path, int length, long position)
        {
            byte[] response = new byte[length];
            using FileStream fileStream = new FileStream(path, FileMode.Open) { Position = position };
            int offset = 0;
            while (offset < length)
            {
                int read = await fileStream.ReadAsync(response, offset, length - offset);
                if (read == 0)
                {
                    throw new Exception("Can not read file");
                }

                offset += read;
            }

            return response;
        }

        public async Task WriteDataAsync(string path, byte[] data)
        {
            if (File.Exists(path))
            {
                await using FileStream fileStream = new FileStream(path, FileMode.Append);
                await fileStream.WriteAsync(data, 0, data.Length);
            }
            else
            {
                await using FileStream fileStream = new FileStream(path, FileMode.Create);
                await fileStream.WriteAsync(data, 0, data.Length);
            }
        }
    }
}