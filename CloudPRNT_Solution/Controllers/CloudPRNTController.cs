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


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CloudPRNT_Solution.Controllers
{

    [Route("/CloudPRNT")]
    //[ApiController]
    public class CloudPRNTController : Controller
    {

        string BeginPostResponse(bool jobReady, string token)
        {
            //PollResponse pollResponses = new PollResponse();
            CloudPRNTPostResponse pollResponse = new CloudPRNTPostResponse();
            pollResponse.jobReady = jobReady;
            pollResponse.jobToken = token;
            pollResponse.mediaTypes = new List<string>();
            pollResponse.mediaTypes.AddRange(Document.GetOutputTypesFromType("text/vnd.star.markup"));
            return JsonConvert.SerializeObject(pollResponse);
        }

        string BeginFalsePostResponse(bool jobReady)
        {
            //PollResponse pollResponses = new PollResponse();
            CloudPRNTPostResponse pollResponse = new CloudPRNTPostResponse();
            pollResponse.jobReady = jobReady;
            pollResponse.mediaTypes = new List<string>();
            pollResponse.mediaTypes.AddRange(Document.GetOutputTypesFromType("text/vnd.star.markup"));

            return JsonConvert.SerializeObject(pollResponse);
        }

        string GetPrintableArea()
        {
            Request.Headers.TryGetValue("X-Star-Print-Width", out var printableArea);
            return printableArea;
        }

        private readonly PrintQueueContext _context;

        public CloudPRNTController(PrintQueueContext context)
        {
            _context = context;
        }

        //POST: /CloudPRNT
        [HttpPost]
        public IActionResult PostCloudPRNT([FromBody] CloudPRNTPostBody request, [FromHeader] CloudPRNTPostHeaders headers)
        {

            Console.WriteLine("PrinterMAC: " + request.PrinterMAC + "StatusCode: " + request.StatusCode + "Printing In Progress: " + request.PrintingInProgress + "Status: " + request.Status);

            //Console.WriteLine("Full Header: " + headers.ToString());
            //Console.WriteLine("Authentication Headers: " + headers.Authentication);

            if (request.PrintingInProgress)
            {
                Console.WriteLine("Printing In Progress");
                return Ok();
            }
            //if (headers.Authentication == null)
            //{
            //    Console.WriteLine("Got here");
            //    //var responsed = Unauthorized();
            //    HttpContext.Response.Headers.Add("WWW-Authenticate","Basic realm=\"Authentication Required\"");
            //    return Unauthorized();
            //}
            else
            {


                //var filena = printQueue.OrderName;
                //Console.WriteLine("Printer mac: " + request.PrinterMAC + " DateTime: "+ DateTime.Now);
                Console.WriteLine("POST Request Time: " + DateTime.Now);
                var code = request.StatusCode.Split("%20")[0];
                var description = request.StatusCode.Replace("%20", " ");
                string printerMac = request.PrinterMAC;
                //var mediaTypes = new string[] { "application/vnd.star.starprnt" };

                switch (code)
                {
                    case "200":

                        if (_context.PrintQueue.Any(m => m.PrinterMac == printerMac))
                        {
                            //Console.WriteLine("Job available and printer Ready to Print " + description);
                            //Console.WriteLine("Case 200 printer mac: " + printerMac);
                            //var printerMac = request.PrinterMAC;
                            PrintQueue printQueueTopItem = _context.PrintQueue.First(m => m.PrinterMac == printerMac);
                            var filenames = printQueueTopItem.OrderName;
                            Console.WriteLine("POST Filename: " + filenames);

                            bool jobReady = true;
                            var postresponse = BeginPostResponse(jobReady, filenames);
                            return Ok(postresponse);
                        }
                        else
                        {
                            Console.WriteLine("NO Job available but printer Ready to Print " + description);
                            bool jobReady = false;
                            var postresponse = BeginFalsePostResponse(jobReady);
                            return Ok(postresponse);
                        }
                        

                    case "211":

                        if (_context.PrintQueue.Any())
                        {
                            Console.WriteLine("Warning: " + description + "\n");
                            Console.WriteLine("Warning: Job available and printer paper low ");

                            //string printerMac = request.PrinterMAC;
                            var printQueueTopItem = _context.PrintQueue.First(m => m.PrinterMac == printerMac);
                            var filenames = printQueueTopItem.OrderName;

                            bool jobReady = true;
                            var postresponse = BeginPostResponse(jobReady, filenames);
                            return Ok(postresponse);
                        }
                        else
                        {
                            Console.WriteLine("Warning: " + description + "\n");
                            Console.WriteLine("Warning: NO Job available and printer paper low " + description);
                            bool jobReady = false;
                            var postresponse = BeginFalsePostResponse(jobReady);
                            return Ok(postresponse);
                        }
                        

                    case "410":
                        if (_context.PrintQueue.Any())
                        {
                            Console.WriteLine("Error: " + description.ToUpper());
                            Console.WriteLine("Error: Job available and printer out of paper ");
                        }
                        else
                        {
                            Console.WriteLine("Error: " + description.ToUpper());
                            Console.WriteLine("Error: NO Job available and printer out of paper ");
                        }
                        break;

                    case "420":
                        if (_context.PrintQueue.Any())
                        {
                            Console.WriteLine("Error: " + description.ToUpper());
                            Console.WriteLine("Error: Job available and printer cover open ");
                        }
                        else
                        {
                            Console.WriteLine("Error: " + description.ToUpper());
                            Console.WriteLine("Error: NO Job available and printer cover open ");
                        }
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

        // GET: /CloudPRNT
        [HttpGet]
        public IActionResult GetCloudPRNT([FromQuery] CloudPRNTGETQuery request)
        {

            var outputData = new MemoryStream();
            var outputFormat = "application/vnd.star.starprnt";
            var printableArea = GetPrintableArea();
            var printableAreaDots = Int32.Parse(printableArea) * 8;
            Console.WriteLine("Printable Area Dots: " + printableAreaDots);
            string filenames = request.Token;
            PrintQueue printQueueItem = _context.PrintQueue.First(m => m.OrderName == filenames);
            //Console.WriteLine("GET Filename: " + filenames + " DateTime: " + DateTime.Now);
            Console.WriteLine("GET Filename: " + request.Token + " GET Request Time: " + DateTime.Now);
            if (printQueueItem != null)
            {
                Console.WriteLine("File Exists in GET");
                var testing = printQueueItem.OrderContent;
                //Console.WriteLine(testing);
                byte[] OrderContent = Encoding.UTF8.GetBytes(printQueueItem.OrderContent.ToString());
                // ICpDocument markupDoc = Document.GetDocumentFromFile(filenames, "text/vnd.star.markup");
                ICpDocument markupDoc = Document.GetDocument(OrderContent, "text/vnd.star.markup");
                //markupDoc.JobConversionOptions.JobEndCutType = CutType.Partial;
                //markupDoc.JobConversionOptions.DeviceWidth = printableAreaDots;
                markupDoc.JobConversionOptions.OpenCashDrawer = DrawerOpenTime.EndOfJob;
                markupDoc.convertTo(outputFormat, outputData);
                //_context.PrintQueue.Remove(printQueueItem);
                //_context.SaveChanges();
                //var testtt = "G0AbUgAbHkYAGyAAG2wAG3oBGwcUCgcbHSlVAgAwARsdKVUCAEAAGx1hARsdYQENChtpAQENChtFQ2FzaCBUaXAgVHJhbnNhY3Rpb24NChtpAAANChtGDQpUZXN0IFBPUw0KMzAxIENvbmdyZXNzLCBTdWl0ZSAzMDUsIEF1c3RpbiwgVFggNzg3NzcNCjUxMjk5OTg4NTUNCkRhdGU6IDAyLzA2LzIwMjQgMDg6NDdBTQ0KDQoNCj09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PQ0KGx1BAABTdGFmZhsdQe+/vQFBbW91bnQNCj09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PQ0KGx1BAABOYWRhdiBHaXZvbmkbHUHvv70BJDEyLjAwDQotLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0NChsdYQINClRvdGFsIMKgwqDCoMKgwqDCoMKgwqAkMTIuMDANCg0KDQotLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0NChsdYQANCkNhc2hpZXI6IE5hZGF2IEdpdm9uaQ0KDQoNChtkAw==";
                //byte[] testttt = System.Convert.FromBase64String(testtt);
                return new FileContentResult(outputData.ToArray(), outputFormat);
                //return Ok("\x07");

                //StarMicronics.CloudPrnt.Document.Convert(OrderContent, "text/vnd.star.markup", outputData, outputFormat, null);
                //var reader = new StreamReader(outputData);
                //outputData.Seek(0, SeekOrigin.Begin);
                //var response = reader.ReadToEnd();
                //return Ok(response);

            }
            else
            {
                Console.WriteLine("File Does not Exists in GET");
                return Ok();
            }
            //return Ok();

        }

        // DELETE: /CloudPRNT
        [HttpDelete]
        public IActionResult DeleteCloudPRNT([FromQuery] CloudPRNTDeleteQuery request)
        {

            //Console.WriteLine("Code: " + request.Code + " " + "Mac: " + request.Mac + " " + "Token: " + request.Token + " " + "Firmware: " + request.Firmware + " " + "Config: " + request.Config + " " + "SKip: " + request.Skip + " " + "Error: " + request.Error + " " + "Retry: " + request.Retry + "\n");
            //System.IO.File.Delete(@"markup.stm");
            var code = request.Code.Split(" ")[0];
            var description = request.Code.Split(" ")[1];
            string filenames = request.Token;
            PrintQueue printQueueItem = _context.PrintQueue.First(m => m.OrderName == filenames);
            var printQueueItemID = printQueueItem.Id;

            //Console.WriteLine("Item ID: " + printQueueItemID);

            var testdeleteitem = _context.PrintQueue.Find(printQueueItemID);

            //Console.WriteLine(description);
            //Console.WriteLine("DELETE Filename: " + filenames + " DateTime: " + DateTime.Now);
            Console.WriteLine("DELETE Filename: " + request.Token + " DELETE Request Time: " + DateTime.Now);
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
                    if (System.IO.File.Exists(filenames))
                    {
                        System.IO.File.Delete(filenames);
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

