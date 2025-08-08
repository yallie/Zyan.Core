using System.Collections.Generic;
using System.Linq;
using Zyan.InterLinq;

namespace Zyan.Tests.Tools;

/// <summary>
/// In-memory database wrapper for Linq unit-tests
/// </summary>
public class DataWrapper : IEntitySource
{
    private static IEnumerable<SampleEntity> Entities { get; } =
        SampleEntity.GetSampleEntities();

    public IQueryable<T> Get<T>() where T : class
    {
        if (typeof(T) == typeof(SampleEntity))
        {
            return Entities.OfType<T>().AsQueryable();
        }

        return Enumerable.Empty<T>().AsQueryable();
    }
}
