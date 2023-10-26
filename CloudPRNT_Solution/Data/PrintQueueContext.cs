using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CloudPRNT_Solution.Models;

namespace CloudPRNT_Solution.Data
{
    public class PrintQueueContext : DbContext
    {
        public PrintQueueContext (DbContextOptions<PrintQueueContext> options)
            : base(options)
        {
        }

        public DbSet<CloudPRNT_Solution.Models.PrintQueue> PrintQueue { get; set; } = default!;
    }
}
