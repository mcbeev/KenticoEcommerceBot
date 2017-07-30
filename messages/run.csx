#r "Newtonsoft.Json"
#load "KenticoManager.csx"
#load "RootDialog.csx"
#load "OrderHistoryForm.csx"
#load "OrderTrackingNumberForm.csx"
#load "models/Customer.csx"
#load "models/Order.csx"
#load "models/CustomersResponse.csx"
#load "models/OrdersResponse.csx"
#load "models/OrderStatusesResponse.csx"
#load "models/OrderItemsResponse.csx"
#load "models/OrderAddressesResponse.csx"

using System;

using System.Net;
using System.Threading;
using Newtonsoft.Json;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info($"Webhook was triggered!");

    // Initialize the azure bot
    using (BotService.Initialize())
    {
        // Deserialize the incoming activity
        string jsonContent = await req.Content.ReadAsStringAsync();
        var activity = JsonConvert.DeserializeObject<Activity>(jsonContent);
        
        // authenticate incoming request and add activity.ServiceUrl to MicrosoftAppCredentials.TrustedHostNames
        // if request is authenticated
        if (!await BotService.Authenticator.TryAuthenticateAsync(req, new [] {activity}, CancellationToken.None))
        {
            return BotAuthenticator.GenerateUnauthorizedResponse(req);
        }
    
        if (activity != null)
        {
            // one of these will have an interface and process it
            switch (activity.GetActivityType())
            {
                case ActivityTypes.Message:
                    //if the user types certain messages, quit all dialogs and start over
                    string messageText = activity.Text.ToLower().Trim();
                    if (messageText == "start over" || messageText == "exit" || messageText == "quit" || messageText == "done" || messageText == "end" || messageText == "restart" || messageText == "leave" || messageText == "reset")
                    {
                        //This is where the conversation gets reset!
                        activity.GetStateClient().BotState.DeleteStateForUser(activity.ChannelId, activity.From.Id);
                    }
                    else
                    {
                        //This is where the real conversation starts and calls our LUIS intents
                        await Conversation.SendAsync(activity, () => new RootDialog());
                    }
                    break;
                case ActivityTypes.ConversationUpdate:
                    var client = new ConnectorClient(new Uri(activity.ServiceUrl));
                    IConversationUpdateActivity update = activity;
                    if (update.MembersAdded.Any())
                    {
                        var reply = activity.CreateReply();
                        var newMembers = update.MembersAdded?.Where(t => t.Id != activity.Recipient.Id);
                        foreach (var newMember in newMembers)
                        {
                            reply.Text = "Welcome";
                            if (!string.IsNullOrEmpty(newMember.Name))
                            {
                                reply.Text += $" {newMember.Name}";
                            }
                            reply.Text += $"! How can we help you today?\nWould you like to check your order history or find an order tracking number?";
                            await client.Conversations.ReplyToActivityAsync(reply);
                        }
                    }
                    break;
                case ActivityTypes.ContactRelationUpdate:
                case ActivityTypes.Typing:
                case ActivityTypes.DeleteUserData:
                case ActivityTypes.Ping:
                default:
                    log.Error($"Unknown activity type ignored: {activity.GetActivityType()}");
                    break;
            }
        }
        return req.CreateResponse(HttpStatusCode.Accepted);
    }    
}