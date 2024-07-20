using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CloudPRNT_Solution.Models;

namespace CloudPRNT_Solution.Data
{
    public class LocationTableContext : DbContext
    {
        public LocationTableContext (DbContextOptions<LocationTableContext> options)
            : base(options)
        {
        }

        public DbSet<CloudPRNT_Solution.Models.LocationTable> LocationTable { get; set; } = default!;

    }
}
