using Newtonsoft.Json;
using StarMicronics.CloudPrnt.CpMessage;
using System;
using System.Collections.Generic;
namespace CloudPRNT_Solution.Controllers
{
	public class CloudPRNTPostResponse
	{
		public bool jobReady { get; set; }
		public List<string> mediaTypes { get; set; }
		public string jobToken { get; set; }

	}

}

