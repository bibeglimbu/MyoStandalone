using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyoTest
{
    static class HubConnector
    {
        public static ConnectorHub.ConnectorHub myConnector;

        public static void StartConnection()
        {
            myConnector = new ConnectorHub.ConnectorHub();
            myConnector.init();
            myConnector.sendReady();
        }

        public static void SendData(List<String> values)
        {
            myConnector.storeFrame(values);
        }

        public static void SetValuesName(List<String> names)
        {
            myConnector.setValuesName(names);
        }
    }
}
