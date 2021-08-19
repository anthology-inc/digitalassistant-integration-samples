using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReneeSampleSkillBot.Dialogs
{
    public class StudentDialog:ComponentDialog
    {
        public StudentDialog()
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)/*, CheckStudent()*/));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)/*,ChoiceValidation()*/));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                SelectProgramAsync,
                SelectAcademicYearAsync,
                ConfirmAsync,
                FinalStepAsync,

            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private PromptValidator<FoundChoice> ChoiceValidation()
        {
            throw new NotImplementedException();
        }

        private PromptValidator<string> CheckStudent()
        {
            throw new NotImplementedException();
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var student = (Student)stepContext.Options ?? new Student();
            if (student.Id <=0)
            {
                await stepContext.Context.SendActivityAsync("Welcome to Student grade System");
                var promptMessage = MessageFactory.Text("Enter your StudentId", "Enter your StudentId", InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            return await stepContext.NextAsync(student.Id, cancellationToken);
            
        }

        private async Task<DialogTurnResult> SelectProgramAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var student = (Student)stepContext.Options;
            student.Id = Convert.ToInt32(stepContext.Result);
            if (string.IsNullOrEmpty(student.Program))
            {
                var promptMessage = MessageFactory.Text("Choose your Program", "Choose your Program", InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions { Choices = ProgramChoices(), Prompt = promptMessage, RetryPrompt = promptMessage });

            }
            return await stepContext.NextAsync(student.Program, cancellationToken);
        }

        private async Task<DialogTurnResult> SelectAcademicYearAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var student = (Student)stepContext.Options;
            student.Program = ((FoundChoice)stepContext.Result).Value;
            if (string.IsNullOrEmpty(student.AcademicYear))
            {
                var promptMessage = MessageFactory.Text("Choose your Academic year", "Choose your Academic year", InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions { Choices = AcademicChoices(), Prompt = promptMessage, RetryPrompt = promptMessage });
            }
            return await stepContext.NextAsync(student.AcademicYear, cancellationToken);
        }

        

        private  async Task<DialogTurnResult> ConfirmAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var student = (Student)stepContext.Options;
            student.AcademicYear = ((FoundChoice)stepContext.Result).Value;
            if (student.Mark<=0)
            {
                var random = new Random(10);
                var marks= random.Next(20, 100).ToString("P");
             
                var confirmMessage= $"Please confirm for {student.Id}  the {student.AcademicYear} year in program {student.Program} is correct";

                var promptMessage = MessageFactory.Text(confirmMessage, confirmMessage);

                //var message = $"The marks for {student.Id} awarded for the {student.AcademicYear} year in program {student.Program} is {marks}";
                //await stepContext.Context.SendActivityAsync(message,null,InputHints.AcceptingInput, cancellationToken);
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage });
            }
            return await stepContext.NextAsync(student.Mark, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var student = (Student)stepContext.Options;
            if (!(bool)stepContext.Result)
                return await stepContext.ReplaceDialogAsync(InitialDialogId, new Student(), cancellationToken);

            
            var random = new Random();
            var marks = random.Next(20, 100);

            var message = $"The mark of student {student.Id} in {student.AcademicYear} year participating in program {student.Program} is {marks}%";
            await stepContext.Context.SendActivityAsync(message, null, InputHints.IgnoringInput, cancellationToken);

            var endOfConversation = Activity.CreateEndOfConversationActivity();
            endOfConversation.Code = EndOfConversationCodes.CompletedSuccessfully;
            endOfConversation.Text = "Ending the conversation";
            endOfConversation.ChannelData = new
            {
                Channel = "skillbot"
            };

            await stepContext.EndDialogAsync(null, cancellationToken);
            await stepContext.Context.SendActivityAsync(endOfConversation, cancellationToken);
            return EndOfTurn;

        }
        public override Task<DialogTurnResult> ContinueDialogAsync(DialogContext outerDc, CancellationToken cancellationToken = default)
        {
            return base.ContinueDialogAsync(outerDc, cancellationToken);
        }

        private  IList<Choice> ProgramChoices()
        {

            return new List<Choice> { new Choice { Value="Engineering"},
            new Choice{Value="Arts"},
            new Choice{Value="Science"},
            new Choice{Value="Humanities"},
            new Choice{Value="Medicine"},
            new Choice{Value="Management"}
            };
        }
        private IList<Choice> AcademicChoices()
        {
            return new List<Choice> { new Choice { Value = "First" },
                new Choice { Value = "Second" }, 
                new Choice { Value = "Third" }, 
                new Choice { Value = "Fourth" } };
        }

    }

    public class Student
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("academicYear")]
        public string AcademicYear { get; set; }
        [JsonProperty("program")]
        public string Program { get; set; }
        [JsonProperty("mark")]
        public decimal Mark { get; set; }
    }
}
