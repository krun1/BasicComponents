namespace BasicComponents;

public static class TypeExtensions
{
    public static bool IsSubclassOfGenericInterface(this Type type, Type genericDefinition)
    {
        if (!genericDefinition.IsGenericTypeDefinition)
            throw new ArgumentException($"Type {genericDefinition} is not a generic type definition");
        if (type.IsGenericType && type.GetGenericTypeDefinition() == genericDefinition)
            return true;
        return type.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericDefinition);
    }

    public static Type[] GetGenericParametersOfGenericInterface(this Type type, Type genericDefinition)
    {
        if (!genericDefinition.IsGenericTypeDefinition)
            throw new ArgumentException($"Type {genericDefinition} is not a generic type definition");
        var i = type.GetInterfaces()
            .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericDefinition);

        return i.GetGenericArguments();
    }
    
    public static bool IsSubclassOfGenericClass(this Type type, Type genericDefinition)
    {
        if (!genericDefinition.IsGenericTypeDefinition)
            throw new ArgumentException($"Type {genericDefinition} is not a generic type definition");
        if (type.GetGenericTypeDefinition() == genericDefinition)
            return true;
        if (type == typeof(object) || type.BaseType == null)
            return false;
        return type.BaseType.IsSubclassOfGenericClass(genericDefinition);
    }

    public static bool ImplementInterface(this Type type, Type @interface)
    {
        if (!@interface.IsInterface)
            throw new ArgumentException($"Type {@interface} is not an interface");
        return type.GetInterfaces().Contains(@interface);
    }
}