using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using HttpServerCSharp;

namespace HttpServerCSharp {
    public class HttpServer : macros {
        private TcpCommunication _tcpCommunication;
        private RouterHandler _routeHandler;
        private MiscFunc _miscFunc;
        private readonly int _fPort;
        private readonly IPAddress _fAddr;
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private readonly TcpListener _serverListener;
        private STATUS_CODE statusCode;
        private string cliMsg;
        private string[] validUrls;
        private STATUS_CODE transmissionProtocol;
        private STATUS_CODE receptionProtocol;
        private readonly List<string?> _clientConnectedList;

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

            // _routeHandler = new Dictionary<string, MethodInfo>();
            _routeHandler = new RouterHandler(controllerInstance);
            _tcpCommunication = new TcpCommunication();
            _clientConnectedList = new List<string?>();
            _miscFunc = new MiscFunc();
            
            _routeHandler.RegisterRoutes();

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
                srvMsg = _routeHandler.HandleRequest_ViaRouteHandler(_urlPath, networkStream);
            } else if (responseStatus == STATUS_CODE.INVALID_HTTP_VERSION) {
                srvMsg = _miscFunc.retStr(errCode: "405 Method Not Allowed", errMsg: "Unsupported HTTP version");
            } else if (responseStatus == STATUS_CODE.INVALID_HTTP_VERB) {
                srvMsg = _miscFunc.retStr(errCode: "405 Method Not Allowed", errMsg: "Unsupported HTTP Verb");
            } else if (responseStatus == STATUS_CODE.INVALID_URL_PATH) {
                srvMsg = _miscFunc.retStr(errCode: "404 Not Found", errMsg: "Invalid URL Path");
            } else {
                srvMsg = _miscFunc.retStr(errCode: "500 Internal Server Error", errMsg: "whoooppssie we dont know the error :3");
            }

            if (transmissionProtocol == STATUS_CODE.CONTINOUS) {
                _tcpCommunication.SendAllBytes(networkStream, srvMsg);
            } else if (transmissionProtocol == STATUS_CODE.DISCRETE) {
                _tcpCommunication.SendDataByteByByte(networkStream, srvMsg);
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
                cliMsg = _tcpCommunication.ReadAllBytes(netStream);
            } if (receptionProtocol == STATUS_CODE.DISCRETE) {
                cliMsg = _tcpCommunication.ReadDataByteByByte(netStream);
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
                )
                {
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