namespace SGT_BRIDGE.Utils
{
    public static class EntityHelper
    {
        public static void SetValue<T>(T entity, string propertyName, object value)
        {
            var prop = typeof(T).GetProperty(propertyName);
            if (prop == null)
            {
                throw new ArgumentException($"Property {propertyName} not found on {typeof(T).Name}");
            }

            prop.SetValue(entity, Convert.ChangeType(value, prop.PropertyType));
        }

        public static object GetValue<T>(T entity, string propertyName) 
        {
            var prop = typeof(T).GetProperty(propertyName);

            if(prop == null)
            {
                throw new ArgumentException($"Property {propertyName} not found on {typeof(T).Name}");
            }

            return prop.GetValue(entity);
        }
    }
}
