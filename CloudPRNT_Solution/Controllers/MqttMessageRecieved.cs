using Microsoft.AspNetCore.Mvc;
using CloudPRNT_Solution.Controllers;
using MQTTnet.Server;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CloudPRNT_Solution.Data;
using Microsoft.AspNetCore.SignalR;
using CloudPRNT_Solution.Hubs;
using Newtonsoft.Json.Linq;
using CloudPRNT_Solution.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudPRNT_Solution.Controllers;

    [Route("api/[controller]")]
    [ApiController]
    public class MqttMessageRecieved : ControllerBase
    {
        private readonly DeviceTableContext _context;

        // public MqttMessageRecieved(DeviceTableContext context){
        //  _context = context;
        // }

        private readonly IHubContext<NotificationHub> _hubContext;

        public MqttMessageRecieved(IHubContext<NotificationHub> hubContext, DeviceTableContext context)
        {
            _hubContext = hubContext;
            _context = context;
        }

        [HttpPost("client-status/{printerMac}")]
        public async Task<OkObjectResult> HandleClientStatus(string printerMac, [FromBody] JObject payload)
        {
            var clientActionArray = payload["clientAction"] as JArray;
            var clientTypeResult = "";
            var clientVersionResult = "";
            var status = payload["statusCode"];
            var printWidth = "";

            if (clientActionArray != null)
            {
                clientTypeResult = clientActionArray.FirstOrDefault(x => x["request"]?.ToString() == "ClientType")?["result"]?.ToString();
                clientVersionResult = clientActionArray.FirstOrDefault(x => x["request"]?.ToString() == "ClientVersion")?["result"]?.ToString();
                var pageInfoResult = clientActionArray.FirstOrDefault(x => x["request"]?.ToString() == "PageInfo")?["result"];

                string dotWidth = null;
                if (pageInfoResult != null && pageInfoResult["printWidth"] != null)
                {
                    printWidth = pageInfoResult["printWidth"]?.ToString() + "mm";
                }

                Console.WriteLine($"ClientType Result: {clientTypeResult}");
                Console.WriteLine($"ClientVersion Result: {clientVersionResult}");
                Console.WriteLine($"DotWidth (PrintWidth) Result: {dotWidth}");

                // Add your custom logic here using clientTypeResult and clientVersionResult
                // Query the database to find the existing DeviceTable row
                var deviceInfo = await _context.DeviceTable.FirstOrDefaultAsync(d => d.PrinterMac == printerMac);

                if (deviceInfo != null)
                {
                    // Update the values
                    deviceInfo.ClientType = clientTypeResult;
                    deviceInfo.ClientVersion = clientVersionResult;
                    deviceInfo.PrintWidth = printWidth;
                    deviceInfo.Status = status?.ToString().Replace("%20"," ");

                    // Save the changes to the database
                    _context.Update(deviceInfo);
                    await _context.SaveChangesAsync();
                }
                else
                {
                Console.WriteLine($"No device found with PrinterMac: {printerMac}");
                }
            }else {
                var deviceInfo = await _context.DeviceTable.FirstOrDefaultAsync(d => d.PrinterMac == printerMac);

                if (deviceInfo != null)
                {
                    // Update the values
                    deviceInfo.Status = status?.ToString().Replace("%20"," ");

                    // Save the changes to the database
                    _context.Update(deviceInfo);
                    await _context.SaveChangesAsync();
                }
                else
                {
                Console.WriteLine($"No device found with PrinterMac: {printerMac}");
                }
            }
            await _hubContext.Clients.All.SendAsync("RecieveStatusResult", status?.ToString().Replace("%20"," " ));
            return Ok(new { status });
        }

        [HttpPost("print-result/{printerMac}")]
        public async Task<IActionResult> HandlePrintResult(string printerMac, [FromBody] JObject payload)
        {
            Console.WriteLine($"Handling print result for printer {printerMac} with payload: {payload}");

            bool printSucceeded = payload["printSucceeded"].Value<bool>();
            string status = payload["statusCode"].Value<string>();
            status = status.Replace("%20","_");

            // Send message to all connected clients
            await _hubContext.Clients.All.SendAsync("ReceivePrintResult", printSucceeded, status);

            // Return the result to the client
            return Ok(new { printSucceeded, status });
        }
        

        [HttpPost("client-will/{printerMac}")]
        public void HandleClientWill(string printerMac, [FromBody] JObject payload)
        {
            Console.WriteLine($"Handling client will for printer {printerMac} with payload: {payload}");
            // Add your custom logic here
        }

        private bool ExtractPrintSucceeded(string payload)
        {
            // Assuming payload is a JSON string, extract the printSucceeded value.
            // You'll need to parse the JSON string. Here's a basic example using Newtonsoft.Json:
            dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(payload);
            return json.printSucceeded;
        }
    }
