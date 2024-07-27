using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CloudPRNT_Solution.Models;
using Microsoft.EntityFrameworkCore;
using CloudPRNT_Solution.Data;



namespace CloudPRNT_Solution.Controllers;

public class DeviceTableController : Controller
{
    private readonly DeviceTableContext _context;

    public DeviceTableController(DeviceTableContext context){
         _context = context;
    }
    public IActionResult Create(){

        return View();
    }
    

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPrinter([Bind("Id, PrinterMac, QueueID, PrintWidth, Status, ClientType, ClientVerion, LastPoll")] DeviceTable deviceInfo ){

        if (ModelState.IsValid){
            
            var printerMacLower = deviceInfo.PrinterMac.ToLower();
            deviceInfo.PrinterMac = printerMacLower;
            deviceInfo.QueueID = deviceInfo.QueueID;
            _context.Add(deviceInfo);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Management");

        }
        return View(deviceInfo);
    }

    // GET: PrintQueue/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null || _context.DeviceTable == null)
        {
            return NotFound();
        }

        var deviceTable = await _context.DeviceTable
            .FirstOrDefaultAsync(m => m.Id == id);
        if (deviceTable == null)
        {
            return NotFound();
        }

        return View(deviceTable);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDevice(int id)
    {
        if (_context.DeviceTable == null)
        {
            return Problem("Entity set 'DeviceTableContext.DeviceTable'  is null.");
        }
        var deviceTable = await _context.DeviceTable.FindAsync(id);
        if (deviceTable != null)
        {
            _context.DeviceTable.Remove(deviceTable);
        }
        
        await _context.SaveChangesAsync();
        return RedirectToAction("Index", "Management");
    }

    private bool DeviceTableExists(int id)
    {
        return _context.DeviceTable.Any(e => e.Id == id);
    }
}