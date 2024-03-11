using System.Dynamic;
using Blazor.DynamicJsInterop.Contracts;
using Blazor.DynamicJsInterop.Options;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace Blazor.DynamicJsInterop;

internal class DynamicJSRuntime : DynamicObject, IDynamicJSRuntime {
    private readonly IJSRuntime _js;
    private readonly IOptions<JavaScriptReferencesOptions> _options;
    private readonly IAssemblyNameResolver _assemblyNameResolver;
    private string _currentNamespace = string.Empty;

    public virtual dynamic Global => this;

    public DynamicJSRuntime(IJSRuntime js, IOptions<JavaScriptReferencesOptions> options,
        IAssemblyNameResolver assemblyNameResolver) {
        _js = js;
        _options = options;
        _assemblyNameResolver = assemblyNameResolver;
    }

    public virtual async ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) {
        var result = await _js.InvokeAsync<TValue>(identifier, args);
        return result;
    }

    public virtual async ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken,
        object?[]? args) {
        return await _js.InvokeAsync<TValue>(identifier, cancellationToken, args);
    }

    public async ValueTask DisposeAsync() {
    }


    public override bool TryGetMember(GetMemberBinder binder, out object? result) {
        var newNamespace = _currentNamespace + "." + binder.Name;
        newNamespace = newNamespace.Trim('.');
        result = new DynamicJSRuntime(_js, _options, _assemblyNameResolver) {
            _currentNamespace = newNamespace
        };

        return true;
    }

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
                var method = typeof(DynamicJSRuntime).GetMethod(nameof(InvokeAsync),
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