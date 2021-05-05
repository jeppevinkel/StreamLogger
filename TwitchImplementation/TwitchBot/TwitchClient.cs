using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using StreamLogger;
using TwitchImplementation.TwitchBot.Client;
using TwitchImplementation.TwitchBot.Client.Models;

namespace TwitchImplementation.TwitchBot
{
    public class TwitchClient
    {
        private readonly string _ip = "irc.chat.twitch.tv";
        private readonly int _port = 6697;

        private readonly string _username = "justinfan123";
        private readonly string _password = "justinfan123";
        
        private TaskCompletionSource<int> connected = new TaskCompletionSource<int>();

        private TcpClient tcpClient;
        private SslStream sslStream;
        
        private StreamReader streamReader;
        private StreamWriter streamWriter;
        
        public TwitchClient()
        {
            Task.Run(Run);
        }

        public TwitchClient(string username, string password)
        {
            _username = username;
            _password = password;

            Task.Run(Run);
        }

        public async Task JoinChannel(string channel)
        {
            await connected.Task;//Wait to be connected
            await streamWriter.WriteLineAsync($"JOIN #{channel.ToLower()}");
        }

        public async Task JoinChannels(IEnumerable<string> channels)
        {
            await connected.Task;//Wait to be connected
            foreach (string channel in channels)
            {
                await JoinChannel(channel);
            }
        }
        
        public async Task SendMessage(string channel, string message)
        {
            await connected.Task;//Wait to be connected
            await streamWriter.WriteLineAsync($"PRIVMSG #{channel} :{message}");
        }
        
        private async Task Run()
        {
            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(_ip, _port);
            sslStream = new SslStream(
                tcpClient.GetStream(),
                false,
                ValidateServerCertificate,
                null);
            await sslStream.AuthenticateAsClientAsync(_ip);

            streamReader = new StreamReader(sslStream);
            streamWriter = new StreamWriter(sslStream) { NewLine = "\r\n", AutoFlush = true};
        
            await streamWriter.WriteLineAsync($"PASS oauth:{_password}");
            await streamWriter.WriteLineAsync($"NICK {_username}");
            
            await streamWriter.WriteLineAsync($"CAP REQ :twitch.tv/membership twitch.tv/tags twitch.tv/commands");
            
            connected.SetResult(0);
            Log.Info("Connected to Twitch!");
            
            while (true)
            {
                string line = await streamReader.ReadLineAsync();
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                Log.Debug(line);
                        
                string[] split = line.Split(' ');
                //PING :tmi.twitch.tv
                //Respond with PONG :tmi.twitch.tv
                if (line.StartsWith("PING"))
                {
                    await streamWriter.WriteLineAsync($"PONG {split[1]}");
                }

                IrcMessage ircMessage = IrcParser.ParseIrcMessage(line);
                IrcHandler.HandleIrc(ircMessage);
            }
        }
        
        //Outside of start we need to define ValidateServerCertificate
        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None;
        }
    }
}