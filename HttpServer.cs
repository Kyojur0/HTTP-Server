using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using HttpServerCSharp;

namespace HttpServerCSharp {

    public enum STATUS_CODE: Int16 {
        INVALID_HTTP_VERSION = 1,
        INVALID_URL_PATH = 2,
        VALID_URL_PATH = 3,
        INVALID_HTTP_VERB = 4,
        DISCRETE = 5,
        CONTINOUS = 6
    }

    delegate object RouteHandler(object paramter);

    public class HttpServer {
        private readonly int _fPort;
        private readonly IPAddress _fAddr;
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private byte[] _rcvBuffer = new byte[1024];
        private byte[] _sndBuffer = new byte[1024];
        private readonly TcpListener _serverListener;
        private STATUS_CODE statusCode;
        private string cliMsg;
        private string[] validUrls;
        private STATUS_CODE transmissionProtocol;
        private STATUS_CODE receptionProtocol;
        private readonly List<string?> _clientConnectedList = new List<string?>();
        private readonly Dictionary<string, MethodInfo> _routeHandler;
        private readonly object _controllerInstance;

        public HttpServer(
            object controllerInstance,
            int port = 0,
            string ipAddr = "",
            STATUS_CODE usrTransmissionType = STATUS_CODE.CONTINOUS,
            STATUS_CODE usrReceptionProtocol = STATUS_CODE.CONTINOUS
        ) {
            cliMsg = "";

            transmissionProtocol = usrTransmissionType;
            receptionProtocol = usrReceptionProtocol;

            validUrls = new string[3];
            validUrls[0] = "/index";
            validUrls[1] = "/home";
            validUrls[2] = "/";

            _routeHandler = new Dictionary<string, MethodInfo>();
            _controllerInstance = controllerInstance;
            
            RegisterRoutes(_controllerInstance);

            bool usrPort = port != 0 ? true : false;
            bool usrAddr = ipAddr != "" ? true : false;
            _fAddr = usrAddr ? IPAddress.Parse(ipAddr) : IPAddress.Any;
            _fPort = usrPort ? port : 4221;
            _serverListener = new TcpListener(
                localaddr: _fAddr,
                port: _fPort
            );
            Console.WriteLine("Starting Local HTTP Server....");
            Console.WriteLine("Connect to server at http://{0}:{1}", _fAddr, _fPort);
            _serverListener.Start();
        }

