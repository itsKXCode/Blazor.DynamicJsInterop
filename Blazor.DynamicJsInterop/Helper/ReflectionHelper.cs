using System.Reflection;

namespace Blazor.DynamicJsInterop.Helper;

public static class ReflectionHelper {
    /// <summary>
    /// Gets the value of a Private field
    /// </summary>
    /// <exception cref="Exception">Field not found</exception>
    public static T GetPrivatePropertyValue<T>(object instance, string fieldName) {
        var property = GetPrivateProperty(instance.GetType(), fieldName);
        if (property is null) {
            throw new Exception($"Field '{fieldName}' not found");
        }

        return (T)property.GetValue(instance)!;
    }

    /// <summary>
    /// Gets a Private Property of a class even if its in a base class
    /// </summary>
    public static PropertyInfo? GetPrivateProperty(Type type, string name) {
        const BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        PropertyInfo? fi;
        while ((fi = type.GetProperty(name, bf)) == null && (type = type.BaseType) != null);
        return fi;
    }

    /// <summary>
    /// Gets a Private Method of a class even if its in a base class
    /// </summary>
    public static MethodInfo? GetPrivateMethod(object instance, string name, params object?[]? args) {
        const BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic;

        var type = instance.GetType();
        var method = type.GetMethod(name, bf, args?.Select(x => x?.GetType()).ToArray());
        return method;
    }
}