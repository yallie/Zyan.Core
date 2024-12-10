using System;
using System.Threading.Tasks;
using CoreRemoting.Channels.Websocket;
using Xunit;
using Zyan.Communication;
using Zyan.Tests.Tools;

namespace Zyan.Tests;

public partial class RegressionTests : TestBase
{
    [Fact] // related to issue: https://github.com/theRainbird/CoreRemoting/issues/72
    public async Task SyncTwoClientEventsFailing()
    {
        var encr = true;
        var hostConfig = Set(HostConfig, c =>
        {
            c.MessageEncryption = encr;
            c.Channel = new WebsocketServerChannel();
        });

        var connConfig1 = Set(ConnConfig, c =>
        {
            c.MessageEncryption = encr;
            c.Channel = new WebsocketClientChannel();
            c.SendTimeout = 3;
        });

        var connConfig2 = Set(ConnConfig, c =>
        {
            c.MessageEncryption = encr;
            c.Channel = new WebsocketClientChannel();
            c.SendTimeout = 0;
        });

        using var host = new ZyanComponentHost(hostConfig).RegisterComponent<IEventServer, EventServer>();
        using var conn2 = new ZyanConnection(connConfig2);

        var max = 200;
        var timeout = TimeSpan.FromSeconds(5);

        var cnt1 = new AsyncCounter(max);
        var conn1 = new ZyanConnection(connConfig1);
        var proxy1 = conn1.CreateProxy<IEventServer>();
        proxy1.MyEvent += (s, e) => cnt1.Increment();

        var cnt2 = new AsyncCounter(max);
        var proxy2 = conn2.CreateProxy<IEventServer>();
        proxy2.MyEvent += (s, e) => cnt2.Increment();

        // close the first client
        conn1.Dispose();

        // second client should get the event
        await Task.Delay(1000);
        proxy2.OnMyEvent();
        await Task.Delay(100);

        Assert.False(cnt1.CurrentValue > 0);
        Assert.True(cnt2.CurrentValue > 0);

        await Task.Delay(300);
    }
}