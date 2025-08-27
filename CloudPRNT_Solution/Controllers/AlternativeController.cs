using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StarMicronics.CloudPrnt;
using StarMicronics.CloudPrnt.CpMessage;
using CloudPRNT_Solution.Data;
using CloudPRNT_Solution.Models;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using Newtonsoft.Json.Serialization;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
//This file handles CloudPRNT HTTP communication protocol

namespace CloudPRNT_Solution.Controllers
{

    [Route("/Alternative")]
    //[ApiController]
    public class AlternativeController : Controller
    {
        private readonly PrintQueueContext _context;
        private readonly DeviceTableContext _DBcontext;


        public AlternativeController(PrintQueueContext context, DeviceTableContext _DBContext)
        {
            _context = context;
            _DBcontext = _DBContext;
        }

         string updatePollingGetResponse(int pollingTime)
        {
            StarConfiguration pollResponse = new StarConfiguration(pollingTime);

            // Configure JSON serializer settings with SnakeCaseNamingStrategy
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                Formatting = Formatting.Indented // To maintain the indented formatting
            };

            // Serialize the response with the settings
            return JsonConvert.SerializeObject(pollResponse, settings);
        }

        string firmwareUpdateGetResponse()
        {
            FirmwareConfiguration pollResponse = new FirmwareConfiguration();
            
            // Configure JSON serializer settings with SnakeCaseNamingStrategy
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                Formatting = Formatting.Indented // To maintain the indented formatting
            };

