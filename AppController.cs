namespace HttpServerCSharp;

public class AppController {
    [Route("/index")]
    public string IndexRoute() => "Welcome to the Index Page";
    
    [Route("/home")]
    public string HomeRoute() => "Welcome to the Home Page";
    
    [Route("/about")]
    public string AboutRoute() => "Welcome to the About Page";
}