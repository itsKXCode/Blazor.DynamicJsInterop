using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Blazor.DynamicJsInterop.Contracts;
using Blazor.DynamicJsInterop.Options;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace Blazor.DynamicJsInterop;

/// <summary>
/// Base Class for all Dynamic JS implementation, provides implementations for TryGetMember and TryInvokeMember
/// </summary>
internal abstract class DynamicJSBase : DynamicObject {
    public readonly IJSRuntime JSRuntime;
    public readonly IOptions<JavaScriptReferencesOptions> Options;
    public readonly IAssemblyNameResolver AssemblyNameResolver;
    
    protected virtual string JsGetPropertyTypeMethod => "getPropertyType";
    protected virtual string JsGetPropertyMethod => "getProperty";
    protected virtual string JsInvokeMethodWrapped => "invokeMethodWrapped";
    protected virtual string JsInvokeMethod => "invokeMethod";

    protected DynamicJSBase(IJSRuntime jsRuntime, IOptions<JavaScriptReferencesOptions> options,
        IAssemblyNameResolver assemblyNameResolver) {
        JSRuntime = jsRuntime;
        Options = options;
        AssemblyNameResolver = assemblyNameResolver;
    }

    public abstract ValueTask<TValue> InvokeAsync<TValue>(string identifier, params object?[]? args);
    public abstract ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args);

    /// <summary>
    /// Gets a Task to receive the value of a JavaScript Object Property
    /// </summary>
    /// <returns>JSTask</returns>
    public override bool TryGetMember(GetMemberBinder binder, out object? result) {
        async ValueTask<object> GetValue() {
            //Get it as JsonElement first so we can get Informations about the Property as we cant call
            //InvokeAsync<IJSObjectReference> on non objects (string, number etc)
            try {
                var property = await InvokeAsync<JsonElement>(JsGetPropertyMethod, binder.Name);
                return await GetValueFromJsonElement(property, binder.Name);
            } catch (Exception) {
                //It throws an error if the Value is a JS Object which cant be serialized
                
                //Now check whats the type of the value, If its no Object, we cant do anything, something is wrong
                var valueKind = await InvokeAsync<JsonValueKind>(JsGetPropertyTypeMethod, binder.Name);
                if (valueKind != JsonValueKind.Object)
                    throw;

                return GetObject(await InvokeAsync<IJSObjectReference>(JsGetPropertyMethod, binder.Name));
            }
        }
        
        result = new JSTask(this, GetValue());
        return true;
    }

    /// <summary>
    /// Gets a Task which calls a JavaScript Method
    /// </summary>
    /// <returns>JSTask</returns>
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
                async ValueTask<object> GetValue() {
                    //We need to get the IJSObject reference first as we dont want to double invoke the method
                    //the returned object will have a "value" property in it with the actual object/value
                    var result = await InvokeAsync<IJSObjectReference>(JsInvokeMethodWrapped, arguments.ToArray());
                    
                    //Use our object to get the actual Value
                    await using dynamic internalObject = GetObject(result);
                    return await internalObject.value;
                }

                result = new JSTask(this, GetValue());
                return true;
            } else if (typeArgs?.Count == 1) {
                var genericType = typeArgs[0];
                var method = typeof(DynamicJSBase).GetMethod(nameof(InvokeAsync),
                    [typeof(string), typeof(object?[])]);
                var genericMethod = method?.MakeGenericMethod(genericType);

                if (genericMethod != null) {
                    var jsTaskType = typeof(JSTask<>).MakeGenericType(genericType);
                    result = jsTaskType.GetConstructors( BindingFlags.NonPublic | BindingFlags.Instance).First().Invoke([
                        this, genericMethod.Invoke(this, [JsInvokeMethod, arguments.ToArray()])
                    ]);
                    return true;
                }
            } else {
                throw new NotSupportedException("Multiple Generic Arguments are not supported");
            }
        }

        result = null;
        return false;
    }
    
    /// <summary>
    /// Converts a JavaScript Value to a C# Object/Value
    /// </summary>
    protected async ValueTask<object> GetValueFromJsonElement(JsonElement element, object propertyName) {
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

    /// <summary>
    /// Creates a new DynamicJSObjectReference
    /// </summary>
    protected DynamicJSObjectReference GetObject(IJSObjectReference jsObjectReference, JsonElement jsonElement) {
        return new DynamicJSObjectReference(jsObjectReference, jsonElement, JSRuntime, Options, AssemblyNameResolver);
    }
    
    /// <summary>
    /// Creates a new DynamicJSObjectReference
    /// </summary>
    protected DynamicJSObjectReference GetObject(IJSObjectReference jsObjectReference) {
        return new DynamicJSObjectReference(jsObjectReference, JSRuntime, Options, AssemblyNameResolver);
    }
}