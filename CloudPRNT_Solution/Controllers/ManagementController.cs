using Microsoft.AspNetCore.Mvc;
using CloudPRNT_Solution.Data;
using CloudPRNT_Solution.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace CloudPRNT_Solution.Controllers
{
    public class ManagementController : Controller
    {
        private readonly DeviceTableContext _deviceContext;
        private readonly LocationTableContext _locationContext;

        public ManagementController(DeviceTableContext deviceContext, LocationTableContext locationContext)
        {
            _deviceContext = deviceContext;
            _locationContext = locationContext;
        }

        // GET: Management
        public async Task<IActionResult> Index()
        {
            // Fetch all locations
            var locations = await _locationContext.LocationTable.ToListAsync();
            var locationIds = locations.Select(l => l.Id); // Adjust if your LocationTable has different ID property

            // Fetch devices where QueueID matches any LocationID
            var devices = await _deviceContext.DeviceTable
                .Where(d => locationIds.Contains(d.QueueID))
                .ToListAsync();
            var viewModel = new ComninedTableModel
            {
                Devices = devices,
                Locations = locations
            };

            return View(viewModel);
        }
    }
}
