using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DriveTogetherBot;

public class MessageProcessor
{
    private TelegramBotClient _botClient;    
    private readonly Dictionary<long, User> _userStates = new();
    private Message _message;
    private CancellationToken _token;

    public MessageProcessor(TelegramBotClient botClient, Message message, CancellationToken token)
    {
        _botClient = botClient;
        _message = message;
        _token = token;
    }

    internal async Task Process()
    {
        if (_message?.From?.Id is not {} UserId)
            return;
        
        if (_message.Text is not { } messageText)
            return;

        if (Common.Users.TryGetValue(UserId, out User user))
        {
            await ProcessFormStep(messageText, user);
            return;
        }
        else if (messageText.StartsWith('/'))
           await OnCommand(messageText, _message, UserId);
    }

    private async Task ProcessFormStep(string input, User user)
    {
        switch (user.CurrentStep)
        {
            case FormStep.Name:
                user.Name = input;
                user.CurrentStep = FormStep.Phone;
                await _botClient.SendMessage(
                    _message.Chat,
                    text: "Здорово! Теперь введите ваш номер телефона:");
                break;
                
            case FormStep.Phone:
                user.Phone = input;
                user.CurrentStep = FormStep.Complete;
                await ProcessCompletedForm(user);
                break;
        }
    }

    private async Task ProcessCompletedForm(User user)
    {
        await _botClient.SendMessage(
            _message.Chat,
            text: $"Спасибо! Вот что вы ввели:\n" +
                  $"Имя: {user.Name}\n" +
                  $"Телефон: {user.Phone}");
    }

    private async Task OnCommand(string messageText, Message message, long UserId)
    {
        if (messageText == "/register")
        {
            User user = new User();
            user.CurrentStep = FormStep.Name;
            user.Id = UserId;
            user.Username = message.From.Username;

            Common.Users.TryAdd(UserId, user);
            
            await _botClient.SendMessage(
                _message.Chat,
                text: "Введите ваше имя:",
                cancellationToken: _token);
        }
        else
        {
            await Task.CompletedTask;
        }
    }
}
