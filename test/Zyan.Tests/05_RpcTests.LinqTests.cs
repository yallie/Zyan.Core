using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using CoreRemoting.DependencyInjection;
using CoreRemoting.Threading;
using CoreRemoting.Toolbox;
using Xunit;
using Zyan.Communication;
using Zyan.InterLinq;
using Zyan.Tests.Tools;

namespace Zyan.Tests;

public partial class RpcTests : TestBase
{
    public interface IExpressionService
    {
        string CompileAndExecuteExpression(Expression<Func<string, string>> ex, string data);
        Expression ProcessExpression(Expression<Action<string>> ex, string data);
        IEnumerable<T> GetList<T>() where T : class, new();
        IQueryable<T> Query<T>() where T : class, new();
    }

    public class ExpressionService : IExpressionService
    {
        public string CompileAndExecuteExpression(Expression<Func<string, string>> ex, string data)
        {
            var func = ex.Compile();
            return func(data);
        }

        public Expression ProcessExpression(Expression<Action<string>> ex, string data)
        {
            return Expression.Invoke(ex, Expression.Constant(data));
        }

        public IEnumerable<T> GetList<T>() where T : class, new()
        {
            foreach (var i in Enumerable.Range(1, 10))
                yield return new T();

            yield break;
        }

        public IQueryable<T> Query<T>() where T : class, new()
        {
            return Enumerable.Range(1, 5).Select(i => new T()).ToArray().AsQueryable();
        }
    }

    public class RandomValue
    {
        public const int MinValue = 500;
        public const int MaxValue = 1000;
        static Random random = new Random();

        public int Value { get; private set; }

        public RandomValue()
        {
            Value = random.Next(MinValue, MaxValue);
        }
    }

    private ZyanComponentHost CreateZyanComponentHost() =>
        new ZyanComponentHost(HostConfig)
            .RegisterComponent<IExpressionService, ExpressionService>()
            .RegisterComponent<IObjectSource, SampleObjectSource>(new SampleObjectSource(["Hello", "World!"]))
            .RegisterComponent<IObjectSource, SampleObjectSource>("Sample1", new SampleObjectSource(["this", "is", "an", "example"]))
            .RegisterComponent<IObjectSource, SampleObjectSource>("Sample6")
            .RegisterComponent<IObjectSource, SampleObjectSource>(ServiceLifetime.SingleCall, "Sample7")
            .RegisterComponent<IEntitySource, DataWrapper>("DbSample", new DataWrapper())
            .RegisterComponent<ISampleMethodSource, SampleMethodSource>(ServiceLifetime.SingleCall, "Disposable");

    [Fact]
    public void Linq_TestUntitledComponent()
    {
        using var host = CreateZyanComponentHost();
        using var conn = new ZyanConnection(ConnConfig);

        var proxy = conn.CreateProxy<IObjectSource>();
        var query =
            from s in proxy.Get<string>()
            where s.EndsWith("!")
            select s;

        var result = query.FirstOrDefault();
        Assert.NotNull(result);
        Assert.Equal("World!", result);
    }

    [Fact]
    public void Linq_TestSample1Component()
    {
        using var host = CreateZyanComponentHost();
        using var conn = new ZyanConnection(ConnConfig);

        var proxy = conn.CreateProxy<IObjectSource>("Sample1");
        var query =
            from s in proxy.Get<string>()
            where s.Length > 2
            orderby s
            select s + s.ToUpper();

        var result = string.Concat(query.ToArray());
        Assert.Equal("exampleEXAMPLEthisTHIS", result);
    }

    [Fact]
    public void Linq_TestSample6Component()
    {
        using var host = CreateZyanComponentHost();
        using var conn = new ZyanConnection(ConnConfig);

        var proxy = conn.CreateProxy<IObjectSource>("Sample6");
        var query =
            from s in proxy.Get<string>()
            where s == "fox" || s == "dog" || s == "frog" || s == "mouse"
            select s.Replace('o', 'i');

        var result = string.Join(" & ", query.ToArray());
        Assert.Equal("fix & dig", result);
    }

    [Fact]
    public void Linq_TestSample7Component()
    {
        using var host = CreateZyanComponentHost();
        using var conn = new ZyanConnection(ConnConfig);

        var proxy = conn.CreateProxy<IObjectSource>("Sample7");
        var query =
            from s in proxy.Get<string>()
            where Regex.IsMatch(s, "[nyg]$")
            select s;

        var result = string.Join(" ", query.ToArray());
        Assert.Equal("brown lazy dog", result);
    }

