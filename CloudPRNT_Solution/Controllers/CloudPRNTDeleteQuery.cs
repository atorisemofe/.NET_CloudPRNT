using System;
namespace CloudPRNT_Solution.Controllers
{
	public class CloudPRNTDeleteQuery
	{
        public string Uid { get; set; }
        public string Code { get; set; }
        public string Mac { get; set; }
        public string Token { get; set; }
        public string Firmware { get; set; }
        public string Config { get; set; }
        public string Skip { get; set; }
        public string Error { get; set; }
        public string Retry { get; set; }
    }
}

