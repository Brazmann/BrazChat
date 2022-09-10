using Brazchat;
using Grpc.Core;

namespace BrazChatClient
{
    class Program
    {
        public static string username = "";
        public static string host = "";
        public static void Main(string[] args)
        {
            StartScreen();
        }

        public static void StartScreen()
        {
            Console.Clear();
            Console.WriteLine("Please enter a hostname (ip:port)");
            host = Console.ReadLine();
            Console.WriteLine(host);
            try
            {
                Channel channel = new Channel(host, ChannelCredentials.Insecure);

                var client = new Messaging.MessagingClient(channel);

                var reply = client.Greeting(new GreetingRequest { });

                channel.ShutdownAsync().Wait();
                WelcomeScreen();
            }
            catch (RpcException)
            {
                Console.WriteLine("Invalid host! Press any key to try again.");
                Console.ReadKey();
                Console.Clear();
                StartScreen();
            }
        }
        public static void WelcomeScreen()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Clear();
            string[] WelcomeMessage = GetWelcomeMessage();
            Console.WriteLine(WelcomeMessage[0]);
            Console.WriteLine(WelcomeMessage[1]);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Type in a username please!");
            username = Console.ReadLine();
            Console.WriteLine("Press any key to start sending messages!");
            Console.ReadKey();
            ChatScreen();
        }
        public static void ChatScreen()
        {
            Console.Clear();
            while (!Console.KeyAvailable)
            {
                Console.CursorVisible = false;
                CustomClear();
                Console.SetCursorPosition(0, 0);
                var Messages = GetMessages();
                if (Messages.Confirmation == "Nope!")
                {
                    ConnectionFailureScreen();
                }
                else
                {
                    Console.WriteLine($"{Messages.Messages}\n");
                    //Console.WriteLine("----------------------");
                    Console.WriteLine("T - Type message \nL - View logs");
                    Thread.Sleep(100);
                }
            }
                ConsoleKey input = Console.ReadKey(true).Key;
                switch (input)
                {
                    case ConsoleKey.T:
                        TypeScreen();
                        break;
                    case ConsoleKey.L:
                        ChatLogScreen();
                        break;
                    default:
                        ChatScreen();
                        break;
                }
        }

        public static void TypeScreen()
        {
            Console.Clear();
            var Messages = GetMessages();
            if (Messages.Confirmation == "Nope!")
            {
                ConnectionFailureScreen();
            }
            else
            {
                Console.WriteLine(Messages.Messages);
                Console.WriteLine("Type your message!");
                string? message = Console.ReadLine();
                SendMessage(message);
                ChatScreen();
            }
        }

        public static void ChatLogScreen()
        {
            Console.Clear();
            var Messages = GetMessages();
            if(GetMessages().Confirmation == "Nope!")
            {
                Console.WriteLine("Connection to server lost! Press any key to return to connection menu!");
                ConnectionFailureScreen();
            }else{
            Console.WriteLine("--------------------------");
            Console.WriteLine(GetMessageLogs().Log);
            Console.WriteLine("--------------------------");
            Console.ReadKey();
            Console.Clear();
            ChatScreen();
            }
        }

        public static void ConnectionFailureScreen()
        {
            Console.Clear();
            Console.WriteLine("Connection to server lost! Press any key to return to connection menu!");
            Console.ReadKey();
            StartScreen();
        }
        public static string[] GetWelcomeMessage()
        {
            try
            {
                Channel channel = new Channel(host, ChannelCredentials.Insecure);

                var client = new Messaging.MessagingClient(channel);

                var reply = client.WelcomeMessage(new WelcomeMessageRequest { });

                string[] WelcomeMessage = { reply.WelcomeLogo, reply.MOTD };

                channel.ShutdownAsync().Wait();
                return WelcomeMessage;
            }
            catch (RpcException)
            {
                string[] Fallback = { "Brazchat", "ERRORNOCONNECTION" }; //0 = Logo, 1 = MOTD
                return Fallback;
            }
        }
        public static void SendMessage(string? message)
        {
            try
            {
                Channel channel = new Channel(host, ChannelCredentials.Insecure);

                var client = new Messaging.MessagingClient(channel);

                var reply = client.SendMessage(new SendMessageRequest { ClientUsername = username, Message = message, Color = "Red" });
                Console.WriteLine("Message sent. Result:" + reply.Confirmation);

                channel.ShutdownAsync().Wait();
            }
            catch (Grpc.Core.RpcException)
            {
                ConnectionFailureScreen();
            }
        }

        public static GetMessagesReply GetMessages()
        {
            try
            {
                Channel channel = new Channel(host, ChannelCredentials.Insecure);

                var client = new Messaging.MessagingClient(channel);
                String clientUsername = "Brazman";

                var reply = client.GetMessages(new GetMessagesRequest { ClientUsername = clientUsername });
                channel.ShutdownAsync().Wait();
                return reply;
            }
            catch (Grpc.Core.RpcException)
            {
                //lord forgive me
                var Fallback = new GetMessagesReply { Confirmation = "Nope!", Messages = "No connection! Retrying!\n" };
                return Fallback;
            }
        }
        public static GetMessageLogsReply GetMessageLogs()
        {
            Channel channel = new Channel(host, ChannelCredentials.Insecure);

            var client = new Messaging.MessagingClient(channel);

            var reply = client.GetMessageLogs(new GetMessageLogsRequest { });
            channel.ShutdownAsync().Wait();
            return reply;
        }

        public static void CustomClear()
        {
            Console.SetCursorPosition(0, 0);
            for (int y = 0; y < Console.WindowHeight; y++)
                Console.Write(new String(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, 0);
        }

    }
}