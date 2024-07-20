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

namespace CloudPRNT_Solution.Controllers
{
    public class PrintController : Controller
    {
        private readonly PrintQueueContext _context;

        public PrintController(PrintQueueContext context)
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

         // GET: /Print/Print
        public async Task<IActionResult> Print(string mac, string protocol = null, string method = null, string jobType = null, PrintQueue printQueue = null)
        {
            // Process the parameters as needed
            // For example, you might want to log them or use them to perform a print job

            // Add logic here to handle the print request based on the parameters
            string chars = "1234567890abcdefghijklmnopqrstuvwxyz";
            var random = new Random();
            string jobToken = new string(chars.OrderBy(c => random.Next()).Take(16).ToArray());

            if (method == "print-job"){
                await Publish_Application_Message(mac,method,jobType,jobToken);
            }else if (method == "request-post") {

                //TriggerPost printing
                //HTTP printing
                StringBuilder OrderContent = new StringBuilder();
                OrderContent.Append("[align: center][font: a]\n");
                OrderContent.Append("[image: url http://star-emea.com/wp-content/uploads/2015/01/logo.jpg; width 60%; min-width 48mm]\n");
                OrderContent.Append("[magnify: width 2; height 2]\n");
                OrderContent.Append("[magnify: width 3; height 2]Columns[magnify]\n");
                OrderContent.Append("[align: left]\n");
                OrderContent.Append("[column: left: Item 1;      right: $10.00]\n");
                OrderContent.Append("[column: left: Item 2;      right: $9.95]\n");
                OrderContent.Append("[column: left: Item 3;      right: $103.50]\n");
                OrderContent.Append("[align: centre]\n");
                OrderContent.Append("[barcode: type code39; data 123456789012; height 15mm; module 0; hri]\n");
                OrderContent.Append("[align]\\");
                OrderContent.Append("Thank you for trying the new Star Document Markup Language\\ we hope you will find it useful. Please let us know!");
                OrderContent.Append("[cut: feed; partial]");

                printQueue.OrderContent = OrderContent.ToString();
                printQueue.PrinterMac = mac;
                printQueue.OpenDrawer = "no";
                printQueue.OrderName = jobToken;
                printQueue.OrderDate = DateTime.Now;

                _context.Add(printQueue);
                await _context.SaveChangesAsync();
                await Publish_Application_Message(mac,method,jobType,jobToken);

            }else{
                //HTTP printing
                StringBuilder OrderContent = new StringBuilder();
                OrderContent.Append("[align: center][font: a]\n");
                OrderContent.Append("[image: url http://star-emea.com/wp-content/uploads/2015/01/logo.jpg; width 60%; min-width 48mm]\n");
                OrderContent.Append("[magnify: width 2; height 2]\n");
                OrderContent.Append("[magnify: width 3; height 2]Columns[magnify]\n");
                OrderContent.Append("[align: left]\n");
                OrderContent.Append("[column: left: Item 1;      right: $10.00]\n");
                OrderContent.Append("[column: left: Item 2;      right: $9.95]\n");
                OrderContent.Append("[column: left: Item 3;      right: $103.50]\n");
                OrderContent.Append("[align: centre]\n");
                OrderContent.Append("[barcode: type code39; data 123456789012; height 15mm; module 0; hri]\n");
                OrderContent.Append("[align]\\");
                OrderContent.Append("Thank you for trying the new Star Document Markup Language\\ we hope you will find it useful. Please let us know!");
                OrderContent.Append("[cut: feed; partial]");

                printQueue.OrderContent = OrderContent.ToString();
                printQueue.PrinterMac = mac;
                printQueue.OpenDrawer = "no";
                printQueue.OrderName = jobToken;
                printQueue.OrderDate = DateTime.Now;

                _context.Add(printQueue);
                await _context.SaveChangesAsync();
            }

                


            // Return a JSON response or any other type of response
            
            return Json(new { success = true, message = "Print triggered.", mac = mac });
        } 

        public static async Task Publish_Application_Message(string mac, string method, string jobType, string jobToken)
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
                    .WithTcpServer("broker.hivemq.com", 1883)
                    .WithCleanSession(true)
                    .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                var topic = "";
                
                var payload = new Dictionary<string, object>();
                var printerControl = new Dictionary<string, object>();
                var cutter = new Dictionary<string, object>{
                    ["type"] = "partial",
                    ["feed"] = true
                };

                printerControl["cutter"] = cutter;

                if (method == "print-job"){
                    topic = "star/cloudprnt/to-device/" + mac + "/" + method;
                    
                    payload["title"] = method;
                    payload["jobToken"] = jobToken;

                    if (jobType == "raw"){
                        payload["jobType"] = jobType;
                        payload["mediaTypes"] = new List<string> { "text/plain" };
                        string printDataText = "StarMicoronics.\n\nCloudPRNT Version MQTT\n\nPrint by Full MQTT.";
                        payload["printData"] = printDataText;
                        payload["printerControl"] = printerControl;
                    }
                    else if (jobType == "url"){
                        payload["jobType"] = jobType;
                        payload["mediaTypes"] = new List<string> { "text/plain" };
                        payload["printData"] = "http://192.168.86.24:7148/CloudPRNT/PassURL";
                        payload["printerControl"] = printerControl;
                    }

                }else if (method == "request-post") {
                    topic = "star/cloudprnt/to-device/" + mac + "/" + method;
                    payload["title"] = method;
                }

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

                Console.WriteLine("MQTT application message is published.");
            }
        }
    }
}