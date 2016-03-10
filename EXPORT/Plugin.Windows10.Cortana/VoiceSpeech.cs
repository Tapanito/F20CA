
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;

using Windows.Media.SpeechSynthesis;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using System.IO;

namespace Plugin.Windows10
{
    public class VoiceSpeech
    {
        static SpeechRecognizer speechRecognizer = null;
        static bool isListening = false;
        public static string Output;
        public static MediaElement Media;
        
        static List<string> questions = new List<string>();
        static List<string> answers = new List<string>();
        static VoiceBot.Result args = new VoiceBot.Result();
        static bool hasSpoken = false;
        public async void StartListening(object sender, EventArgs e)
        {
            try
           {    
                //args = e;
                speechRecognizer = new SpeechRecognizer();
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                var uri = new System.Uri("ms-appx:///Assets/TestGrammar.xml");
                var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
                speechRecognizer.Constraints.Clear();
                speechRecognizer.Constraints.Add(new SpeechRecognitionGrammarFileConstraint(file));
                SpeechRecognitionCompilationResult compilationResult = await speechRecognizer.CompileConstraintsAsync();
                if (compilationResult.Status != SpeechRecognitionResultStatus.Success)
                    throw new Exception("Grammar compilation failed");

                

                speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
                speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;

                Debug.WriteLine("Listener initialized");
                isListening = true;
                await speechRecognizer.ContinuousRecognitionSession.StartAsync();
                uri = new System.Uri("ms-appx:///Assets/ResponseTemplates.xml");
                file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
                var t = new DialogueManager(file);
                var qq = t.GenerateResponse(new Dictionary<string, string>() { { "ACTION", "DESTINATION" }, { "DESTINATION", "COFFEE_SHOP" } }, ref args);
                Debug.WriteLine(qq);
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
               () =>
               {
                   Speak(qq);
               });






            }
            catch (Exception ex)
            {
                isListening = false;
            }

            //return "I was returned";
        }

        public void WriteMessage()
        {
            Debug.WriteLine("I was returned");
        }

        public async void StopListening(object sender, EventArgs e)
        {
            if (speechRecognizer != null)
            {
                try
                {
                    if (isListening)
                    {
                        Debug.WriteLine("Stopping to listen");

                        speechRecognizer.ContinuousRecognitionSession.Completed -= ContinuousRecognitionSession_Completed;
                        speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= ContinuousRecognitionSession_ResultGenerated;
                        var state = speechRecognizer.State;
                        await speechRecognizer.ContinuousRecognitionSession.StopAsync();
                        isListening = false;
                    }
                }
                catch (Exception ex)
                {
                    speechRecognizer.Dispose();
                    speechRecognizer = null;
                    isListening = false;
                }
            }
        }

        private async Task Speak(string text)
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();
            SpeechSynthesisStream synthStream =
                await synth.SynthesizeTextToStreamAsync(text);
            Media.SetSource(synthStream, synthStream.ContentType);
        }

        private async void VoiceResponse(string resp)
        {
            if (isListening)
            {
                await speechRecognizer.ContinuousRecognitionSession.StopAsync();
                isListening = false;

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    Speak(resp);
                });

                await speechRecognizer.ContinuousRecognitionSession.StartAsync();
                isListening = true;
            }
        }




        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs arguments)
        {
            Debug.WriteLine("User Input: " + arguments.Result.Text);

            var recoResult = arguments.Result;
            var prop = recoResult.SemanticInterpretation.Properties as IReadOnlyDictionary<string, IReadOnlyList<string>>;
            var dict = new Dictionary<string, string>();
            foreach (var key in prop.Keys)
            {
                var item = prop[key];
                dict.Add(key, item[0]);
            }
            var uri = new System.Uri("ms-appx:///Assets/ResponseTemplates.xml");
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
            DialogueManager dm = new DialogueManager(file);
            args.Name = "COFFEE_SHOP";
            string response = dm.GenerateResponse(dict, ref args);
            hasSpoken = true;
            if (response != null)
                VoiceResponse(response);
            //Debug.WriteLine(args.ToString());
            //ParseUtteranceMeaning(prop);
        }

        private void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            Debug.WriteLine("Completed");
        }
    }
}