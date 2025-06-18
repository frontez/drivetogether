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
using System.Collections.Concurrent;

namespace DriveTogetherBot;

public class MessageProcessor
{
    private IConfiguration _configuration;
    private TelegramBotClient _botClient;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<long, User> _userStates = new();
    private Message _message;
    private CancellationToken _token;
    private User CurrentUser { get; set; }

    public MessageProcessor(TelegramBotClient botClient, Message message, CancellationToken token, IConfiguration configuration)
    {
        _configuration = configuration;
        _botClient = botClient;
        _httpClient = new HttpClient();
        _message = message;
        _token = token;
    }

    internal async Task Process()
    {
        if (_message?.From?.Id is not { } UserId)
            return;

        if (_message.Text is not { } messageText)
            return;
            
        var user = await Common.GetUser(UserId);
        if (user != null)
        {
            CurrentUser = user;
        }
        else
        {
            user = await GetUser(UserId);

            if (user != null)
            {
                user.CurrentStep = FormStep.Complete;                
                CurrentUser = user;
                await Common.AddOrUpdateUser(CurrentUser);
            }
        }

        if (messageText.StartsWith('/'))
        {
            await OnCommand(messageText, _message, UserId);
        }
        else if (CurrentUser != null)
        {
            await ProcessFormStep(messageText, user);
        }
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
                    text: "Теперь введите ваш номер телефона:");
                break;

