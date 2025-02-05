using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using System.Threading;

namespace DriveTogetherBot
{
    public sealed class TelegramHandler 
    {
        TelegramBotClient _botClient;
        CancellationTokenSource _cts;
        bool _onShutdownRegistered = false;

        private TelegramHandler()
        {
            _cts = new();
            _botClient = new TelegramBotClient(GetTelegramToken(), cancellationToken: _cts.Token);
        }

        private static Lazy<TelegramHandler> _lazyTelegramHandler = new Lazy<TelegramHandler>(() => new TelegramHandler());

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
            await Task.CompletedTask;

            // switch (update.Type)
            // {
            //     case UpdateType.Message:

            //         if (update.Message is not { } message)
            //             return;
            //         AntispamProcessor antispamProcessor = new AntispamProcessor(_botClient, message, _cts.Token);
            //         await antispamProcessor.Process();
            //         break;

            //     //case UpdateType.MessageReaction:
            //     //    Console.WriteLine("Добавлена реакция на сообщение");
            //     //    break;
            // }
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