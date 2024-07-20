using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CloudPRNT_Solution.Models;
using Microsoft.EntityFrameworkCore;
using CloudPRNT_Solution.Data;



namespace CloudPRNT_Solution.Controllers;

public class LocationTableController : Controller
{
    private readonly LocationTableContext _context;

    public LocationTableController(LocationTableContext context){
         _context = context;
    }
    public IActionResult Create(){

        return View();
    }
    

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddLocation([Bind("Id, LocationName")] LocationTable locationInfo ){

        if (ModelState.IsValid){
            
            locationInfo.LocationName = locationInfo.LocationName;
            _context.Add(locationInfo);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Management");

        }
        return View(locationInfo);
    }


    // GET: PrintQueue/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null || _context.LocationTable == null)
        {
            return NotFound();
        }

        var locationTable = await _context.LocationTable
            .FirstOrDefaultAsync(m => m.Id == id);
        if (locationTable == null)
        {
            return NotFound();
        }

        return View(locationTable);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLocation(int id)
    {
        if (_context.LocationTable == null)
        {
            return Problem("Entity set 'LocationTableContext.LocationTable'  is null.");
        }
        var locationTable = await _context.LocationTable.FindAsync(id);
        if (locationTable != null)
        {
            _context.LocationTable.Remove(locationTable);
        }
        
        await _context.SaveChangesAsync();
        return RedirectToAction("Index", "Management");
    }

    private bool LocationTableExists(int id)
    {
        return _context.LocationTable.Any(e => e.Id == id);
    }

}