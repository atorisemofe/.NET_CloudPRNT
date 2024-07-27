using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Client;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace CloudPRNT_Solution.Controllers
{
    public class Client_Subscribe : Controller
    {
        private static IServiceProvider _serviceProvider;

        public static void Configure(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static async Task Connect_Client()
        {
            var mqttFactory = new MqttFactory();
            var subscribed = false;

            var managedMqttClient = mqttFactory.CreateManagedMqttClient();

            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer("broker.hivemq.com")
                .Build();

            var managedMqttClientOptions = new ManagedMqttClientOptionsBuilder()
                .WithClientOptions(mqttClientOptions)
                .Build();

            managedMqttClient.ApplicationMessageReceivedAsync += HandleReceivedApplicationMessageAsync;

            await managedMqttClient.StartAsync(managedMqttClientOptions);

            Console.WriteLine("The managed MQTT client is connected.");

            managedMqttClient.SubscriptionsChangedAsync += args => SubscriptionsResultAsync(args, ref subscribed);
            await managedMqttClient.SubscribeAsync("star/cloudprnt/to-server/#").ConfigureAwait(false);

            SpinWait.SpinUntil(() => subscribed, 1000);
            Console.WriteLine($"Subscription properly done: {subscribed}");
        }

        private static Task SubscriptionsResultAsync(SubscriptionsChangedEventArgs arg, ref bool subscribed)
        {
            foreach (var mqttClientSubscribeResult in arg.SubscribeResult)
            {
                Console.WriteLine($"Subscription reason: {mqttClientSubscribeResult.ReasonString}");
                foreach (var item in mqttClientSubscribeResult.Items)
                {
                    Console.WriteLine($"For topic filter {item.TopicFilter}, result code: {item.ResultCode}");

                    if (item.TopicFilter.Topic == "star/cloudprnt/to-server/#" && item.ResultCode == MqttClientSubscribeResultCode.GrantedQoS0 && !subscribed)
                    {
                        subscribed = true;
                    }
                }
            }

            foreach (var mqttClientUnsubscribeResult in arg.UnsubscribeResult)
            {
                Console.WriteLine($"Unsubscription reason: {mqttClientUnsubscribeResult.ReasonString}");
                foreach (var item in mqttClientUnsubscribeResult.Items)
                {
                    Console.WriteLine($"For topic filter {item.TopicFilter}, result code: {item.ResultCode}");
                }
            }

            return Task.CompletedTask;
        }

        private static async Task HandleReceivedApplicationMessageAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            var topic = eventArgs.ApplicationMessage.Topic;
            var payload = eventArgs.ApplicationMessage.Payload == null ? null : Encoding.UTF8.GetString(eventArgs.ApplicationMessage.Payload);
            var qos = eventArgs.ApplicationMessage.QualityOfServiceLevel;
            var retain = eventArgs.ApplicationMessage.Retain;
            
            Console.WriteLine($"Received message: Topic = {topic}, Payload = {payload}, QoS = {qos}, Retain = {retain}");


            var topicSegments = topic.Split('/');
            if (topicSegments.Length >= 5)
            {
                var printerMac = topicSegments[3];
                var messageType = topicSegments[4];

                using (var scope = _serviceProvider.CreateScope())
                {
                    var controller = scope.ServiceProvider.GetRequiredService<MqttMessageRecieved>();
                    JObject payloadObj = JObject.Parse(payload);

                    switch (messageType)
                    {
                        case "client-status":
                            controller.HandleClientStatus(printerMac, payloadObj);
                            break;
                        case "print-result":
                            controller.HandlePrintResult(printerMac, payloadObj);
                            break;
                        case "client-will":
                            controller.HandleClientWill(printerMac, payloadObj);
                            break;
                        default:
                            Console.WriteLine($"Unknown message type: {messageType}");
                            break;
                    }
                }
            }
        }
    }
}
