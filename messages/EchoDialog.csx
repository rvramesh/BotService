using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Text.RegularExpressions;

// For more information about this template visit http://aka.ms/azurebots-csharp-basic
[Serializable]
public class EchoDialog : IDialog<object>
{
    protected int count = 1;
    protected string previousMessage = string.Empty;

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
        if(message.Text.ToUpper().Contains("HI"))
        {
            previousMessage = "HI";
            await context.PostAsync($"Do you want to submit your time sheets for this week as R-0034567895-000010-01 9 9 9 9 9") ;
            context.Wait(MessageReceivedAsync);
            //PromptDialog.Confirm(
            //    context,
            //    AfterResetAsync,
            //    $"Do you want to submit your time sheets for this week as R-0034567895-000010-01 9 9 9 9 9",
            //    $"Didn't get that!",
            //    promptStyle: PromptStyle.Auto);
        }
        else if (message.Text.ToUpper() == "YES" && previousMessage == "HI")
        {
            await context.PostAsync($"Your time entries are submitted");
            previousMessage = string.Empty;
            context.Wait(MessageReceivedAsync);
        }
        else if(message.Text.ToUpper() == "NO" && previousMessage == "HI")
        {
            await context.PostAsync($"Please specify your time entries in valid format(Submit WBS hour perday with space between each day)");
            context.Wait(MessageReceivedAsync);
        }
        else if (Regex.IsMatch(message.Text.ToUpper(), @"R-[0-9]{10}-[0-9]{6}-[0-9]{2}.*"))
        {
            await context.PostAsync($"Your time entries are submitted");
            previousMessage = string.Empty;
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
}
