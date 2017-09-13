#load "Message.csx"

using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.ConnectorEx;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


// For more information about this template visit http://aka.ms/azurebots-csharp-basic
[Serializable]
public class EchoDialog : IDialog<object>
{
    protected int count = 1;
    //protected string previousMessage = string.Empty;

    public Task StartAsync(IDialogContext context)
    {
        try
        {
            context.Wait(MessageReceivedAsync);
        }
        catch (OperationCanceledException error)
        {
            return Task.FromCanceled(error.CancellationToken);
        }
        catch (Exception error)
        {
            return Task.FromException(error);
        }

        return Task.CompletedTask;
    }

    public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
    {
        //var regX = new Regex(@"R-[0-9]{10}-[0-9]{6}-[0-9]{2}*");

        var message = await argument;
        //if (message.Text.ToUpper().Contains("INTIATE FILLING"))
        //{
        //    var storageAccount = CloudStorageAccount.Parse(Utils.GetAppSetting("AzureWebJobsStorage"));

        //    // Create the queue client.
        //    var queueClient = storageAccount.CreateCloudQueueClient();

        //    // Retrieve a reference to a queue.
        //    var queue = queueClient.GetQueueReference("bot-queue");
        IActivity triggerEvent = context.Activity;
        //    var tMessage = JsonConvert.DeserializeObject<Message>(((JObject)triggerEvent.Value).GetValue("Message").ToString());
        //    var messageactivity = (Activity)tMessage.RelatesTo.GetPostToBotMessage();

        //    var client = new ConnectorClient(new Uri(messageactivity.ServiceUrl));
        //    var triggerReply = messageactivity.CreateReply();
        //    triggerReply.Text = $"trigger! {message.Text}";
        //    await client.Conversations.ReplyToActivityAsync(triggerReply);
        //}
        //else 
        if (message.Text.ToUpper().Contains("HI"))
        {
            //previousMessage = "HI";
            //PromptDialog.Confirm(
            //    context,
            //    AfterResetAsync,
            //    $"Do you want to submit your time sheets for this week as R-0034567895-000010-01 9 9 9 9 9",
            //    $"Didn't get that!",
            //    promptStyle: PromptStyle.Auto);
            // Create a queue Message
            var queueMessage = new Message
            {
                RelatesTo = context.Activity.ToConversationReference(),
                Text = $"Do you want to submit your time sheets for this week as R-0034567895-000010-01 9 9 9 9 9"
            };
            var tableMessage = new MessageString((count++).ToString());

            tableMessage.SerializedMessage = JsonConvert.SerializeObject(queueMessage);
            tableMessage.IsActive = "Y";

            // write the queue Message to the queue
            //await AddMessageToQueueAsync(JsonConvert.SerializeObject(queueMessage));
            await AddMessageToTableAsync(tableMessage);
            //await context.PostAsync($"Do you want to submit your time sheets for this week as R-0034567895-000010-01 9 9 9 9 9");
            await context.PostAsync($"Your subscription is saved");
            context.Wait(MessageReceivedAsync);
        }
        else if (message.Text.ToUpper() == "YES")
        {
            await context.PostAsync($"Your time entries are submitted");
            context.Wait(MessageReceivedAsync);
        }
        else if (message.Text.ToUpper() == "NO")
        {
            await context.PostAsync($"Please specify your time entries in valid format(WBS 9 0 8 8 9)");
            context.Wait(MessageReceivedAsync);
        }
        else if (Regex.IsMatch(message.Text.ToUpper(), @"R-[0-9]{10}-[0-9]{6}-[0-9]{2}\s[0-9]\s[0-9]\s[0-9]\s[0-9]\s[0-9]"))
        {
            await context.PostAsync($"Your time entries are submitted");
            context.Wait(MessageReceivedAsync);
        }
        else
        {
            await context.PostAsync($"{message.Text} is not recognised format. Please enter valid message.");
            context.Wait(MessageReceivedAsync);
        }
    }

    public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            this.count = 1;
            await context.PostAsync($"Your time entries are submitted");
        }
        else
        {
            await context.PostAsync($"Please specify your time entries in valid format(Submit WBS hour perday with space between each day)");
        }
        context.Wait(MessageReceivedAsync);
    }

    public static async Task AddMessageToQueueAsync(string message)
    {
        // Retrieve storage account from connection string.
        var storageAccount = CloudStorageAccount.Parse(Utils.GetAppSetting("AzureWebJobsStorage"));

        // Create the queue client.
        var queueClient = storageAccount.CreateCloudQueueClient();

        // Retrieve a reference to a queue.
        var queue = queueClient.GetQueueReference("bot-queue");

        // Create the queue if it doesn't already exist.
        await queue.CreateIfNotExistsAsync();

        // Create a message and add it to the queue.
        var queuemessage = new CloudQueueMessage(message);
        await queue.AddMessageAsync(queuemessage);
    }

    public static async Task AddMessageToTableAsync(MessageString myMessageTableEntity)
    {
        // Retrieve storage account from connection string.
        var storageAccount = CloudStorageAccount.Parse(Utils.GetAppSetting("AzureWebJobsStorage"));

        // Create the table client.
        var tableClient = storageAccount.CreateCloudTableClient();

        // Retrieve a reference to a table.
        CloudTable messageTable = tableClient.GetTableReference("messageTable");

        // Create the queue if it doesn't already exist.
        await messageTable.CreateIfNotExistsAsync();

        // Create a insert query
        TableOperation insertOperation = TableOperation.Insert(myMessageTableEntity);

        // Execute the insert operation.
        await messageTable.ExecuteAsync(insertOperation);
    }



}
