using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.Notifications
{
    public enum PushNotificationType
    {
        Generic = 0,

        System = -1,
        Account = -2,

        GenericGuild = 1,
        DinosaurDeath = 2,

    }
}
