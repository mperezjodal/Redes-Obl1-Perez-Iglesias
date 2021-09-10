using System;

namespace RedesObl1
{
    class SocketUtils
    {
        private const int ProtocolFixedSize = 4;
        static void Listen(Socket clientSocket)
        {
            while (true)
            {
                //1 Creo la parte fija del protocolo
                byte[] dataLength = new byte[ProtocolFixedSize];
                //2 Recibo los datos fijos
                clientSocket.Receive(dataLength);
                //3 Interpreto dichos bytes para obtener cuanto serán los datos variables
                int length = BitConverter.ToInt32(dataLength);
                //4 Creo que el buffer del tamaño exacto de el mensaje que va a venir
                byte[] data = new byte[length];
                //5 Recibo el mensaje (largo variable, que ahora se que es length)
                clientSocket.Receive(data);
                //6 Convierto los datos a string
                string message = Encoding.UTF8.GetString(data);
                //7 Muestro los datos
                Console.WriteLine(message);
            }
        }
        static void SendData(Socket clientSocket)
        {
            int totalDataSent = 0;
            while (totalDataSent < dataLength.Length)
            {
                int sent = clientSocket.Send(dataLength, totalDataSent, dataLength.Length-totalDataSent, SocketFlags.None);
                if (sent == 0)
                {
                    throw new SocketException();
                }
                totalDataSent += sent;
            }
        }
    }
}