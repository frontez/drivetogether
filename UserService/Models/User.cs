using System;

namespace UserService.Models;

public class User
{
    public long Id { get; set; }
    public long TelegramId { get; set; }
    public string Username { get; set; }
    public string Name {get; set; }
    public string Phone { get; set; }
}