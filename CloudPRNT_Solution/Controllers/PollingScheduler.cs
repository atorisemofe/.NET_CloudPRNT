using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CloudPRNT_Solution.Data;
using CloudPRNT_Solution.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

public class PollingScheduler : BackgroundService
{
    private readonly ILogger<PollingScheduler> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PollingScheduler(ILogger<PollingScheduler> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Polling Scheduler is running.");

        while (!stoppingToken.IsCancellationRequested)
        {
            DateTime currentTime = DateTime.Now;

            if (currentTime.Hour == 06 && currentTime.Minute == 47 && currentTime.Second == 0)
            {
                _logger.LogInformation("It's 11:59 PM. Triggering action to reduce polling.");
                await AddJobToQueueAsync("reduce polling", stoppingToken); //reduce
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            if (currentTime.Hour == 12 && currentTime.Minute == 12 && currentTime.Second == 0)
            {
                _logger.LogInformation("It's 5 AM. Triggering action to increase polling.");
                await AddJobToQueueAsync("increase polling", stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            // Wait for 1 second before checking again
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task AddJobToQueueAsync(string polling, CancellationToken stoppingToken)
    {
    try
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var _deviceContext = scope.ServiceProvider.GetRequiredService<DeviceTableContext>();
            var _context = scope.ServiceProvider.GetRequiredService<PrintQueueContext>();

            // Retrieve devices from the deviceTable where ClientType is "Star mC-Label3"
            var devices = _deviceContext.DeviceTable
                                        .Where(d => d.ClientType == "Star mC-Label3")
                                        .ToList();

            if (!devices.Any())
            {
                _logger.LogInformation($"No devices found with ClientType 'Star mC-Label3'.");
                return; // Exit the method if no devices are found
            }

            foreach (var device in devices)
            {
                // Get the PrinterMac value from each filtered device
                var printerMac = device.PrinterMac;

                // Add a new job to the PrintQueue table
                var newPrintQueueItem = new PrintQueue
                {
                    PrinterMac = printerMac, // Assuming PrinterMac is a column in the PrintQueue table
                    OrderName = polling,
                    OrderDate = DateTime.Now
                };

                _context.PrintQueue.Add(newPrintQueueItem);
                _logger.LogInformation($"Added new print job for device with PrinterMac: {printerMac}");
            }

            // Save changes to the database
            await _context.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("All jobs have been added to the print queue successfully.");
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "An error occurred while adding jobs to the print queue.");
    }
}

}
