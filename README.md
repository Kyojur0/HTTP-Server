![alt text](<AURORA.png>)

# Aurora

## Introduction

Aurora is a custom-built HTTP server primarily designed as a 
learning project,but WHYY??? a while back i watched this video 
by Low Level youtuber where he said inorder to understand
or get a good grasp of some langauge build HTTP server or 
Heap Allocator or something like that so yea this is why. 
Also, a little bit different with this one is that i tried to 
implement decorator from Python Flask like. But, other than that 
it only supports GET method and that is pretty much it with ofc 
Synchronous and concurrent request handling(i think, im not sure 100%).

## Main Functionalities

### GET Support Only

Like i said this project for now only supportss GET and nothing else. Might implement other 
HTTP verbs later on but for now i think its "okay".

### Flask-like Decorators for Routing

One thing that i really like about this project is the Flask like decorator
feature for routing and this was achieved through custom attributes in C#.
This allows the devs to annotate methods with specific URL paths.
Took me awhile but i got it :3.

## Function and Files Overview
Alr, i wont yap too much from now.

### `HttpServer.cs`

The main server class that initializes and runs the TCP listener, handling incoming connections and dispatching them to appropriate handlers based on the URL and method of the request.

### `HttpParser.cs`

A parser dedicated to interpreting incoming HTTP requests, ensuring they conform to expected standards (currently HTTP/1.1) and extracting key components like the method, URL, and HTTP version.

### `TcpCommunication.cs`

Handles low-level TCP communications, managing the asynchronous sending and receiving of data across the network. This file contains methods for both byte-by-byte and bulk data transmission.

### `RouterHandler.cs`

Manages routing of requests to the correct controller methods based on URL paths. It uses reflection to map URLs to methods that are decorated with the `Route` attribute, similar to Flask's routing system.

### `MiscFunc.cs`

Provides utility functions, primarily for generating formatted HTTP response strings complete with headers and status codes.

### `macros.cs`

Defines enums and constants used throughout the server, such as HTTP status codes and settings for transmission and reception protocols.

### `RouteAttribute.cs`

Defines a custom attribute that can be used to decorate methods to specify the route path, mimicking Flask's decorator-based routing.

## How to Run/Use

### Setup

Clone the repository(master not main!) and navigate to the project directory:

```
> git clone https://github.com/yourusername/HttpServerCSharp.git
> cd HttpServerCSharp
```
Build the project using .NET CLI (ensure you have .NET9.0 cause 
i had troubles compiling/running it on .NET8.0 so idk, iz just me):
Also, i would recommend you use some IDE(I used Rider) for this cause
then it would be alot easier. But if you want CLI  version then...

```aiignore
> dotnet build
> dotnet run
```
:P.

If all goes well you should see following output from server
```aiignore
Registered Route: /index => IndexRoute
Registered Route: /home => HomeRoute
Registered Route: /about => AboutRoute
Starting Local HTTP Server....
Connect to server at http://127.0.0.1:3601
```

These are predefined routes in AppController.cs you can change them to your own liking.
Now open up another shell/whatever and make a curl request like following 
```aiignore
> curl -i GET http://127.0.0.1:3601/
```

The output you see should be 
```aiignore
curl: (6) Could not resolve host: GET
HTTP/1.1 404 Not Found

Path not found.
```
Since, i haven't redirect `/` to anywhere in my predefined routes :3

Now same command but for `/index` 
```aiignore
> curl -i GET http://127.0.0.1:3601/index                   13/12/24 08:17:49
curl: (6) Could not resolve host: GET
HTTP/1.1 200 OK
Content-Length: 25

Welcome to the Index Page
```

### `Program.cs`
Contains basic usage of the `HttpServer class` so just look at it and the source code 
you will get hang of what it is. 

Whelp, thank you for reading till end. :P

I might/might not update this code base later on, maybe 
try adding few more things like 

1. Sending files
2. Receiving files
3. Handle POST
4. Few more which i dont recall rn...

