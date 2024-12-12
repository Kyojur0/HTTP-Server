using System.Globalization;
namespace HttpServerCSharp;

public class MiscFunc
{
    public MiscFunc() {/* :3 */}

    public string retStr(string errCode, string errMsg) {
        DateTime now = DateTime.UtcNow;
        string dateString = now.ToString("ddd, dd MMM yyyy HH:mm:ss GMT", CultureInfo.InvariantCulture);
        return $"HTTP/1.1 {errCode}\r\n" +
               $"Date: {dateString}\r\n" +
               $"Server: Aurora\r\n" +
               $"Content-Type: text/plain\r\n\r\n{errMsg}\r\n";
    }
}