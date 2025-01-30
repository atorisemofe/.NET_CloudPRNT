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
        public async Task<IActionResult> Print(string mac, string protocol = null, string method = null, string jobType = null, PrintQueue printQueue = null, string drawerAction = null)
        {
            // Process the parameters as needed
            // For example, you might want to log them or use them to perform a print job

            // Add logic here to handle the print request based on the parameters
            string chars = "1234567890abcdefghijklmnopqrstuvwxyz";
            var random = new Random();
            string jobToken = new string(chars.OrderBy(c => random.Next()).Take(16).ToArray());

            if (method == "print-job"){
                await Publish_Application_Message(mac,method,jobType,jobToken,drawerAction);
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
                await Publish_Application_Message(mac,method,jobType,jobToken,drawerAction);

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

        public static async Task Publish_Application_Message(string mac, string method, string jobType, string jobToken, string drawerAction)
        {
            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer("broker.hivemq.com",1883)
                    .WithCleanSession(true)
                    .WithWillQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                try
                {
                    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                    var topic = $"star/cloudprnt/to-device/{mac}/{method}";

                    var payload = new Dictionary<string, object>();
                    var printerControl = new Dictionary<string, object>();
                    var cutter = new Dictionary<string, object>
                    {
                        ["type"] = "partial",
                        ["feed"] = true
                    };

                    printerControl["cutter"] = cutter;

                    if (method == "print-job")
                    {
                        payload["title"] = method;
                        payload["jobToken"] = jobToken;

                        if (jobType == "raw")
                        {
                            if (drawerAction == "open-drawer")
                            {
                                payload["jobType"] = jobType;
                                payload["mediaTypes"] = new List<string> { "application/vnd.star.starprnt" };
                                payload["printData"] = "Bw=="; // Base64 encoded drawer open command
                            }
                            else
                            {
                                payload["jobType"] = jobType;
                                payload["mediaTypes"] = new List<string> { "text/plain" };

                                //test flawless printRaw data
                                StringBuilder job = new StringBuilder();
                                job.Append("[align: centre]\n");
                                job.Append($"[image: url https://cloudprint.flawlessretail.com/receipt.png]");
                                job.Append("[cut: feed; partial]");
                                byte[] jobData = Encoding.UTF8.GetBytes(job.ToString());
                                // get the requested output media type from the query string
                                using (var ms = new MemoryStream())
                                {
                                    StarMicronics.CloudPrnt.Document.Convert(jobData, "text/vnd.star.markup", ms, "application/vnd.star.starprnt", null);
                                    payload["printData"] = Convert.ToBase64String(ms.ToArray());
                                }

                                // payload["printData"] = "StarMicoronics.\n\nCloudPRNT Version MQTT\n\nPrint by Full MQTT.";
                                payload["printerControl"] = printerControl;
                            }
                        }
                        else if (jobType == "url")
                        {
                            payload["jobType"] = jobType;
                            payload["mediaTypes"] = new List<string> { "application/vnd.star.starprnt" };
                            payload["printData"] = "https://cloudprnt-solution-1e68.onrender.com/CloudPRNT/PassURL";
                            payload["printerControl"] = printerControl;
                        }
                    }
                    else if (method == "request-post")
                    {
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
                    Console.WriteLine("MQTT application message is published.");
                }
                catch (MQTTnet.Adapter.MqttConnectingFailedException ex)
                {
                    Console.WriteLine($"MQTT connection failed: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
                finally
                {
                    await mqttClient.DisconnectAsync();
                }
            }
        }

    }
}