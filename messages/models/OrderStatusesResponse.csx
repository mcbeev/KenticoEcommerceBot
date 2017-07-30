using System;
/// <summary>
/// Auto-gen class from Visual Studio's Paste JSON as Class feature from the Kentico JSON
/// Just remember to rename RootObject to what you want, thanks @themarkschmidt!
/// </summary>

public class OrderStatusesResponse
{
    public Ecommerce_Orderstatuses[] ecommerce_orderstatuses { get; set; }
}

public class Ecommerce_Orderstatuses
{
    public COM_Orderstatus[] COM_OrderStatus { get; set; }
    public Totalrecord[] TotalRecords { get; set; }
}

public class COM_Orderstatus
{
    public string StatusName { get; set; }
    public string StatusColor { get; set; }
    public bool StatusOrderIsPaid { get; set; }
    public int StatusID { get; set; }
    public bool StatusEnabled { get; set; }
    public int StatusSiteID { get; set; }
    public bool StatusSendNotification { get; set; }
    public string StatusGUID { get; set; }
    public int StatusOrder { get; set; }
    public string StatusDisplayName { get; set; }
    public DateTime StatusLastModified { get; set; }
}

public class Totalrecord
{
    public string TotalRecords { get; set; }
}
