using System.Dynamic;
using Blazor.DynamicJsInterop.Contracts;
using Blazor.DynamicJsInterop.Extensions;
using Blazor.DynamicJsInterop.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace Blazor.DynamicJsInterop;

internal class DynamicIsolatedJSRuntime<TComponent> : DynamicJSRuntime, IDynamicJSRuntime<TComponent> where TComponent : ComponentBase {
    private readonly IJSRuntime _jsRuntime;
    private Lazy<Task<IJSObjectReference>> _jsModule;
    private string _currentNamespace = string.Empty;

    public dynamic Exports => this;
    public override dynamic Global { get; }

    public DynamicIsolatedJSRuntime(IJSRuntime jsRuntime, IOptions<JavaScriptReferencesOptions> options,
        IAssemblyNameResolver assemblyNameResolver, IDynamicJSRuntime dynamicJsRuntime) 
        : base(jsRuntime, options, assemblyNameResolver) {
        _jsRuntime = jsRuntime;
        _jsModule = new Lazy<Task<IJSObjectReference>>(() =>
            _jsRuntime.ImportIsolatedJavaScriptAsync<TComponent>(options.Value, assemblyNameResolver));

        Global = dynamicJsRuntime;
    }

    public override async ValueTask<T> InvokeAsync<T>(string identifier, object?[]? args) {
        var module = await _jsModule.Value;
        var result = await module.InvokeAsync<T>(identifier, args);
        return result;
    }

    public override async ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken cancellationToken, object?[]? args) {
        var module = await _jsModule.Value;
        return await module.InvokeAsync<T>(identifier, cancellationToken, args);
    }

    public async ValueTask DisposeAsync() {
        var module = await _jsModule.Value;
        await module.DisposeAsync();
    }


    // public override bool TryGetMember(GetMemberBinder binder, out object? result) {
    //     var newNamespace = CurrentNamespace + "." + binder.Name;
    //     newNamespace = newNamespace.Trim('.');
    //     result = new DynamicIsolatedJSRuntime<TComponent>(_jsRuntime, _options, _assemblyNameResolver) {
    //         CurrentNamespace = newNamespace
    //     };
    //         
    //     return true;
    // }

    public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result) {
        var csharpBinder = binder.GetType()
            .GetInterface("Microsoft.CSharp.RuntimeBinder.ICSharpInvokeOrInvokeMemberBinder");
        if (csharpBinder != null) {
            var typeArgs = (csharpBinder.GetProperty("TypeArguments")?.GetValue(binder, null) as IList<Type>);

            var fullName = !string.IsNullOrWhiteSpace(_currentNamespace)
                ? _currentNamespace + "." + binder.Name
                : binder.Name;
            
            if (typeArgs?.Count == 0) {
                result = InvokeAsync<ExpandoObject>(fullName, args);
                return true;
            } else if (typeArgs?.Count == 1) {
                var method = typeof(DynamicIsolatedJSRuntime<TComponent>).GetMethod(nameof(InvokeAsync),
                    new[] { typeof(string), typeof(object?[]) });
                var generic = method?.MakeGenericMethod(typeArgs[0]);

                if (generic != null) {
                    result = generic.Invoke(this, new object[] { fullName, args ?? new object[] { } });
                    return true;
                }
            }
        }

        result = null;
        return false;
    }

}