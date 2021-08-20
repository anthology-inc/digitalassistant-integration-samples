# Overview

This Skill bot sample is recreated from microsoft sample created for testing with Renee. It uses BotFramework V4 version 4.9.2
The skill bot supports following activity a.k.a skills
1. BookFlight
2. StudentInformation
3. CampusInformation

*BookFlight* & *StudentInformation* skill uses simple multi-turn context without Luis to fullfill the request.\
*CampusInformation* skill uses QnA maker to fullfill the user request.

Any skill after fullfulling the request need to send the EndOfConversationActivity(EOC) to the calling bot. Below example is from StudentDialog wherein after reaching the *FinalStepAsync* method sends the EOC.
```bash
var endOfConversation = Activity.CreateEndOfConversationActivity();
            endOfConversation.Code = EndOfConversationCodes.CompletedSuccessfully;
            endOfConversation.Text = "Ending the conversation";
```
Without this, the Renee bot will not be able to implicitly determine whether the request for the skill has completed

## Configuration
The following section describes the configuration that need to be done in appsettings.json and in skill manifest file
### Security
To ensure that this skill bot handles request only for authrozied bot, go to appsettings.json and enter the appId of the bot that needs to be allowed in *AllowedCallers* section. By default, it allows any bot to access this skill
```language
 "AllowedCallers": [ "appId1","appId2" ]
```
**Note**: While testing in BotFramework Emulator, this check is skipped by botframework SDK

### QnA maker setting
Currently the code supports only one QnA maker KnowledgeBase for any one skill. If the request is to support multiple QnA for different skills, then the code can be enhanced to support it
Go to appsettings.json and enter the following details. The below configuration describes as to how the QnA maker information need to be configured

```language
"KnowledgeBase": {
    "endpointKey": "xxxxxxx-xxx-xxxxx-xxxx-xxxxxxxxxxx", //Value to be fetched from QnA maker website for the particular knowledgebase
    "hostname": "https://xxxxx.xxxx.xxxx/qnamaker", // To be fetched from QnaMaker azure appservice
    "id": "Faq", //unique identifier
    "kbId": "xxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxx", //Knowledgebase Id to be fetched from Qna Maker website
    "name": "FAQ", //Knowledgebase name
    "subscriptionKey": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" // To be fetched from QnaMaker azure Cognitive resource in Keys & EdnPoint section
  }

```
### MicrosoftAppId & MicrosoftPassword
This value can be retrieved once this skillbot is deployed in azure and registered using bot channel registration.\
The value can either be updated in "Configuration" section in azure appservice where this skillbot is deployed or updated in appsettings.json
```language
"MicrosoftAppId": "",
"MicrosoftAppPassword": "", 
```
### Endpoint Configuration
Once the bot is deployed in azure, edit the [Manifest file](./wwwroot/dialogchildbot-manifest-1.0.json) present under wwwroot. Go to *endpoints* section and update the value of the key endpointUrl & msAppId
```language
"endpointUrl": "[url of the Skill Bot]/api/messages",
"msAppId": "[skill bot microsoftApp Id]"
``` 
For further reading click on the link below
- [How to write skill manifest](https://docs.microsoft.com/en-us/azure/bot-service/skills-write-manifest-2-1?view=azure-bot-service-4.0) 
- [Microsoft Sample Skill Manifest](https://schemas.botframework.com/schemas/skills/v2.1/skill-manifest.json)

## What this Skill bot doesn't support
1. Doesn't has luis configured to determine the intent of the user
2. Basic structure to invoke the pass through of message is present but doesn't support detail flow. The code to invoke general passthough of message is present in ActivityRouterDialog.cs as shown below
```bash
# To determine whether the call is for event or message
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
```
## Exception Handling
Any exceptions are handled in AdapterWithErrorHandler class. This class 
- Logs the exception
- Explicitly sends the EndOfConversation activity to the calling bot with error message and code to the activity object. This helps in Renee bot to determine the next step and also log the error in application insight.
- Clears the Conversation State

## Known Issue
When this sample is migrated to Bot framework migration 4.13.2 , the below  code throws socket exception error in return statement. Need to raise an issue in microsoft github
```bash
 private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
{
    var student = (Student)stepContext.Options;
    if (!(bool)stepContext.Result)
        return await stepContext.ReplaceDialogAsync(InitialDialogId, new Student(), cancellationToken);

            ......
            .......
            .......
              var endOfConversation = Activity.CreateEndOfConversationActivity();
            endOfConversation.Code = EndOfConversationCodes.CompletedSuccessfully;
            endOfConversation.Text = "Ending the conversation";

            await stepContext.Context.SendActivitiesAsync(new Activity[] { (Activity)endOfConversation }, cancellationToken);

           /*This return statement throws socket exception*/
           return await stepContext.EndDialogAsync(student, cancellationToken);

``` 
