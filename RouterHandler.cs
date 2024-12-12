using System.Net;
using System.Reflection;

namespace HttpServerCSharp;

public class RouterHandler
{
    private readonly Dictionary<string, MethodInfo> _router;

    public RouterHandler() {
        _router = new Dictionary<string, MethodInfo>();
    }

    public void RegisterRoutes(object target) {
        var methods = target.GetType().GetMethods(
            BindingFlags.Instance | 
            BindingFlags.Public | 
            BindingFlags.NonPublic);
        foreach (var method in methods) {
            var attribute = method.GetCustomAttribute<RouteAttribute>();
            if (attribute != null) {
                _router[attribute.Path] = method;
            }
        }
    }

    public void HandleRequest(string path, object target) {
        if (_router.TryGetValue(path, out var method)) {
            method.Invoke(target, null);
        } else {
            Console.WriteLine("Route not found for path: " + path);
        }
    }
}