using System.Threading;
using System.Threading.Tasks;

namespace Zyan.Tests.Tools;

public class AsyncCounter
{
	public AsyncCounter(int max)
	{
		MaxValue = max;
	}

	public int MaxValue { get; }

	public int CurrentValue => currentValue;

	private int currentValue;

	public TaskCompletionSource<bool> CompletionSource { get; } =
		new TaskCompletionSource<bool>();

	public Task<bool> Task => CompletionSource.Task;

	public int Increment()
	{
		var result = Interlocked.Increment(ref currentValue);
		if (result >= MaxValue)
		{
			CompletionSource.TrySetResult(true);
		}

		return result;
	}
}
