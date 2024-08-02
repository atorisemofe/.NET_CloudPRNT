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


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CloudPRNT_Solution.Controllers
{

    [Route("/CloudPRNT")]
    //[ApiController]
    public class CloudPRNTController : Controller
    {


        

        string BeginTruePostResponse(bool jobReady, string token)
        {
            //PollResponse pollResponses = new PollResponse();
            CloudPRNTPostResponse pollResponse = new CloudPRNTPostResponse
            {
                jobReady = jobReady,
                jobToken = token,
                mediaTypes = new List<string>()
            };
            // pollResponse.mediaTypes = new List<string>();
            pollResponse.mediaTypes.AddRange(Document.GetOutputTypesFromType("text/vnd.star.markup"));
            return JsonConvert.SerializeObject(pollResponse, Formatting.Indented);
        }



        // string BeginFalsePostResponse(bool jobReady)
        // {
        //     //PollResponse pollResponses = new PollResponse();
        //     CloudPRNTPostResponse pollResponse = new CloudPRNTPostResponse
        //     {
        //         jobReady = jobReady,
        //         //mediaTypes = new List<string>(),
        //         clientAction = new List<Models.ClientActionRequest>
        //         {
        //             new Models.ClientActionRequest("GetPollInterval", ""),
        //             new Models.ClientActionRequest("ClientType", ""),
        //             new Models.ClientActionRequest("ClientVersion", "")
        //         }
        //     };
        //     //pollResponse.mediaTypes.AddRange(Document.GetOutputTypesFromType("text/vnd.star.markup"));
        //     // pollResponse.clientAction.Add(new ClientActionRequest { Request = "ClientType", Options = "" });
        //     // pollResponse.clientAction.Add(new ClientActionRequest { Request = "GetPollInterval", Options = "" });
        //     // pollResponse.clientAction["Request"] = "Clienttype";


        //     return JsonConvert.SerializeObject(pollResponse, Formatting.Indented);
        // }

        // string BeginTruePostResponsePlusClientAction(bool jobReady, string token)
        // {
        //     PollResponse pollResponse = new PollResponse{
        //         jobReady = jobReady,
        //     };
        //     return JsonConvert.SerializeObject(pollResponse, Formatting.Indented);
        // }

        string BeginFalsePostResponse(bool jobReady)
        {
            PollResponse pollResponse = new PollResponse{
                jobReady = jobReady,
            };
            return JsonConvert.SerializeObject(pollResponse, Formatting.Indented);
        }

        string BeginFalsePostResponsePlusClientVersion(bool jobReady)
        {
            PollResponse pollResponse = new PollResponse{
                jobReady = jobReady,
                clientAction = new List<StarMicronics.CloudPrnt.CpMessage.ClientActionRequest>{
                    new StarMicronics.CloudPrnt.CpMessage.ClientActionRequest("ClientType",""),
                    new StarMicronics.CloudPrnt.CpMessage.ClientActionRequest("ClientVersion",""),
                    // new StarMicronics.CloudPrnt.CpMessage.ClientActionRequest("PageInfo","")
                }
            };
            return JsonConvert.SerializeObject(pollResponse, Formatting.Indented);
        }

        string GetPrintableArea()
        {
            Request.Headers.TryGetValue("X-Star-Print-Width", out var printableArea);
            return printableArea;
        }

        private readonly PrintQueueContext _context;
        private readonly DeviceTableContext _DBcontext;


        public CloudPRNTController(PrintQueueContext context, DeviceTableContext _DBContext)
        {
            _context = context;
            _DBcontext = _DBContext;
        }

        //POST: /CloudPRNT
        [HttpPost]
        public async Task<IActionResult> PostCloudPRNT([FromBody] CloudPRNTPostBody request, [FromHeader] CloudPRNTPostHeaders headers)
        {
            var deviceInfo = await _DBcontext.DeviceTable.FirstOrDefaultAsync(d => d.PrinterMac == request.PrinterMAC);
            if (deviceInfo !=null){

            
                string ClientTypes = "";
                string ClientVersions = "";
                // string PrintWidth = "";
                // List<Object> PageInfos = new List<object>();
                
                if (request == null)
                {
                    Console.WriteLine("Request body cannot be null");
                }
                
                Console.WriteLine("PrinterMAC: " + request.PrinterMAC + "\nStatusCode: " + request.StatusCode + "\nPrinting In Progress: " + request.PrintingInProgress + "\nStatus: " + request.Status  + "\n\n\n");
                if (request.clientAction != null){
                    foreach (var action in request.clientAction)
                    {
                        if (action != null)
                        {
                            string clientActionString = action.ToString();
                            try
                            {
                                var jsonDoc = JsonDocument.Parse(clientActionString);
                                var root = jsonDoc.RootElement;

                                if (root.TryGetProperty("request", out JsonElement requestElement))
                                {
                                    string requestType = requestElement.GetString();
                                    switch (requestType)
                                    {
                                        case "ClientType":
                                            if (root.TryGetProperty("result", out JsonElement clientTypeResult))
                                            {
                                                var clientTypeValue = clientTypeResult.GetString();
                                                // Handle ClientType request
                                                Console.WriteLine($"ClientType Result: {clientTypeValue}");
                                                ClientTypes = clientTypeValue;
                                            }
                                            else
                                            {
                                                Console.WriteLine("ClientType result property not found.");
                                            }
                                            break;

                                        case "ClientVersion":
                                            if (root.TryGetProperty("result", out JsonElement clientVersionResult))
                                            {
                                                var clientVersionValue = clientVersionResult.GetString();
                                                // Handle ClientVersion request
                                                Console.WriteLine($"ClientVersion Result: {clientVersionValue}");
                                                ClientVersions = clientVersionValue;
                                            }
                                            else
                                            {
                                                Console.WriteLine("ClientVersion result property not found.");
                                            }
                                            break;

                                        // case "PageInfo":
                                        //     if (root.TryGetProperty("result", out JsonElement pageInfoResult))
                                        //     {
                                        //         var pageInfoValue = pageInfoResult.GetString();
                                        //         // Handle PageInfo request
                                        //         Console.WriteLine($"PageInfo Result: {pageInfoValue}");
                                        //     }
                                        //     else
                                        //     {
                                        //         Console.WriteLine("PageInfo result property not found.");
                                        //     }
                                        //     break;

                                        default:
                                            Console.WriteLine("Request is not ClientType, ClientVersion, or PageInfo.");
                                            break;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Request property not found.");
                                }
                            }
                            catch (System.Text.Json.JsonException ex)
                            {
                                Console.WriteLine($"Failed to parse action JSON: {ex.Message}");
                            }
                            
                        }
                        else
                        {
                            Console.WriteLine("ClientAction is null.");
                        }
                    }
                    // Update the values
                    deviceInfo.ClientType = ClientTypes;
                    deviceInfo.ClientVersion = ClientVersions;
                    deviceInfo.Status = request.StatusCode?.ToString().Replace("%20"," ");

                    // Save the changes to the database
                    _DBcontext.Update(deviceInfo);
                    await _DBcontext.SaveChangesAsync();
                    Console.WriteLine($"device found with PrinterMac: {request.PrinterMAC}" + "Database updated");
                    
                }
                else{
                    Console.WriteLine("No client actions received.");
                    deviceInfo.Status = request.StatusCode?.ToString().Replace("%20"," ");

                    // Save the changes to the database
                    _DBcontext.Update(deviceInfo);
                    await _context.SaveChangesAsync();
                    
                }           

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

                            if (deviceInfo.ClientType != null && deviceInfo.ClientVersion != null)
                            {
                                if (_context.PrintQueue.Any(m => m.PrinterMac == printerMac))
                                    {
                                        Console.WriteLine("Job available and printer Ready to Print " + description);
                                        PrintQueue printQueueTopItem = _context.PrintQueue.First(m => m.PrinterMac == printerMac);
                                        var filenames = printQueueTopItem.OrderName;
                                        Console.WriteLine("POST Filename: " + filenames);

                                        bool jobReady = true;
                                        var postresponse = BeginTruePostResponse(jobReady, filenames);
                                        return Ok(postresponse);
                                        
                                        
                                    }
                                else
                                {
                                    Console.WriteLine("NO Job available but printer Ready to Print " + description);
                                    bool jobReady = false;
                                    var postresponse = BeginFalsePostResponse(jobReady);
                                    return Ok(postresponse);
                                }
                            }
                            else{
                                bool jobReady = false;
                                var postresponse = BeginFalsePostResponsePlusClientVersion(jobReady);
                                return Ok(postresponse);
                            }

                        case "211":

                            if (deviceInfo.ClientType != null && deviceInfo.ClientVersion != null)
                            {
                                if (_context.PrintQueue.Any())
                                {
                                    Console.WriteLine("Warning: " + description + "\n");
                                    Console.WriteLine("Warning: Job available and printer paper low ");

                                    //string printerMac = request.PrinterMAC;
                                    var printQueueTopItem = _context.PrintQueue.First(m => m.PrinterMac == printerMac);
                                    var filenames = printQueueTopItem.OrderName;

                                    bool jobReady = true;
                                    var postresponse = BeginTruePostResponse(jobReady, filenames);
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
                            }
                            else{
                                bool jobReady = false;
                                var postresponse = BeginFalsePostResponsePlusClientVersion(jobReady);
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
                    
                }
            }
            else {
                Console.WriteLine("Printer Does not Exist in Database\n\n");
            }
            return Ok();
        }

        //GET: /CloudPRNT/PassURL
        [HttpGet("PassURL")]
        public IActionResult GetCloudPRNTPassURL()
        {
            string printDataText = "StarMicoronics.\n\nCloudPRNT Version MQTT\n\nPrint by Pass URL.";

            return Ok(printDataText);
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
            
            Console.WriteLine("GET Filename: " + request.Token + "\nGET Request Time: " + DateTime.Now + "\n\n\n");

            if (printQueueItem != null)
            {
                Console.WriteLine("File Exists in GET");
                byte[] OrderContent = Encoding.UTF8.GetBytes(printQueueItem.OrderContent.ToString());
                if (printQueueItem.OpenDrawer.ToString() == "yes")
                {
                    ICpDocument markupDoc = Document.GetDocument(OrderContent, "text/vnd.star.markup");
                    markupDoc.JobConversionOptions.OpenCashDrawer = DrawerOpenTime.EndOfJob;
                    markupDoc.convertTo(outputFormat, outputData);
                    //return new FileContentResult(outputData.ToArray(), outputFormat);

                }else if ((printQueueItem.OpenDrawer.ToString() == "no")){
                    
                    ICpDocument markupDoc = Document.GetDocument(OrderContent, "text/vnd.star.markup");
                    markupDoc.convertTo(outputFormat, outputData);
                    //return new FileContentResult(outputData.ToArray(), outputFormat);

                }

                // ICpDocument markupDoc = Document.GetDocumentFromFile(filenames, "text/vnd.star.markup");
                //ICpDocument markupDoc = Document.GetDocument(OrderContent, "text/vnd.star.markup");

                //markupDoc.JobConversionOptions.JobEndCutType = CutType.Partial;
                //markupDoc.JobConversionOptions.DeviceWidth = printableAreaDots;
                //markupDoc.JobConversionOptions.OpenCashDrawer = DrawerOpenTime.EndOfJob;

                //markupDoc.convertTo(outputFormat, outputData);

                //return new FileContentResult(outputData.ToArray(), outputFormat);
                return new FileContentResult(outputData.ToArray(), outputFormat);

            }
            else
            {
                Console.WriteLine("File Does not Exists in GET");
                return Ok();
            }
            return Ok();


            //MQTT URL Print
            

        }

        // DELETE: /CloudPRNT
        [HttpDelete]
        public IActionResult DeleteCloudPRNT([FromQuery] CloudPRNTDeleteQuery request)
        {

            
            var code = request.Code.Split(" ")[0];
            var description = request.Code.Split(" ")[1];
            string filenames = request.Token;
            
            PrintQueue printQueueItem = _context.PrintQueue.First(m => m.OrderName == filenames);
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

