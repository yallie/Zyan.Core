using System.Threading.Tasks;

namespace Zyan.Tests.Tools;

public interface IHelloServer
{
    string Hello(string hello);

    Task<string> HelloAsync(string hello);

    ValueTask<string> HelloValueTaskAsync(string hello);

    void Error(string msg);

    Task ErrorAsync(string msg);

    void NonSerializableError(string msg, params string[] data);
}
