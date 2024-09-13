using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blazor.DynamicJsInterop.Contracts;

public interface IDynamicJSRuntime<T> : IDynamicJSRuntime where T : ComponentBase {
    dynamic Module { get; }
    Task ImportIsolatedModule();
}

public interface IDynamicJSRuntime : IJSRuntime  {
    dynamic Window { get; }
    ValueTask WriteOperation { get; }
}