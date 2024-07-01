using System.Threading.Tasks;

namespace Zyan.Tests.Tools
{
    public interface IHelloServer
    {
        string Hello(string hello);

        Task<string> HelloAsync(string hello);
    }
}
