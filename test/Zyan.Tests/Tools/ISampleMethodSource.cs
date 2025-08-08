using System.Linq;

namespace Zyan.Tests.Tools;

public interface ISampleMethodSource
{
    IQueryable<T> GetTable<T>() where T : class;
}
