using System;
/// <summary>
/// Auto-gen class from Visual Studio's Paste JSON as Class feature from the Kentico JSON
/// Just remember to rename RootObject to what you want, thanks @themarkschmidt!
/// </summary>

public class OrderItemsResponse
{
    public Ecommerce_Orderitems[] ecommerce_orderitems { get; set; }
}

public class Ecommerce_Orderitems
{
    public COM_Orderitem[] COM_OrderItem { get; set; }
    public Totalrecord[] TotalRecords { get; set; }
}

public class COM_Orderitem
{
    public decimal OrderItemUnitPrice { get; set; }
    public string OrderItemSKUName { get; set; }
    public int OrderItemUnitCount { get; set; }
    public int OrderItemID { get; set; }
}

public class Totalrecord
{
    public string TotalRecords { get; set; }
}
