using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XRayBuilderGUI.Libraries.Reflection.Util
{
    public static class ReflectionUtil
    {
        public static IEnumerable<Type> GetConcreteFromAbstract<TAbstract>() where TAbstract : class
            => Assembly.GetAssembly(typeof(TAbstract))
                .GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(TAbstract)));

        public static IEnumerable<Type> GetConcreteFromInterface<TInterface>()
            => Assembly.GetAssembly(typeof(TInterface))
                .GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && typeof(TInterface).IsAssignableFrom(type));

        public static IEnumerable<Type> GetConcreteFromInterface(Type @interface)
            => Assembly.GetAssembly(@interface)
                .GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && @interface.IsAssignableFrom(type));
    }
}
