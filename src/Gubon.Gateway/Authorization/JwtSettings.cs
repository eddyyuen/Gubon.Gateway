namespace Gubon.Gateway.Authorization
{
    public class JwtSettings
    {
        public string? Secret { get; set; }
        public int ExpiredTime { get; set; }
    }
}
