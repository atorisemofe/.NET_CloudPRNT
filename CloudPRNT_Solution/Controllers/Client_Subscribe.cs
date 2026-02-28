// using MQTTnet;
// using MQTTnet.Extensions.ManagedClient;
// using MQTTnet.Client;
// using Microsoft.AspNetCore.Mvc;
// using System.Threading.Tasks;
// using System;
// using System.Text;
// using System.Threading;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.DependencyInjection;
// using Newtonsoft.Json.Linq;

// namespace CloudPRNT_Solution.Controllers
// {
//     public class Client_Subscribe : Controller
//     {
//         private static IServiceProvider _serviceProvider;

//         public static void Configure(IServiceProvider serviceProvider)
//         {
//             _serviceProvider = serviceProvider;
//         }

//         public static async Task Connect_Client()
//         {
//             var mqttFactory = new MqttFactory();
//             var subscribed = false;

//             var managedMqttClient = mqttFactory.CreateManagedMqttClient();

//             var mqttClientOptions = new MqttClientOptionsBuilder()
//                 .WithTcpServer("broker.hivemq.com")
//                 .Build();

//             var managedMqttClientOptions = new ManagedMqttClientOptionsBuilder()
//                 .WithClientOptions(mqttClientOptions)
//                 .Build();

//             managedMqttClient.ApplicationMessageReceivedAsync += HandleReceivedApplicationMessageAsync;

//             await managedMqttClient.StartAsync(managedMqttClientOptions);

//             Console.WriteLine("The managed MQTT client is connected.");

//             managedMqttClient.SubscriptionsChangedAsync += args => SubscriptionsResultAsync(args, ref subscribed);
//             await managedMqttClient.SubscribeAsync("star/cloudprnt/to-server/#").ConfigureAwait(false);

//             SpinWait.SpinUntil(() => subscribed, 1000);
//             Console.WriteLine($"Subscription properly done: {subscribed}");
//         }

//         private static Task SubscriptionsResultAsync(SubscriptionsChangedEventArgs arg, ref bool subscribed)
//         {
//             foreach (var mqttClientSubscribeResult in arg.SubscribeResult)
//             {
//                 Console.WriteLine($"Subscription reason: {mqttClientSubscribeResult.ReasonString}");
//                 foreach (var item in mqttClientSubscribeResult.Items)
//                 {
//                     Console.WriteLine($"For topic filter {item.TopicFilter}, result code: {item.ResultCode}");

//                     if (item.TopicFilter.Topic == "star/cloudprnt/to-server/#" && item.ResultCode == MqttClientSubscribeResultCode.GrantedQoS0 && !subscribed)
//                     {
//                         subscribed = true;
//                     }
//                 }
//             }

//             foreach (var mqttClientUnsubscribeResult in arg.UnsubscribeResult)
//             {
//                 Console.WriteLine($"Unsubscription reason: {mqttClientUnsubscribeResult.ReasonString}");
//                 foreach (var item in mqttClientUnsubscribeResult.Items)
//                 {
//                     Console.WriteLine($"For topic filter {item.TopicFilter}, result code: {item.ResultCode}");
//                 }
//             }

//             return Task.CompletedTask;
//         }

//         private static async Task HandleReceivedApplicationMessageAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
//         {
//             var topic = eventArgs.ApplicationMessage.Topic;
//             var payload = eventArgs.ApplicationMessage.Payload == null ? null : Encoding.UTF8.GetString(eventArgs.ApplicationMessage.Payload);
//             var qos = eventArgs.ApplicationMessage.QualityOfServiceLevel;
//             var retain = eventArgs.ApplicationMessage.Retain;
            
//             Console.WriteLine($"Received message: Topic = {topic}, Payload = {payload}, QoS = {qos}, Retain = {retain}");


//             var topicSegments = topic.Split('/');
//             if (topicSegments.Length >= 5)
//             {
//                 var printerMac = topicSegments[3];
//                 var messageType = topicSegments[4];

