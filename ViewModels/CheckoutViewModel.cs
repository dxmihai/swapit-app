namespace vinted2.ViewModels
{
    public class CheckoutViewModel
    {
        public int ProductId { get; set; }

        public string FullName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string DeliveryMethod { get; set; } = null!;
    }
}
