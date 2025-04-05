using System;

namespace DriveTogetherBot;

public class TokenResponse
{
    public string access_token { get; set; }
    public int expires_in { get; set; }
    public string token_type { get; set; }
    // add other properties if needed
}