using System.Reflection;
using Blazor.DynamicJsInterop.Contracts;
using Microsoft.AspNetCore.Components;

namespace Blazor.DynamicJsInterop;

internal sealed class AssemblyNameResolver : IAssemblyNameResolver {
    public string GetEntryAssembly() => Assembly.GetEntryAssembly()?.FullName ?? string.Empty;
    public string GetComponentAssembly<T>() where T : ComponentBase => typeof(T).Assembly.FullName ?? string.Empty;
    public bool IsExternalAssembly<T>() where T : ComponentBase => GetEntryAssembly() != GetComponentAssembly<T>();
}