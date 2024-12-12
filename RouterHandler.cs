using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace HttpServerCSharp;

public class RouterHandler
{
    private readonly Dictionary<string, MethodInfo> _routeHandler;
    private readonly object _controllerInstance;
    
    public RouterHandler(object controllerInstance) {
        _routeHandler = new Dictionary<string, MethodInfo>();
        _controllerInstance = controllerInstance;
    }
    
    public void RegisterRoutes()
    {
        var methods = _controllerInstance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<RouteAttribute>();
            if (attribute != null)
            {
                _routeHandler[attribute.Path] = method;
                Console.WriteLine($"Registered Route: {attribute.Path} => {method.Name}");
            }
        }
    }
    
    // maybe i should mark this for GET only and try make another for PUT or other 
    // HTTP verbs like DELETE or stuff like that idk for now it works for GET
    public string HandleRequest_ViaRouteHandler(string urlPath, NetworkStream networkStream) {
        if (_routeHandler.TryGetValue(urlPath, out var method)) {
            Console.WriteLine($"Handling Request for {urlPath}");
            object? response = method.Invoke(_controllerInstance, null);
            string rsvStr = response?.ToString();
            Console.WriteLine("Response Received: {0}", response.ToString());
            string responseString = string.Format("HTTP/1.1 200 OK\r\nContent-Length: {0}\r\n\r\n{1}", rsvStr.Length, rsvStr); 
            Console.WriteLine("Message which server will be sending to the client is: {0}", responseString);
            return responseString;
        } else {
            string rsvStr = "HTTP/1.1 404 Not Found\r\n\r\nPath not found.";
            return "";
        }
    }
}