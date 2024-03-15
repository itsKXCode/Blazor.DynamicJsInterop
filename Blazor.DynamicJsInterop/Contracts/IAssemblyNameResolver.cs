using Microsoft.AspNetCore.Components;

namespace Blazor.DynamicJsInterop.Contracts;

public interface IAssemblyNameResolver
{
    string GetEntryAssembly();

    string GetComponentAssembly<T>() where T: ComponentBase;

    bool IsExternalAssembly<T>() where T : ComponentBase;
}