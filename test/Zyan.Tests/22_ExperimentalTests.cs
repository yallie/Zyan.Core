using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Zyan.Tests;

public class ExperimentalTests
{
    /// <summary>
    /// This sample class has an illusion of an awaitable constructor, i.e.:
    /// var x = await new AwaitableConstructor();
    /// </summary>
    private class AwaitableConstructor
    {
        public string Status { get; private set; }

        // constructor is synchronous
        public AwaitableConstructor() => Status = "Created";

        private bool Initialized { get; set; }

        // initialization is asynchronous
        private async Task<AwaitableConstructor> Initiaze()
        {
            // note: disposable class should self-dispose
            // and rethrow if exception is thrown
            if (!Initialized)
            {
                Initialized = true;
                Status = "Initialized";
                await Task.Delay(1);
            }

            return this;
        }

        /// <summary>
        /// This is the trick that enables async new expressions.
        /// </summary>
        public TaskAwaiter<AwaitableConstructor> GetAwaiter() =>
            Initiaze().GetAwaiter();
    }

    [Fact]
    public async Task Class_construction_can_be_awaitable()
    {
        void IsType<T>(object x) =>
            Assert.IsType<T>(x);

        // plain new: created, but not initialized
        var syncSample = new AwaitableConstructor();
        IsType<AwaitableConstructor>(syncSample);
        Assert.Equal("Created", syncSample.Status);

        // await new: created and initialized asynchronously
        var asyncSample = await new AwaitableConstructor();
        IsType<AwaitableConstructor>(asyncSample);
        Assert.Equal("Initialized", asyncSample.Status);

        var x = syncSample;
        void consume(AwaitableConstructor s) => x = s;
        consume(syncSample);
        consume(asyncSample);

        AwaitableConstructor echo(AwaitableConstructor s) => s;

        // note: discard "_" is required due to warning CS4014
        _ = echo(syncSample);
        _ = echo(asyncSample);
    }

    /// <summary>
    /// This sample class has an illusion of an awaitable constructor, i.e.:
    /// var x = await new AwaitableConstructor();
    /// </summary>
    private class AwaitableDisposable(bool fail = false) : IAsyncDisposable
    {
        // constructor is synchronous
        public string Status { get; private set; } = "Created";

        private bool Initialized { get; set; }

        public ValueTask DisposeAsync()
        {
            Status = "Disposed";
            return default;
        }

        // initialization is asynchronous
        private async Task<AwaitableDisposable> Initiaze()
        {
            // note: disposable class should self-dispose
            // and rethrow if exception is thrown
            if (!Initialized)
            {
                Initialized = true;
                try
                {
                    Status = "Initialized";
                    await Task.Delay(1);
                    if (fail)
                    {
                        throw new InvalidOperationException("Failed to initialize");
                    }
                }
                catch
                {
                    await DisposeAsync();
                    throw;
                }
            }

            return this;
        }

        /// <summary>
        /// This is the trick that enables async new expressions.
        /// </summary>
        public TaskAwaiter<AwaitableDisposable> GetAwaiter() =>
            Initiaze().GetAwaiter();
    }

    [Fact]
    public async Task AsyncDisposableClass_construction_can_be_awaitable()
    {
        void IsType<T>(object x) =>
            Assert.IsType<T>(x);

        // plain new: created, but not initialized
        await using var syncSample = new AwaitableDisposable();
        IsType<AwaitableDisposable>(syncSample);
        Assert.Equal("Created", syncSample.Status);

        // await new: created and initialized asynchronously
        await using var asyncSample = await new AwaitableDisposable();
        IsType<AwaitableDisposable>(asyncSample);
        Assert.Equal("Initialized", asyncSample.Status);

        // failed initialization
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await using var s = new AwaitableDisposable();
            IsType<AwaitableDisposable>(s);

            // throws during initialization and disposes of the instance
            await using var d = await new AwaitableDisposable(fail: true);
        });
    }
}
