using Microsoft.JSInterop;

namespace Blazor.DynamicJsInterop.Contracts;

public interface IDynamicJSObjectReference : IJSObjectReference {
    ValueTask WriteOperation { get; }
}