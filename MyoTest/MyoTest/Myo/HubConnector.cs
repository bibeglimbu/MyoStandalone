using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyoHub.Myo
{
        static class HubConnector
        {
            public static ConnectorHub.ConnectorHub myConnector;
            public static ConnectorHub.FeedbackHub myFeedback;


            public static void StartConnection()
            {
                myConnector = new ConnectorHub.ConnectorHub();
                myFeedback = new ConnectorHub.FeedbackHub();

                myConnector.init();
                myConnector.sendReady();
            }
        }
}
