using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CloudPRNT_Solution.Data;

namespace CloudPRNT_Solution.Models
{
	public static class SeedData
	{
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new PrintQueueContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<PrintQueueContext>>()))
            {
                // Look for any movies.
            if (context.PrintQueue.Any())
                    {
                        return;   // DB has been seeded
                    }
                context.PrintQueue.AddRange(
                    new PrintQueue
                    {
                        PrinterMac = "When Harry Met Sally",
                        OrderName = "",
                        OrderDate = DateTime.Parse("1989-2-12 7:00:00 AM"),
                        OrderContent = "Romantic Comedy",
                    },
                    new PrintQueue
                    {
                        PrinterMac = "Ghost Busters",
                        OrderName = "",
                        OrderDate = DateTime.Parse("1989-2-12 8:00:00 AM"),
                        OrderContent = "Comedy",
                    },
                    new PrintQueue
                    {
                        PrinterMac = "Ghost Busters 2",
                        OrderName = "",
                        OrderDate = DateTime.Parse("1989-2-12 9:00:00 AM"),
                        OrderContent = "Comedy",
                    },
                    new PrintQueue
                    {
                        PrinterMac = "Rio Bravo",
                        OrderName = "",
                        OrderDate = DateTime.Parse("1989-2-12 10:00:00 AM"),
                        OrderContent = "Western",
                    }
                );
                context.SaveChanges();
            }
        }
    }
}

