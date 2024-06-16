using FashionShop.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.Controllers
{
	public class ContactController :  Controller
	{
        [Route("Contact")]
        public IActionResult Index()
		{
			return View();
		}
	}
}
