using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConnectorHub;

namespace MyoTest.MyoManager
{
    public class LearningHubListener
    {
        //public static int listeningTCPPort = 31001; //look for the port in the xml file
        //public static int listeningUDPPort = 31002;

        private string receivedText="";

        public ConnectorHub.FeedbackHub myFeedbackHub;

        public LearningHubListener()
        {
            myFeedbackHub = new FeedbackHub();
            myFeedbackHub.init();
            myFeedbackHub.feedbackReceivedEvent += MyFeedbackHub_feedbackReceivedEvent;
        }

        private void MyFeedbackHub_feedbackReceivedEvent(object sender, string feedback)
        {
            Debug.WriteLine(feedback);
            ReadStream(feedback);
        }

        private void ReadStream(String s)
        {
            Debug.WriteLine("LearningHubListener/ Length of the message Received:" +s.Length);
            if (s.Contains("Myo"))
            {
                MyoTest.MyoManager.MyoManager.pingMyo();
                int currentIndex = 0;
                try
                {
                    currentIndex = s.IndexOf("Myo");
                    int startIndex = currentIndex + 26;
                    receivedText = s.Substring(startIndex, (s.IndexOf("\"", startIndex) - startIndex));
                    Debug.WriteLine(s.Substring(startIndex, (s.IndexOf("\"", startIndex) - startIndex)));
                    currentIndex++;
                }
                catch
                {
                    Debug.WriteLine("exception reading the stream");
                }
            }

        }
    }
}
