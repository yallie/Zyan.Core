using System;
using System.Threading.Tasks;

namespace Zyan.Tests.Tools;

public interface ISessionServer
{
    Guid GetSessionID();
    Task<Guid> GetSessionIDAsync();
    bool SessionsAreSame();
}
