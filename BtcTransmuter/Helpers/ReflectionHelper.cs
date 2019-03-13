using System;

namespace BtcTransmuter.Helpers
{
    public static class ReflectionHelper
    {
        public static bool Implements<I>(this Type type, I @interface) where I : class
        {
            if(((@interface as Type)==null) || !(@interface as Type).IsInterface)
                throw new ArgumentException("Only interfaces can be 'implemented'.");

            return (@interface as Type).IsAssignableFrom(type);
        }
        
        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic ) {
            while (toCheck != null && toCheck != typeof(object)) {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur) {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}