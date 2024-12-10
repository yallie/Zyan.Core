using System;
using System.Threading.Tasks;

namespace Zyan.Tests.Tools;

public interface IEventServer
{
    event EventHandler MyEvent;

    void OnMyEvent();

    void StressTest(int count);

    Task OnMyEventAsync();

    Task StressTestAsync(int count);
}
