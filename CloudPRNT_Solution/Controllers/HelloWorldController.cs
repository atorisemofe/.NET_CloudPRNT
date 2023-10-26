using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;

namespace CloudPRNT_Solution.Controllers;


public class HelloWorldController : Controller
{
    
    
    public IActionResult Index()
    {
        return View();
    }
    // 
    // GET: /HelloWorld/Welcome/ 
    public string Welcome()
    {
        return "This is the Welcome action method...";
    }
}