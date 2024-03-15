using Blazor.DynamicJsInterop.Options;
using Microsoft.AspNetCore.Components;

namespace Blazor.DynamicJsInterop.Contracts;

public interface IPathFormatterResolver {
    PathFormatter ResolvePathFormatter<TComponent>() where TComponent : ComponentBase;
}