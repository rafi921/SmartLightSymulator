using System;

namespace SmartLightSymulator
{
    public class DeviceMode
    {
        public static DeviceMode AUTOMATIC { get; internal set; }

        public static DeviceMode MANUAL { get; internal set; }

        internal dynamic ToString(string v)
        {
            throw new NotImplementedException();
        }
    }
}