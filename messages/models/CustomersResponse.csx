using System;

/// <summary>
/// Auto-gen class from Visual Studio's Paste JSON as Class feature from the Kentico JSON
/// Just remember to rename RootObject to what you want, thanks @themarkschmidt!
/// </summary>
public class CustomersResponse
{
    public Ecommerce_Customers[] ecommerce_customers { get; set; }
}

public class Ecommerce_Customers
{
    public COM_Customer[] COM_Customer { get; set; }
    public Totalrecord[] TotalRecords { get; set; }
}

public class COM_Customer
{
    public string CustomerLastName { get; set; }
    public string CustomerGUID { get; set; }
    public string CustomerFirstName { get; set; }
    public DateTime CustomerCreated { get; set; }
    public object CustomerCompany { get; set; }
    public DateTime CustomerLastModified { get; set; }
    public object CustomerUserID { get; set; }
    public object CustomerFax { get; set; }
    public object CustomerTaxRegistrationID { get; set; }
    public int CustomerID { get; set; }
    public object CustomerStateID { get; set; }
    public object CustomerOrganizationID { get; set; }
    public object CustomerPhone { get; set; }
    public object CustomerCountryID { get; set; }
    public int CustomerSiteID { get; set; }
    public string CustomerEmail { get; set; }
}

public class Totalrecord
{
    public string TotalRecords { get; set; }
}
