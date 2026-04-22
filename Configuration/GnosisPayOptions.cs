using System;
using System.Collections.Generic;
using System.Text;

namespace gnosispay_sync.Configuration
{
    public sealed class GnosisPayOptions
    {
        public const string SectionName = "GnosisPay";
        public string ApiBaseUrl { get; set; } = "https://api.gnosispay.com";
        public string PrivateKey { get; set; } = string.Empty;
        public string SiweDomain { get; set; } = "localhost";
        public string SiweUri { get; set; } = "https://api.gnosispay.com/";
        public string SiweStatement { get; set; } = "Sign in with Ethereum to Gnosis Pay";
        public int ChainId { get; set; } = 100;
        public int JwtTtlInSeconds { get; set; } = 86400;
    }
}
