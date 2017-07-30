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

/// <summary>
/// Collect OrderTracking Intent Information.
/// </summary>
[Serializable]
public class OrderTrackingNumberForm
{
    [Prompt("Can I get your {&} for the order?")]
    public string OrderNumber { get; set; }

    /// <summary>
    /// Builds the form for Order Tracking Number lookup.
    /// </summary>
    /// <returns>Instance of form flow.</returns>
    public static IForm<OrderTrackingNumberForm> BuildForm()
    {
        return new FormBuilder<OrderTrackingNumberForm>()
            .Field(nameof(OrderTrackingNumberForm.OrderNumber))
            .Build();
    }
}