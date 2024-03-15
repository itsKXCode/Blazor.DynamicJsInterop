using Blazor.DynamicJsInterop.Contracts;
using Blazor.DynamicJsInterop.Options;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace Blazor.DynamicJsInterop;

internal class DynamicJSRuntime : DynamicJSBase, IDynamicJSRuntime {
    private readonly IJSRuntime _jsRuntime;
    private readonly IOptions<JavaScriptReferencesOptions> _options;
    private readonly IAssemblyNameResolver _assemblyNameResolver;
    private string _currentNamespace = string.Empty;

    public virtual dynamic Window => this;

    public DynamicJSRuntime(IJSRuntime jsRuntime, IOptions<JavaScriptReferencesOptions> options,
        IAssemblyNameResolver assemblyNameResolver) : base(jsRuntime, options, assemblyNameResolver) {
        _jsRuntime = jsRuntime;
        _options = options;
        _assemblyNameResolver = assemblyNameResolver;
    }

    public override async ValueTask<TValue> InvokeAsync<TValue>(string identifier, params object?[]? args) {
        var result = await _jsRuntime.InvokeAsync<TValue>(identifier, args);
        return result;
    }

    public override async ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken,
        params object?[]? args) {
        return await _jsRuntime.InvokeAsync<TValue>(identifier, cancellationToken, args);
    }
}