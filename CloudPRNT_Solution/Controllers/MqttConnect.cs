using MQTTnet.Client;
using MQTTnet;
using System.Security.Authentication;
using MQTTnet.Samples.Helpers;

namespace CloudPRNT_Solution.Controllers
{
    public class MqttConnect
    {
        public static async Task Clean_Disconnect()
        {
            /*
             * This sample disconnects in a clean way. This will send a MQTT DISCONNECT packet
             * to the server and close the connection afterwards.
             *
             * See sample _Connect_Client_ for more details.
             */

            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("broker.hivemq.com").Build();
                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                // This will send the DISCONNECT packet. Calling _Dispose_ without DisconnectAsync the 
                // connection is closed in a "not clean" way. See MQTT specification for more details.
                await mqttClient.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection).Build());
            }
        }

        public static async Task Connect_Client()
        {
            /*
             * This sample creates a simple MQTT client and connects to a public broker.
             *
             * Always dispose the client when it is no longer used.
             * The default version of MQTT is 3.1.1.
             */

            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                // Use builder classes where possible in this project.
                var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("broker.hivemq.com").Build();

                // This will throw an exception if the server is not available.
                // The result from this message returns additional data which was sent 
                // from the server. Please refer to the MQTT protocol specification for details.
                var response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                Console.WriteLine("The MQTT client is connected.");

                response.DumpToConsole();

                // Send a clean disconnect to the server by calling _DisconnectAsync_. Without this the TCP connection
                // gets dropped and the server will handle this as a non clean disconnect (see MQTT spec for details).
                var mqttClientDisconnectOptions = mqttFactory.CreateClientDisconnectOptionsBuilder().Build();

                await mqttClient.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);
            }
        }


    }
}
