using System;
using System.Linq;
using SimpleInjector;
using SimpleInjector.Diagnostics;
using XRayBuilderGUI.Libraries.Reflection.Util;

namespace XRayBuilderGUI.Libraries.SimpleInjector.Extensions
{
    public static class SimpleInjectorExtensions
    {
        /// <summary>
        /// Register <typeparamref name="TConcrete"/> as transient, ignoring the disposable transient warning
        /// </summary>
        public static void RegisterTransientIgnore<TConcrete>(this Container container, string message) where TConcrete : class
        {
            container.Register<TConcrete>(Lifestyle.Transient);
            var registration = container.GetRegistration(typeof(TConcrete)).Registration;
            registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, message);
        }

        /// <summary>
        /// Auto-register any types derived from <typeparamref name="TAbstract"/> with the given <paramref name="lifestyle"/>
        /// </summary>
        public static void AutoregisterConcreteFromAbstract<TAbstract>(this Container container, Lifestyle lifestyle) where TAbstract : class
        {
            if (!typeof(TAbstract).IsAbstract)
                throw new ArgumentException("Non-abstract type", nameof(TAbstract));

            var types = ReflectionUtil.GetConcreteFromAbstract<TAbstract>();
            foreach (var type in types)
                container.Register(type, type, lifestyle);
        }

        /// <summary>
        /// Auto-register any types derived from <typeparamref name="TInterface"/> with the given <paramref name="lifestyle"/>
        /// </summary>
        public static void AutoregisterConcreteFromInterface<TInterface>(this Container container, Lifestyle lifestyle)
        {
            if (!typeof(TInterface).IsInterface)
                throw new ArgumentException("Type not an interface", nameof(TInterface));

            var types = ReflectionUtil.GetConcreteFromInterface<TInterface>().ToArray();
            foreach (var type in types)
                container.Register(type, type, lifestyle);
        }

        /// <summary>
        /// Auto-register any types derived from <paramref name="@interface"/> with the given <paramref name="lifestyle"/>
        /// </summary>
        public static void AutoregisterConcreteFromInterface(this Container container, Type @interface, Lifestyle lifestyle)
        {
            if (!@interface.IsInterface)
                throw new ArgumentException("Type not an interface", nameof(@interface));

            var types = ReflectionUtil.GetConcreteFromInterface(@interface).ToArray();
            foreach (var type in types)
                container.Register(type, type, lifestyle);
        }

        /// <summary>
        /// Auto-register any types derived from <typeparamref name="TInterface"/> as transient, ignoring the disposable transient warning
        /// </summary>
        public static void AutoregisterDisposableTransientConcreteFromInterface<TInterface>(this Container container, string message)
        {
            if (!typeof(TInterface).IsInterface)
                throw new ArgumentException("Type not an interface", nameof(TInterface));

            var types = ReflectionUtil.GetConcreteFromInterface<TInterface>().ToArray();
            foreach (var type in types)
            {
                container.Register(type, type, Lifestyle.Transient);
                var registration = container.GetRegistration(type).Registration;
                registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, message);
            }
        }
    }
}
