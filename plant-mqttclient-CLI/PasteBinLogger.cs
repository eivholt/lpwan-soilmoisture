using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace plant_mqttclient_CLI
{
    public class PasteBinLogger : IMessageAppender
    {
        private HttpClient m_httpClient;

        public PasteBinLogger(HttpClient httpClient)
        {
            m_httpClient = httpClient;
        }

        public async Task AppendMessageAsync(string text)
        {
            m_httpClient.DefaultRequestHeaders.Accept.Clear();
            m_httpClient.DefaultRequestHeaders.Clear();
            m_httpClient.DefaultRequestHeaders.Add("User-Agent", "plant-mqttclient-CLI");

            var stringTask = m_httpClient.PostAsync("https://enpisuapwklg.x.pipedream.net/", 
                new StringContent(JsonConvert.SerializeObject(text), Encoding.UTF8, "application/json"));

            var msg = await stringTask;
            Console.Write(msg);
        }
    }
}
