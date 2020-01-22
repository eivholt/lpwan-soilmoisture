using System.Threading.Tasks;

namespace plant_mqttclient_CLI
{
    public interface IMessageAppender
    {
        Task AppendMessageAsync(string text);
    }
}