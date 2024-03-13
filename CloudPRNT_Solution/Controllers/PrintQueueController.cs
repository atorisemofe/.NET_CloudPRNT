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

        // POST: PrintQueue/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PrinterMac,OrderName,OrderDate")] PrintQueue printQueue)
        {
            if (ModelState.IsValid)
            {
                var printerMacLower = printQueue.PrinterMac.ToLower();
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
                OrderContent.Append("[cut: feed; partial]");

                printQueue.PrinterMac = printerMacLower;
                printQueue.OrderContent = OrderContent.ToString();
                //var test = new GenerateReceipt.Print(printQueue.PrinterMac, printQueue.OrderName, printQueue.OrderDate);
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
                //OrderContent.Append("\x07");
                //OrderContent.Append("[align: center][font: a]\n");
                //OrderContent.Append("[image: url http://star-emea.com/wp-content/uploads/2015/01/logo.jpg; width 60%; min-width 48mm]\n");
                //OrderContent.Append("[magnify: width 2; height 2]\n");
                //OrderContent.Append("Customer Name: " + customerName + "\n");
                //OrderContent.Append("[magnify: width 3; height 2]Columns[magnify]\n");
                //OrderContent.Append("[align: left]\n");
                //OrderContent.Append("[column: left: Item 1;      right: $10.00]\n");
                //OrderContent.Append("[column: left: Item 2;      right: $9.95]\n");
                //OrderContent.Append("[column: left: Item 3;      right: $103.50]\n");
                //OrderContent.Append("[align: centre]\n");
                //OrderContent.Append("[barcode: type code39; data 123456789012; height 15mm; module 0; hri]\n");
                //OrderContent.Append("[align]\\");
                //OrderContent.Append("Thank you for trying the new Star Document Markup Language\\ we hope you will find it useful. Please let us know!");
                //OrderContent.Append("[cut: feed; partial]");

                printQueue.PrinterMac = printerMacLower;
                printQueue.OrderContent = OrderContent.ToString();
                //var test = new GenerateReceipt.Print(printQueue.PrinterMac, printQueue.OrderName, printQueue.OrderDate);
                _context.Add(printQueue);
                await _context.SaveChangesAsync();
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
