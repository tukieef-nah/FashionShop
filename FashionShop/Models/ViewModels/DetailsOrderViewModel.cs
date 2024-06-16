namespace FashionShop.Models.ViewModels
{
    public class DetailsOrderViewModel
    {

        public string NameAccount { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public OrderModel Orders { get; set; }

        public List<OrderDetails> OrderDetails { get; set; }
    }
}
