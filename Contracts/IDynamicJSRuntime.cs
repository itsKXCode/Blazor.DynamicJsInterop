using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blazor.DynamicJsInterop.Contracts;

public interface IDynamicJSRuntime<T> : IDynamicJSRuntime where T : ComponentBase {
    public dynamic Exports { get; }
}

public interface IDynamicJSRuntime : IJSObjectReference {
    public dynamic Global { get; }
}