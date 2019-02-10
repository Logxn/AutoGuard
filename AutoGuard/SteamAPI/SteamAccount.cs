using System;
using System.Net;
using RestSharp;
using Newtonsoft.Json;
using AutoGuard.SteamAPI.Interface;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AutoGuard.SteamAPI
{
    internal class SteamAccount
    {
        private static string _username;
        private static string _password;

        private static string _captchaGid;

        public SteamAccount(string username, string password)
        {
            _username = username;
            _password = password;
            _captchaGid = "";

#if DEBUG
            ISteamWebApiUtil steamServer = new SteamServer();
            var serverTime = steamServer.GetServerInfo().ServerTime;
            Console.WriteLine($"[Steam ServerInfo] Time: {serverTime}");
#endif
        }


        private GetRsaKeyResponse GetRsaKey()
        {
            var client = new RestClient(new Uri(ApiEndpoints.SteamBaseUrl));
            var request = new RestRequest(ApiEndpoints.GetRsaKey, Method.GET);
            request.AddParameter("username", _username, ParameterType.QueryString);

            var response = client.ExecuteAsGet(request, "GET");
            if (response.StatusCode != HttpStatusCode.OK || response.Content.Contains("false")) throw new Exception("Invalid Request For GetRsaKey()");
            
            return JsonConvert.DeserializeObject<GetRsaKeyResponse>(response.Content);
        }

        public async Task<ELoginResult> Login(string twoFactor = null, string emailAuth = null, string captchaGid = null, string captchaText = null)
        {
            var rsaInfo = GetRsaKey();

            var rsaParam = new RsaParameters
            {
                Exponent = rsaInfo.Exponent,
                Modulus = rsaInfo.Modulus,
                Password = _password
            };

            var encrypted = string.Empty;
            while (encrypted.Length < 2 || encrypted.Substring(encrypted.Length - 2) != "==")
            {
                encrypted = Utilities.EncryptPassword(rsaParam);
            }

#if DEBUG
            Console.WriteLine("[Steam DoLogin] -> Done Encrypting Password");
            Console.WriteLine("[Steam DoLogin] -> Sending Request");
#endif

            var cookieContainer = new CookieContainer();
            var msgHandler = new HttpClientHandler { CookieContainer = cookieContainer };
            var httpClient = new HttpClient(msgHandler);

            if(cookieContainer.Count == 0)
            {
                cookieContainer.Add(new Cookie("mobileClientVersion", "2.0.10", "/", ".steamcommunity.com"));
                cookieContainer.Add(new Cookie("mobileClient", "ios", "/", ".steamcommunity.com"));
                cookieContainer.Add(new Cookie("Steam_Language", "english", "/", ".steamcommunity.com"));
            }

            var data = new Dictionary<string, string>
            {
                { "username", _username },
                { "password", encrypted },
                { "twofactorcode", twoFactor ?? "" },
                { "emailauth", emailAuth ?? "" },
                { "loginfriendlyname", "" },
                { "captchagid", captchaGid ?? "-1" },
                { "captcha_text", captchaText ?? "" },
                { "emailsteamid", "" },
                { "rsatimestamp", rsaInfo.Timestamp },
                { "remember_login", "false" },
                { "oauth_client_id", "3638BFB1" }
            };

            httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            httpClient.DefaultRequestHeaders.Add("Referer", "https://steamcommunity.com/mobilelogin?oauth_client_id=3638BFB1&oauth_scope=read_profile%20write_profile%20read_client%20write_client");

            var request = await httpClient.PostAsync(ApiEndpoints.SteamBaseUrl + ApiEndpoints.Login, new FormUrlEncodedContent(data));
            var result = await request.Content.ReadAsStringAsync();

            var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(result);

            if(!loginResponse.Success)
            {
                if(loginResponse.Message.Contains("incorrect"))
                {
                    return ELoginResult.BadCredentials;
                }

                if(loginResponse.Message.Contains("too many"))
                {
                    return ELoginResult.TooManyFailedLogins;
                }
            }

            if(loginResponse.TwoFactorNeeded)
            {
                Console.Write("Please enter your 2FA code: ");
                var twoF = Console.ReadLine();

                await Login(twoF);
                return ELoginResult.NeedCaptcha;
            }

            if(loginResponse.CaptchaNeeded)
            {
                _captchaGid = loginResponse.CaptchaGID;

                Process.Start($"https://store.steampowered.com/login/rendercaptcha?gid={_captchaGid}");

                Console.Write($"Please enter the captcha: ");
                var captchaInput = Console.ReadLine();

                await Login(null, null, captchaGid, captchaInput);
                return ELoginResult.NeedCaptcha;
            }

            if(loginResponse.EmailAuthNeeded) return ELoginResult.NeedEmail;
            if (loginResponse.TwoFactorNeeded) return ELoginResult.Need2Fa;


            Console.WriteLine($"[Steam DoLogin] -> Received OAuth: {loginResponse.OAuthData.OAuthToken}");

            return ELoginResult.Ok;
        }
    }

    public enum ELoginResult
    {
        Ok,
        UnhandledException,
        BadCredentials,
        NeedCaptcha,
        Need2Fa,
        NeedEmail,
        TooManyFailedLogins,
    }
}
