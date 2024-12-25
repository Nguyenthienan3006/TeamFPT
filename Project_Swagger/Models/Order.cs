namespace Project_Swagger.Models
{
    public class Order
    {
        public int OrderId { get; set; }         // order_id trong bảng
        public User UserId { get; set; }          // user_id trong bảng (FK)
        public DateTime OrderDate { get; set; }  // order_date trong bảng
        public decimal TotalAmount { get; set; } // total_amount trong bảng

    }
}
