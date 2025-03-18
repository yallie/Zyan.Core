using System;
using System.Threading.Tasks;

namespace Zyan.Tests.Tools;

public interface ICallbackService
{
    void RegisterCallback(Action callback);

    void DoCallback();

    Task DoCallbackAsync();
}

