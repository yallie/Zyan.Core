using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreRemoting.DependencyInjection;
using Zyan.Communication.Toolbox;

namespace Zyan.InterLinq;

/// <summary>
/// Wraps component method returning IEnumerable{T} or IQueryable{T}.
/// </summary>
internal class ZyanMethodQueryHandler : IQueryHandler
{
	public IDependencyInjectionContainer Container { get; private set; }

	public string ComponentName { get; private set; }

	public MethodInfo MethodInfo { get; private set; }

	public string MethodQueryHandlerName =>
		GetMethodQueryHandlerName(ComponentName, MethodInfo);

	public static string GetMethodQueryHandlerName(string componentName, MethodInfo method) =>
		componentName + "." + method.GetSignature();

	static MethodInfo getMethodInfo = typeof(ZyanMethodQueryHandler).GetMethod("Get", []);

	public ZyanMethodQueryHandler(IDependencyInjectionContainer container, string componentName, MethodInfo method)
	{
		Container = container;
		ComponentName = componentName;
		MethodInfo = method;
	}

	private class CurrentQuerySession
	{
		public ServiceRegistration Registration { get; set; }

		public object Instance { get; set; }
	}

	[ThreadStatic]
	private static CurrentQuerySession querySession;

	public bool StartSession()
	{
		// get component instance and save to the current session
		var registration = Container.GetServiceRegistration(ComponentName);
		querySession = new CurrentQuerySession
		{
			Registration = registration,
			Instance = Container.GetService(ComponentName),
		};

		return true;
	}

	public bool CloseSession()
	{
		// cleanup component instance and release the current session
		if (querySession != null)
		{
			var instance = querySession.Instance;
			var registration = querySession.Registration;
			if (instance != null && registration != null) // && registration.ActivationType == ActivationType.SingleCall)
			{
				//Catalog.CleanUpComponentInstance(registration, instance);
				querySession.Instance = null;
			}

			querySession = null;
		}

		return true;
	}

	public IQueryable Get(Type type)
	{
		// create generic version of this method and invoke it
		var genericMethodInfo = getMethodInfo.MakeGenericMethod(type);
		var result = genericMethodInfo.Invoke(this, []);
		return (IQueryable)result;
	}

	public IQueryable<T> Get<T>() where T : class
	{
		if (querySession == null)
		{
			throw new InvalidOperationException("Session is not started. ZyanMethodQueryHandler requires that StartSession method is called before Get<T>.");
		}

		// get component instance created by
		var instance = querySession.Instance;
		var instanceType = instance.GetType();

		// create generic method
		var genericMethodInfo = instanceType.GetMethod(MethodInfo.Name, [typeof(T)], argumentTypes: []);
		if (genericMethodInfo == null)
		{
			var methodSignature = MessageHelpers.GetMethodSignature(instanceType, MethodInfo.Name, []);
			var exceptionMessage = "Method not found: " + methodSignature;
			throw new MissingMethodException(exceptionMessage);
		}

		// invoke Get<T> method and return IQueryable<T>
		object result = genericMethodInfo.Invoke(instance, []);
		if (result is IQueryable<T>)
		{
			return result as IQueryable<T>;
		}

		if (result is IEnumerable<T>)
		{
			return (result as IEnumerable<T>).AsQueryable();
		}

		return null;
	}
}
