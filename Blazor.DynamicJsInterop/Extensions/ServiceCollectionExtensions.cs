using System.Reflection;
using Blazor.DynamicJsInterop.Contracts;
using Blazor.DynamicJsInterop.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Blazor.DynamicJsInterop.Extensions;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddDynamicJSRuntime(this IServiceCollection services) {
        var callingAssembly = Assembly.GetCallingAssembly();
        return AddDynamicJSRuntime(services, options => options.MapAssembly(callingAssembly));
    }

    public static IServiceCollection AddDynamicJSRuntime(this IServiceCollection services,
        Action<JavaScriptReferencesOptions>? configure) {
        if (configure == null) {
            var callingAssembly = Assembly.GetCallingAssembly();
            configure = options => options.MapAssembly(callingAssembly);
        }

        services.Configure(configure);

        services.AddSingleton<IAssemblyNameResolver, AssemblyNameResolver>();
        services.AddTransient<IDynamicJSRuntime, DynamicJSRuntime>();
        services.AddTransient(typeof(IDynamicJSRuntime<>), typeof(DynamicIsolatedJSRuntime<>));

        return services;
    }
}