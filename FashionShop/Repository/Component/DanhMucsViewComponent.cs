using FashionShop.Respository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FashionShop.Repository.Components
{
	public class DanhMucsViewComponent : ViewComponent
	{
		private readonly DataContext _dataContext;
		public DanhMucsViewComponent(DataContext context)
		{
			_dataContext = context;
		}
		public async Task<IViewComponentResult> InvokeAsync() => View(await _dataContext.DanhMucs.ToListAsync());
	}
}