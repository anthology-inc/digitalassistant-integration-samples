using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReneeSampleSkillBot.Bots
{
    public class SkillActivityHandler<T>:ActivityHandler where T: Dialog
    {
        private readonly ConversationState _conversationState;
        private readonly Dialog _dialog;
        public SkillActivityHandler(ConversationState conversationState, T mainDialog)
        {
            _conversationState = conversationState;
            _dialog = mainDialog;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);

            
        }
    }
}
