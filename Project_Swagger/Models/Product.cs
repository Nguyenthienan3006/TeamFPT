namespace Project_Swagger.Models
{
    public class Product
    {
        public int ProductId { get; set; }      // product_id trong bảng
        public string Name { get; set; }        // name trong bảng
        public string Description { get; set; } // description trong bảng
        public decimal Price { get; set; }      // price trong bảng
        public DateTime CreatedAt { get; set; } // created_at trong bảng
    }
}
