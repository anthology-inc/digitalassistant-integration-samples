using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReneeSampleSkillBot.Dialogs
{
    public class CampusQnADialog:ComponentDialog
    {
        private readonly ConversationState _conversationState;
        private IStatePropertyAccessor<bool> _conversationStatePropertyAccessor;
        private readonly IConfiguration _configuration;
        private const string FaqDialogId = "Faq";
        public CampusQnADialog(ConversationState conversationState, IConfiguration configuration)
        {
            _conversationState = conversationState;
            _configuration = configuration;
            
            _conversationStatePropertyAccessor=conversationState.CreateProperty<bool>(nameof(CampusQnADialog));

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                WelcomeStepAsync,
                AskQuestionStepAsync,
                ProcessStepAsync,
                FinalStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
            
        }

        private async Task<DialogTurnResult> WelcomeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var isRepeat = await _conversationStatePropertyAccessor.GetAsync(stepContext.Context, () => false, cancellationToken);

            if (!isRepeat)
            {
                var message = $"Welcome to Campus Management system. You can ask question related to Campus";
                var promptMessage = MessageFactory.Text(message, message, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(promptMessage, cancellationToken);
            }
            return await stepContext.NextAsync(stepContext.Options,cancellationToken: cancellationToken);

        }

        private async Task<DialogTurnResult> AskQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var isRepeat = await _conversationStatePropertyAccessor.GetAsync(stepContext.Context, () => false, cancellationToken);
            if (isRepeat && (bool)(stepContext?.Options ?? true))
                return await stepContext.NextAsync( stepContext.Options, cancellationToken: cancellationToken);

            var message = MessageFactory.Text("What can i help you with?", "What can i help you with?",InputHints.ExpectingInput);
            await _conversationStatePropertyAccessor.SetAsync(stepContext.Context, false, cancellationToken);
            
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = message });
        }

        private async Task<DialogTurnResult> ProcessStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Context.Activity.Text == "error")
            {
                throw new Exception("unhandled exception from skillbot");
            }
            var isRepeat = await _conversationStatePropertyAccessor.GetAsync(stepContext.Context, () => false, cancellationToken);
            if (isRepeat)
            {
                var message = $"Do you want to ask more question?";
                var promptMessage = MessageFactory.Text(message, message, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            var question = (string)stepContext.Result;
            var qnaMaker = new QnAMakerDialog(FaqDialogId,
                _configuration["KnowledgeBase:endpointKey"],
                _configuration["KnowledgeBase:hostname"],
                noAnswer: MessageFactory.Text("Do you want to ask a different question"))
            {
                
                Id = _configuration["KnowledgeBase:id"],
                KnowledgeBaseId = _configuration["KnowledgeBase:kbId"],
            };
            var result= await qnaMaker.BeginDialogAsync(stepContext, null, cancellationToken);
            await _conversationStatePropertyAccessor.SetAsync(stepContext.Context, true, cancellationToken);
            return await stepContext.ReplaceDialogAsync(InitialDialogId, true, cancellationToken);

        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!(bool)stepContext.Result)
            {
                var endOfActivity = Activity.CreateEndOfConversationActivity();
                endOfActivity.Code = EndOfConversationCodes.CompletedSuccessfully;
                endOfActivity.Text = "Conversation ended";
                endOfActivity.ChannelData = new
                {
                    Channel = "skillbot"
                };
                await stepContext.EndDialogAsync(null, cancellationToken);
                await stepContext.Context.SendActivityAsync(endOfActivity, cancellationToken);
                return EndOfTurn;
            }
            return await stepContext.ReplaceDialogAsync(InitialDialogId, false, cancellationToken);
            
        }
    }

    
}
