using System;
/// <summary>
/// Auto-gen class from Visual Studio's Paste JSON as Class feature from the Kentico JSON
/// Just remember to rename RootObject to what you want, thanks @themarkschmidt!
/// </summary>
public class OrderAddressesResponse
{
    public Ecommerce_Orderaddresses[] ecommerce_orderaddresses { get; set; }
}

public class Ecommerce_Orderaddresses
{
    public COM_Orderaddress[] COM_OrderAddress { get; set; }
    public Totalrecord[] TotalRecords { get; set; }
}

public class COM_Orderaddress
{
    public object AddressPhone { get; set; }
    public object AddressLine2 { get; set; }
    public int AddressID { get; set; }
    public object AddressStateID { get; set; }
    public DateTime AddressLastModified { get; set; }
    public int AddressCountryID { get; set; }
    public string AddressPersonalName { get; set; }
    public string AddressGUID { get; set; }
    public string AddressCity { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressZip { get; set; }
}

public class Totalrecord
{
    public string TotalRecords { get; set; }
}
