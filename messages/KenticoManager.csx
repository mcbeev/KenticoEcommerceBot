using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

public class KenticoManager
{
    private string baseURL;
    private AuthenticationHeaderValue authHeader;
    private string responseFormat;

    /// <summary>
    /// Default Constructor
    /// </summary>
    public KenticoManager()
	{
        //Pull from root appsettings.json file
        var username = Environment.GetEnvironmentVariable("KenticoRestUserName");
        var password = Environment.GetEnvironmentVariable("KenticoRestPassword");
        baseURL = Environment.GetEnvironmentVariable("KenticoSiteUrlBase");

        //Create the required Authorization Header Values by base 64 encoding a valid Kentico user's credentials
        var byteArray = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", username, password));
        authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        //Specify to Kentico REST API that we expect JSON response format instead of the default XML
        responseFormat = "json";
    }
    
    /// <summary>
    /// Generic interaction with the Kentico REST service, returns a single object
    /// </summary>
    /// <typeparam name="T">Type of return desired</typeparam>
    /// <param name="UrlEndPointPath">Path of Kentico endpoint, ex: "/ecommerce.orderaddress/"</param>
    /// <param name="UrlParams">Parameters always start with ex: "Where=AddressID=10"</param>
    /// <returns>Instance of T</returns>
    public async Task<T> GetObjectFromRestService<T>(string UrlEndPointPath, string UrlParams)
    {
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Authorization = authHeader;

            string url = string.Format("{0}{1}?format={2}&{3}",
                baseURL,
                UrlEndPointPath,
                responseFormat,
                UrlParams);

            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                T o = JsonConvert.DeserializeObject<T>(json);
                return o;

            }
        }
        return default(T);
    }

    /// <summary>
    /// Call Kentico REST API to determine Customer by email address
    /// </summary>
    /// <param name="EmailAddress">string of Customer's email address</param>
    /// <returns>Customer object</returns>
    public async Task<Customer> GetEcommerceCustomerByEmail(string EmailAddress)
    {
        Customer c = new Customer();

        //Build URL based on correct Kentico REST endpoint '/ecommerce.customer' and
        // WHERE CustomerEmail = 'email address' (url encoded where clause)
        var d = await GetObjectFromRestService<CustomersResponse>(
            "/ecommerce.customer/",
            string.Format("Where=CustomerEmail%3D%27{0}%27", WebUtility.UrlEncode(EmailAddress)));

        if (d != null)
        {
            if (d.ecommerce_customers[0] != null)
            {
                if (d.ecommerce_customers[0].COM_Customer != null)
                {
                    if (d.ecommerce_customers[0].COM_Customer[0] != null)
                    {
                        c.CustomerID = d.ecommerce_customers[0].COM_Customer[0].CustomerID;
                        c.CustomerFirstName = d.ecommerce_customers[0].COM_Customer[0].CustomerFirstName;
                        c.CustomerLastName = d.ecommerce_customers[0].COM_Customer[0].CustomerLastName;
                    }
                }
            }
        }

        return c;
    }

    /// <summary>
    /// Call Kentico REST API to determine orders by Customer and Billing Zip
    /// </summary>
    /// <param name="CustomerID">int CustomerID from Kentico</param>
    /// <param name="CustomerEnteredZipCode">Billing ZipCode to validate</param>
    /// <returns>List of Orders</returns>
    public async Task<List<Order>> GetEcommerceOrdersByCustomer(int CustomerID, string CustomerEnteredZipCode)
    {
        List<Order> orders = new List<Order>();

        var d = await GetObjectFromRestService<OrdersResponse>(
            "/ecommerce.order/",
            string.Format("Where=ordercustomerid={0}&columns=OrderID,OrderDate,OrderStatusID,OrderInvoiceNumber,OrderTotalPriceInMainCurrency,OrderBillingAddressID,OrderTrackingNumber", WebUtility.UrlEncode(CustomerID.ToString())));
       
        if (d.ecommerce_orders[0] != null)
        {
            if (d.ecommerce_orders[1] != null)
            {
                foreach (var comOrder in d.ecommerce_orders[0].COM_Order)
                {
                    Order o = new Order();
                    o.OrderID = comOrder.OrderID;
                    o.OrderNumber = comOrder.OrderInvoiceNumber;
                    o.OrderStatusID = comOrder.OrderStatusID;
                    o.OrderTotalPrice = comOrder.OrderTotalPriceInMainCurrency;
                    o.OrderDate = comOrder.OrderDate;
                    o.OrderBillingAddressID = comOrder.OrderBillingAddressID;

                    //check out the billing zip code of the order to ensure security
                    bool zipCodeMatchesOrder = await DoesCustomerZipMatchOrderBillingAddress(CustomerEnteredZipCode, o.OrderBillingAddressID);
                    if (zipCodeMatchesOrder)
                    {
                        o.OrderStatusName = await GetEcommerceOrderStatusName(o.OrderStatusID);

                        o.OrderItems = await GetEcommerceOrderItemsByOrderID(o.OrderID);

                        orders.Add(o);
                    }
                }
            }
            
        }
 
        return orders;
    }

    /// <summary>
    /// Calls Kentico REST API to determine if the provided billing zip code matches the Order info
    /// </summary>
    /// <param name="CustomerEnteredZipCode">string of Billing Zip Code</param>
    /// <param name="OrderBillingAddressID">int of Kentico Address</param>
    /// <returns>true if match, fales if not</returns>
    public async Task<bool> DoesCustomerZipMatchOrderBillingAddress(string CustomerEnteredZipCode, int OrderBillingAddressID)
    {
        if (string.IsNullOrEmpty(CustomerEnteredZipCode))
            return false;

        bool doesMatch = false;
        string orderAddressZipCode = string.Empty;

        var d = await GetObjectFromRestService<OrderAddressesResponse>(
            "/ecommerce.orderaddress/",
            string.Format("Where=AddressID={0}", OrderBillingAddressID));

        if (d.ecommerce_orderaddresses[0] != null)
        {
            if (d.ecommerce_orderaddresses[1] != null)
            {
                foreach (var comAddress in d.ecommerce_orderaddresses[0].COM_OrderAddress)
                {
                    orderAddressZipCode = comAddress.AddressZip;
                    if(!string.IsNullOrEmpty(orderAddressZipCode))
                    {
                        doesMatch = (orderAddressZipCode.ToLower() == CustomerEnteredZipCode.ToLower());
                    }
                }
            }
        }
            
        return doesMatch;
    }

    /// <summary>
    /// Call Kentico REST API to determine order status display name
    /// </summary>
    /// <param name="OrderStatusID">int of OrderStatus in Kentico</param>
    /// <returns>string of OrderStatus name</returns>
    public async Task<string> GetEcommerceOrderStatusName(int OrderStatusID)
    {
        string orderStatusName = "New"; //default it to 'New' just in case

        var d = await GetObjectFromRestService<OrderStatusesResponse>(
            "/ecommerce.orderstatus/",
            string.Format("Where=StatusID={0}", OrderStatusID));

        if (d.ecommerce_orderstatuses[0] != null)
        {
            if (d.ecommerce_orderstatuses[1] != null)
            {
                foreach (var comStatus in d.ecommerce_orderstatuses[0].COM_OrderStatus)
                {
                    orderStatusName = comStatus.StatusName;
                }
            }
        }
                   
        return orderStatusName;
    }

    /// <summary>
    /// Call Kentico REST API to determine order line items in an order
    /// </summary>
    /// <param name="OrderID">int of Kentico Order</param>
    /// <returns>List of Kentico OrderItems</returns>
    public async Task<List<OrderItem>> GetEcommerceOrderItemsByOrderID(int OrderID)
    {
        List<OrderItem> items = new List<OrderItem>();

        var d = await GetObjectFromRestService<OrderItemsResponse>(
            "/ecommerce.orderitem/",
            string.Format("Where=OrderItemOrderID={0}&columns=OrderItemID,OrderItemSKUName,OrderItemUnitCount,OrderItemUnitPrice", OrderID));
        
        if (d.ecommerce_orderitems[0] != null)
        {
            if (d.ecommerce_orderitems[1] != null)
            {
                foreach (var comOI in d.ecommerce_orderitems[0].COM_OrderItem)
                {
                    OrderItem oi = new OrderItem();
                    oi.OrderItemID = comOI.OrderItemID;
                    oi.SkuName = comOI.OrderItemSKUName;
                    oi.Quantity = comOI.OrderItemUnitCount;
                    oi.ItemPrice = comOI.OrderItemUnitPrice;
                                
                    items.Add(oi);
                }
            }
        }
       
        return items;
    }

    /// <summary>
    /// Call Kentico REST API to determine order tracking number from an order number
    /// </summary>
    /// <param name="OrderNumber">string of Kentico OrderNumber</param>
    /// <returns>string of OrderTrackingNumber</returns>
    public async Task<string> GetEcommerceOrderTrackingNumberByOrderNumber(string OrderNumber)
    {
        string orderTrackingNumber = string.Empty;

        var d = await GetObjectFromRestService<OrdersResponse>(
           "/ecommerce.order/",
           string.Format("Where=OrderInvoiceNumber={0}&columns=OrderID,OrderDate,OrderStatusID,OrderInvoiceNumber,OrderTotalPriceInMainCurrency,OrderBillingAddressID,OrderTrackingNumber", WebUtility.UrlEncode(OrderNumber)));
        
        if (d.ecommerce_orders[0] != null)
        {
            if (d.ecommerce_orders[1] != null)
            {
                foreach (var comOrder in d.ecommerce_orders[0].COM_Order)
                {
                    orderTrackingNumber = comOrder.OrderTrackingNumber;
                }
            }
        }

        return orderTrackingNumber;
    }
}