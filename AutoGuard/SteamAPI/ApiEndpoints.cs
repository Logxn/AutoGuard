namespace AutoGuard.SteamAPI
{
    public class ApiEndpoints
    {
        public const string ApiBaseUrl = "https://api.steampowered.com";
        public const string SteamBaseUrl = "https://steamcommunity.com";

        #region ISteamWebAPIUtil
        public const string GetServerInfo = "/ISteamWebAPIUtil/GetServerInfo/v0001/?";
        #endregion

        #region SteamCommunity
        public const string GetRsaKey = "/login/getrsakey";
        public const string Login = "/login/dologin";
        #endregion

        public const string UrlEncoded = "application/x-www-form-urlencoded";
    }
}
