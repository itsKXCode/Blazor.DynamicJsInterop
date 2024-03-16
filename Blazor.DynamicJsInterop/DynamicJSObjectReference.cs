using System.Dynamic;
using System.Text.Json;
using Blazor.DynamicJsInterop.Contracts;
using Blazor.DynamicJsInterop.Options;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace Blazor.DynamicJsInterop;

internal class DynamicJSObjectReference : DynamicJSBase, IDynamicJSObjectReference {
    private IJSObjectReference _objectReference;
    private readonly JsonElement _jsonElement;

    public DynamicJSObjectReference(IJSObjectReference objectReference, JsonElement jsonElement, IJSRuntime jsRuntime,
        IOptions<JavaScriptReferencesOptions> options,
        IAssemblyNameResolver assemblyNameResolver) : base(jsRuntime, options, assemblyNameResolver) {
        _objectReference = objectReference;
        _jsonElement = jsonElement;
    }
    
    internal DynamicJSObjectReference(IJSObjectReference objectReference, IJSRuntime jsRuntime,
        IOptions<JavaScriptReferencesOptions> options,
        IAssemblyNameResolver assemblyNameResolver) : base(jsRuntime, options, assemblyNameResolver) {
        _objectReference = objectReference;
    }

    public override ValueTask<TValue> InvokeAsync<TValue>(string identifier, params object?[]? args) {
        return _objectReference.InvokeAsync<TValue>(identifier, args);
    }

    public override ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken,
        object?[]? args) {
        return _objectReference.InvokeAsync<TValue>(identifier, cancellationToken, args);
    }

    public override bool TryConvert(ConvertBinder binder, out object? result) {
        result = _jsonElement.Deserialize(binder.Type);
        return true;
    }
    
    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result) {
        if (indexes.Length > 0) {
            //TODO Support Multidimensional Arrays
            throw new NotSupportedException(); 
        }
        
        async Task<object> GetValue() {
            //Get it as JsonElement first so we can get Informations about the Property as we cant call
            //InvokeAsync<IJSObjectReference> on non objects (string, number etc)
            var property = await InvokeAsync<JsonElement>(JsGetPropertyMethod, indexes[0]);
            return await GetValueFromJsonElement(property, indexes[0]);
        }
        
        result = new JsTask(this, GetValue());
        return true;
    }
    
    public ValueTask DisposeAsync() {
        return _objectReference.DisposeAsync();
    }
}