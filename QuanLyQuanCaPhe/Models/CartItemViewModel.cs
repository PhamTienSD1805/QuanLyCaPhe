namespace QuanLyQuanCaPhe.Models
{
    public class CartItemViewModel
    {
        public int     DrinkId   { get; set; }
        public string  DrinkName { get; set; } = string.Empty;
        public int     Quantity  { get; set; }
        public decimal UnitPrice { get; set; }

        public decimal LineTotal => Quantity * UnitPrice;
    }
}
