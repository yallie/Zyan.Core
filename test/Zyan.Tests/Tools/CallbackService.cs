﻿using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Zyan.Communication;

namespace Zyan.Tests.Tools;

public class CallbackService : ICallbackService
{
    private static ConcurrentDictionary<Guid, Action> Callbacks { get; } = [];

    private Guid ClientID => ServerSession.CurrentSession.SessionID;

    public void RegisterCallback(Action callback)
    {
        // register my delegate
        Callbacks[ClientID] = callback;
        Console.WriteLine("Client {0} registered the delegate.", ClientID);
    }

    public void DoCallback()
    {
        if (Callbacks.TryGetValue(ClientID, out var action))
        {
            Console.WriteLine("Calling client {0}'s delegate.", ClientID);

            // call my delegate, if registered
            action?.Invoke();
        }
    }

    public Task DoCallbackAsync() =>
        Task.Run(DoCallback);
}
