using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC
{
    public enum RPCOpcode
    {
        RPCSetSessionID = 0, //Sets the initial session ID, sent by the server
        DinosaurUpdateEvent = 1, //Synced dino was updated
        LogMessage = 2, //Logs a message, eRPCPayloadLogMessage
        CanvasChange = 3, //Called when a canvas is changed
        PlayerListChanged = 4, //Called when the player list is updated
        DinoPrefsChanged = 5, //Called when dino prefs change
        PutNotification = 6, //Sends a new notification
        LiveUpdate = 7, //An update to the live endpoint
    }
}
