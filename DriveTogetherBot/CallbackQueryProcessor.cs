using System;
using System.Text;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Net;
using DriveTogetherBot.Entities;
using User = DriveTogetherBot.Entities.User;

namespace DriveTogetherBot;

public class CallbackQueryProcessor
{
    private IConfiguration _configuration;
    private TelegramBotClient _botClient;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<long, User> _userStates = new();
    private CallbackQuery _callbackQuery;
    private CancellationToken _token;
    private User CurrentUser { get; set; }

    public CallbackQueryProcessor(TelegramBotClient botClient, CallbackQuery callbackQuery , CancellationToken token, IConfiguration configuration)
    {
        _configuration = configuration;
        _botClient = botClient;
        _httpClient = new HttpClient();
        _callbackQuery = callbackQuery;
        _token = token;
    }

    internal async Task Process()
    {
        if (_callbackQuery?.From?.Id is not { } UserId)
            return;

        if (_callbackQuery.Data.StartsWith("datepick_"))
        {
            await HandleDateSelectionAsync(_callbackQuery);
        }
        else if (_callbackQuery.Data.StartsWith("timepick_"))
        {
            await HandleTimeSelectionAsync(_callbackQuery);
        }
        else if (_callbackQuery.Data.StartsWith("startlocationpick_"))
        {
            await HandleStartLocationSelectionAsync(_callbackQuery);
        }
        else if (_callbackQuery.Data.StartsWith("endlocationpick_"))
        {
            await HandleEndLocationSelectionAsync(_callbackQuery);
        }
    }

    public async Task HandleDateSelectionAsync(CallbackQuery callbackQuery)
    {
        var selectedDateStr = callbackQuery.Data.Split('_')[1];
        var selectedDate = DateTime.Parse(selectedDateStr);
        
        await SendTimeSelectionAsync(callbackQuery.Message.Chat.Id, selectedDate);    
    }

    public async Task HandleStartLocationSelectionAsync(CallbackQuery callbackQuery)
    {
        var selectedStartLocationId = Convert.ToInt64(callbackQuery.Data.Split('_')[1]);   

        await SendEndLocationSelectionAsync(callbackQuery.Message.Chat.Id, selectedStartLocationId);    
    }
    
    public async Task HandleEndLocationSelectionAsync(CallbackQuery callbackQuery)
    {
        var selectedStartLocationId = Convert.ToInt64(callbackQuery.Data.Split('_')[1]);   
        var selectedEndLocationId = Convert.ToInt64(callbackQuery.Data.Split('_')[2]);   

        var userId = callbackQuery?.From?.Id;

        if (Common.Users.TryGetValue(userId.Value, out User user))
        {
            user.TripOffer.StartLocationId = selectedStartLocationId;
            user.TripOffer.EndLocationId = selectedEndLocationId;
            user.TripStepEnum = TripStep.DepartureDate;
            await SendDateSelectionAsync(callbackQuery.Message.Chat.Id);
        }   
    }

    public async Task SendDateSelectionAsync(long chatId)
    {
        var today = DateTime.Today;
        var buttons = new List<InlineKeyboardButton[]>();

        // Create 14 buttons (2 rows of 7)
        for (int i = 0; i < 14; i += 7)
        {
            var row = new List<InlineKeyboardButton>();
            for (int j = 0; j < 7 && (i + j) < 14; j++)
            {
                var date = today.AddDays(i + j);
                var buttonText = date.ToString("dd.MM");
                var callbackData = $"datepick_{date:yyyy-MM-dd}";
                row.Add(InlineKeyboardButton.WithCallbackData(buttonText, callbackData));
            }
            buttons.Add(row.ToArray());
        }

        var markup = new InlineKeyboardMarkup(buttons);

        await _botClient.SendMessage(
            chatId: chatId,
            text: "📅 Выберите дату поездки:",
            replyMarkup: markup
        );
    }

    public async Task SendEndLocationSelectionAsync(long chatId, long selectedStartLocationId)
    {
        var buttons = new List<InlineKeyboardButton>();

        foreach (var endlocation in Common.Locations.Where(x => x.Key != selectedStartLocationId))
        {
            var buttonText = endlocation.Value;
            var callbackData = $"endlocationpick_{selectedStartLocationId}_{endlocation.Key}";
            var button = InlineKeyboardButton.WithCallbackData(buttonText, callbackData);

            buttons.Add(button);
        }

        var markup = new InlineKeyboardMarkup(buttons);

        await _botClient.SendMessage(
            chatId: chatId,
            text: "Выберите пункт назначения:",
            replyMarkup: markup
        );
    }

    public async Task SendTimeSelectionAsync(long chatId, DateTime selectedDate)
    {
        var buttons = new List<InlineKeyboardButton[]>();

        // Create time slots from 00:00 to 23:30 with 30-minute intervals
        for (int hour = 0; hour < 24; hour++)
        {
            var row = new List<InlineKeyboardButton>();

            // Add :00 and :30 for each hour
            for (int minute = 0; minute < 60; minute += 30)
            {
                var timeText = $"{hour:00}:{minute:00}";
                var callbackData = $"timepick_{selectedDate:yyyy-MM-dd}_{timeText}";
                row.Add(InlineKeyboardButton.WithCallbackData(timeText, callbackData));
            }

            buttons.Add(row.ToArray());
        }

        var markup = new InlineKeyboardMarkup(buttons);

        await _botClient.SendMessage(
            chatId: chatId,
            text: $"🕒 Выберите время поездки {selectedDate:dd.MM.yyyy}:",
            replyMarkup: markup
        );
    }

    public async Task HandleTimeSelectionAsync(CallbackQuery callbackQuery)
    {
        var parts = callbackQuery.Data.Split('_');
        var selectedDate = DateTime.Parse(parts[1]);
        var selectedTime = TimeSpan.Parse(parts[2]);

        var dateTime = selectedDate.Add(selectedTime);

        var userId = callbackQuery?.From?.Id;

        if (Common.Users.TryGetValue(userId.Value, out User user))
        {
            user.TripOffer.DepartureTime = dateTime;
            user.TripStepEnum = TripStep.AvailableSeats;
            await _botClient.SendMessage(
                            callbackQuery.Message.Chat.Id,
                            text: "Введите количество свободных мест:");
        }
    }
}
