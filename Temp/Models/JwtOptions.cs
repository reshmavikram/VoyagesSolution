namespace Data.Solution.Models
{
    public class JwtOptions
    {
        public string secretKey { get; set; }
        public string issuer { get; set; }
        public bool validateLifetime { get; set; }
    }
}