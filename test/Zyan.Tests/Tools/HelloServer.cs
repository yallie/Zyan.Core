using System;
using System.Threading.Tasks;

namespace Zyan.Tests.Tools;

internal class HelloServer : IHelloServer
{
	public string Hello(string hello) => hello + " World!";

	public async Task<string> HelloAsync(string hello)
	{
		await Task.Delay(1);
		return Hello(hello);
	}
	public void Error(string msg) => throw new Exception(msg);

	public async Task ErrorAsync(string msg)
	{
		await Task.Delay(1);
		Error(msg);
	}

	private class NonSerializable : Exception
	{
		public NonSerializable(string message)
			: base(message)
		{
		}
	}

	public void NonSerializableError(string msg, params string[] data)
	{
		var ex = new NonSerializable(msg);

		foreach (var item in data)
			ex.Data[item] = item;

		throw ex;
	}
}
