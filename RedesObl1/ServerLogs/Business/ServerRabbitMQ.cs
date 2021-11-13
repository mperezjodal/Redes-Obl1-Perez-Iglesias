using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using Domain;
using RabbitMQ.Client.Events;

namespace ServerLogs
{
    class ServerRabbitMq : IServerRabbitMq
    {
        private const string SimpleQueue = "m6bBasicQueue";
        public static List<LogEntry> LogEntries;
        public ServerRabbitMq()
        {
            LogEntries = new List<LogEntry>();
            ConnectionFactory connectionFactory = new ConnectionFactory { HostName = "localhost" };
            using IConnection connection = connectionFactory.CreateConnection();
            using IModel channel = connection.CreateModel();

            DeclareQueue(channel);
            ReciveMessages(channel);
        }

        public void ReciveMessages(IModel channel)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += ((sender, args) =>
            {
                byte[] data = args.Body.ToArray();
                string message = Encoding.UTF8.GetString(data);
                Console.WriteLine((message));
                LogEntry logEntry = LogEntry.Decode(message);
                LogEntries.Add(logEntry);
            });

            channel.BasicConsume(
                queue: SimpleQueue,
                autoAck: true,
                consumer: consumer);
        }

        public List<LogEntry> Log()
        {
            foreach (var entry in LogEntries)
            {
                Console.WriteLine(entry.Action);
            }
            Console.WriteLine("3");
            return LogEntries;
            Console.WriteLine("4");
        }

        public void DeclareQueue(IModel channel)
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