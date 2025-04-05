using System;
using System.Text;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Net.Http.Headers;

namespace DriveTogetherBot;

public class MessageProcessor
{
    private IConfiguration _configuration;
    private TelegramBotClient _botClient;     
    private readonly HttpClient _httpClient;   
    private readonly Dictionary<long, User> _userStates = new();
    private Message _message;
    private CancellationToken _token;

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
        if (_message?.From?.Id is not {} UserId)
            return;
        
        if (_message.Text is not { } messageText)
            return;

        if (Common.Users.TryGetValue(UserId, out User user))
        {
            await ProcessFormStep(messageText, user);
        }
        if (messageText.StartsWith('/'))
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
                    text: "Теперь введите ваш номер телефона:");
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

        await RegisterUser(user);
    }

    private async Task OnCommand(string messageText, Message message, long UserId)
    {
        if (messageText == "/register")
        {
            User user = new User();
            user.CurrentStep = FormStep.Name;
            user.Id = UserId;
            user.Username = message.From.Username;

            if (Common.Users.TryAdd(UserId, user))
            {
                await _botClient.SendMessage(
                    _message.Chat,
                    text: "Введите ваше имя:",
                    cancellationToken: _token);
            }
            else
            {
                if (Common.Users.TryGetValue(UserId, out User existingUser))
                {
                    await _botClient.SendMessage(
                        _message.Chat,
                        text: $"Вы уже зарегистрированы.\n" +
                            $"Имя: {existingUser.Name}\n" +
                            $"Телефон: {existingUser.Phone}");
                }
                else
                {
                    await _botClient.SendMessage(
                        _message.Chat,
                        text: $"Не удалось зарегистрироваться.\n");
                }
            }  
        }
        else
        {
            await Task.CompletedTask;
        }
    }

    private async Task<string> RegisterUser(User telegramUser)
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
}
