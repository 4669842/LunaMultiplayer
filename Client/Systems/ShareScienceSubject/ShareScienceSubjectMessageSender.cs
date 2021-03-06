﻿using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Utilities;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Interface;
using System;

namespace LunaClient.Systems.ShareScienceSubject
{
    public class ShareScienceSubjectMessageSender : SubSystem<ShareScienceSubjectSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ShareProgressCliMsg>(msg)));
        }

        public void SendScienceSubjectMessage(ScienceSubject subject)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressScienceSubjectMsgData>();
            msgData.ScienceSubject.Id = subject.id;

            var configNode = ConvertScienceSubjectToConfigNode(subject);
            if (configNode == null) return;

            var data = ConfigNodeSerializer.Serialize(configNode);
            var numBytes = data.Length;

            msgData.ScienceSubject.NumBytes = numBytes;
            if (msgData.ScienceSubject.Data.Length < numBytes)
                msgData.ScienceSubject.Data = new byte[numBytes];

            Array.Copy(data, msgData.ScienceSubject.Data, numBytes);

            SendMessage(msgData);

            LunaLog.Log($"Science experiment \"{subject.id}\" sent");
        }

        private static ConfigNode ConvertScienceSubjectToConfigNode(ScienceSubject subject)
        {
            var configNode = new ConfigNode();
            try
            {
                subject.Save(configNode.AddNode("Science"));
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error while saving science subject: {e}");
                return null;
            }

            return configNode;
        }
    }
}
