using System;

public class Order
{
    public int OrderID { get; set; }
    public string OrderNumber { get; set; }
    public string OrderTrackingNumber { get; set; }
    public decimal OrderTotalPrice { get; set; }
    public DateTime OrderDate { get; set; }
    public int OrderStatusID { get; set; }
    public string OrderStatusName { get; set; }
    public int OrderBillingAddressID { get; set; }
    public List<OrderItem> OrderItems { get; set; }
}

public class OrderItem
{
    public int OrderItemID { get; set; }
    public int OrderID { get; set; }
    public string SkuNumber { get; set; }
    public int Quantity { get; set; }
    public decimal ItemPrice { get; set; }
    public string SkuName { get; set; }
}