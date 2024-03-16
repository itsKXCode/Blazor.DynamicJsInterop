using Blazor.DynamicJsInterop.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blazor.DynamicJsInterop.Extensions;

internal static class JSRuntimeExtensions {
    /// <summary>
    /// Tries to Import the Isolated JavaScript File of the given Component
    /// </summary>
    internal static async Task<IJSObjectReference> ImportIsolatedJavaScriptAsync<TComponent>(this IJSRuntime js,
        IPathFormatterResolver formatterResolver, IAssemblyNameResolver assemblyNameResolver)
        where TComponent : ComponentBase {
        
        var pathFormatter = formatterResolver.ResolvePathFormatter<TComponent>();
        if (pathFormatter != null) {
            var path = pathFormatter(typeof(TComponent).Assembly, typeof(TComponent),
                assemblyNameResolver.IsExternalAssembly<TComponent>());
            var module = await js.InvokeAsync<IJSObjectReference>("import", path);
            return module;
        } else {
            throw new InvalidOperationException("Unable to resolve javascript file path since there is no matching path formatter and the default formatter is not defined.");
        }
    }
}