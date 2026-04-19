using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DojoAppMaui.Models
{
	public class LoginResponse
	   {
		   public string Token { get; set; }
		   public int UserId { get; set; }
		   public string Message { get; set; }
		   public string Role { get; set; }
	   }
}