//                 using (var scope = _serviceProvider.CreateScope())
//                 {
//                     var controller = scope.ServiceProvider.GetRequiredService<MqttMessageRecieved>();
//                     JObject payloadObj = JObject.Parse(payload);

//                     switch (messageType)
//                     {
//                         case "client-status":
//                             await controller.HandleClientStatus(printerMac, payloadObj);
//                             break;
//                         case "print-result":
//                             await controller.HandlePrintResult(printerMac, payloadObj);
//                             break;
//                         case "client-will":
//                             controller.HandleClientWill(printerMac, payloadObj);
//                             break;
//                         default:
//                             Console.WriteLine($"Unknown message type: {messageType}");
//                             break;
//                     }
//                 }
//             }
//         }
//     }
// }

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace CloudPRNT_Solution.Controllers
{
    public class Client_Subscribe : Controller
    {
        private static IServiceProvider _serviceProvider;

        // Help wait for connection and subscription status
        private static readonly TaskCompletionSource<bool> _connectedTcs = new();
        private static readonly TaskCompletionSource<bool> _subscribedTcs = new();

        public static void Configure(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static async Task Connect_Client()
        {
            var mqttFactory = new MqttFactory();
            var managedMqttClient = mqttFactory.CreateManagedMqttClient();
            string rawPassword = "oreoawslambdapassword";

            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer("a3u49iypdlbgbs-ats.iot.us-east-2.amazonaws.com", 443)
                .WithoutPacketFragmentation()
                .WithTlsOptions(o =>
                {
                    o.UseTls();

                    // REQUIRED for AWS IoT custom auth on port 443
                    o.WithApplicationProtocols(new List<SslApplicationProtocol>
                    {
                        new SslApplicationProtocol("mqtt")
                    });

                    // Force TLS versions AWS supports (helps on some .NET runtimes/OS combos)
                    o.WithSslProtocols(System.Security.Authentication.SslProtocols.Tls12);
                })
                .WithCredentials("cloudPrntServerOreoNetAppSubscriber", rawPassword)
                // .WithClientId("cloudPrntServer01")
                .WithClientId("cloudPrntServerOreoNetAppSubscriber")
                .WithCleanSession(false)
                .Build();

            var managedOptions = new ManagedMqttClientOptionsBuilder()
                .WithClientOptions(mqttClientOptions)
                .Build();

            // ── Connection events ────────────────────────────────────────────────
            managedMqttClient.ConnectedAsync += e =>
            {
                Console.WriteLine($"[MQTT] Connected successfully. Result code: {e.ConnectResult.ResultCode}");
                if (e.ConnectResult.ResultCode == MqttClientConnectResultCode.Success)
                {
                    _connectedTcs.TrySetResult(true);
                }
                else
                {
                    _connectedTcs.TrySetException(new Exception($"Connect failed: {e.ConnectResult.ResultCode} - {e.ConnectResult.ReasonString}"));
                }
                return Task.CompletedTask;
            };

            managedMqttClient.ConnectingFailedAsync += e =>
            {
                Console.WriteLine($"[MQTT] Connection attempt failed: {e.Exception?.Message}");
                if (e.Exception?.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {e.Exception.InnerException.Message}");
                }
                _connectedTcs.TrySetException(e.Exception ?? new Exception("Connection failed - no details"));
                return Task.CompletedTask;
            };

            // ── Message received ─────────────────────────────────────────────────
            managedMqttClient.ApplicationMessageReceivedAsync += HandleReceivedApplicationMessageAsync;
            Console.WriteLine("[DEBUG] ApplicationMessageReceivedAsync handler registered.");

            // ── Subscription changes ─────────────────────────────────────────────
            managedMqttClient.SubscriptionsChangedAsync += args =>
            {
                bool success = false;

                foreach (var subResult in args.SubscribeResult)
                {
                    Console.WriteLine($"[MQTT] Subscription result reason: {subResult.ReasonString}");

                    foreach (var item in subResult.Items)
                    {
                        Console.WriteLine($"  Topic: {item.TopicFilter}, Result: {item.ResultCode}");

                        if (item.TopicFilter.Topic == "star/cloudprnt/to-server/#" &&
                            (item.ResultCode == MqttClientSubscribeResultCode.GrantedQoS0 ||
                             item.ResultCode == MqttClientSubscribeResultCode.GrantedQoS1 ||
                             item.ResultCode == MqttClientSubscribeResultCode.GrantedQoS2))
                        {
                            success = true;
                        }
                    }
                }

                if (success)
                {
                    Console.WriteLine("[MQTT] Subscription to star/cloudprnt/to-server/# granted");
                    _subscribedTcs.TrySetResult(true);
                }

                return Task.CompletedTask;
            };

            Console.WriteLine("Starting managed MQTT client to AWS IoT...");

            try
            {
                await managedMqttClient.StartAsync(managedOptions);

                // Wait for connection (up to ~15 seconds)
                var connectTask = await Task.WhenAny(_connectedTcs.Task, Task.Delay(TimeSpan.FromSeconds(15)));

                if (!_connectedTcs.Task.IsCompletedSuccessfully)
                {
                    Console.WriteLine("Failed to connect within timeout. Check credentials, authorizer, network.");
                    return;
                }

                Console.WriteLine("MQTT connection established → subscribing...");

                // Subscribe
                await managedMqttClient.SubscribeAsync(new[]
                {
                    new MqttTopicFilterBuilder()
                        .WithTopic("star/cloudprnt/to-server/#")
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                        .Build()
                });

                // Give time for subscription ACK (usually very fast)
                await Task.WhenAny(_subscribedTcs.Task, Task.Delay(TimeSpan.FromSeconds(8)));

                if (_subscribedTcs.Task.IsCompletedSuccessfully)
                {
                    Console.WriteLine("Subscription confirmed.");
                }
                else
                {
                    Console.WriteLine("Subscription ACK not received in time – but client is connected.");
                }

                // Keep running (the managed client handles reconnects)
                Console.WriteLine("MQTT client is now running in background.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical MQTT startup error: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($" → {ex.InnerException.Message}");
            }
        }

        private static async Task HandleReceivedApplicationMessageAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            Console.WriteLine("[DEBUG] Handler invoked for topic: " + eventArgs.ApplicationMessage.Topic);
            try
            {
                var topic = eventArgs.ApplicationMessage.Topic;
                var payloadBytes = eventArgs.ApplicationMessage.PayloadSegment;
                var payload = payloadBytes.Count == 0 ? null : Encoding.UTF8.GetString(payloadBytes);

                Console.WriteLine($"[MQTT] Received → Topic: {topic}, QoS: {eventArgs.ApplicationMessage.QualityOfServiceLevel}, Retain: {eventArgs.ApplicationMessage.Retain}");
                if (payload != null)
                    Console.WriteLine($"Payload: {payload}");

                var segments = topic.Split('/');
                if (segments.Length < 5) return;

                var printerMac = segments[3];
                var messageType = segments[4];

                using var scope = _serviceProvider.CreateScope();
                var controller = scope.ServiceProvider.GetRequiredService<MqttMessageRecieved>();

                if (payload == null) return;

                var payloadObj = JObject.Parse(payload);

                switch (messageType.ToLowerInvariant())
                {
                    case "client-status":
                        await controller.HandleClientStatus(printerMac, payloadObj);
                        break;

                    case "print-result":
                        await controller.HandlePrintResult(printerMac, payloadObj);
                        break;

                    case "client-will":
                        controller.HandleClientWill(printerMac, payloadObj);
                        break;

                    default:
                        Console.WriteLine($"[MQTT] Unknown message type: {messageType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing received message: {ex.Message}");
            }
        }
    }
}