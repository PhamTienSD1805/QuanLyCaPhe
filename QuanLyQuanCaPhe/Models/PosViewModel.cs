namespace QuanLyQuanCaPhe.Models
{
    public class PosViewModel
    {
        public List<Drink>             Drinks     { get; set; } = [];
        public List<Category>          Categories { get; set; } = [];
        public List<CartItemViewModel> Cart       { get; set; } = [];
        public string?                 SearchName { get; set; }
        public int?                    CategoryId { get; set; }

        public decimal CartTotal => Cart.Sum(i => i.LineTotal);
    }
}
