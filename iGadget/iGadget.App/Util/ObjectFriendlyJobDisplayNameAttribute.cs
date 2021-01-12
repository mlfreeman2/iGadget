using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using Hangfire;
using Hangfire.Common;
using Hangfire.Dashboard;

namespace iGadget.App.Util
{

    /// <summary>
    /// Specifies a display name for a job method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ObjectFriendlyJobDisplayNameAttribute : JobDisplayNameAttribute
    {
        private static readonly ConcurrentDictionary<Type, ResourceManager> _resourceManagerCache = new ConcurrentDictionary<Type, ResourceManager>();

        public ObjectFriendlyJobDisplayNameAttribute(string displayName) : base(displayName) { }

        public override string Format(DashboardContext context, Job job)
        {
            var format = DisplayName;

            if (ResourceType != null)
            {
                format = _resourceManagerCache
                    .GetOrAdd(ResourceType, InitResourceManager)
                    .GetString(DisplayName, CultureInfo.CurrentUICulture);

                if (string.IsNullOrEmpty(format))
                {
                    // failed to localize display name string, use it as is
                    format = DisplayName;
                }
            }

            return string.Format(new ReflectionFormatProvider(), format, job.Args.ToArray());
        }

        private static ResourceManager InitResourceManager(Type type)
        {
            var prop = type.GetTypeInfo().GetDeclaredProperty("ResourceManager");
            if (prop != null && prop.PropertyType == typeof(ResourceManager) && prop.CanRead && prop.GetMethod.IsStatic)
            {
                // use existing resource manager if possible
                return (ResourceManager)prop.GetValue(null);
            }

            return new ResourceManager(type);
        }
    }

    // Shamelessly borrowed from https://stackoverflow.com/a/357780
    public class ReflectionFormatProvider : IFormatProvider, ICustomFormatter
    {
        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            string[] formats = (format ?? string.Empty).Split(new char[] { ':' }, 2);
            string propertyName = formats[0].TrimEnd('}');
            string suffix = formats[0].Substring(propertyName.Length);
            string propertyFormat = formats.Length > 1 ? formats[1] : null;

            PropertyInfo pi = arg.GetType().GetProperty(propertyName);
            if (pi == null || pi.GetGetMethod() == null)
            {
                // Pass thru
                return (arg is IFormattable) ?
                    ((IFormattable)arg).ToString(format, formatProvider)
                    : arg.ToString();
            }

            object value = pi.GetGetMethod().Invoke(arg, null);
            return (propertyFormat == null) ?
                (value ?? string.Empty).ToString() + suffix
                : string.Format("{0:" + propertyFormat + "}", value);
        }
    }
}
