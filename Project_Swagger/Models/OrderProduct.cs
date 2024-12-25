namespace Project_Swagger.Models
{
    public class OrderProduct
    {
        public Order OrderId { get; set; }     // order_id trong bảng (FK)
        public Product ProductId { get; set; }   // product_id trong bảng (FK)
        public int Quantity { get; set; }    // quantity trong bảng
    }
}
