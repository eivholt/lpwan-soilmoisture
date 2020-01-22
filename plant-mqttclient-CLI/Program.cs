using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace plant_mqttclient_CLI
{
    class Program
    {
        private static string s_defaultOutputFileName = "output.txt";
        private static readonly HttpClient s_httpClient = new HttpClient();

        public static async Task Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            var consoleLogger = new ConsoleLogger();
            var pasteBinLogger = new PasteBinLogger(s_httpClient);
            FileLogger fileLogger;

            var app = new CommandLineApplication<Program>(throwOnUnexpectedArg: true);

            await consoleLogger.AppendMessageAsync($"Current dir: {Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}.");

            var verboseOption = app.Option("-v|--verbose", "Display operation details"
                , CommandOptionType.NoValue);

            app.Command("subscribe",
                (subscribe) =>
                {
                    var brokerHostNameOption = subscribe.Option("-host|--broker-hostname", "Broker hostname"
                        , CommandOptionType.SingleValue).IsRequired();
                    var applicationIdOption = subscribe.Option("-appid|--applicationid", "TTN Application ID"
                        , CommandOptionType.SingleValue).IsRequired();
                    var applicationAccessKeyOption = subscribe.Option("-appkey|--application-accesskey", "TTN Application Access Key"
                        , CommandOptionType.SingleValue).IsRequired();
                    var outputOption = subscribe.Option<string>("-o|--output", "Output File"
                        , CommandOptionType.SingleValue);


                    subscribe.HelpOption("-? | -h | --help");

                    subscribe.OnExecuteAsync(async (cancellationToken) =>
                    {
                        string outputFile = CreateOutputFilePath(outputOption);

                        fileLogger = new FileLogger { FileName = outputFile };

                        if (verboseOption.HasValue())
                        {
                            if (brokerHostNameOption.HasValue())
                                await consoleLogger.AppendMessageAsync($"Using broker {brokerHostNameOption.Value()}.");

                            if (!string.IsNullOrEmpty(outputFile))
                                await consoleLogger.AppendMessageAsync($"Using output file {outputFile}.");
                        }

                        var brokerClient = new BrokerClient(
                            brokerHostNameOption.Value(),
                            applicationIdOption.Value(),
                            applicationAccessKeyOption.Value(),
                            fileLogger,
                            pasteBinLogger,
                            verbose: verboseOption.HasValue());
                        await brokerClient.Subscribe(token);

                        var startTime = DateTime.Now;

                        while (brokerClient.IsConnected)
                        {
                            var sleepTask = Task.Delay(1000);
                            await sleepTask;
                        }

                        await consoleLogger.AppendMessageAsync($"Ran for {(DateTime.Now - startTime).ToString()}.");
                    });
                }
            );

            app.HelpOption("-h|--help|-?");
            await app.ExecuteAsync(args);
        }

        private static string CreateOutputFilePath(CommandOption<string> outputOption)
        {
            return outputOption.HasValue() ? $"{System.AppDomain.CurrentDomain.BaseDirectory}{outputOption.Value()}" : $"{System.AppDomain.CurrentDomain.BaseDirectory}{s_defaultOutputFileName}";
        }
    }
}