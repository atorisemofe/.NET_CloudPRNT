using System;
namespace CloudPRNT_Solution.Models
{
    public class CloudPRNTPostHeaders
    {
        [Microsoft.AspNetCore.Mvc.FromHeader]
        public string? Authentication { get; set; }
    }
}

