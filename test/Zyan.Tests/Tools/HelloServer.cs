using System.Threading.Tasks;

namespace Zyan.Tests.Tools
{
    internal class HelloServer : IHelloServer
    {
        public string Hello(string hello) => hello + " World!";

        public async Task<string> HelloAsync(string hello)
        {
            await Task.Delay(1);
            return Hello(hello);
        }
    }
}
