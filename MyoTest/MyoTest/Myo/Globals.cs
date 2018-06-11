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
    }
}
