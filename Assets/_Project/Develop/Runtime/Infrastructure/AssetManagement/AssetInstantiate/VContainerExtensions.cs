using System;
using System.Runtime.CompilerServices;
using VContainer;

namespace Infrastructure.AssetManagement
{
    public static class VContainerExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterNonLazy<T>(this IContainerBuilder builder, Lifetime lifetime = Lifetime.Singleton)
        {
            return RegisterNonLazy<T>(builder, null, lifetime);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterNonLazy<T>(this IContainerBuilder builder, Action<T> executeAfterResolving,
            Lifetime lifetime = Lifetime.Singleton)
        {
            RegistrationBuilder registrationBuilder = builder.Register<T>(lifetime);

            builder.RegisterBuildCallback(container =>
            {
                var result = container.Resolve<T>();
                executeAfterResolving?.Invoke(result);
            });
            return registrationBuilder;
        }


        /// <summary>
        /// Instantiate class (not unity object)
        /// </summary>
        /// <param name="resolver">Container for resolve dependencies</param>
        /// <param name="lifetime"> IMPORTANT: For dependency that instance in additive scene set Scoped or Transient.
        /// For IDisposable read doc https://vcontainer.hadashikick.jp/scoping/lifetime-overview)</param>
        /// <typeparam name="T">Type of instance to create</typeparam>
        /// <returns>Instance of type with resolved dependencies</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Instantiate<T>(this IObjectResolver resolver, Lifetime lifetime = Lifetime.Singleton)
        {
            var registrationBuilder = new RegistrationBuilder(typeof(T), lifetime);
            Registration registration = registrationBuilder.Build();
            return (T)resolver.Resolve(registration);
        }


        /// <inheritdoc cref = "Instantiate{T}(IObjectResolver, Lifetime)"/>
        /// <param name="args"> optional parameters that not registered in container</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Instantiate<T>(this IObjectResolver resolver, Lifetime lifetime = Lifetime.Singleton, params object[] args)
        {
            var registrationBuilder = new RegistrationBuilder(typeof(T), lifetime);

            for (int i = 0; i < args.Length; i++)
            {
                registrationBuilder.WithParameter(args[i].GetType(), args[i]);
            }

            Registration registration = registrationBuilder.Build();
            return (T)resolver.Resolve(registration);
        }


//Obsolete, do not delete
        /*
         /// <summary>
        /// Creates an instance of a class with dependency resolving. Important: Only the first constructor of the class is used.
        /// </summary>
        public static T Instantiate<T>(this IObjectResolver resolver) where T : class
        {
            var instanceType = typeof(T);
            var constructorInfo = instanceType.GetConstructors()[0];
            var constructorParamsInfo = constructorInfo.GetParameters();
            var constructorParams = new object[constructorParamsInfo.Length];

            for (var i = 0; i < constructorParamsInfo.Length; i++)
            {
                var parameterInfo = constructorParamsInfo[i];
                var type = parameterInfo.ParameterType;
                constructorParams[i] = resolver.Resolve(type);
            }

            var instance = Activator.CreateInstance(instanceType, constructorParams);

            return (T)instance;
        }

        /// <summary>
        /// Creates an instance of a class with dependency resolving.
        /// The order of elements in args must be the same as in the constructor.
        /// Container dependencies are resolved automatically.But items from args are in higher priority, than from container (when type matches)
        /// </summary>
        public static T Instantiate<T>(this IObjectResolver resolver, params object[] args) where T : class
        {
            var instanceType = typeof(T);
            var constructorInfo = instanceType.GetConstructors()[0];
            var constructorParamsInfo = constructorInfo.GetParameters();
            var constructorParams = new object[constructorParamsInfo.Length];
            var argsInd = 0;

            for (var i = 0; i < constructorParamsInfo.Length; i++)
            {
                var parameterInfo = constructorParamsInfo[i];
                var type = parameterInfo.ParameterType;
                var byArgs = args[argsInd].GetType() == type;
                var parameter = byArgs ? args[argsInd] : resolver.Resolve(type);

                constructorParams[i] = parameter;

                if (byArgs)
                {
                    argsInd++;
                    argsInd = Math.Min(argsInd, args.Length - 1);
                }
            }

            var instance = Activator.CreateInstance(instanceType, constructorParams);

            return (T)instance;
        }


        public static RegistrationBuilder Register<T>(this IContainerBuilder builder, Lifetime lifetime = Lifetime.Singleton, params object[] args) where T : class
        {
            return builder.Register(resolver => resolver.Instantiate<T>(lifetime, args), lifetime);
        }*/
    }
}