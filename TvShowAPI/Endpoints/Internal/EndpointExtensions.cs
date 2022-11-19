using System.Reflection;

namespace TvShowAPI.Endpoints.Internal; 

public static class EndpointExtensions {

    public static void AddEnpoints<TMarker>(this IServiceCollection services,
        IConfiguration configuration) {
        
        AddEnpoints(services,typeof(TMarker), configuration);
    }
    
    public static void AddEnpoints(this IServiceCollection services,
        Type typeMarker,IConfiguration configuration) {
        var endpointTypes = GetEnpointTypesFromAssemblyContaining(typeMarker);

        foreach (var endpointType in endpointTypes) {
            endpointType.GetMethod(nameof(IEndpoints.AddServices))!
                .Invoke(null, new object[] { services, configuration });
        }
    }

   

    public static void UseEndpoints<TMarker>(this IApplicationBuilder app) {
        UseEndpoints(app,typeof(TMarker));
    }
    
    public static void UseEndpoints(this IApplicationBuilder app, Type typeMarker) {
        var endpointTypes = GetEnpointTypesFromAssemblyContaining(typeMarker);

        foreach (var endpointType in endpointTypes) {
            endpointType.GetMethod(nameof(IEndpoints.DefineEndpoints))!
                .Invoke(null, new object[] { app });
        }
    }
    
    private static IEnumerable<TypeInfo> GetEnpointTypesFromAssemblyContaining(Type typeMarker) {
        var endpointTypes = typeMarker.Assembly.DefinedTypes
            .Where(x => !x.IsAbstract && !x.IsInterface &&
                        typeof(IEndpoints).IsAssignableFrom(x));
        return endpointTypes;
    }
    
}