using System.Reflection;
using Blazor.DynamicJsInterop.Contracts;
using Microsoft.AspNetCore.Components;

namespace Blazor.DynamicJsInterop.Options;

public delegate string PathFormatter(Assembly assembly, Type componentType, bool isExternalAssembly);

public sealed class JavaScriptReferencesOptions : IPathFormatterResolver {
    private readonly IDictionary<string, PathFormatter> _assemblyFormatters = new Dictionary<string, PathFormatter>();

    private PathFormatter _defaultFormatter =
        (assembly, componentType, isExternal) => {
            var root = isExternal ? "/_content" : "";
            var assemblyName = assembly.GetName().Name;

            return $".{root}/{assemblyName}" + //This part depends on the acutal AssemblyName
                   $"{componentType.Namespace.Replace(assemblyName, string.Empty).Replace(".", "/")}" + //TODO wee need to set the actual path here not the namespace
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