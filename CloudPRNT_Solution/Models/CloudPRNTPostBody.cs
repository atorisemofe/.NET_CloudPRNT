using System;
using StarMicronics.CloudPrnt.CpMessage;

namespace CloudPRNT_Solution.Models
{
	public class CloudPRNTPostBody
	{
		public string? Status { get; set; }
		public string? PrinterMAC { get; set; }
		public string? StatusCode { get; set; }
		public bool PrintingInProgress { get; set; }
		public string? User { get; set; }
		public string? Password { get; set; }
		public List<Object>? clientAction { get; set; }

	}
}

