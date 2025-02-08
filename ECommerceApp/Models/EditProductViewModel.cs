namespace ECommerceApp.Models
{
    public class EditProductViewModel
    {
        public Product Product { get; set; }
        public List<ProductImage> ProductImages { get; set; }
        public IFormFile Attachment { get; set; }
    }
}
