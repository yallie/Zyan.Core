﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Zyan.Communication.CallInterception;

/// <summary>
/// Allows building call interceptors with strong-typed fluent interface
/// </summary>
public class CallInterceptorHelper<T> : IEnumerable<CallInterceptor>
{
    private string _uniqueNameFilter;
    CallInterceptorCollection Interceptors { get; set; }

    /// <summary>
    /// Creates CallInterceptorHelper instance
    /// </summary>
    /// <param name="interceptors">Collection of call interceptors to add to</param>
    public CallInterceptorHelper(CallInterceptorCollection interceptors)
    {
        Interceptors = interceptors;
    }

    /// <summary>
    /// Unique name regex filter. Once again... Some refactoring required.
    /// </summary>
    public string UniqueNameFilter
    {
        get
        {
            if (string.IsNullOrEmpty(_uniqueNameFilter))
                return ".*";
            return _uniqueNameFilter;
        }
        private set => _uniqueNameFilter = value;
    }

    /// <summary>
    /// Creates new CallInterceptor for the given method and adds it to the interceptors collection.
    /// </summary>
    /// <param name="expression">LINQ expression of the method to intercept.</param>
    /// <param name="handler">Interception handler.</param>
    public CallInterceptorHelper<T> Add(Expression<Action<T>> expression, Action<CallInterceptionData> handler)
    {
        CheckNotNull(handler);

        Parse(expression, out var memberType, out var memberName);

        Interceptors.Add(new CallInterceptor(
            typeof(T), memberType, memberName, [], handler)
                .WithUniqueNameFilter(UniqueNameFilter));

        return this;
    }

    /// <summary>
    /// Creates new CallInterceptor for the given method and adds it to the interceptors collection.
    /// </summary>
    /// <param name="expression">LINQ expression of the method to intercept.</param>
    /// <param name="handler">Interception handler.</param>
    public CallInterceptorHelper<T> Add<T1>(Expression<Func<T, T1>> expression, Func<CallInterceptionData<T1>, T1> handler)
    {
        CheckNotNull(handler);

        Parse(expression, out var memberType, out var memberName);

        Interceptors.Add(new CallInterceptor(
            typeof(T), memberType, memberName, [],
                data => data.ReturnValue = handler(data))
                    .WithUniqueNameFilter(UniqueNameFilter));

        return this;
    }

    /// <summary>
    /// Creates new CallInterceptor for the given method and adds it to the interceptors collection.
    /// </summary>
    /// <param name="expression">LINQ expression of the method to intercept.</param>
    /// <param name="handler">Interception handler.</param>
    public CallInterceptorHelper<T> Add<T1>(Expression<Action<T, T1>> expression, Action<CallInterceptionData, T1> handler)
    {
        CheckNotNull(handler);

        Parse(expression, out var memberType, out var memberName);

        Interceptors.Add(new CallInterceptor(
            typeof(T), memberType, memberName, [typeof(T1)],
                data => handler(data, (T1)data.Parameters[0]))
                    .WithUniqueNameFilter(UniqueNameFilter));

        return this;
    }

    /// <summary>
    /// Creates new CallInterceptor for the given method and adds it to the interceptors collection.
    /// </summary>
    /// <param name="expression">LINQ expression of the method to intercept.</param>
    /// <param name="handler">Interception handler.</param>
    public CallInterceptorHelper<T> Add<T1, T2>(Expression<Func<T, T1, T2>> expression, Func<CallInterceptionData<T2>, T1, T2> handler)
    {
        CheckNotNull(handler);

        Parse(expression, out var memberType, out var memberName);

        Interceptors.Add(new CallInterceptor(
            typeof(T), memberType, memberName, [typeof(T1)],
                data => data.ReturnValue = handler(
                    data, (T1)data.Parameters[0]))
                        .WithUniqueNameFilter(UniqueNameFilter));

        return this;
    }

    /// <summary>
    /// Creates new CallInterceptor for the given method and adds it to the interceptors collection.
    /// </summary>
    /// <param name="expression">LINQ expression of the method to intercept.</param>
    /// <param name="handler">Interception handler.</param>
    public CallInterceptorHelper<T> Add<T1, T2>(Expression<Action<T, T1, T2>> expression, Action<CallInterceptionData, T1, T2> handler)
    {
        CheckNotNull(handler);

        Parse(expression, out var memberType, out var memberName);

        Interceptors.Add(new CallInterceptor(
            typeof(T), memberType, memberName, [typeof(T1), typeof(T2)],
                data => handler(data, (T1)data.Parameters[0], (T2)data.Parameters[1]))
                    .WithUniqueNameFilter(UniqueNameFilter));

        return this;
    }

