using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace plant_mqttclient_CLI
{
    public class FileLogger : IMessageAppender
    {
        public string FileName { get; set; }

        public async Task AppendMessageAsync(string text)
        {
            byte[] encodedText = Encoding.Unicode.GetBytes(text + Environment.NewLine);

            using (FileStream sourceStream = new FileStream(FileName,
                FileMode.Append, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            };
        }
    }
}
