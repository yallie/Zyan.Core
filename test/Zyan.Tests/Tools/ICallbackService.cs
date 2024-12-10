using System;

namespace Zyan.Tests.Tools;

public interface ICallbackService
{
    void RegisterCallback(Action callback);

    void DoCallback();
}

