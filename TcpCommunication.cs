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
    
    public async Task SendAllBytes(NetworkStream netstream, string srvRsp) {
        _sndBuffer = Encoding.UTF8.GetBytes(srvRsp);
        await netstream.WriteAsync(_sndBuffer, 0, _sndBuffer.Length);
    }

    public async Task SendDataByteByByte(NetworkStream netStream, string srvRsp) {
        _sndBuffer = Encoding.UTF8.GetBytes(srvRsp);
        foreach (byte b in _sndBuffer) {
            await netStream.WriteAsync(new[] { b }, 0, 1);
        }
    }

    public async Task<string> ReadAllBytes(NetworkStream netStream) {
        int bytesRecieved = await netStream.ReadAsync(_rcvBuffer, 0, _rcvBuffer.Length);
        string data = Encoding.UTF8.GetString(_rcvBuffer.AsSpan(0, bytesRecieved));
        return data;
    }

    public async Task<string> ReadDataByteByByte(NetworkStream netStream) {
        StringBuilder data = new StringBuilder();
        while (netStream.DataAvailable) {
            int byteData = await Task.Run(() => netStream.ReadByte()); // ReadByte doesn't have async version
            if (byteData != -1) {
                Console.WriteLine("Incoming Byte: {0} | Converted Char: {1}", byteData, (char)byteData);
                data.Append((char)byteData);
            } else {
                break;
            }
        }
        return data.ToString();
    }

}