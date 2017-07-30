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
/// Collect Order History Intent Information.
/// </summary>
[Serializable]
public class OrderHistoryForm
{
    [Prompt("Can I get your {&} for the order?")]
    public string EmailAddress { get; set; }

    [Prompt("Please enter your billing {&}.")]
    public string ZipCode { get; set; }

    // email regex is from: https://html.spec.whatwg.org/multipage/forms.html#valid-e-mail-address
    private const string EmailRegExPattern = @"^[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";
    private const string ZipRegExPattern = @"^([0-9]{5})(?:[-\s]*([0-9]{4}))?$";

    /// <summary>
    /// Email address validator
    /// </summary>
    private static ValidateAsyncDelegate<OrderHistoryForm> EmailValidator = async (state, response) =>
    {
        var result = new ValidateResult { IsValid = true, Value = response };
        var email = (response as string).Trim();
        if (!Regex.IsMatch(email, EmailRegExPattern))
        {
            result.Feedback = "Sorry, that doesn't look like a valid email address.";
            result.IsValid = false;
        }

        return await Task.FromResult(result);
    };

    /// <summary>
    /// Zip code validator
    /// </summary>
    private static ValidateAsyncDelegate<OrderHistoryForm> ZipValidator = async (state, response) =>
    {
        var result = new ValidateResult { IsValid = true, Value = response };
        var zip = (response as string).Trim();
        if (!Regex.IsMatch(zip, ZipRegExPattern))
        {
            result.Feedback = "Sorry, that is not a valid zip code. A zip code should be 5 digits.";
            result.IsValid = false;
        }

        return await Task.FromResult(result);
    };

    /// <summary>
    /// Builds the Signup form.
    /// </summary>
    /// <returns>An instance of the <see cref="OrderHistoryForm"/> form flow.</returns>
    public static IForm<OrderHistoryForm> BuildForm()
    {
        return new FormBuilder<OrderHistoryForm>()
            .Message("Great, I just need a few pieces of information from you.")
            .Field(nameof(OrderHistoryForm.EmailAddress), validate: EmailValidator)
            .Field(nameof(OrderHistoryForm.ZipCode), validate: ZipValidator)
            .Build();
    }
}