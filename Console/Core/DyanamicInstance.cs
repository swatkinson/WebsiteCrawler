using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pinaka.Core
{
    public class DyanamicInstance<T> where T : class
    {
        public T Instance
        {
            get
            {
                return (T)Activator.CreateInstance(typeof(T));
            }
        }
        public bool SetParameter(string name, object value)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            var objectProperties = this.GetType().GetProperties(flags);
            foreach (var properties in objectProperties.Where(properties => name.Equals(properties.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                properties.SetValue(this, Convert.ChangeType(value, properties.PropertyType), null);
                return true;
            }
            return false;
        }
    }
}
