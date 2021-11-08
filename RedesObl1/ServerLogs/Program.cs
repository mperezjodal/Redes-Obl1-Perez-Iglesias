using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Channels;
using Domain;
using RabbitMQ.Client.Events;

namespace ServerLogs
{
    class Program
    {
        private const string SimpleQueue = "m6bBasicQueue";
        static void Main(string[] args)
        {
            ConnectionFactory connectionFactory = new ConnectionFactory { HostName = "localhost" };
            using IConnection connection = connectionFactory.CreateConnection();
            using IModel channel = connection.CreateModel();

            DeclareQueue(channel);
            ReciveMessages(channel);
            Console.ReadLine();
        }

        private static void ReciveMessages(IModel channel)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += ((sender, args) =>
            {
                byte[] data = args.Body.ToArray();
                string message = Encoding.UTF8.GetString(data);
                Console.WriteLine((message));
                LogEntry logEntry = LogEntry.Decode(message);
                Console.WriteLine(logEntry);
            });

            channel.BasicConsume(
                queue: SimpleQueue,
                autoAck: true,
                consumer: consumer);
        }

        private static void DeclareQueue(IModel channel)
        {
            channel.QueueDeclare(
                queue: SimpleQueue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }
    }
}
