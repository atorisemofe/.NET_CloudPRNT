using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;

namespace CloudPRNT_Solution.Models
{
	public class DeviceTable
	{
        
        public int Id { get; set; }
        public string? PrinterMac { get; set; }
        public int QueueID { get; set; }
        public int? DotWidth { get; set; }
        public string? Status { get; set; }
        public string? ClientType { get; set; }
        public string? ClientVersion { get; set; }
        public string? LastPoll { get; set; }

    }
}

