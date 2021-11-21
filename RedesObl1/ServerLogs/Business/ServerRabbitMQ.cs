using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using Domain;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

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
            ReceiveMessages(channel);
        }

        public void ReceiveMessages(IModel channel)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += ((sender, args) =>
            {
                byte[] data = args.Body.ToArray();
                string message = Encoding.UTF8.GetString(data);
                LogEntry logEntry = LogEntry.Decode(message);
                LogEntries.Add(logEntry);
            });

            channel.BasicConsume(
                queue: SimpleQueue,
                autoAck: true,
                consumer: consumer);
        }
        public List<LogEntry> Log(FilterParams filterParams)
        {
            ConnectionFactory connectionFactory = new ConnectionFactory { HostName = "localhost" };
            using IConnection connection = connectionFactory.CreateConnection();
            using IModel channel = connection.CreateModel();
            ReceiveMessages(channel);

            List<LogEntry> filteredLogEntries = new List<LogEntry>();
            foreach (LogEntry logEntry in LogEntries)
            {
                Console.WriteLine(logEntry.Game.Title);
                if (filterParams.DateFrom.ToString() != "01/01/0001 00:00:00" && logEntry.Date < filterParams.DateFrom)
                {
                    continue;
                }
                if (filterParams.DateTo.ToString() != "01/01/0001 00:00:00" && logEntry.Date > filterParams.DateTo)
                {
                    continue;
                }
                if (logEntry.User != null && filterParams.Username != "" && logEntry.User.Name != filterParams.Username)
                {
                    continue;
                }
                if (logEntry.Game != null && filterParams.GameTitle != "" && logEntry.Game.Title != filterParams.GameTitle)
                {
                    continue;
                }
                
                filteredLogEntries.Add(logEntry);
            }
            return filteredLogEntries;
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