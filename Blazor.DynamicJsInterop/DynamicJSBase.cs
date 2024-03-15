using System.Dynamic;
using System.Text.Json;
using Blazor.DynamicJsInterop.Contracts;
using Blazor.DynamicJsInterop.Options;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace Blazor.DynamicJsInterop;

internal abstract class DynamicJSBase : DynamicObject {
    protected readonly IJSRuntime JSRuntime;
    protected readonly IOptions<JavaScriptReferencesOptions> Options;
    protected readonly IAssemblyNameResolver AssemblyNameResolver;

    public virtual dynamic Global => this;

    protected virtual string JsGetPropertyMethod => "getProperty";
    protected virtual string JsInvokeMethod => "invokeMethod";

    protected DynamicJSBase(IJSRuntime jsRuntime, IOptions<JavaScriptReferencesOptions> options,
        IAssemblyNameResolver assemblyNameResolver) {
        JSRuntime = jsRuntime;
        Options = options;
        AssemblyNameResolver = assemblyNameResolver;
    }

    public abstract ValueTask<TValue> InvokeAsync<TValue>(string identifier, params object?[]? args);

    public abstract ValueTask<TValue> InvokeAsync<TValue>(string identifier,
        CancellationToken cancellationToken,
        object?[]? args);

    public override bool TryGetMember(GetMemberBinder binder, out object? result) {
        async Task<object> GetValue() {
            //Get it as JsonElement first so we can get Informations about the Property as we cant call
            //InvokeAsync<IJSObjectReference> on non objects (string, number etc)
            var property = await InvokeAsync<JsonElement>(JsGetPropertyMethod, binder.Name);
            return await GetValueFromJsonElement(property, binder.Name);
        }
        
        result = GetValue();
        return true;
    }

    protected async Task<object> GetValueFromJsonElement(JsonElement element, object propertyName) {
        return element.ValueKind switch {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.GetDecimal(),
            JsonValueKind.False => false,
            JsonValueKind.True => true,
            JsonValueKind.Null => null,
            JsonValueKind.Array => GetObject(await InvokeAsync<IJSObjectReference>(JsGetPropertyMethod, propertyName), element),
            JsonValueKind.Undefined => null,
            JsonValueKind.Object => GetObject(await InvokeAsync<IJSObjectReference>(JsGetPropertyMethod, propertyName), element)
        };
    }
    

    public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result) {
        var csharpBinder = binder.GetType()
            .GetInterface("Microsoft.CSharp.RuntimeBinder.ICSharpInvokeOrInvokeMemberBinder");
        if (csharpBinder != null) {
            var typeArgs = (csharpBinder.GetProperty("TypeArguments")?.GetValue(binder, null) as IList<Type>);

            var fullName = binder.Name;

            var arguments = new List<object?>() { fullName };
            if (args is not null && args.Length > 0) {
                arguments.AddRange(args);
            }
            
            if (typeArgs?.Count == 0) {
                async Task<object> GetValue() {
                    //We need to get the IJSObject reference first as we dont want to double invoke the method
                    //the returned object will have a "value" property in it with the actual object/value
                    var result = await InvokeAsync<IJSObjectReference>(JsInvokeMethod, arguments.ToArray());
                    
                    //Use our object to get the actual Value
                    await using dynamic internalObject = GetObject(result);
                    return await internalObject.value;
                }

                result = GetValue();
                return true;
            } else if (typeArgs?.Count == 1) {
                var method = typeof(DynamicJSBase).GetMethod(nameof(InvokeAsync),
                    new[] { typeof(string), typeof(object?[]) });
                var generic = method?.MakeGenericMethod(typeArgs[0]);

                if (generic != null) {
                    result = generic.Invoke(this, [JsInvokeMethod, arguments.ToArray()]);
                    return true;
                }
            } else {
                throw new NotSupportedException("Multiple Generic Arguments are not supported");
            }
        }

        result = null;
        return false;
    }
    
    protected DynamicJSObjectReference GetObject(IJSObjectReference jsObjectReference, JsonElement jsonElement) {
        return new DynamicJSObjectReference(jsObjectReference, jsonElement, JSRuntime, Options, AssemblyNameResolver);
    }
    
    protected DynamicJSObjectReference GetObject(IJSObjectReference jsObjectReference) {
        return new DynamicJSObjectReference(jsObjectReference, JSRuntime, Options, AssemblyNameResolver);
    }
}