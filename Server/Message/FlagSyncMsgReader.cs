﻿using LunaCommon.Message.Data.Flag;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Message.Base;
using Server.System;

namespace Server.Message
{
    public class FlagSyncMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var data = (FlagBaseMsgData)message.Data;

            switch (data.FlagMessageType)
            {
                case FlagMessageType.ListRequest:
                    FlagSyncMsgSender.HandleFlagListRequestMessage(client);
                    //We don't use this message anymore so we can recycle it
                    message.Recycle();
                    break;
                case FlagMessageType.FlagData:
                    FlagSyncMsgSender.HandleFlagDataMessage(client, (FlagDataMsgData)data);
                    break;
            }
        }
    }
}
