using System.Net.Sockets;
using System.Threading.Tasks;

namespace FileStreamLibrary
{
    public class NetworkStreamHandler
    {
        private readonly NetworkStream _networkStream;

        public NetworkStreamHandler(NetworkStream networkStream)
        {
            _networkStream = networkStream;
        }

        public async Task<byte[]> ReadDataAsync(int length)
        {
            int offset = 0;
            byte[] response = new byte[length];
            while (offset < length)
            {
                int received = await _networkStream.ReadAsync(
                    response,
                    offset,
                    length - offset);
                if (received == 0)
                {
                    throw new SocketException();
                }

                offset += received;
            }
            return response;
        }

        public async Task SendDataAsync(byte[] data)
        {
            await _networkStream.WriteAsync(data, 0, data.Length);
        }
    }
}