using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.Notifications
{
    public class PushNotificationDisplayInfo
    {
        public PushNotificationStatusIcon status_icon;
        public PushNotificationType type;

        public string big_icon_url;
        public string title;
        public string text;
    }
}
