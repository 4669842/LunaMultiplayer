﻿using JsonFx.Json;
using LMP.MasterServer.Geolocalization.Base;
using System;
using System.Net;
using System.Net.Http;

namespace LMP.MasterServer.Geolocalization
{
    internal class IpApi : BaseGeolocalization
    {
        public static string GetCountry(IPEndPoint externalEndpoint)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    dynamic output = JsonReader.Read(client.GetStringAsync($"https://ipapi.co/{externalEndpoint.Address}/json/").Result);
                    return output["country"];
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