    /// <summary>
    /// Creates new CallInterceptor for the given method and adds it to the interceptors collection.
    /// </summary>
    /// <param name="expression">LINQ expression of the method to intercept.</param>
    /// <param name="handler">Interception handler.</param>
    public CallInterceptorHelper<T> Add<T1, T2, T3>(Expression<Func<T, T1, T2, T3>> expression, Func<CallInterceptionData<T3>, T1, T2, T3> handler)
    {
        CheckNotNull(handler);

        Parse(expression, out var memberType, out var memberName);

        Interceptors.Add(new CallInterceptor(typeof(T),
            memberType, memberName, [typeof(T1), typeof(T2)],
                data => data.ReturnValue = handler(
                    data, (T1)data.Parameters[0], (T2)data.Parameters[1]))
                        .WithUniqueNameFilter(UniqueNameFilter));

        return this;
    }

    /// <summary>
    /// Creates new CallInterceptor for the given method and adds it to the interceptors collection.
    /// </summary>
    /// <param name="expression">LINQ expression of the method to intercept.</param>
    /// <param name="handler">Interception handler.</param>
    public CallInterceptorHelper<T> Add<T1, T2, T3>(Expression<Action<T, T1, T2, T3>> expression, Action<CallInterceptionData, T1, T2, T3> handler)
    {
        CheckNotNull(handler);

        Parse(expression, out var memberType, out var memberName);

        Interceptors.Add(new CallInterceptor(
            typeof(T), memberType, memberName, [typeof(T1), typeof(T2), typeof(T3)],
                data => handler(data, (T1)data.Parameters[0], (T2)data.Parameters[1], (T3)data.Parameters[2]))
                    .WithUniqueNameFilter(UniqueNameFilter));

        return this;
    }

    /// <summary>
    /// Creates new CallInterceptor for the given method and adds it to the interceptors collection.
    /// </summary>
    /// <param name="expression">LINQ expression of the method to intercept.</param>
    /// <param name="handler">Interception handler.</param>
    public CallInterceptorHelper<T> Add<T1, T2, T3, T4>(Expression<Func<T, T1, T2, T3, T4>> expression, Func<CallInterceptionData<T4>, T1, T2, T3, T4> handler)
    {
        CheckNotNull(handler);

        Parse(expression, out var memberType, out var memberName);

        Interceptors.Add(new CallInterceptor(
            typeof(T), memberType, memberName, [typeof(T1), typeof(T2), typeof(T3)],
                data => data.ReturnValue = handler(data, (T1)data.Parameters[0], (T2)data.Parameters[1], (T3)data.Parameters[2]))
                    .WithUniqueNameFilter(UniqueNameFilter));

        return this;
    }

    /// <summary>
    /// Feature to filter interceptor unique name.
    /// </summary>
    /// <param name="nameRegex"></param>
    /// <returns></returns>
    public CallInterceptorHelper<T> WithUniqueNameFilter(string nameRegex)
    {
        UniqueNameFilter = nameRegex;
        return this;
    }

    /// <summary>
    /// Checks whether argument is not null.
    /// </summary>
    internal static void CheckNotNull<THandler>(THandler handler)
    {
        if (handler == null)
        {
            throw new ArgumentNullException("handler");
        }
    }

    /// <summary>
    /// Parse lambda expression and extract memberType and memberName.
    /// </summary>
    internal static void Parse(LambdaExpression lambda, out MemberTypes memberType, out string memberName)
    {
        var mx = ExtractMemberExpression(lambda);
        if (mx != null)
        {
            memberType = mx.Member.MemberType;
            memberName = mx.Member.Name;
            return;
        }

        var mc = ExtractMethodCallExpression(lambda);
        if (mc != null)
        {
            memberType = mc.Method.MemberType;
            memberName = mc.Method.Name;
            return;
        }

        throw new ArgumentException("Invalid expression", "expr");
    }

    /// <summary>
    /// Try to extract MethodCallExpression from lambda expression.
    /// </summary>
    private static MethodCallExpression ExtractMethodCallExpression(LambdaExpression lambda)
    {
        var unaryExpression = lambda.Body as UnaryExpression;
        if (unaryExpression != null)
        {
            return unaryExpression.Operand as MethodCallExpression;
        }

        return lambda.Body as MethodCallExpression;
    }

    /// <summary>
    /// Try to extract MemberExpression from lambda expression.
    /// </summary>
    private static MemberExpression ExtractMemberExpression(LambdaExpression lambda)
    {
        var unaryExpression = lambda.Body as UnaryExpression;
        if (unaryExpression != null)
        {
            return unaryExpression.Operand as MemberExpression;
        }

        return lambda.Body as MemberExpression;
    }

    /// <summary>
    /// Returns call interceptors for the given interface.
    /// </summary>
    public IEnumerator<CallInterceptor> GetEnumerator() =>
        Interceptors.Where(c => c.InterfaceType == typeof(T)).GetEnumerator();

    /// <summary>
    /// Returns call interceptors for the given interface.
    /// </summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
        Interceptors.Where(c => c.InterfaceType == typeof(T)).GetEnumerator();
}
