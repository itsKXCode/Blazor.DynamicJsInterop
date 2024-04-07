using System.Dynamic;
using Blazor.DynamicJsInterop.Contracts;
using Blazor.DynamicJsInterop.Extensions;
using Blazor.DynamicJsInterop.Helper;
using Blazor.DynamicJsInterop.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace Blazor.DynamicJsInterop;

/// <summary>
/// Provides the ability Get Propertys and call Methods in a Isolated JavaScript Module
/// </summary>
internal class DynamicIsolatedJSRuntime<TComponent> : DynamicJSRuntime, IAsyncDisposable, IDynamicJSRuntime<TComponent> where TComponent : ComponentBase {
    private Lazy<Task<IJSObjectReference>> _jsModule;
    private IJSObjectReference? _loadedModule;
    protected override string JsInvokeMethodWrapped => "invokeModuleMethodWrapped";
    protected override string JsGetPropertyValueMethod => "getModulePropertyWrapped";
    protected override string JsInvokeMethod => "invokeModuleMethod";
    
    /// <summary>
    /// Current Module
    /// </summary>
    public dynamic Module => this;
    
    /// <summary>
    /// Global Window Object
    /// </summary>
    public override dynamic Window { get; }

    public DynamicIsolatedJSRuntime(IJSRuntime jsRuntime, IOptions<JavaScriptReferencesOptions> options,
        IAssemblyNameResolver assemblyNameResolver, IDynamicJSRuntime dynamicJsRuntime) 
        : base(jsRuntime, options, assemblyNameResolver){
        _jsModule = new Lazy<Task<IJSObjectReference>>(() =>
            JSRuntime.ImportIsolatedJavaScriptAsync<TComponent>(options.Value, assemblyNameResolver));

        Window = dynamicJsRuntime;
    }

    public Task ImportIsolatedModule() {
        return _jsModule.Value;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result) {
        throw new NotSupportedException(); //TODO Check if possible to get Module exported variables and implement
    }

    /// <summary>
    /// Invokes a Method in the current Isolated JavaScript
    /// </summary>
    public override async ValueTask<T> InvokeAsync<T>(string identifier, params object?[]? args) {
        if (args is null || !args.Any()) 
            throw new ArgumentException("Args needs to have the calling function in first place");
        
        _loadedModule = await _jsModule.Value;
        
        //Unsafe shit
        var targetInstanceId = ReflectionHelper.GetPrivatePropertyValue<long>(_loadedModule, "Id");
       
        //We dont call InvokeAsync on _jsModule because we want to call our custom Methods on the Window
        //Object to retreive the result as a wrapped object
        return await JSRuntime.InvokeAsync<T>(identifier, targetInstanceId, args.First(), args[1..]);
    }

    /// <summary>
    /// Invokes a Method in the current Isolated JavaScript
    /// </summary>
    public override async ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken cancellationToken, params object?[]? args) {
        if (args is null || !args.Any()) 
            throw new ArgumentException("args needs to have the calling function in first place");
        
        _loadedModule = await _jsModule.Value;
        
        //Unsafe shit
        var targetInstanceId = ReflectionHelper.GetPrivatePropertyValue<long>(_loadedModule, "Id");
        
        //We dont call InvokeAsync on _jsModule because we want to call our custom Methods on the Window
        //Object to retreive the result as a wrapped object
        return await JSRuntime.InvokeAsync<T>(identifier, cancellationToken, targetInstanceId, args.First(), args[1..]);
    }

    public ValueTask DisposeAsync() {
        if (_loadedModule != null)
            return _loadedModule.DisposeAsync();
        
        return ValueTask.CompletedTask;
    }
    
}