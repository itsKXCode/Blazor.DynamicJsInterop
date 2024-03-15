using System.Dynamic;
using Blazor.DynamicJsInterop.Contracts;
using Blazor.DynamicJsInterop.Extensions;
using Blazor.DynamicJsInterop.Helper;
using Blazor.DynamicJsInterop.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace Blazor.DynamicJsInterop;

internal class DynamicIsolatedJSRuntime<TComponent> : DynamicJSRuntime, IDynamicJSRuntime<TComponent> where TComponent : ComponentBase {
    private Lazy<Task<IJSObjectReference>> _jsModule;

    public dynamic Exports => this;
    public override dynamic Window { get; }

    protected override string JsInvokeMethod => "invokeModuleMethod";
    protected override string JsGetPropertyMethod => "getModuleProperty";

    public DynamicIsolatedJSRuntime(IJSRuntime jsRuntime, IOptions<JavaScriptReferencesOptions> options,
        IAssemblyNameResolver assemblyNameResolver, IDynamicJSRuntime dynamicJsRuntime) 
        : base(jsRuntime, options, assemblyNameResolver){
        _jsModule = new Lazy<Task<IJSObjectReference>>(() =>
            JSRuntime.ImportIsolatedJavaScriptAsync<TComponent>(options.Value, assemblyNameResolver));

        Window = dynamicJsRuntime;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result) {
        throw new NotSupportedException(); //TODO Check if possible to get Module exported variables and implement
    }

    public override async ValueTask<T> InvokeAsync<T>(string identifier, params object?[]? args) {
        if (args is null || !args.Any()) 
            throw new ArgumentException("args needs to have the calling function in first place");
        
        var module = await _jsModule.Value;
        
        //Unsafe shit
        var targetInstanceId = ReflectionHelper.GetPrivatePropertyValue<long>(module, "Id");
        
        return await JSRuntime.InvokeAsync<T>(identifier, targetInstanceId, args.First(), args[1..]);
    }

    public override async ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken cancellationToken, params object?[]? args) {
        if (args is null || !args.Any()) 
            throw new ArgumentException("args needs to have the calling function in first place");
        
        var module = await _jsModule.Value;
        
        //Unsafe shit
        var targetInstanceId = ReflectionHelper.GetPrivatePropertyValue<long>(module, "Id");
        
        return await JSRuntime.InvokeAsync<T>(identifier, cancellationToken, targetInstanceId, args.First(), args[1..]);
    }

    public async ValueTask DisposeAsync() {
        var module = await _jsModule.Value;
        await module.DisposeAsync();
    }
}