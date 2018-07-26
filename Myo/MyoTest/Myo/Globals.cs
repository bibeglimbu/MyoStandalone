using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyoHub.Myo
{
    public static class Globals
    {
        private static bool _isRecording = false;
        public static bool IsRecording
        {
            get { return _isRecording; }
            set
            {
                _isRecording = value;
            }
        }

        private static bool _vibrateMyo = true;
        public static bool VibrateMyo
        {
            get { return _vibrateMyo; }
            set
            {
                _vibrateMyo = value;
            }
        }

        private static DateTime _lastExecution = DateTime.Now;
        public static DateTime LastExecution
        {
            get { return _lastExecution; }
            set
            {
                _lastExecution = value;
            }
        }
    }
}
