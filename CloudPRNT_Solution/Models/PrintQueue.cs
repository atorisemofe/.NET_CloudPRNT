using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;

namespace CloudPRNT_Solution.Models
{
	public class PrintQueue
	{
        public int Id { get; set; }
        public string? PrinterMac { get; set; }
        public string? OrderName { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime OrderDate { get; set; }
        public string? OrderContent { get; set; }
        public string? OpenDrawer { get; set; }

    }
}

