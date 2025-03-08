using System;

namespace UserService.Models;

// Model for the logout request
public class LogoutRequest
{
    public string IdToken { get; set; }
}