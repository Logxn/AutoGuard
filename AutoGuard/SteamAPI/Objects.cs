using Newtonsoft.Json;
namespace AutoGuard.SteamAPI
{
    public class ServerInfoResponse
    {
        [JsonProperty("servertime")]
        public int ServerTime { get; set; }
        [JsonProperty("servertimestring")]
        public string ServerTimeString { get; set; }
    }

    public class GetRsaKeyResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("publickey_mod")]
        public string Modulus { get; set; }
        [JsonProperty("publickey_exp")]
        public string Exponent { get; set; }
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
        [JsonProperty("token_gid")]
        public string TokenGid { get; set; }
    }

    public class LoginResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("login_complete")]
        public bool LoginComplete { get; set; }

        [JsonProperty("oauth")]
        public string OAuthDataString { get; set; }

        public OAuth OAuthData => OAuthDataString != null ? JsonConvert.DeserializeObject<OAuth>(OAuthDataString) : null;

        [JsonProperty("captcha_needed")]
        public bool CaptchaNeeded { get; set; }

        [JsonProperty("captcha_gid")]
        public string CaptchaGID { get; set; }

        [JsonProperty("emailsteamid")]
        public ulong EmailSteamID { get; set; }

        [JsonProperty("emailauth_needed")]
        public bool EmailAuthNeeded { get; set; }

        [JsonProperty("requires_twofactor")]
        public bool TwoFactorNeeded { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        public class OAuth
        {
            [JsonProperty("steamid")]
            public ulong SteamID { get; set; }

            [JsonProperty("oauth_token")]
            public string OAuthToken { get; set; }

            [JsonProperty("wgtoken")]
            public string SteamLogin { get; set; }

            [JsonProperty("wgtoken_secure")]
            public string SteamLoginSecure { get; set; }

            [JsonProperty("webcookie")]
            public string Webcookie { get; set; }
        }
    }
}
