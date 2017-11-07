﻿using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Utilities;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Flag;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace LunaClient.Systems.Flag
{
    public class FlagMessageHandler : SubSystem<FlagSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            if (!(messageData is FlagBaseMsgData msgData)) return;

            switch (msgData.FlagMessageType)
            {
                case FlagMessageType.ListResponse:
                    {
                        var data = (FlagListResponseMsgData)messageData;
                        foreach (var flag in data.FlagFiles)
                        {
                            var extendedFlagInfo = new ExtendedFlagInfo(flag);
                            System.ServerFlags.TryAdd(extendedFlagInfo.FlagName, extendedFlagInfo);
                        }
                        MainSystem.NetworkState = ClientState.FlagsSynced;
                    }
                    break;
                case FlagMessageType.FlagData:
                    {
                        var data = (FlagDataMsgData)messageData;
                        var extendedFlagInfo = new ExtendedFlagInfo(data.Flag);
                        System.ServerFlags.AddOrUpdate(extendedFlagInfo.FlagName, extendedFlagInfo, (key, existingVal) => extendedFlagInfo);
                    }
                    break;
                case FlagMessageType.FlagDelete:
                    {
                        var data = (FlagDeleteMsgData)messageData;
                        System.ServerFlags.TryRemove(data.FlagName, out _);
                        DeleteFlagFile(CommonUtil.CombinePaths(FlagSystem.FlagPath, data.FlagName));
                    }
                    break;
            }
        }

        private static void DeleteFlagFile(string flagFile)
        {
            try
            {
                if (File.Exists(flagFile))
                {
                    File.Delete(flagFile);
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error deleting flag {flagFile}, exception: {e}");
            }
        }
    }
}