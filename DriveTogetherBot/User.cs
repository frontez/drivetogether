using System;

namespace DriveTogetherBot;

public class User
{
    public FormStep CurrentStep { get; set; }    
    public long Id { get; set; }
    public string Username { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
}

public enum FormStep
{
    Name,
    Phone,
    Complete
}
