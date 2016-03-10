using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;

namespace BotWorldVoiceCommandService
{
    public sealed class BotWorldVoiceCommandService : IBackgroundTask
    {
        VoiceCommandServiceConnection voiceServiceConnection;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            if (triggerDetails != null && triggerDetails.Name == "BotWorldVoiceCommandService")
            {
                voiceServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);
                voiceServiceConnection.VoiceCommandCompleted += VoiceServiceConnection_VoiceCommandCompleted;

                VoiceCommand voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();
                switch(voiceCommand.CommandName)
                {
                    case "checkScore":
                        {
                            await SendAnswer();
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            deferral.Complete();

        }

        private async Task SendAnswer()
        {
            var destContentTiles = new List<VoiceCommandContentTile>();
            
            var destTile = new VoiceCommandContentTile()
            {
                ContentTileType = VoiceCommandContentTileType.TitleWithText,
                Title = "Leaderboard",
                TextLine1 = "1. Vladimir - 9337\n2. Petri - 8000"
            };
            destContentTiles.Add(destTile);
        
            var userMessagePlay = new VoiceCommandUserMessage();
            userMessagePlay.DisplayMessage = "Do you want to play?";
            userMessagePlay.SpokenMessage = "Yes, you are 1337 points behind Vladimir. Do you want to play?";

            var userMessagePlay2 = new VoiceCommandUserMessage();
            userMessagePlay2.DisplayMessage = "You are far behind. Do you want to play the game?";
            userMessagePlay2.SpokenMessage = "You are far behind. Do you want to play the game now?";

            var resp2 = VoiceCommandResponse.CreateResponseForPrompt(userMessagePlay, userMessagePlay2, destContentTiles);
            var confResp2 = await voiceServiceConnection.RequestConfirmationAsync(resp2);

            if(confResp2.Confirmed)
            {
                var umP = new VoiceCommandUserMessage();
                umP.DisplayMessage = "Do you want to play?";
                umP.SpokenMessage = "You are 1337 points behind Vladimir. Do you want to play?";

                var resp3 = VoiceCommandResponse.CreateResponse(umP);

                await voiceServiceConnection.RequestAppLaunchAsync(resp3);
            }
        }

        private void VoiceServiceConnection_VoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
        }
    }
}
