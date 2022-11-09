using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Text;

namespace TelegramBotMarketing.Utility
{
	//https://stackoverflow.com/questions/55476378/how-to-inject-the-dependency-of-the-next-handler-in-a-chain-of-responsibility
	//dude just re-implemented the dependency injection itself for this case. Impressive.
	/*
	 USE:

	 services.Chain<IChainOfResponsibility>()
        .Add<HandlerOne>()
        .Add<HandlerTwo>()
        .Configure();
	 */
	public static class ChainConfigurator
	{
		public static IChainConfigurator<T> Chain<T>(this IServiceCollection services) where T : class
		{
			return new ChainConfiguratorImpl<T>(services);
		}

		public interface IChainConfigurator<T>
		{
			IChainConfigurator<T> Add<TImplementation>() where TImplementation : T;
			void Configure();
		}

		private class ChainConfiguratorImpl<T> : IChainConfigurator<T> where T : class
		{
			private readonly IServiceCollection _services;
			private List<Type> _types;
			private Type _interfaceType;

			public ChainConfiguratorImpl(IServiceCollection services)
			{
				_services = services;
				_types = new List<Type>();
				_interfaceType = typeof(T);
			}

			public IChainConfigurator<T> Add<TImplementation>() where TImplementation : T
			{
				var type = typeof(TImplementation);

				_types.Add(type);

				return this;
			}

			public void Configure()
			{
				if (_types.Count == 0)
					throw new InvalidOperationException($"No implementation defined for {_interfaceType.Name}");

				foreach (var type in _types)
				{
					ConfigureType(type);
				}
			}

			private void ConfigureType(Type currentType)
			{
				// gets the next type, as that will be injected in the current type
				var nextType = _types.SkipWhile(x => x != currentType).SkipWhile(x => x == currentType).FirstOrDefault();

				// Makes a parameter expression, that is the IServiceProvider x 
				var parameter = Expression.Parameter(typeof(IServiceProvider), "x");

				// get constructor with highest number of parameters. Ideally, there should be only 1 constructor, but better be safe.
				var ctor = currentType.GetConstructors().OrderByDescending(x => x.GetParameters().Count()).First();

				// for each parameter in the constructor
				var ctorParameters = ctor.GetParameters().Select(p =>
				{
					// check if it implements the interface. That's how we find which parameter to inject the next handler.
					if (_interfaceType.IsAssignableFrom(p.ParameterType))
					{
						if (nextType is null)
						{
							// if there's no next type, current type is the last in the chain, so it just receives null
							return Expression.Constant(null, _interfaceType);
						}
						else
						{
							// if there is, then we call IServiceProvider.GetRequiredService to resolve next type for us
							return Expression.Call(typeof(ServiceProviderServiceExtensions), "GetRequiredService", new Type[] { nextType }, parameter);
						}
					}

					// this is a parameter we don't care about, so we just ask GetRequiredService to resolve it for us 
					return (Expression)Expression.Call(typeof(ServiceProviderServiceExtensions), "GetRequiredService", new Type[] { p.ParameterType }, parameter);
				});

				// cool, we have all of our constructors parameters set, so we build a "new" expression to invoke it.
				var body = Expression.New(ctor, ctorParameters.ToArray());

				// if current type is the first in our list, then we register it by the interface, otherwise by the concrete type
				var first = _types[0] == currentType;
				var resolveType = first ? _interfaceType : currentType;
				var expressionType = Expression.GetFuncType(typeof(IServiceProvider), resolveType);

				// finally, we can build our expression
				var expression = Expression.Lambda(expressionType, body, parameter);

				// compile it
				var compiledExpression = (Func<IServiceProvider, object>)expression.Compile();

				// and register it in the services collection as transient
				_services.AddTransient(resolveType, compiledExpression);
			}
		}
	}
}
