using MQTTnet.Client;
using MQTTnet;
using System;
using System.Text;
using System.Text.Json;

namespace CloudPRNT_Solution.Controllers
{
    public class Client_Publish
    {

        public static async Task Publish_Application_Message()
        {
            /*
             * This sample pushes a simple application message including a topic and a payload.
             *
             * Always use builders where they exist. Builders (in this project) are designed to be
             * backward compatible. Creating an _MqttApplicationMessage_ via its constructor is also
             * supported but the class might change often in future releases where the builder does not
             * or at least provides backward compatibility where possible.
             */

            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer("broker.hivemq.com")
                    .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                var payloadObject = new
                {
                    title = "request-post"
                };

                // Serialize the payload to a JSON string.
                string jsonPayload = JsonSerializer.Serialize(payloadObject);

                // Convert the JSON string to a byte array.
                byte[] payloadBytes = Encoding.UTF8.GetBytes(jsonPayload);

                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("star/cloudprnt/to-device/1")
                    .WithPayload(payloadBytes)
                    .Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                await mqttClient.DisconnectAsync();

                Console.WriteLine("MQTT application message is published.");
            }
        }

    }
}
