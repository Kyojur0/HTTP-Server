﻿using System.Net;
using System.Net.Sockets;

namespace HttpServerCSharp {
    public class Program {
        public static void Main()
        {
            AppController controller = new AppController();
            // Default initialization
            HttpServer server = new HttpServer(
                controllerInstance: controller,
                ipAddr: "127.0.0.1",
                port: 3601,
                _concurrency: true
            );
            
            server.HandleRequest();
        }
    }
}