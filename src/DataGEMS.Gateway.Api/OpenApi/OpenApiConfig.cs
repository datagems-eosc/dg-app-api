namespace DataGEMS.Gateway.Api.OpenApi
{
    public class OpenApiConfig
    {
        public List<String> Environments { get; set; }
        public string Version { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TermsInfo Terms { get; set; }
        public ContactInfo Contact { get; set; }
        public LicenseInfo License { get; set; }
        public String BasePath { get; set; }
		public String Endpoint { get; set; }
		public OAuth2Info OAuth2 { get; set; }

        public class TermsInfo
        {
            public Uri Url { get; set; }
        }

        public class ContactInfo
        {
            public string Name { get; set; }
            public Uri Url { get; set; }
        }

        public class LicenseInfo
        {
            public string Name { get; set; }
            public Uri Url { get; set; }
        }

        public class OAuth2Info
        {
			public Uri AuthorizationUrl { get; set; }
            public Uri TokenUrl { get; set; }
			public Dictionary<String, String> Scopes { get; set; }
            public String ClientId { get; set; }
            public String ClientName { get; set; }
            public Boolean UsePkce { get; set; }
		}

	}
}
