using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Returns null if the attribute wasn't found.
        /// </summary>
        public static TAttribute GetCustomAttribute<TAttribute>(this Type type, bool inherit = false)
        {
            var customAttributes = type.GetCustomAttributes(typeof(TAttribute), inherit);

            if (customAttributes.Length == 0)
                return default;

            return (TAttribute)customAttributes[0];
        }

        public static TAttribute[] GetCustomAttributes<TAttribute>(this Type type, bool inherit = false) =>
            type.GetCustomAttributes(typeof(TAttribute), inherit).Select(t => (TAttribute)t).ToArray();
    }
}
