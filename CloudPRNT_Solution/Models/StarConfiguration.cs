using System;
using System.Collections.Generic;

namespace CloudPRNT_Solution.Models
{
    public class StarConfiguration
    {
        // Properties for the static values (to ensure they are serialized)
        public string Title { get; } = "star_configuration";
        public string Version { get; } = "1.5.0";
        public string Notification { get; } = "all";
        public string PrintWhenCompletedType { get; } = "ascii";

        public string PrintWhenCompleted { get; } = "Polling Update Complete\n\n\n\n\n";

        // Nested class for password-protected settings
        public class PasswordProtectedSettings
        {
            public string CurrentPassword { get; } = "public";

            // Nested class for cloudprnt settings
            public class CloudPrntSettings
            {
                public int PollingTime { get; set; }

                public CloudPrntSettings(int pollingTime)
                {
                    PollingTime = pollingTime;
                }
            }

            public CloudPrntSettings Cloudprnt { get; }

            public PasswordProtectedSettings(int pollingTime)
            {
                Cloudprnt = new CloudPrntSettings(pollingTime);
            }
        }

        // The list of configurations (in this case, we have only one)
        public List<DeviceConfiguration> Configurations { get; }

        public class DeviceConfiguration
        {
            public string DeviceName { get; } = "mC-Label3";
            public PasswordProtectedSettings PasswordProtectedSettings { get; }

            public DeviceConfiguration(int pollingTime)
            {
                PasswordProtectedSettings = new PasswordProtectedSettings(pollingTime);
            }
        }

        // Constructor to initialize the configuration
        public StarConfiguration(int pollingTime)
        {
            Configurations = new List<DeviceConfiguration>
            {
                new DeviceConfiguration(pollingTime)
            };
        }
    }
}
