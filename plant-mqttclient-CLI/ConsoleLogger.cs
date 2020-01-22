using System;
using System.Threading;
using System.Threading.Tasks;

namespace plant_mqttclient_CLI
{
    public class ConsoleLogger : IMessageAppender
    {
        public async Task AppendMessageAsync(string text)
        {
             await Console.Out.WriteLineAsync(text);
        }
    }
}