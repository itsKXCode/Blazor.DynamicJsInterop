using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blazor.DynamicJsInterop.Contracts;

public interface IDynamicJSRuntime<T> : IDynamicJSRuntime where T : ComponentBase {
    public dynamic Module { get; }
    Task ImportIsolatedModule();
}

public interface IDynamicJSRuntime : IJSRuntime  {
    public dynamic Window { get; }
}