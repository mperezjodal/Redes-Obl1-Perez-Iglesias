using System;
using System.Text.Json;

namespace Domain
{
    public class LogEntry
    {
        public Game Game { get; set; }
        public User User { get; set; }
        public DateTime Date { get; set; }
        public string Action { get; set; }

        public string Encode()
        {
            return JsonSerializer.Serialize(this);
        }

        public static LogEntry Decode(string jsonString)
        {
            return JsonSerializer.Deserialize<LogEntry>(jsonString);
        }
    }
}