    [Fact]
    public void Linq_TestDbSampleComponent1()
    {
        using var host = CreateZyanComponentHost();
        using var conn = new ZyanConnection(ConnConfig);

        var proxy = conn.CreateProxy<IEntitySource>("DbSample");
        var query =
            from s in proxy.Get<SampleEntity>()
            where Regex.IsMatch(s.FirstName, "[rt]$")
            select s.LastName;

        var result = string.Join(" ", query.ToArray());
        Assert.Equal("Einstein Friedmann Kapitsa Oppenheimer Compton Lawrence Wilson Kurchatov", result);
    }

    [Fact]
    public void Linq_TestDbSampleComponent2()
    {
        using var host = CreateZyanComponentHost();
        using var conn = new ZyanConnection(ConnConfig);

        var proxy = conn.CreateProxy<IEntitySource>("DbSample");
        var query =
            from s in proxy.Get<SampleEntity>()
            orderby s.FirstName.Length, s.FirstName
            select s.FirstName;

        var result = string.Join(", ", query.ToArray());
        Assert.Equal(
            "Leó, Lev, Hans, Igor, Glenn, James, Klaus, Leona, Niels, Pyotr, Ralph, Albert, Arthur, Edward, Emilio, " +
            "Enrico, Ernest, George, Harold, Robert, Robert, Richard, William, Alexander, Stanislaw, Chien-Shiung", result);
    }

    [Fact]
    public void Linq_TestSampleMethodQueryComponent()
    {
        using var host = CreateZyanComponentHost();
        using var conn = new ZyanConnection(ConnConfig);

        var proxy = conn.CreateProxy<ISampleMethodSource>("Disposable");
        var query =
            from s in proxy.GetTable<string>()
            where Regex.IsMatch(s, "[nyg]$")
            select s;

        var result = string.Join(" ", query.ToArray());
        Assert.Equal("brown lazy dog", result);
    }

    [Fact]
    public void Linq_TestExpressionParameter()
    {
        using var host = CreateZyanComponentHost();
        using var conn = new ZyanConnection(ConnConfig);

        var proxy = conn.CreateProxy<IExpressionService>();
        Expression<Func<string, string>> ex =
            s => s.ToLower() + "-" + s.ToUpper();

        var result = proxy.CompileAndExecuteExpression(ex, "Ru");
        Assert.Equal("ru-RU", result);
    }

    [Fact]
    public void Linq_TestExpressionReturnValue()
    {
        const string message = "Hello, World!";

        using var host = CreateZyanComponentHost();
        using var conn = new ZyanConnection(ConnConfig);

        var proxy = conn.CreateProxy<IExpressionService>();
        Expression<Action<string>> ex = s => Console.WriteLine(s);
        Expression expression = Expression.Invoke(ex, Expression.Constant(message));

        var result = proxy.ProcessExpression(ex, message);
        Assert.Equal(expression.ToString(), result.ToString());
    }

    [Fact]
    public void Linq_TestSampleEnumerable()
    {
        using var host = CreateZyanComponentHost();
        using var conn = new ZyanConnection(ConnConfig);

        var mc = conn.CreateProxy<IExpressionService>();

        var count = 0;
        foreach (var value in mc.GetList<StringBuilder>())
            count += value.Length + 1;

        Assert.Equal(10, count);
    }

    [Fact]
    public void Linq_TestSampleQueryable1()
    {
        using var host = CreateZyanComponentHost();
        using var conn = new ZyanConnection(ConnConfig);

        var mc = conn.CreateProxy<IExpressionService>();
        var count = mc.Query<RandomValue>().Count();
        Assert.Equal(5, count);
    }

    [Fact]
    public void Linq_TestSampleQueryable2()
    {
        using var host = CreateZyanComponentHost();
        using var conn = new ZyanConnection(ConnConfig);

        var mc = conn.CreateProxy<IExpressionService>();
        var query =
            from sv in mc.Query<RandomValue>()
            where sv.Value < RandomValue.MinValue + 100
            select sv;

        var sum = query.Sum(sv => sv.Value);

        Assert.True(0 <= sum);
        Assert.True(5 * RandomValue.MaxValue >= sum);
    }

    [Fact]
    public void Linq_TestSampleQueryable3()
    {
        using var host = CreateZyanComponentHost();
        using var conn = new ZyanConnection(ConnConfig);

        var mc = conn.CreateProxy<IExpressionService>();
        var param = '3';
        var query =
            from sv in mc.Query<RandomValue>()
            where sv.Value.ToString().Contains(param)
            select new { sv.Value };

        var result = string.Concat(query);

        Assert.True(result == string.Empty || result.Contains(param));
    }

}