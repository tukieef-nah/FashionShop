using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace FashionShop.Models
{
	public class AppUserModel : IdentityUser
	{
		public string NameAccount { get; set; }

		public string Address { get; set; }		
	}
}
