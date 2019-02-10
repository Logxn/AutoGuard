using System;
using System.Net;
using RestSharp;
using Newtonsoft.Json;
using AutoGuard.SteamAPI.Interface;

namespace AutoGuard.SteamAPI
{
    internal class SteamServer : ISteamWebApiUtil
    {
        ServerInfoResponse ISteamWebApiUtil.GetServerInfo()
        {
            var client = new RestClient(new Uri(ApiEndpoints.ApiBaseUrl));
            var request = new RestRequest(ApiEndpoints.GetServerInfo, Method.GET, DataFormat.Json);

            var response = client.ExecuteAsGet(request, "GET");
            if (response.StatusCode != HttpStatusCode.OK) throw new Exception("Invalid Request For GetServerInfo()");

            return JsonConvert.DeserializeObject<ServerInfoResponse>(response.Content);
        }
    }
}
