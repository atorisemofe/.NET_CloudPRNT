using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;

namespace CloudPRNT_Solution.Models
{
	public class LocationTable
	{
        
        public int Id { get; set; }
        public string? LocationName { get; set; }

    }
}