using System.Dynamic;
using System.Text.Json;
using Blazor.DynamicJsInterop.Contracts;
using Blazor.DynamicJsInterop.Options;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace Blazor.DynamicJsInterop;

/// <summary>
/// Provides the ability Get Propertys and call Methods of a associated JavaScript Object
/// </summary>
internal class DynamicJSObjectReference : DynamicJSBase, IDynamicJSObjectReference {
    private IJSObjectReference _objectReference;
    
    /// <summary>
    /// Represents the current Referenced JavaScript object as Json
    /// </summary>
    private readonly JsonElement? _jsonElement;

    public DynamicJSObjectReference(IJSObjectReference objectReference, JsonElement jsonElement, IJSRuntime jsRuntime,
        IOptions<JavaScriptReferencesOptions> options,
        IAssemblyNameResolver assemblyNameResolver) : base(jsRuntime, options, assemblyNameResolver) {
        _objectReference = objectReference;
        _jsonElement = jsonElement;
    }
    
    public DynamicJSObjectReference(IJSObjectReference objectReference, IJSRuntime jsRuntime,
        IOptions<JavaScriptReferencesOptions> options,
        IAssemblyNameResolver assemblyNameResolver) : base(jsRuntime, options, assemblyNameResolver) {
        _objectReference = objectReference;
    }

    /// <summary>
    /// Invokes a Method on the current JavaScript Object
    /// </summary>
    public override ValueTask<TValue> InvokeAsync<TValue>(string identifier, params object?[]? args) {
        return _objectReference.InvokeAsync<TValue>(identifier, args);
    }

    /// <summary>
    /// Invokes a Method on the current JavaScript Object
    /// </summary>
    public override ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) {
        return _objectReference.InvokeAsync<TValue>(identifier, cancellationToken, args);
    }

    /// <summary>
    /// Converts the current JavaScript Object to a C# object by Deserializing the Json
    /// </summary>
    /// <param name="binder"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public override bool TryConvert(ConvertBinder binder, out object? result) {
        
        //In some cases there is no JsonElement of the Object, e.g the Object is not serializable
        if (_jsonElement is null) {
            result = null;
            return false;
        }
        
        result = _jsonElement.Value.Deserialize(binder.Type);
        return true;
    }
    
    /// <summary>
    /// Gets a Task which receives a Object at a Array Index of the current object
    /// </summary>
    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result) {
        if (indexes.Length > 0) {
            //TODO Support Multidimensional Arrays
            throw new NotSupportedException(); 
        }
        
        async ValueTask<object> GetValue() {
            //Get it as JsonElement first so we can get Informations about the Property as we cant call
            //InvokeAsync<IJSObjectReference> on non objects (string, number etc)
            var property = await InvokeAsync<JsonElement>(JsGetPropertyMethod, indexes[0]);
            return await GetValueFromJsonElement(property, indexes[0]);
        }
        
        result = new JSTask(this, GetValue());
        return true;
    }
    
    public ValueTask DisposeAsync() {
        return _objectReference.DisposeAsync();
    }
}