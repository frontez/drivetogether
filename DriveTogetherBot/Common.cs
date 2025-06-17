using System;
using System.Collections.Concurrent;
using DriveTogetherBot.Entities;

namespace DriveTogetherBot;

public static class Common
{
    public static ConcurrentDictionary<long, User> Users = new ConcurrentDictionary<long, User>();
    public static ConcurrentDictionary<long, string> Locations = new ConcurrentDictionary<long, string>();
}