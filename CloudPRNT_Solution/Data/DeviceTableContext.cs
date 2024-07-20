using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CloudPRNT_Solution.Models;

namespace CloudPRNT_Solution.Data
{
    public class DeviceTableContext : DbContext
    {
        public DeviceTableContext (DbContextOptions<DeviceTableContext> options)
            : base(options)
        {
        }

        public DbSet<CloudPRNT_Solution.Models.DeviceTable> DeviceTable { get; set; } = default!;

    }
}
