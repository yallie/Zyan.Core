using System.Collections.Generic;
using Zyan.InterLinq;

namespace Zyan.Tests.Tools;

/// <summary>
/// Sample queryable component implementation
/// </summary>
public class SampleObjectSource : IObjectSource
{
    IEnumerable<string> Strings { get; set; } =
        ["quick", "brown", "fox", "jumps", "over", "the", "lazy", "dog"];

    public SampleObjectSource()
    {
    }

    public SampleObjectSource(string[] strings)
    {
        if (strings.Length > 0)
        {
            Strings = strings;
        }
    }

    public IEnumerable<T> Get<T>() where T : class
    {
        if (typeof(T) == typeof(string))
        {
            foreach (var s in Strings)
            {
                yield return (T)(object)s;
            }
        }
    }
}
