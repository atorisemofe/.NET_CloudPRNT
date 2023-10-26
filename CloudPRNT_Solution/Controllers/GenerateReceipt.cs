using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using System.Text;
using CloudPRNT_Solution.Data;
using Microsoft.EntityFrameworkCore;
using CloudPRNT_Solution.Models;
using System.Linq;

namespace CloudPRNT_Solution.Controllers
{
    public class GenerateReceipt : Controller
    {

        private readonly PrintQueueContext _context;

        public GenerateReceipt(PrintQueueContext context)
        {
            _context = context;
        }

        [HttpPost]
        public string Print()
        {

            PrintQueue printQueue = new PrintQueue();
            DateTime OrderDate = DateTime.Now;
            Random objRandom = new Random();
            int intValue = objRandom.Next(10000, 99999);
            string OrderName = "markup" + intValue.ToString() + ".stm";

            StringBuilder OrderContent = new StringBuilder();
            OrderContent.Append("[align: center][font: a]\n");
            OrderContent.Append("[image: url http://star-emea.com/wp-content/uploads/2015/01/logo.jpg; width 60%; min-width 48mm]\n");
            OrderContent.Append("[magnify: width 2; height 1]\n");
            OrderContent.Append("This is a Star Markup Document!\n");
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
            //OrderContent.ToString();

            printQueue.OrderDate = OrderDate;
            printQueue.OrderName = OrderName;
            printQueue.PrinterMac = "00:11:62:1d:62:89";
            printQueue.OrderContent = OrderContent.ToString();

            _context.Add(printQueue);
            _context.SaveChanges();

            //return RedirectToAction(nameof(Index));
            return "Ticket Submitted";
        }


     

        //public static string filename = "";

        //public GenerateReceipt()
        //{
        //    Random objRandom = new Random();
        //    int intValue = objRandom.Next(10000, 99999);
        //    filename = "markup" + intValue.ToString() + ".stm";
        //    using (StreamWriter writer = new StreamWriter(filename))
        //    {
        //        writer.WriteLine("[align: center][font: a]\\");
        //        writer.WriteLine("[image: url https://star-emea.com/wp-content/uploads/2015/01/logo.jpg; width 60%; min-width 48mm]\\");
        //        writer.WriteLine("[magnify: width 2; height 1]");
        //        writer.WriteLine("This is a Star Markup Document!\n");
        //        writer.WriteLine("[magnify: width 3; height 2]Columns[magnify]");
        //        writer.WriteLine("[align: left]\\");
        //        writer.WriteLine("[column: left: Item 1;      right: $10.00]");
        //        writer.WriteLine("[column: left: Item 2;      right: $9.95]");
        //        writer.WriteLine("[column: left: Item 3;      right: $103.50]");
        //        writer.WriteLine("[align: centre]\\");
        //        writer.WriteLine("[barcode: type code39; data 123456789012; height 15mm; module 0; hri]");
        //        writer.WriteLine("[align]\\");
        //        writer.WriteLine("Thank you for trying the new Star Document Markup Language\\ we hope you will find it useful. Please let us know!");
        //        writer.WriteLine("[cut: feed; partial]");
        //        writer.Flush();
        //    }
        //}

    }
}

