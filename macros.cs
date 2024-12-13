namespace HttpServerCSharp;

public class macros  {
    public enum STATUS_CODE: Int16 {
        INVALID_HTTP_VERSION = 1,
        INVALID_URL_PATH = 2,
        VALID_URL_PATH = 3,
        INVALID_HTTP_VERB = 4,
        DISCRETE = 5,
        CONTINOUS = 6
    }
}