        private void RegisterRoutes(object controller)
        {
            var methods = controller.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
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

        private void SendResponse(NetworkStream networkStream, string response)
        {
            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
            networkStream.Write(responseBytes, 0, responseBytes.Length);
            networkStream.Flush();
        }
        
        public STATUS_CODE HttpParser(
            string httpVerb,
            string urlPath,
            string httpVersion
        ) {
            if (!httpVersion.Contains("HTTP/1.1")) {
                Console.WriteLine("ERROR: HTTP/1.1 not found in client request...");
                Console.WriteLine("Please use HTTP/1.1 for communication.........");
                return STATUS_CODE.INVALID_HTTP_VERSION;
            }
            if (httpVerb == "GET") {
                bool isValidPath = Array.Exists(validUrls, element => element == urlPath);
                if (isValidPath) {
                    return STATUS_CODE.VALID_URL_PATH;
                }
                Console.WriteLine("ERROR: {0} not a valid url path...", urlPath);
                return STATUS_CODE.INVALID_URL_PATH;
            }
            Console.WriteLine("ERROR: {0} not a HTTP verb supported...", httpVerb);
            Console.WriteLine("As of now only GET is supported.....");
            return STATUS_CODE.INVALID_HTTP_VERB;
        }

        public string ReadAllBytes(NetworkStream netStream) {
            int bytesRecieved = netStream.Read(_rcvBuffer);
            string data = Encoding.UTF8.GetString(_rcvBuffer.AsSpan(0, bytesRecieved));
            return data;
        }

        public string ReadDataByteByByte(NetworkStream netStream) {
            StringBuilder data = new StringBuilder();
            while (netStream.DataAvailable) {
                int byteData = netStream.ReadByte();
                if (byteData != -1) {
                    Console.WriteLine("Incooming Byte: {0} | Concerted Char: {1}", byteData, (char)byteData);
                    data.Append((char)byteData);
                } else {
                    break;
                }
            }
            return data.ToString();
        }

        public void SendAllBytes(NetworkStream netstream, string srvRsp) {
            _sndBuffer = Encoding.UTF8.GetBytes(srvRsp);
            netstream.Write(_sndBuffer, 0, _sndBuffer.Length);
        }

        public void SendDataByteByByte(NetworkStream netStream, string srvRsp) {
            _sndBuffer = Encoding.UTF8.GetBytes(srvRsp);
            foreach (byte b in _sndBuffer) {
                netStream.WriteByte(b);
            }
        }

        public void urlResolver(string urlPath) {

            return;
        }

        public void RequestParser(string cliMsg, NetworkStream networkStream) {
            List<string> requestList = cliMsg.Split("\r\n").ToList();
            string[] headerStrings = requestList[0].Split(" ");

            // ReSharper disable InconsistentNaming
            string _httpVerb = headerStrings[0];
            string _urlPath = headerStrings[1];
            string _httpVersion = headerStrings[2];

            STATUS_CODE responseStatus = HttpParser(
                httpVerb: _httpVerb,
                urlPath: _urlPath,
                httpVersion: _httpVersion
            );

            string srvMsg = "";
            if (responseStatus == STATUS_CODE.VALID_URL_PATH) {
                srvMsg = HandleRequest_ViaRouteHandler(_urlPath, networkStream);
            } else if (responseStatus == STATUS_CODE.INVALID_HTTP_VERSION) {
                srvMsg = "HTTP/1.1 400 Unsupported HTTP Version\r\n\r\n";
            } else if (responseStatus == STATUS_CODE.INVALID_HTTP_VERB) {
                srvMsg = "HTTP/1.1 405 Method Not Allowed\r\n\r\n";
            } else if (responseStatus == STATUS_CODE.INVALID_URL_PATH) {
                srvMsg = "HTTP/1.1 404 Not Found\r\n\r\n";
            } else {
                srvMsg = "HTTP/1.1 500 Internal Server Error\r\n\r\n";
            }

            if (transmissionProtocol == STATUS_CODE.CONTINOUS) {
                SendAllBytes(networkStream, srvMsg);
            } else if (transmissionProtocol == STATUS_CODE.DISCRETE) {
                SendDataByteByByte(networkStream, srvMsg);
            }
        }

        public void HandleClient(TcpClient client) {
            // check if client is null i.e. wrongful call to this function
            if (client == null) {
                throw new ArgumentNullException(nameof(client));
            }

            // check if client is trying to send data to server
            using NetworkStream netStream = client.GetStream();
            netStream.ReadTimeout = 2500;
            netStream.WriteTimeout = 250;
            if (receptionProtocol == STATUS_CODE.CONTINOUS) {
                cliMsg = ReadAllBytes(netStream);
            } if (receptionProtocol == STATUS_CODE.DISCRETE) {
                cliMsg = ReadDataByteByByte(netStream);
            }
            Console.WriteLine("Client Sent: \n{0}", cliMsg);

            RequestParser(cliMsg, netStream);
            return;
        }

        public void HandleRequest() {
            bool isRunning = true;
            // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
            while (isRunning) {
                TcpClient tcpClient = _serverListener.AcceptTcpClient();
                if (
                    tcpClient is { Client.Connected: true }
                ) {
                    string? clientEndpoint = tcpClient.Client.RemoteEndPoint?.ToString();
                    if (clientEndpoint != null && clientEndpoint != null) {
                        _clientConnectedList.Add(clientEndpoint);
                        Console.WriteLine("A New Client Connected to Server....");
                        Console.WriteLine("Client info:\n{0}", clientEndpoint);
                        HandleClient(tcpClient);
                    }
                }
                tcpClient.Close();
            }
            isRunning = false;
            _serverListener.Stop();
        }

        public TcpListener GeTcpListener() {
            return _serverListener;
        }
    }
}