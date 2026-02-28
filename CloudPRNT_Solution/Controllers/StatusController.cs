using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using MQTTnet;
using MQTTnet.Client;
using System.Collections.Generic;
using System.Linq;
using System;
using SixLabors.ImageSharp.Formats.Png;
using CloudPRNT_Solution.Controllers;
using CloudPRNT_Solution.Data;
using CloudPRNT_Solution.Models;
using System.Net.Security;

namespace CloudPRNT_Solution.Controllers
{
    public class StatusController : Controller
    {
        private readonly PrintQueueContext _context;

        public StatusController(PrintQueueContext context)
        {
            _context = context;
        }
        public IActionResult Index(string mac)
        {
            if (string.IsNullOrEmpty(mac))
            {
                // Handle the case where no MAC address is provided
                return BadRequest("MAC address is required.");
            }

            string decodedMac = HttpUtility.UrlDecode(mac);

            ViewBag.MacAddress = decodedMac;
            return View();
        }

         // GET: /Status/Status
        public async Task<IActionResult> Status(string mac, string protocol = null, string method = null)
        {
            // Process the parameters as needed
            // For example, you might want to log them or use them to perform a print job

            // Add logic here to handle the print request based on the parameters
        

            if (method == "request-client-status"){
                await Publish_Application_Status_Message(mac,method);
            
            }

            // Return a JSON response or any other type of response
            
            return Json(new { success = true, message = "Status Triggered.", mac = mac });
        } 

        public async Task<IActionResult> ClientAction(string mac, string protocol = null, string method = null)
        {
            if (method == "order-client-action"){
                await Publish_Application_Client_Action_Message(mac,method);
            }

            return Json(new { success = true, message = "Client Action Triggered.", mac = mac });
        }

        public static async Task Publish_Application_Status_Message(string mac, string method)
        {
        
            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
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
                .WithCredentials("cloudPrntServerOreoNetAppPublisherStatus", "oreoawslambdapassword")
                .WithClientId("cloudPrntServerOreoNetAppPublisherStatus")
                .WithCleanSession(false)
                .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                var topic = "star/cloudprnt/to-device/" + mac + "/" + method;
                var payload = new Dictionary<string, object>();
                payload["title"] = method;


                Console.WriteLine(topic);

                 // Serialize the payload to a JSON string.
                string jsonPayload = JsonSerializer.Serialize(payload);

                // Convert the JSON string to a byte array.
                byte[] payloadBytes = Encoding.UTF8.GetBytes(jsonPayload);
                

                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payloadBytes)
                    .Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                await mqttClient.DisconnectAsync();

                Console.WriteLine("MQTT status application message is published.");
            }
        }

        public static async Task Publish_Application_Client_Action_Message(string mac, string method)
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
                .WithCredentials("cloudPrntServerOreoNetAppPublisherClientAction", "oreoawslambdapassword")
                .WithClientId("cloudPrntServerOreoNetAppPublisherClientAction")
                .WithCleanSession(false)
                .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                var topic = "star/cloudprnt/to-device/" + mac + "/" + method;
                
                var payload = new
                {
                    title = "order-client-action",
                    clientAction = new[]
                        {
                            new { request = "ClientType", options = "" },
                            new { request = "ClientVersion", options = ""},
                            new { request = "PageInfo", options = ""}
                        }
                };

               
                // Serialize the payload to a JSON string.
                string jsonPayload = JsonSerializer.Serialize(payload);

                // Convert the JSON string to a byte array.
                byte[] payloadBytes = Encoding.UTF8.GetBytes(jsonPayload);

                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payloadBytes)
                    .Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                await mqttClient.DisconnectAsync();

                Console.WriteLine("MQTT application message is published. ");
            }
        }
    }
}