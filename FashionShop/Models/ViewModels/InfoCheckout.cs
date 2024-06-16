namespace FashionShop.Models.ViewModels
{
    public class InfoCheckout
    {
        public string NameAccount { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public List<GioHangItemModel> GioHangItems { get; set; }

        //Tổng tiền
        public decimal GrandTotal { get; set; }

		public int Paymenttype { get; set; }

        public int PaymentStatus { get; set; }
	}
}