            // Serialize the response with the settings
            return JsonConvert.SerializeObject(pollResponse, settings);
        }

        string GetPrintableArea()
        {
            Request.Headers.TryGetValue("X-Star-Print-Width", out var printableArea);
            return printableArea;
        }

        IActionResult CreatePollingResponse(string jsonResponse)
        {
            // Set custom headers
            HttpContext.Response.ContentType = "application/vnd.star.starconfiguration";
            HttpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(jsonResponse); // Set Content-Length header

            // Return the JSON response in the body
            return Content(jsonResponse);
        }


        // GET: /CloudPRNT
        //HTTP GET response
        [HttpGet("AlternativeGet")]
        public IActionResult GetCloudPRNT([FromQuery] CloudPRNTGETQuery request)
        {

            var outputData = new MemoryStream();
            var outputFormat = "application/vnd.star.starprnt";

            var printableArea = GetPrintableArea();
            var printableAreaDots = Int32.Parse(printableArea) * 8;
            Console.WriteLine("Printable Area Dots: " + printableAreaDots);
            PrintQueue printQueueItem;

            string filenames = request.Token;
            string macadd = request.Mac;

            if (filenames != null){
                printQueueItem = _context.PrintQueue.First(m => m.OrderName == filenames);
            }else{
                printQueueItem = _context.PrintQueue.First(m => m.PrinterMac == macadd);
            }
            
            
            Console.WriteLine("Alternative URL GET Filename: " + request.Token + "\nGET Request Time: " + DateTime.Now + "\n\n\n");

            if (printQueueItem != null)
            {
                Console.WriteLine("File Exists in GET");
                //byte[] OrderContent = Encoding.UTF8.GetBytes(printQueueItem.OrderContent.ToString());
                if (printQueueItem.OpenDrawer?.ToString() == "yes")
                {
                    byte[] OrderContent = Encoding.UTF8.GetBytes(printQueueItem.OrderContent.ToString());
                    ICpDocument markupDoc = Document.GetDocument(OrderContent, "text/vnd.star.markup");
                    markupDoc.JobConversionOptions.OpenCashDrawer = DrawerOpenTime.EndOfJob;
                    markupDoc.convertTo(outputFormat, outputData);
                    //return new FileContentResult(outputData.ToArray(), outputFormat);

                }else if (printQueueItem.OpenDrawer?.ToString() == "no"){
                    byte[] OrderContent = Encoding.UTF8.GetBytes(printQueueItem.OrderContent.ToString());
                    ICpDocument markupDoc = Document.GetDocument(OrderContent, "text/vnd.star.markup");
                    markupDoc.convertTo(outputFormat, outputData);
                    return new FileContentResult(outputData.ToArray(), outputFormat);
                    // StringBuilder data = new StringBuilder();
                    // data.Append("[image: url \"data:image/jpeg;base64,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\"]");
                    // byte[] jobData = Encoding.UTF8.GetBytes(data.ToString());
                    // ICpDocument markupDoc = Document.GetDocument(jobData, "text/vnd.star.markup");
                    // markupDoc.convertTo(outputFormat, outputData);
                    // return new FileContentResult(outputData.ToArray(), outputFormat);

                }

                // ICpDocument markupDoc = Document.GetDocumentFromFile(filenames, "text/vnd.star.markup");
                //ICpDocument markupDoc = Document.GetDocument(OrderContent, "text/vnd.star.markup");

                //markupDoc.JobConversionOptions.JobEndCutType = CutType.Partial;
                //markupDoc.JobConversionOptions.DeviceWidth = printableAreaDots;
                //markupDoc.JobConversionOptions.OpenCashDrawer = DrawerOpenTime.EndOfJob;

                //markupDoc.convertTo(outputFormat, outputData);

                //return new FileContentResult(outputData.ToArray(), outputFormat);
                //return new FileContentResult(outputData.ToArray(), outputFormat);
                else if (printQueueItem.OrderName == "reduce polling")
                {
                    var getResponse = updatePollingGetResponse(3600);
                    return CreatePollingResponse(getResponse);
                }
                else if (printQueueItem.OrderName == "increase polling")
                {
                    var getResponse = updatePollingGetResponse(5);
                    return CreatePollingResponse(getResponse);
                }
                else if (printQueueItem.OrderName == "firmware update")
                {
                    var getResponse = firmwareUpdateGetResponse();
                    return CreatePollingResponse(getResponse);
                }

            }
            
            else
            {
                Console.WriteLine("File Does not Exists in GET");
                return Ok();
            }
            return Ok();


            //MQTT URL Print
            

        }


        // DELETE: /Alternative/Delete
        //HTTP DELETE handler
        [HttpDelete("AlternativeDelete")]
        public IActionResult DeleteCloudPRNT([FromQuery] CloudPRNTDeleteQuery request)
        {

            Console.WriteLine("Alternative URL DELETE Request: " + request);
            var code = request.Code.Split(" ")[0];
            var description = request.Code.Split(" ")[1];
            string filenames = request.Token;
            string macadd = request.Mac;

            Console.WriteLine("DELETE Status: " + code + " " + description);

            var config = request.Config;
            Console.WriteLine("Configuration: " + config);

            var firmwareUpdateStatus = request.Firmware;
            Console.WriteLine("firmware Update Status: " + firmwareUpdateStatus);

            PrintQueue printQueueItem;
            
            // PrintQueue printQueueItem = _context.PrintQueue.First(m => m.OrderName == filenames);
            if (filenames != null){
                printQueueItem = _context.PrintQueue.First(m => m.OrderName == filenames);
            }else{
                printQueueItem = _context.PrintQueue.First(m => m.PrinterMac == macadd);
            }
            var printQueueItemID = printQueueItem.Id;

            var testdeleteitem = _context.PrintQueue.Find(printQueueItemID);

            Console.WriteLine("DELETE Filename: " + request.Token + "\nDELETE Request Time: " + DateTime.Now + "\n\n\n");
            switch (code)
            {
                case "200":
                    if (testdeleteitem != null)
                    {
                        _context.PrintQueue.Remove(testdeleteitem);
                        _context.SaveChanges(); 
                    }
                    break;
                case "211":
                    if (testdeleteitem != null)
                    {
                        _context.PrintQueue.Remove(testdeleteitem);
                        _context.SaveChanges(); 
                    }
                    break;

                case "221":
                    if (testdeleteitem != null)
                    {
                        _context.PrintQueue.Remove(testdeleteitem);
                        _context.SaveChanges(); 
                    }
                    break;

                case "410":
                    Console.WriteLine("Error: " + description.ToUpper());
                    break;

                case "420":
                    Console.WriteLine("Error: " + description.ToUpper());
                    break;

                case "510":
                    Console.WriteLine("Error: " + description.ToUpper());
                    break;

                case "511":
                    Console.WriteLine("Error: " + description.ToUpper());
                    break;

                case "512":
                    Console.WriteLine("Error: " + description.ToUpper());
                    break;

                case "520":
                    Console.WriteLine("Error: " + description.ToUpper());
                    break;

                case "521":
                    Console.WriteLine("Error: " + description.ToUpper());
                    break;

            }

            return Ok();

        }

    }
}
