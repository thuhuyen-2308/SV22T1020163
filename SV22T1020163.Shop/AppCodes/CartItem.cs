namespace SV22T1020163.Shop
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "";
        public string Photo { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => Price * Quantity;
    }
}
