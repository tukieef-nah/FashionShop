using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace FashionShop.Models
{
    public class GioHangItemModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public string Image { get; set; }

        public decimal Total
        {
            get { return Quantity * Price; }

        }

        public GioHangItemModel()
        {

        }
        public GioHangItemModel(SanPhamModel sanPham)
        {
            Id = sanPham.Id;
            Name = sanPham.Name;
            Price = sanPham.Price;
            Quantity = 1;
            Image = sanPham.Image;
        }
    }
}
