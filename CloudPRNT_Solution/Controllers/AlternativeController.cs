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
                if (printQueueItem.OpenDrawer?.ToString() == "yes")
                {
                    byte[] OrderContent = Encoding.UTF8.GetBytes(printQueueItem.OrderContent.ToString());
                    ICpDocument markupDoc = Document.GetDocument(OrderContent, "text/vnd.star.markup");
                    markupDoc.JobConversionOptions.OpenCashDrawer = DrawerOpenTime.EndOfJob;
                    markupDoc.convertTo(outputFormat, outputData);
                    return new FileContentResult(outputData.ToArray(), outputFormat);

                }
                else if (printQueueItem.OpenDrawer?.ToString() == "no")
                {
                    byte[] OrderContent = Encoding.UTF8.GetBytes(printQueueItem.OrderContent.ToString());
                    ICpDocument markupDoc = Document.GetDocument(OrderContent, "text/vnd.star.markup");
                    markupDoc.convertTo(outputFormat, outputData);
                    return new FileContentResult(outputData.ToArray(), outputFormat);

                    //DUTCHIE TESTING
                    // Hardcoded Base64 string from your logs
                    // string base64Content = "G0AbHmEAGx5GABsgMBtzMDAbejESGy0wG0YbNRtpAAAbbAAbUTAbbAAbUTAbHWEAGx1BAAAbHVKQABstMBtGGzUbaQAAG2kBAVRlc3QgUmVjZWlwdAobLTAbRhs1G2kAABtsABtRMBtsABtRMBsdYQAbHUEAABstMBtGGzUbaQAAIAobLTAbRhs1G2kAABtsABtRMBtsABtRMBsdYQAbHUEAABsdUgAAGy0wG0YbNRtpAABMZWZ0Gx1BIAEbHVLkABstMBtGGzUbaQAAUmlnaHQKGy0wG0YbNRtpAAAbbAAbUTAbbAAbUTAbHWEAGx1BAAAbLTAbRhs1G2kAACAKGy0wG0YbNRtpAAAbbAAbUTAbbAAbUTAbHWEAGx1BAAAbHVIAABstMBtGGzUbaQAAUHJvZHVjdCBBGx1BIAEbHVLYABstMBtGGzUbaQAAJDEwLjAwChstMBtGGzUbaQAAG2wAG1EwG2wAG1EwGx1hARtiNjExSDEyMzQ1Njc4OR4bLTAbRhs1G2kAABtsABtRMBtsABtRMBsdYQAbHUEAABstMBtGGzUbaQAAIAobLTAbRhs1G2kAABtsABtRMBtsABtRMBsdYQEbHVMBDQBkAAD////w8A/w8AD////w////8PAP8PAA////8P////DwD/DwAP////D////w8A/w8AD////w8AAA8P/w//Dw8AAA8PAAAPD/8P/w8PAAAPDwAADw//D/8PDwAADw8AAA8P/w//Dw8AAA8PD/8PAA//8A8PD/8PDw//DwAP//APDw//Dw8P/w8AD//wDw8P/w8PD/8PAA//8A8PD/8PDw//DwAPAP8ADw//Dw8P/w8ADwD/AA8P/w8PD/8PAA8A/wAPD/8PDw//DwAPAP8ADw//Dw8P/w8P//APAA8P/w8PD/8PD//wDwAPD/8PDw//Dw//8A8ADw//Dw8P/w8P//APAA8P/w8PAAAPDwDwDwAPAAAPDwAADw8A8A8ADwAADw8AAA8PAPAPAA8AAA8PAAAPDwDwDwAPAAAPD////w8PDw8PD////w////8PDw8PDw////8P////Dw8PDw8P////D////w8PDw8PD////wAAAAAPD/8P/wAAAAAAAAAADw//D/8AAAAAAAAAAA8P/w//AAAAAAAAAAAPD/8P/wAAAAAP/wD/D/8A/w///wD/D/8A/w//AP8P//8A/w//AP8P/wD/D///AP8P/wD/D/8A/w///wD/DwD/AAD/APDw//Dw/w8A/wAA/wDw8P/w8P8PAP8AAP8A8PD/8PD/DwD/AAD/APDw//Dw/wDw/w//APAA8A///w8A8P8P/wDwAPAP//8PAPD/D/8A8ADwD///DwDw/w//APAA8A///w8A/wDw8PAA8A/w8PAAAP8A8PDwAPAP8PDwAAD/APDw8ADwD/Dw8AAA/wDw8PAA8A/w8PAADwAP//Dw//D/D/AADw8AD//w8P/w/w/wAA8PAA//8PD/8P8P8AAPDwAP//Dw//D/D/AADwAP8ADwAA8A///wAP8AD/AA8AAPAP//8AD/AA/wAPAADwD///AA/wAP8ADwAA8A///wAP8P/w8PD/AP//APAP8PD/8PDw/wD//wDwD/Dw//Dw8P8A//8A8A/w8P/w8PD/AP//APAP8PAA//8AAP/w8AAP/wAAAP//AAD/8PAAD/8AAAD//wAA//DwAA//AAAA//8AAP/w8AAP/wAA//AP/wDwD/D///APAP/wD/8A8A/w///wDwD/8A//APAP8P//8A8A//AP/wDwD/D///APAAAAAADw8A/w8ADwAPAAAAAA8PAP8PAA8ADwAAAAAPDwD/DwAPAA8AAAAADw8A/w8ADwAPD////wD/8AAPDw8ADw////8A//AADw8PAA8P////AP/wAA8PDwAPD////wD/8AAPDw8ADw8AAA8P/wDw/wAPAP8PAAAPD/8A8P8ADwD/DwAADw//APD/AA8A/w8AAA8P/wDw/wAPAP8PD/8PAPD/8P///wD/Dw//DwDw//D///8A/w8P/w8A8P/w////AP8PD/8PAPD/8P///wD/Dw//DwD/DwAA8A8P8A8P/w8A/w8AAPAPD/APD/8PAP8PAADwDw/wDw//DwD/DwAA8A8P8A8P/w8P8A//8PD/8P8PD/8PD/AP//Dw//D/Dw//Dw/wD//w8P/w/w8P/w8P8A//8PD/8P8PAAAPD///Dw8A/wAADwAADw///w8PAP8AAA8AAA8P//8PDwD/AAAPAAAPD///Dw8A/wAAD////w8PAP8PAADwDw////8PDwD/DwAA8A8P////Dw8A/w8AAPAPD////w8PAP8PAADwDwGy0wG0YbNRtpAAAbbAAbUTAbbAAbUTAbHWEAGx1BAAAbLTAbRhs1G2kAACAKG2QzGx0DAQAA";

                    // // Convert Base64 to raw binary buffer
                    // byte[] buffer = Convert.FromBase64String(base64Content);

                    // // Return the binary array directly
                    // return new FileContentResult(buffer, "application/vnd.star.starprnt");
                    
                    //DUTCHIE TESTING END

                }
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
