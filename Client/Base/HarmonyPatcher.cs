﻿using Harmony;
using System.Reflection;

namespace LunaClient.Base
{
    public static class HarmonyPatcher
    {
        public static HarmonyInstance HarmonyInstance = HarmonyInstance.Create("LunaMultiplayer");

        public static void Awake()
        {
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
