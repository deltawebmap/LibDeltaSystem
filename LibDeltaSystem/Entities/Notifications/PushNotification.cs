using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.Notifications
{
    public class PushNotification
    {
        public int id;
        public PushNotificationDisplayInfo info;
        public PushNotificationServer server;
    }
}
