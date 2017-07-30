using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using System.Net.Http.Headers;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.FormFlow;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// For more information about this template visit http://aka.ms/azurebots-csharp-luis
[Serializable]
public class RootDialog : LuisDialog<object>
{
    //Use this when running in Azure Portal it will use those settings, or use the local appsettings.json in debug mode
    public RootDialog() : base(new LuisService(new LuisModelAttribute(Utils.GetAppSetting("LuisAppId"), Utils.GetAppSetting("LuisAPIKey"))))
    {
    }

    // Go to https://luis.ai and create a new intent, then train/publish your luis app.
    [LuisIntent("None")]
    public async Task NoneIntent(IDialogContext context, LuisResult result)
    {
        await context.PostAsync($"You have reached the none intent. You said: {result.Query}");
        context.Wait(MessageReceived);
    }
        
    [LuisIntent("OrderHistory")]
    public async Task OrderHistoryLookup(IDialogContext context, LuisResult result)
    {     
        await context.PostAsync("Ok let's get started.");

        var form = new FormDialog<OrderHistoryForm>(
            new OrderHistoryForm(),
            OrderHistoryForm.BuildForm,
            FormOptions.PromptInStart,
            PreprocessEntities(result.Entities));

        context.Call<OrderHistoryForm>(form, OrderHistoryLookupComplete);
    }

    [LuisIntent("OrderTrackingNumber")]
    public async Task OrderTrackingNumberLookup(IDialogContext context, LuisResult result)
    {
        await context.PostAsync("So you want to find your package eh?");

        var form = new FormDialog<OrderTrackingNumberForm>(
            new OrderTrackingNumberForm(),
            OrderTrackingNumberForm.BuildForm,
            FormOptions.PromptInStart,
            PreprocessEntities(result.Entities));

        context.Call<OrderTrackingNumberForm>(form, OrderTrackingNumberLookupComplete);
    }

    private async Task OrderHistoryLookupComplete(IDialogContext context, IAwaitable<OrderHistoryForm> result)
    {
        OrderHistoryForm form = null;
        try
        {
            form = await result;
        }
        catch (OperationCanceledException ex)
        {
            await context.PostAsync($"There was an error with your request: {ex.Message}");
        }

        if (form == null)
        {
            await context.PostAsync("You canceled the form.");
        }
        else
        {
            var message = string.Empty;
            var rest = new KenticoManager();

            //Find customer by email
            Customer c = await rest.GetEcommerceCustomerByEmail(form.EmailAddress);
            if (c.CustomerID > 0)
            {
                message += $"Thank you {c.CustomerFirstName}, ";

                List<Order> orders = await rest.GetEcommerceOrdersByCustomer(c.CustomerID, form.ZipCode);
                if (orders.Count > 0)
                {
                    message += $"we found {orders.Count} orders with that email address.\n";

                    int i = 1;
                    foreach (Order o in orders)
                    {
                        message += $"{i}. On {o.OrderDate}\n Order #**{o.OrderNumber}** was placed for a total of **{o.OrderTotalPrice:c}**.\n This order has a status of **{o.OrderStatusName}**.\n";
                        foreach (OrderItem oi in o.OrderItems)
                        {
                            message += $"* {oi.SkuName} ({oi.Quantity} @ {oi.ItemPrice:c})\n";
                        }
                        i++;
                        message += "\n";
                    }
                }
                else
                {
                    message += $"I was unable to find orders for that customer, or the zip code that was provided ({form.ZipCode}) didn't match.";
                }
            }
            else
            {
                message += $"However, I'm sorry we could not find any customers with an email address of {form.EmailAddress}";
            }

            await context.PostAsync(message);
        }

        context.Wait(MessageReceived);
    }

    private async Task OrderTrackingNumberLookupComplete(IDialogContext context, IAwaitable<OrderTrackingNumberForm> result)
    {
        OrderTrackingNumberForm form = null;
        try
        {
            form = await result;
        }
        catch (OperationCanceledException ex)
        {
            await context.PostAsync($"There was an error with your request: {ex.Message}");
        }

        if (form == null)
        {
            await context.PostAsync("You canceled the form.");
        }
        else
        {
            var message = string.Empty;
            var rest = new KenticoManager();

            var trackingNumber = await rest.GetEcommerceOrderTrackingNumberByOrderNumber(form.OrderNumber);
            if(!string.IsNullOrEmpty(trackingNumber))
            {
                message = $"Your tracking number is **{trackingNumber}**\n";
            }
            else
            {
                message = $"Sorry we could not find an order with the Order Number **{form.OrderNumber}**\n";
            }

            await context.PostAsync(message);

        }

        context.Wait(MessageReceived);
    }

    private IList<EntityRecommendation> PreprocessEntities(IList<EntityRecommendation> entities)
    {
        // remove spaces from email address
        var emailEntity = entities.Where(e => e.Type == "EmailAddress").FirstOrDefault();
        if (emailEntity != null)
        {
            emailEntity.Entity = Regex.Replace(emailEntity.Entity, @"\s+", string.Empty);
        }
        return entities;
    }
}