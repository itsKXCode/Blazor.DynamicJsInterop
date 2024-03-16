using Blazor.DynamicJsInterop.Contracts;
using Blazor.DynamicJsInterop.Options;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace Blazor.DynamicJsInterop;

/// <summary>
/// Provides the ability Get Propertys and call Methods of the JavaScript Object
/// </summary>
internal class DynamicJSRuntime : DynamicJSBase, IDynamicJSRuntime {
    private readonly IJSRuntime _jsRuntime;

    public virtual dynamic Window => this;

    public DynamicJSRuntime(IJSRuntime jsRuntime, IOptions<JavaScriptReferencesOptions> options,
        IAssemblyNameResolver assemblyNameResolver) : base(jsRuntime, options, assemblyNameResolver) {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Invokes a Method on the JavaScript Window Object
    /// </summary>
    public override async ValueTask<TValue> InvokeAsync<TValue>(string identifier, params object?[]? args) {
        var result = await _jsRuntime.InvokeAsync<TValue>(identifier, args);
        return result;
    }

    /// <summary>
    /// Invokes a Method on the JavaScript Window Object
    /// </summary>
    public override async ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, params object?[]? args) {
        return await _jsRuntime.InvokeAsync<TValue>(identifier, cancellationToken, args);
    }
}