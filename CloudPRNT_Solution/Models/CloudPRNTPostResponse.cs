using Newtonsoft.Json;
using System;
using System.Collections.Generic;
namespace CloudPRNT_Solution.Models
{
	public class CloudPRNTPostResponse
    {
        public bool jobReady { get; set; }
        public List<string>? mediaTypes { get; set; }
        public string? jobToken { get; set; }
        public List<Object>? clientAction { get; set; }

    }

  

}

