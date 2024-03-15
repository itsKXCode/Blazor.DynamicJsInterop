using System.Reflection;

namespace Blazor.DynamicJsInterop.Helper;

public class ReflectionHelper {
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
        const BindingFlags bf = BindingFlags.Instance | 
                                BindingFlags.NonPublic | 
                                BindingFlags.DeclaredOnly;

        PropertyInfo? fi;
        while ((fi = type.GetProperty(name, bf)) == null && (type = type.BaseType) != null);
        return fi;
    }
    
    /// <summary>
    /// Invokes a Private Async Method
    /// </summary>
    /// <exception cref="Exception">Method not found</exception>
    public static ValueTask<T> InvokePrivateAsyncMethod<T>(object instance, string methodName, params object?[]? args) {
        MethodInfo? dynMethod = GetPrivateMethod(instance, methodName, args);
        if (dynMethod is null) {
            throw new Exception($"Method '{methodName}' not found");
        }
        var method = dynMethod.MakeGenericMethod(typeof(T));
        return (ValueTask<T>)method.Invoke(instance, args)!;
    }
    
    /// <summary>
    /// Invokes a Private Async Method
    /// </summary>
    /// <exception cref="Exception">Method not found</exception>
    public static ValueTask InvokePrivateAsyncMethod(object instance, string methodName) {
        MethodInfo? dynMethod = GetPrivateMethod(instance, methodName);
        if (dynMethod is null) {
            throw new Exception($"Method '{methodName}' not found");
        }
        
        
        return (ValueTask)dynMethod.Invoke(instance, null)!;
    }
    
    public static MethodInfo? GetPrivateMethod(object instance, string name, params object?[]? args) {
        const BindingFlags bf = BindingFlags.Instance | 
                                BindingFlags.NonPublic;
        var type = instance.GetType();
        var method = type.GetMethod(name, bf, args?.Select(x => x?.GetType()).ToArray());
        return method;
    }
}