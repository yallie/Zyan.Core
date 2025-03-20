using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using CoreRemoting.Toolbox;

namespace Zyan.Communication.CallInterception;

/// <summary>
/// General implementation of a call interception device.
/// </summary>
public class CallInterceptor
{
    /// <summary>
    /// Creates a new instance of the CallInterceptor class.
    /// </summary>
    /// <param name="interfaceType">Interface type of the intercepted component</param>
    /// <param name="uniqueName">Unique name of the intercepted component</param>
    /// <param name="memberType">Type of the intercepted member</param>
    /// <param name="memberName">Name of the intercepted member</param>
    /// <param name="parameterTypes">Types of parameters for the intercepted member</param>
    /// <param name="interceptionHandler">Callback for custom call interception logic</param>
    public CallInterceptor(Type interfaceType, string uniqueName, MemberTypes memberType, string memberName, Type[] parameterTypes, Action<CallInterceptionData> interceptionHandler)
    {
        InterfaceType = interfaceType;
        UniqueNameFilter = string.IsNullOrEmpty(uniqueName) ? Regex.Escape(interfaceType.FullName) : uniqueName;
        MemberType = memberType;
        MemberName = memberName;
        ParameterTypes = parameterTypes;
        InterceptionHandler = interceptionHandler;
        Enabled = true;
    }

    /// <summary>
    /// Creates a new instance of the CallInterceptor class.
    /// </summary>
    /// <param name="interfaceType">Interface type of the intercepted component</param>
    /// <param name="memberType">Type of the intercepted member</param>
    /// <param name="memberName">Name of the intercepted member</param>
    /// <param name="parameterTypes">Types of parameters for the intercepted member</param>
    /// <param name="onInterception">Callback for custom call interception logic</param>
    public CallInterceptor(Type interfaceType, MemberTypes memberType, string memberName, Type[] parameterTypes, Action<CallInterceptionData> onInterception)
        : this(interfaceType, null, memberType, memberName, parameterTypes, onInterception)
    {
    }

    /// <summary>
    /// Compare unique name via regex.
    /// </summary>
    /// <param name="name"></param>
    public bool IsNameMatch(string name)
    {
        // performance optimization
        if (name == UniqueNameFilter)
            return true;

        return Regex.IsMatch(name, UniqueNameFilter);
    }

    /// <summary>
    /// Gets the interface type of the intercepted component.
    /// </summary>
    public Type InterfaceType { get; private set; }

    /// <summary>
    /// Gets the unique name filter of intercepted component.
    /// </summary>
    public string UniqueNameFilter { get; internal set; }

    /// <summary>
    /// Gets the Type of the intercepted member.
    /// </summary>
    public MemberTypes MemberType { get; private set; }

    /// <summary>
    /// Gets the name of the intercepted member.
    /// </summary>
    public string MemberName { get; private set; }

    /// <summary>
    /// Gets the types of parameters for the intercepted member.
    /// <remarks>
    /// CAUTION! Order is relevant.
    /// </remarks>
    /// </summary>
    public Type[] ParameterTypes { get; private set; }

    /// <summary>
    /// Get a callback for custom call interception logic
    /// </summary>
    public Action<CallInterceptionData> InterceptionHandler { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="CallInterceptor"/> is enabled.
    /// </summary>
    /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
    public bool Enabled { get; set; }

    /// <summary>
    /// Pauses call interception for the current thread.
    /// </summary>
    public static IDisposable PauseInterception()
    {
        var oldValue = isPaused.Value;
        isPaused.Value = true;
        return Disposable.Create(() => isPaused.Value = oldValue);
    }

    internal static AsyncLocal<bool> isPaused = new();

    /// <summary>
    /// Gets or sets a value indicating whether call interception is paused for the current thread.
    /// </summary>
    public static bool IsPaused => isPaused.Value;

    /// <summary>
    /// Returns strong-typed call interceptor builder for the component with the specified interface.
    /// </summary>
    /// <typeparam name="TInterface">Component interface.</typeparam>
    public static CallInterceptorBuilder<TInterface> For<TInterface>() =>
        new CallInterceptorBuilder<TInterface>();

    /// <summary>
    /// Returns strong-typed call interceptor builder for the component with the specified interface.
    /// </summary>
    /// <typeparam name="TInterface">Component interface.</typeparam>
    /// <param name="uniqueName">Unique name of the component.</param>
    public static CallInterceptorBuilder<TInterface> For<TInterface>(string uniqueName) =>
        new CallInterceptorBuilder<TInterface>(uniqueName);

    /// <inheritdoc/>
    public override string ToString()
    {
        var parameters = string.Join(", ", (ParameterTypes ?? []).Select(t => t.Name).ToArray());
        if (!string.IsNullOrEmpty(parameters))
        {
            parameters = "(" + parameters + ")";
        }

        return $"{InterfaceType}.{MemberName}{parameters}";
    }

    /// <summary>
    /// Filters unique name over regex.
    /// </summary>
    /// <param name="nameFilter"></param>
    /// <returns></returns>
    public CallInterceptor WithUniqueNameFilter(string nameFilter)
    {
        UniqueNameFilter = nameFilter;
        return this;
    }
}
