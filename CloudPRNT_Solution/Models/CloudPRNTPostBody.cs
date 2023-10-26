using System;
namespace CloudPRNT_Solution.Models
{
	public class CloudPRNTPostBody
	{
		public string? Status { get; set; }
		public string? PrinterMAC { get; set; }
		public string? StatusCode { get; set; }
		public bool PrintingInProgress { get; set; }
		
	}
}

