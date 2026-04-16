// Services/MqttClientService.cs
using MQTTnet.Extensions.ManagedClient;

namespace CloudPRNT_Solution.Services
{
    public class MqttClientService
    {
        public IManagedMqttClient Client { get; set; }
    }
}