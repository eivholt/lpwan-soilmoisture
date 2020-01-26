using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace plant_mqttclient_CLI
{
    public class BrokerClient
    {
        private IMqttClient m_mqttClientSubscriber;
        private readonly string m_brokerHostName;
        private readonly string m_applicationId;
        private readonly string m_applicationAccessKey;
        private bool m_verbose;
        private readonly IMessageAppender m_logger;
        private readonly IMessageAppender m_consoleLogger;
        private readonly IMessageAppender m_pasteBinLogger;

        public BrokerClient(
            string brokerHostName, 
            string applicationId, 
            string applicationAccessKey, 
            IMessageAppender fileLogger, 
            IMessageAppender pasteBinLogger,
            bool verbose = false)
        {
            m_brokerHostName = brokerHostName;
            m_applicationId = applicationId;
            m_applicationAccessKey = applicationAccessKey;
            m_logger = fileLogger;
            m_pasteBinLogger = pasteBinLogger;
            m_verbose = verbose;
            m_consoleLogger = new ConsoleLogger();
        }

        public async Task Subscribe(CancellationToken cancellationToken)
        {
            var mqttFactory = new MqttFactory();

            var clientOptionsBuilder = new MqttClientOptionsBuilder()
                .WithClientId(System.Reflection.Assembly.GetEntryAssembly().FullName)
                .WithTcpServer(m_brokerHostName)
                .WithCredentials(m_applicationId, m_applicationAccessKey)
                .Build();

            m_mqttClientSubscriber = mqttFactory.CreateMqttClient();
            m_mqttClientSubscriber.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnSubscriberConnected);
            m_mqttClientSubscriber.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnSubscriberDisconnected);
            m_mqttClientSubscriber.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(OnSubscriberMessageReceived);

            await m_mqttClientSubscriber.ConnectAsync(clientOptionsBuilder, cancellationToken);
            await m_consoleLogger.AppendMessageAsync($"ConnectAsync IsConnected: {m_mqttClientSubscriber.IsConnected}");
        }

        private async Task OnSubscriberMessageReceived(MqttApplicationMessageReceivedEventArgs arg)
        {
            var item = $"Timestamp: {DateTime.Now:O} | Topic: {arg.ApplicationMessage.Topic} | Payload: {arg.ApplicationMessage.ConvertPayloadToString()} | QoS: {arg.ApplicationMessage.QualityOfServiceLevel}";

            var loggingStart = DateTime.Now;
            

            var consoleTask = m_consoleLogger.AppendMessageAsync(item);
            var fileTask = m_logger?.AppendMessageAsync(item);
            var pastebinTask = m_pasteBinLogger?.AppendMessageAsync(arg.ApplicationMessage.ConvertPayloadToString());

            await Task.WhenAll(consoleTask, fileTask, pastebinTask);

            await m_consoleLogger.AppendMessageAsync($"Logging completed in {(DateTime.Now - loggingStart).ToString()}");
        }

        private async Task OnSubscriberConnected(MqttClientConnectedEventArgs arg)
        {
            if (m_verbose) 
            {
                await m_consoleLogger.AppendMessageAsync("Subscriber Connected");
                //ConsoleLogger.AppendText(JsonConvert.SerializeObject(arg));
            }
            await m_mqttClientSubscriber.SubscribeAsync(new TopicFilterBuilder().WithTopic("#").Build());
        }

        private async Task OnSubscriberDisconnected(MqttClientDisconnectedEventArgs arg)
        {
            await m_consoleLogger.AppendMessageAsync($"Subscriber Disconnected: {JsonConvert.SerializeObject(arg)}");
        }

        public bool IsConnected { get { return m_mqttClientSubscriber.IsConnected; } }
    }
}
