using System.Net.Sockets;
using System.Text;

namespace HttpServerCSharp;

public class TcpCommunication
{
    private byte[] _rcvBuffer;
    private byte[] _sndBuffer;

    public TcpCommunication() {
        _rcvBuffer = new byte[1024];
        _sndBuffer = new byte[1024];
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

}