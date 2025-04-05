using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using System.Threading;
using Telegram.Bot.Types.ReplyMarkups;

namespace DriveTogetherBot
{
    public sealed class TelegramHandler 
    {        
        private static Lazy<TelegramHandler> _lazyTelegramHandler = new Lazy<TelegramHandler>(() => new TelegramHandler());
        private static IConfiguration _configuration;

        TelegramBotClient _botClient;
        CancellationTokenSource _cts;
        bool _onShutdownRegistered = false;

        private TelegramHandler()
        {
            if (_configuration == null)
            {
                throw new InvalidOperationException("Configuration must be initialized first. Call Initialize() before getting an instance.");
            }

            _cts = new();
            _botClient = new TelegramBotClient(GetTelegramToken(), cancellationToken: _cts.Token);
        }

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public static TelegramHandler GetInstance(CancellationToken cancellationToken)
        {
            var handler = _lazyTelegramHandler.Value;

            if (!handler._onShutdownRegistered && cancellationToken != CancellationToken.None)
            {
                cancellationToken.Register(handler.OnShutdown);
                handler._onShutdownRegistered = true;
            }

            return handler;
        }

        public void Start()
        {
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Enum.GetValues<UpdateType>(), //лучше подписаться только на нужные события, а не так как сейчас сделано
                DropPendingUpdates = true
            };

            _botClient.OnUpdate += OnUpdate;
            Console.WriteLine("Bot is running. V:1.0.0");
        }

        private async Task OnUpdate(Update update)
        {
            Console.WriteLine("OnUpdate fired!");

             switch (update.Type)
             {
                 case UpdateType.Message:
                     if (update.Message is not { } message)
                         return;
                     MessageProcessor antispamProcessor = new MessageProcessor(_botClient, message, _cts.Token, _configuration);
                     await antispamProcessor.Process();
                     break;

                default:
                    await Task.CompletedTask;
                    break;
            }            
        }

        private string GetTelegramToken()
        {
            string token = Environment.GetEnvironmentVariable("TelegramToken", EnvironmentVariableTarget.Machine);

            if (token == null)
            {
                token = Environment.GetEnvironmentVariable("TelegramToken", EnvironmentVariableTarget.User);
            }

            if (token == null)
            {
                token = Environment.GetEnvironmentVariable("TelegramToken");
            }

            if (token == null)
            {
                throw new Exception("TelegramToken not found");
            }

            return token;
        }


        private void OnShutdown()
        {
            _cts.Cancel();
            Console.WriteLine("Bot stopped.");
        }
    }
}