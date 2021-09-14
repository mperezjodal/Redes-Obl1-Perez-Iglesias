using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Domain;
using SocketUtils;

namespace ClientSocket
{
    public class Client
    {
        private const string ServerIpAddress = "127.0.0.1";
        private const int ServerPort = 6000;
        private const string ClientIpAddress = "127.0.0.1";
        private const int ClientPort = 0;
        
        static void Main(string[] args)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Parse(ClientIpAddress), ClientPort);
            
            clientSocket.Bind(clientEndPoint);
            Console.WriteLine("Conectando al servidor...");
            
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(ServerIpAddress), ServerPort);
            clientSocket.Connect(serverEndPoint);
            var connected = true;
            Console.WriteLine("Conectado al servidor.");

            
            while (connected)
            {
                Display.ClientMenu();
                var option = Console.ReadLine();
                switch (option)
                {
                    case "exit":
                        clientSocket.Shutdown(SocketShutdown.Both); //apagamos el server
                        clientSocket.Close();
                        connected = false;
                        break;
                    case "1": //publicar
                        Game gameToPublish = Display.InputGame();

                        var message = gameToPublish.Encode();
                        var header = new Header(HeaderConstants.Request, CommandConstants.PublishGame, message.Length);
                        var data = header.GetRequest();
                        Utils.Send(clientSocket, data, message);
                        
                        Console.WriteLine(Utils.ReciveMessageData(clientSocket));
                        break;
                    case "2": //modificar   
                    case "3": //eliminar
                    case "4": //buscar
                    case "5": //calificar
                        // Utils.SendCommand(clientSocket, Int32.Parse(option));
                        break;
                    default:
                        Console.WriteLine("option invalida");
                        break;
                }
            }

            // new Thread(() => Utils.ReciveData(clientSocket)).Start();
            // Utils.SendData(clientSocket);
        }

        
    }
}