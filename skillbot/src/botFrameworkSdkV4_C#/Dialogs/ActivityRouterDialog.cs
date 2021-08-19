using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ReneeSampleSkillBot.Dialogs
{
    public class ActivityRouterDialog:ComponentDialog
    {

        public ActivityRouterDialog(ConversationState conversationState, IConfiguration configuration)
        {
            AddDialog(new CampusQnADialog(conversationState,configuration));
            AddDialog(new GeneralConversationDialog());
            AddDialog(new StudentDialog());
            AddDialog(new BookingDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog),new WaterfallStep[]
            {
                ProcessActivityAsync,

            }));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ProcessActivityAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            switch (stepContext.Context.Activity.Type)
            {
                case "event":
                    return await InvokeDialog(stepContext, cancellationToken);
                    
                //Need to code for message in GeneralConversationDialog. Just created the structure
                case "message":
                    return await stepContext.BeginDialogAsync(nameof(GeneralConversationDialog), null, cancellationToken);

                default:
                    return new DialogTurnResult(DialogTurnStatus.Complete);
            }

            
        }
        private async Task<DialogTurnResult> InvokeBookingInformation(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = new BookingDetails();
            if (stepContext.Context.Activity.Value != null)
            {
                bookingDetails = JsonConvert.DeserializeObject<BookingDetails>(JsonConvert.SerializeObject(stepContext.Context.Activity.Value));
            }
            var dialog = FindDialog(nameof(BookingDialog));
            return await stepContext.BeginDialogAsync(dialog.Id, bookingDetails, cancellationToken);
        }
        private async Task<DialogTurnResult> InvokeStudentInformation(WaterfallStepContext stepContext,CancellationToken cancellationToken)
        {
            var student = new Student();
            if (stepContext.Context.Activity.Value != null)
            {
                student = JsonConvert.DeserializeObject<Student>(JsonConvert.SerializeObject(stepContext.Context.Activity.Value));
            }
            var dialog= FindDialog(nameof(StudentDialog));
            return await stepContext.BeginDialogAsync(dialog.Id, student, cancellationToken);
        }

        private async Task<DialogTurnResult> InvokeCampusInformation(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //var student = new Student();
            //if (stepContext.Context.Activity.Value != null)
            //{
            //    student = JsonConvert.DeserializeObject<Student>(JsonConvert.SerializeObject(stepContext.Context.Activity.Value));
            //}
            var dialog = FindDialog(nameof(CampusQnADialog));
            return await stepContext.BeginDialogAsync(dialog.Id, false, cancellationToken);
        }


        private async Task<DialogTurnResult> InvokeDialog(WaterfallStepContext stepContext, CancellationToken cancellationToken) 
        {
            
            switch (stepContext.Context.Activity.Name.ToLower())
            {
                case "studentinformation":
                    return await InvokeStudentInformation(stepContext, cancellationToken);
                case "campusinformation":
                    return await InvokeCampusInformation(stepContext, cancellationToken);
                case "bookflight":
                    return await InvokeBookingInformation(stepContext, cancellationToken);
                default:
                    return new DialogTurnResult(DialogTurnStatus.Complete);
            };
            
        }

        

    }
}