            case FormStep.Phone:
                user.Phone = input;
                user.CurrentStep = FormStep.Complete;
                await ProcessCompletedForm(user);
                break;
        }

        if (user.CurrentStep == FormStep.Complete)
        {
            switch (user.TripStepEnum)
            {
                case TripStep.AvailableSeats:
                    user.TripOffer.AvailableSeats = Convert.ToInt32(input);
                    user.TripStepEnum = TripStep.Price;
                    await _botClient.SendMessage(
                        _message.Chat,
                        text: "Введите стоимость за одно место:");
                    break;

                case TripStep.Price:
                    user.TripOffer.PricePerSeat = Convert.ToInt32(input);
                    user.TripStepEnum = TripStep.Description;
                    await _botClient.SendMessage(
                        _message.Chat,
                        text: "Введите дополнительное описание:");
                    break;

                case TripStep.Description:
                    user.TripOffer.Description = input;
                    user.TripStepEnum = TripStep.Complete;
                    await _botClient.SendMessage(
                        _message.Chat,
                        text: "Успешно!");

                    await CreateTrip(user.TripOffer);
                    user.TripOffer = null;
                    
                    break;
            }
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

    private async Task ProcessCompletedForm(User user)
    {
        await _botClient.SendMessage(
            _message.Chat,
            text: $"Спасибо! Вот что вы ввели:\n" +
                  $"Имя: {user.Name}\n" +
                  $"Телефон: {user.Phone}");

        await RegisterUser(user);
    }

    private async Task OnCommand(string messageText, Message message, long UserId)
    {
        var user = await Common.GetUser(UserId);
        if (user != null)
        {
            CurrentUser = user;
        }
        else
        {
            user = await GetUser(UserId);

            if (user != null)
            {
                user.CurrentStep = FormStep.Complete;
                CurrentUser = user;
                await Common.AddOrUpdateUser(CurrentUser);                
            }
        }

        if (messageText == "/register")
        {
            Console.WriteLine("register is used");

            if (CurrentUser is null)
            {
                CurrentUser = new User();
                CurrentUser.CurrentStep = FormStep.Name;
                CurrentUser.Id = UserId;
                CurrentUser.Username = message.From.Username;

                await Common.AddOrUpdateUser(CurrentUser);
                await _botClient.SendMessage(
                        _message.Chat,
                        text: "Введите ваше имя:",
                        cancellationToken: _token);
            }
            else
            {
                await _botClient.SendMessage(
                    _message.Chat,
                    text: $"Вы уже зарегистрированы.\n" +
                        $"Имя: {CurrentUser.Name}\n" +
                        $"Телефон: {CurrentUser.Phone}");
            }
        }
        else if (CurrentUser is null)
        {
            await _botClient.SendMessage(
                _message.Chat,
                text: $"Вам нужно зарегистрироваться. Команда /register.\n");
            return;
        }

        if (messageText == "/users")
        {
            Console.WriteLine("users is used");

            var users = await GetAllUsers();

            string usersLine = string.Join("\n", users.Select(x => x.Username));

            await _botClient.SendMessage(
                _message.Chat,
                text: $"Пользователи:\n" + usersLine);
        }
        else if (messageText == "/current_user")
        {
            Console.WriteLine("current_user is used");

            await _botClient.SendMessage(
                _message.Chat,
                text: $"Пользователь:\n" + CurrentUser.Username);
        }

        else if (messageText == "/create_trip")
        {
            Console.WriteLine("create_trip is used");

            if (CurrentUser.TripOffer == null)
            {
                CurrentUser.TripOffer = new TripOffer();
                CurrentUser.TripOffer.DriverId = UserId;
                CurrentUser.TripStepEnum = TripStep.TripStartLocation;
                await Common.AddOrUpdateUser(CurrentUser);
                await GetAllLocations();
                await SendStartLocationSelectionAsync(_message.Chat.Id);
            }
            else
            {
                await _botClient.SendMessage(
                    _message.Chat,
                    text: $"У вас уже есть поездка.\n");                
            }
        }
        else if (messageText == "/my_trip")
        {
            Console.WriteLine("my_trip is used");
        }
        else if (messageText == "/cancel_trip")
        {
            Console.WriteLine("cancel_trip is used");
        }
        else if (messageText == "/find_trip")
        {
            Console.WriteLine("find_trip is used");
        }
        else if (messageText == "/join_trip")
        {
            Console.WriteLine("join_trip is used");
        }
        else if (messageText == "/quit_trip")
        {
            Console.WriteLine("quit_trip is used");
        }

        else if (messageText == "/get_requests")
        {
            Console.WriteLine("get_requests is used");
        }
        else if (messageText == "/accept_request")
        {
            Console.WriteLine("accept_request is used");
        }
        else if (messageText == "/reject_request")
        {
            Console.WriteLine("reject_request is used");
        }
        else
        {
            await Task.CompletedTask;
        }
    }

    public async Task SendStartLocationSelectionAsync(long chatId)
    {
        var buttons = new List<InlineKeyboardButton>();

        foreach (var location in Common.Locations)
        {
            var buttonText = location.Value;
            var callbackData = $"startlocationpick_{location.Key}";
            var button = InlineKeyboardButton.WithCallbackData(buttonText, callbackData);

            buttons.Add(button);
        }
        
        var markup = new InlineKeyboardMarkup(buttons);

        await _botClient.SendMessage(
            chatId: chatId,
            text: "Выберите пункт отправления:",
            replyMarkup: markup
        );
    }

    private async Task GetAllLocations()
    {
        try
        {
            var token = await GetServiceTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Assuming your UserService base URL is the same as RegisterUrl without the last segment
            var getAllUsersUrl = _configuration["TripService:LocationUrl"]?.TrimEnd('/');

            var response = await _httpClient.GetAsync(getAllUsersUrl);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API error: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var locations = JsonSerializer.Deserialize<List<Entities.Location>>(responseContent, options);

            foreach (var location in locations)
            {
                Common.Locations.TryAdd(location.Id, location.Name);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
            throw;
        }
    }

    private async Task<string> CreateTrip(TripOffer trip)
    {
        try
        {
            var json = JsonSerializer.Serialize(trip);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var token = await GetServiceTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Typically this would be a POST request to /users or /api/users endpoint
            var response = await _httpClient.PostAsync(_configuration["TripService:TripOfferUrl"], content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                try
                {
                    var errorObject = JsonSerializer.Deserialize<Dictionary<string, object>>(errorContent);
                    var detailedError = JsonSerializer.Serialize(errorObject, new JsonSerializerOptions { WriteIndented = true });
                    throw new Exception($"API error: {response.StatusCode} - {detailedError}");
                }
                catch
                {
                    // If not JSON, just show the raw content
                    throw new Exception($"API error: {response.StatusCode} - {errorContent}");
                }
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return $"Поездка создана";
        }
        catch (Exception ex)
        {
            // Log the full exception including inner exceptions
            var fullError = GetFullExceptionDetails(ex);
            Console.WriteLine(fullError);
            throw new Exception(fullError, ex);
        }
    }

    private string GetFullExceptionDetails(Exception ex)
    {
        var sb = new StringBuilder();
        sb.AppendLine(ex.Message);
        sb.AppendLine(ex.StackTrace);

        // Recursively get inner exception details
        var inner = ex.InnerException;
        while (inner != null)
        {
            sb.AppendLine("--- Inner Exception ---");
            sb.AppendLine(inner.Message);
            sb.AppendLine(inner.StackTrace);
            inner = inner.InnerException;
        }

        return sb.ToString();
    }


    #region UserService

    private async Task<string> RegisterUser(User telegramUser)
    {
        try
        {
            var json = JsonSerializer.Serialize(telegramUser);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var token = await GetServiceTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Typically this would be a POST request to /users or /api/users endpoint
            var response = await _httpClient.PostAsync(_configuration["UserService:RegisterUrl"], content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API error: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return $"Successfully registered! Welcome, {telegramUser.Username}";
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
            throw;
        }
    }

    private async Task<List<User>> GetAllUsers()
    {
        try
        {
            var token = await GetServiceTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Assuming your UserService base URL is the same as RegisterUrl without the last segment
            var getAllUsersUrl = _configuration["UserService:RegisterUrl"]?.TrimEnd('/');

            var response = await _httpClient.GetAsync(getAllUsersUrl);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API error: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<List<User>>(responseContent, options) ?? new List<User>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
            throw;
        }
    }
    private async Task<User> GetUser(long id)
    {
        try
        {
            var token = await GetServiceTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var baseUrl = _configuration["UserService:RegisterUrl"]?.TrimEnd('/');
            var getUserUrl = $"{baseUrl}/{id}";

            var response = await _httpClient.GetAsync(getUserUrl);

            // Explicitly handle 404 as null return
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API error: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<User>(responseContent, options) ?? null;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
            throw;
        }
    }

    public async Task<string> GetServiceTokenAsync()
    {
        var request = new Dictionary<string, string>
        {
            {"client_id", _configuration["Keycloak:ClientId"]},
            {"client_secret", _configuration["Keycloak:ClientSecret"]},
            {"grant_type", "client_credentials"}
        };

        var response = await _httpClient.PostAsync(
            _configuration["Keycloak:TokenUrl"],
            new FormUrlEncodedContent(request));

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content);
        return tokenResponse.access_token;
    }
    
    #endregion
}
