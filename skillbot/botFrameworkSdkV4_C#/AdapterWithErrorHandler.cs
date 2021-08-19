// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.13.2

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ReneeSampleSkillBot
{
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger, ICredentialProvider credentialProvider, AuthenticationConfiguration authenticationConfiguration, ConversationState conversationSate)
            : base(credentialProvider,authenticationConfiguration)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                if(turnContext.Activity.Text == "error")
                {
                    throw new System.Exception("unhandled exception from skillbot");
                }
                // Log any leaked exception from the application.
                logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

                // Send a message to the user
                await turnContext.SendActivityAsync("The bot encountered an error or bug.");
                await turnContext.SendActivityAsync("To continue to run this bot, please fix the bot source code.");

                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");

                /*This is to notify the Renee bot that error has happened and need to come out of the skill.
                 * The Renee bot need to check the Activity code in turncontext by comparing it with success message through EndOfConversationCodes
                 * as an e.g (code==EndOfConversationCodes.CompletedSuccessfully)
                */
                var endOfConversation = Activity.CreateEndOfConversationActivity();
                endOfConversation.Code = "Error in processing Skill";
                endOfConversation.Text = exception.Message;
                endOfConversation.ChannelData = new
                {
                    Channel = "skillbot"
                };
                await turnContext.SendActivityAsync(endOfConversation);

                /*clearing the conversation state as well. Verify during the Renee testing with skillbot
                 * as to how the behavior is. Technically there shouln't be any change. But need to cross check
                 */
                await conversationSate?.DeleteAsync(turnContext);

            };
        }
    }
}
