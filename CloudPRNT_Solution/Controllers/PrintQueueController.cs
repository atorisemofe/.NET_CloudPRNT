using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CloudPRNT_Solution.Data;
using CloudPRNT_Solution.Models;
using System.Text;
using System.Text.Json;
using MQTTnet.Client;
using MQTTnet;
using StarMicronics.CloudPrnt;
using System.IO;

namespace CloudPRNT_Solution.Controllers
{
    public class PrintQueueController : Controller
    {
        private readonly PrintQueueContext _context;

        public PrintQueueController(PrintQueueContext context)
        {
            _context = context;
        }


        // GET: PrintQueue
        public async Task<IActionResult> Index()
        {
              return View(await _context.PrintQueue.ToListAsync());
        }

        // GET: PrintQueue/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PrintQueue == null)
            {
                return NotFound();
            }

            var printQueue = await _context.PrintQueue
                .FirstOrDefaultAsync(m => m.Id == id);
            if (printQueue == null)
            {
                return NotFound();
            }

            return View(printQueue);
        }

        // GET: PrintQueue/Create
        public IActionResult Create()
        {
            return View();
        }

        // GET: PrintQueue/Drawer
        public IActionResult Drawer()
        {
            return View();
        }

        //MQTT Trigger Post Raw Data
        [Obsolete]
        public static async Task Publish_Application_Message_Raw(string mac, string topic, byte[] printJob, string jobtoken)
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
                    //.WithClientId("OreoluwaPC")
                    .WithTcpServer("broker.hivemq.com", 1883)
                    //.WithCredentials("oreoluwa", "#Nigeria245")
                   // .WithTls(new MqttClientOptionsBuilderTlsParameters
                   // {
                   //     UseTls = true, // Enable TLS
                  //      AllowUntrustedCertificates = true, // You can set this to true if you are using a self-signed certificate
                  //      CertificateValidationHandler = context => true // Set this to false in production, use a proper certificate validation logic
                  //  })
                    .WithCleanSession(true)
                    //.WithKeepAlivePeriod(TimeSpan.FromSeconds(60)) // Set keep-alive period
                    .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);


                var payloadObject = new
                {
                    title = topic,
                    jobToken = jobtoken,
                    jobType = "raw",
                    mediaTypes = new[] { "application/vnd.star.starprnt" },
                    printData = Convert.ToBase64String(printJob)
                    //printerControl = new { cashDrawer = openDrawer }
                };

                // Serialize the payload to a JSON string.
                string jsonPayload = JsonSerializer.Serialize(payloadObject);

                // Convert the JSON string to a byte array.
                byte[] payloadBytes = Encoding.UTF8.GetBytes(jsonPayload);

                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("star/cloudprnt/to-device/" + mac + "/" + topic)
                    .WithPayload(payloadBytes)
                    .Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                await mqttClient.DisconnectAsync();

                Console.WriteLine("MQTT application message is published.");
            }
        }



        //MQTT Trigger Post URL Data
        [Obsolete]
        public static async Task Publish_Application_Message_URL(string mac, string topic, string jobtoken)
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
                    //.WithClientId("OreoluwaPC")
                    .WithTcpServer("broker.hivemq.com", 1883)
                    //.WithCredentials("oreoluwa", "#Nigeria245")
                    // .WithTls(new MqttClientOptionsBuilderTlsParameters
                    // {
                    //     UseTls = true, // Enable TLS
                    //      AllowUntrustedCertificates = true, // You can set this to true if you are using a self-signed certificate
                    //      CertificateValidationHandler = context => true // Set this to false in production, use a proper certificate validation logic
                    //  })
                    .WithCleanSession(true)
                    //.WithKeepAlivePeriod(TimeSpan.FromSeconds(60)) // Set keep-alive period
                    .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);


                var payloadObject = new
                {
                    title = topic,
                    jobToken = jobtoken,
                    jobType = "url",
                    mediaTypes = new[] { "application/vnd.star.starprnt" },
                    printData = "http://192.168.86.24:7148/CloudPRNT"
                    //printerControl = new { cashDrawer = openDrawer }
                };

                // Serialize the payload to a JSON string.
                string jsonPayload = JsonSerializer.Serialize(payloadObject);

                // Convert the JSON string to a byte array.
                byte[] payloadBytes = Encoding.UTF8.GetBytes(jsonPayload);

                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("star/cloudprnt/to-device/" + mac + "/" + topic)
                    .WithPayload(payloadBytes)
                    .Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                await mqttClient.DisconnectAsync();

                Console.WriteLine("MQTT application message is published.");
            }
        }


        // POST: PrintQueue/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PrinterMac,OrderName,OrderDate,OpenDrawer")] PrintQueue printQueue)
        {
            if (ModelState.IsValid)
            {
                var printerMacLower = printQueue.PrinterMac.ToLower();
                var openDrawer = printQueue.OpenDrawer.ToString().ToLower();
                var customerName = printQueue.OrderName.ToString();
                StringBuilder OrderContent = new StringBuilder();
                //OrderContent.Append("\x07");
                OrderContent.Append("[align: center][font: a]\n");
                OrderContent.Append("[image: url http://star-emea.com/wp-content/uploads/2015/01/logo.jpg; width 60%; min-width 48mm]\n");
                OrderContent.Append("[magnify: width 2; height 2]\n");
                OrderContent.Append("Customer Name: " + customerName + "\n");
                OrderContent.Append("[magnify: width 3; height 2]Columns[magnify]\n");
                OrderContent.Append("[align: left]\n");
                OrderContent.Append("[column: left: Item 1;      right: $10.00]\n");
                OrderContent.Append("[column: left: Item 2;      right: $9.95]\n");
                OrderContent.Append("[column: left: Item 3;      right: $103.50]\n");
                OrderContent.Append("[align: centre]\n");
                OrderContent.Append("[barcode: type code39; data 123456789012; height 15mm; module 0; hri]\n");
                OrderContent.Append("[align]\\");
                OrderContent.Append("Thank you for trying the new Star Document Markup Language\\ we hope you will find it useful. Please let us know!");
                OrderContent.Append("\\x07"); //open drawer
                OrderContent.Append("[cut: feed; partial]");

                printQueue.PrinterMac = printerMacLower;
                printQueue.OrderContent = OrderContent.ToString();
                printQueue.OpenDrawer = openDrawer;

                //var printJob = Encoding.UTF8.GetBytes(OrderContent.ToString());

               // var outputData = new MemoryStream();
                //var outputFormat = "application/vnd.star.starprnt";

                
                //ICpDocument markupDoc = Document.GetDocument(printJob, "text/vnd.star.markup");
                //markupDoc.convertTo(outputFormat, outputData);

              //  if (openDrawer.ToString() == "start")
              //  {
              //      ICpDocument markupDoc = Document.GetDocument(printJob, "text/vnd.star.markup");
              //      markupDoc.JobConversionOptions.OpenCashDrawer = DrawerOpenTime.StartOfJob;
              //      markupDoc.convertTo(outputFormat, outputData);
                    //return new FileContentResult(outputData.ToArray(), outputFormat);
//
              //  }
             //  else if ((openDrawer.ToString() == "end"))
               // {

               //     ICpDocument markupDoc = Document.GetDocument(printJob, "text/vnd.star.markup");
               //     markupDoc.JobConversionOptions.OpenCashDrawer = DrawerOpenTime.EndOfJob;
              //      markupDoc.convertTo(outputFormat, outputData);
                    //return new FileContentResult(outputData.ToArray(), outputFormat);

              //  }

                //var test = outputData.ToArray();


                //var test = new GenerateReceipt.Print(printQueue.PrinterMac, printQueue.OrderName, printQueue.OrderDate);
                
                //await Publish_Application_Message(printerMacLower, "print-job", test, customerName); //MQTT Request POST 
                await Publish_Application_Message_URL(printerMacLower, "print-job", customerName); //MQTT Request POST
                _context.Add(printQueue);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(printQueue);
        }

        // POST: PrintQueue/Drawer
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Drawer([Bind("Id,PrinterMac,OrderName,OrderDate")] PrintQueue printQueue)
        {
            if (ModelState.IsValid)
            {
                var printerMacLower = printQueue.PrinterMac.ToLower();
                var customerName = printQueue.OrderName.ToString();
                StringBuilder OrderContent = new StringBuilder();
                OrderContent.Append("\\x07");
                printQueue.PrinterMac = printerMacLower;
                printQueue.OrderContent = OrderContent.ToString();
                printQueue.OpenDrawer = "yes";

                _context.Add(printQueue);
                await _context.SaveChangesAsync();
                //await Publish_Application_Message(printerMacLower, "request-post");
                return RedirectToAction(nameof(Index));
            }
            return View(printQueue);
        }

        // GET: PrintQueue/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PrintQueue == null)
            {
                return NotFound();
            }

            var printQueue = await _context.PrintQueue.FindAsync(id);
            if (printQueue == null)
            {
                return NotFound();
            }
            return View(printQueue);
        }

        // POST: PrintQueue/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PrinterMac,OrderName,OrderDate")] PrintQueue printQueue)
        {
            if (id != printQueue.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var printerMacLower = printQueue.PrinterMac.ToLower();
                    var customerName = printQueue.OrderName.ToString();
                    StringBuilder OrderContent = new StringBuilder();

                    OrderContent.Append("[align: center][font: a]\n");
                    OrderContent.Append("[image: url http://star-emea.com/wp-content/uploads/2015/01/logo.jpg; width 60%; min-width 48mm]\n");
                    OrderContent.Append("[magnify: width 2; height 2]\n");
                    OrderContent.Append("Customer Name: " + customerName + "\n");
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
                    printQueue.PrinterMac = printerMacLower;

                    _context.Update(printQueue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrintQueueExists(printQueue.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(printQueue);
        }

        // GET: PrintQueue/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PrintQueue == null)
            {
                return NotFound();
            }

            var printQueue = await _context.PrintQueue
                .FirstOrDefaultAsync(m => m.Id == id);
            if (printQueue == null)
            {
                return NotFound();
            }

            return View(printQueue);
        }

        // POST: PrintQueue/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PrintQueue == null)
            {
                return Problem("Entity set 'PrintQueueContext.PrintQueue'  is null.");
            }
            var printQueue = await _context.PrintQueue.FindAsync(id);
            if (printQueue != null)
            {
                _context.PrintQueue.Remove(printQueue);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PrintQueueExists(int id)
        {
          return _context.PrintQueue.Any(e => e.Id == id);
        }
    }
}
