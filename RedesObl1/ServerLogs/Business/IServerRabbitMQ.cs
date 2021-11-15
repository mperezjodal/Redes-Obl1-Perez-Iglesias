using System;
using System.Collections.Generic;
using Domain;
using RabbitMQ.Client;

namespace ServerLogs
{
    public class FilterParams
    {
        public string GameTitle { get; set; }
        public string Username { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }

    public interface IServerRabbitMq
    {
        public void ReceiveMessages(IModel channel);
        public void DeclareQueue(IModel channel);
        public List<LogEntry> Log(FilterParams filterParams);
    }
}