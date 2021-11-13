using System.Collections.Generic;
using Domain;
using RabbitMQ.Client;

namespace ServerLogs
{
    public interface IServerRabbitMq
    {
        public void ReciveMessages(IModel channel);
        public void DeclareQueue(IModel channel);
        public List<LogEntry> Log();
    }
}