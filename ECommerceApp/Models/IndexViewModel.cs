namespace ECommerceApp.Models
{
    public class IndexViewModel
    {
        //public List<Product> Products { get; set; }
        //public Users CurrentUser { get; set; }

        public List<Product> Products { get; set; }
        public Users CurrentUser { get; set; }
        public Dictionary<int, List<ProductImage>> ProductImages { get; set; }
    }
}
