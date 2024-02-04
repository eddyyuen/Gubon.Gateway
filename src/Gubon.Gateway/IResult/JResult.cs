namespace Gubon.Gateway.IResult
{
    public class JResult
    {
        public int status = 1;
        public int code = 0;
        public string? error;
        public object? data;
        public long total = 0;
    }
}
