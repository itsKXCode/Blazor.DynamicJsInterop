using System.Reflection;
using Blazor.DynamicJsInterop.Contracts;
using Microsoft.AspNetCore.Components;

namespace Blazor.DynamicJsInterop.Options;

public delegate string PathFormatter(Assembly assembly, Type componentType, bool isExternalAssembly);

public sealed class JavaScriptReferencesOptions : IPathFormatterResolver {
    private readonly IDictionary<string, PathFormatter> _assemblyFormatters = new Dictionary<string, PathFormatter>();

    private PathFormatter _defaultFormatter =
        (assembly, componentType, isExternal) => {
            var assemblyName = assembly.GetName().Name;
            var root = isExternal ? $"/_content/{assemblyName}" : "";//This part depends on the acutal AssemblyName
            var nameSpace = componentType.Namespace.StartsWith(assemblyName)
                ? componentType.Namespace.Remove(0, assemblyName.Length)
                : componentType.Namespace;
            
            return $".{root}" + 
                   $"/{nameSpace.Replace(".", "/").Trim('/')}" + //TODO wee need to set the actual path here not the namespace
                   $"/{componentType.Name}.razor.js";
        };

    public JavaScriptReferencesOptions MapAssembly(Assembly assembly) {
        _assemblyFormatters[assembly.FullName] = _defaultFormatter;
        return this;
    }

    public PathFormatter ResolvePathFormatter<TComponent>()
        where TComponent : ComponentBase {
        if (!_assemblyFormatters.TryGetValue(typeof(TComponent).Assembly.FullName!, out var resolver)) {
            resolver = _defaultFormatter;
        }

        return resolver;
    }
}