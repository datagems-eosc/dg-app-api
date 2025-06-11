namespace DataGEMS.Gateway.Api.Authorization
{
    public class IdpClientConfig
    {
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ClientAccessTokenUrl { get; set; }
        public bool RequireHttps { get; set; }
        public bool ValidateIssuer { get; set; }
        public bool ValidateAudience { get; set; }
        public bool ValidateIssuerSigningKey { get; set; }
        public bool MapInboundClaims { get; set; }
    }
}
