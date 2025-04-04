using System;
using System.Collections.Concurrent;

namespace DriveTogetherBot;

public static class Common
{
    public static ConcurrentDictionary<long, User> Users = new ConcurrentDictionary<long, User>();
}
