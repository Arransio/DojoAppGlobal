namespace Api_Dojo_App.Models
{
	public class LoginResponse
	{
		public string Token { get; set; }
		public int UserId { get; set; }
		public string Role { get; set; }
	}
}
