using System;
/// <summary>
/// Auto-gen class from Visual Studio's Paste JSON as Class feature from the Kentico JSON
/// Just remember to rename RootObject to what you want, thanks @themarkschmidt!
/// </summary>
public class OrdersResponse
{
    public Ecommerce_Orders[] ecommerce_orders { get; set; }
}

public class Ecommerce_Orders
{
    public COM_Order[] COM_Order { get; set; }
    public Totalrecord[] TotalRecords { get; set; }
}

public class COM_Order
{
    public decimal OrderTotalPriceInMainCurrency { get; set; }
    public int OrderID { get; set; }
    public int OrderStatusID { get; set; }
    public DateTime OrderDate { get; set; }
    public string OrderInvoiceNumber { get; set; }
    public int OrderBillingAddressID { get; set; }
    public string OrderTrackingNumber { get; set; }
}

public class Totalrecord
{
    public string TotalRecords { get; set; }
}
