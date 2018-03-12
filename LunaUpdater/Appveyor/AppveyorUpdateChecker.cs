﻿using JsonFx.Json;
using JsonFx.Serialization;
using JsonFx.Serialization.Resolvers;
using LmpGlobal;
using LunaUpdater.Appveyor.Contracts;
using System;
using System.Net;

namespace LunaUpdater.Appveyor
{
    public class AppveyorUpdateChecker
    {
        private static readonly JsonReader Reader = new JsonReader(new DataReaderSettings(new DataContractResolverStrategy()));

        public static RootObject LatestBuild
        {
            get
            {
                try
                {
                    using (var wc = new WebClient())
                    {
                        wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                        var json = wc.DownloadString(RepoConstants.AppveyorUrl);
                        return Reader.Read<RootObject>(json);
                    }
                }
                catch (Exception)
                {
                    //Ignore as either we don't have internet connection or something like that...
                }

                return null;
            }
        }

        public static Version GetLatestVersion()
        {
            return LatestBuild != null ?
                new Version(LatestBuild.build.version.Substring(0, LatestBuild.build.version.LastIndexOf('.'))) :
                new Version("0.0.0");
        }
    }
}
