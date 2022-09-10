using System;
using System.IO;
using Grpc;
using Grpc.Core;
using Brazchat;
using Google.Protobuf;
using Newtonsoft.Json;

namespace BrazChatServer
{
    class MessagingImpl : Messaging.MessagingBase
    {

        public override Task<SendMessageReply> SendMessage(SendMessageRequest request, ServerCallContext context)
        {
            string username = request.ClientUsername;
            if(request.ClientUsername == null)
            {
                username = "Guest";
            }
            Console.WriteLine($"Message received: '{request.Message}' from {username}");
            Utilities.LogNewMessage(request.Message, username);
            return Task.FromResult(new SendMessageReply { Confirmation = "Successfully sent message to target!" });
        }
        public override Task<GetMessageLogsReply> GetMessageLogs(GetMessageLogsRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GetMessageLogsReply { Log = File.ReadAllText(@$"{Program.brazChatPath}\log\ChatLog.txt")});
        }
        public override Task<GetMessagesReply> GetMessages(GetMessagesRequest request, ServerCallContext context)
        {
            List<string> text = File.ReadLines(@$"{Program.brazChatPath}\log\ChatLog.txt").Reverse().Take(20).Reverse().ToList();
            string messages = string.Join("\n", text);
            return Task.FromResult(new GetMessagesReply { Confirmation = "Successful!", Messages = messages});
        }
        public override Task<WelcomeMessageReply> WelcomeMessage(WelcomeMessageRequest request, ServerCallContext context)
        {
            return Task.FromResult(new WelcomeMessageReply {WelcomeLogo = Figgle.FiggleFonts.Banner3D.Render("BrazChat") , MOTD = File.ReadAllText(@$"{Program.brazChatPath}\config\MOTD.txt") }); //Make this read the logo from a file in the future.
        }
        public override Task<GreetingReply> Greeting(GreetingRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GreetingReply { });
        }

    }
    class Program
    {
        public static string brazChatPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        const int Port = 50051;
        public static void Main(string[] args)
        {
            CheckNecessaryFiles();
            List<Config> configItems = new List<Config>();
            using (StreamReader r = new StreamReader(@$"{Program.brazChatPath}\config\config.json"))
            {
                string json = r.ReadToEnd();
                configItems = JsonConvert.DeserializeObject<List<Config>>(json);
            }
            Server server = new Server
            {
                Services = { Messaging.BindService(new MessagingImpl()) },
                Ports = { new ServerPort(configItems[0].Host, configItems[0].Port, ServerCredentials.Insecure)}
            };
            server.Start();
            Console.WriteLine("Server running. Press any key to stop server.");
            Console.ReadKey();
        }
        public static void CheckNecessaryFiles()
        {
            if (File.Exists(@$"{Program.brazChatPath}\config\MOTD.txt") == false)
            {
                Directory.CreateDirectory($@"{Program.brazChatPath}\config");
                File.WriteAllText(@$"{Program.brazChatPath}\config\MOTD.txt", "Default MOTD");
            }
            if (File.Exists(@$"{Program.brazChatPath}\config\config.json") == false)
            {
                Console.WriteLine("No config file found! Exiting.");
                Environment.Exit(2);
            }
            if (File.Exists(@$"{Program.brazChatPath}\log\ChatLog.txt") == false)
            {
                Directory.CreateDirectory($@"{Program.brazChatPath}\log");
                File.WriteAllText(@$"{Program.brazChatPath}\log\ChatLog.txt", "Say something!\n");
            }
        }
    }
    class Utilities
    {
        public static void LogNewMessage(string message, string username)
        {
            string path = @$"{Program.brazChatPath}\log\ChatLog.txt";

            File.AppendAllText(path, $"{username}: {message}\n");
        }
    }

    public class Config
    {
        public string Host;
        public int Port;
    }
}


