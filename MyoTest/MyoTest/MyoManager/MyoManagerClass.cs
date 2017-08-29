using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using MyoSharp.Communication;
using MyoSharp.Device;
using MyoSharp.Exceptions;
using MyoSharp.Poses;

namespace MyoTest.MyoManager
{
    class MyoManagerClass
    {
        IChannel channel;
        public IHub hub;
        MainWindow mWindow;

        Int32[] firstPreEmgValue = new Int32[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        Int32[] secPreEmgValue = new Int32[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

        public void InitMyoManagerHub(MainWindow m)
        {
            this.mWindow = m;
            channel = Channel.Create(
                ChannelDriver.Create(ChannelBridge.Create(),
                MyoErrorHandlerDriver.Create(MyoErrorHandlerBridge.Create())));
            hub = Hub.Create(channel);

            // listen for when the Myo connects
            hub.MyoConnected += (sender, e) =>
            {
                Debug.WriteLine("Myo {0} has connected!", e.Myo.Handle);
                e.Myo.Vibrate(VibrationType.Short);
                e.Myo.EmgDataAcquired += Myo_EmgDataAcquired;
                e.Myo.OrientationDataAcquired += Myo_OrientationAcquired;
                e.Myo.SetEmgStreaming(true);
            };

            // listen for when the Myo disconnects
            hub.MyoDisconnected += (sender, e) =>
            {
                Debug.WriteLine("Oh no! It looks like {0} arm Myo has disconnected!", e.Myo.Arm);
                e.Myo.SetEmgStreaming(false);
                e.Myo.EmgDataAcquired -= Myo_EmgDataAcquired;
            };

            // start listening for Myo data
            channel.StartListening();


        }

        private void Myo_EmgDataAcquired(object sender, EmgDataEventArgs e)
        {

            CheckEmg(e);
        }

        private void Myo_OrientationAcquired(object sender, OrientationDataEventArgs e)
        {

            CheckOrientation(e);
        }

        /// <summary>
        /// Iterate through each emg sensor in myo and assign 1 if the sum of the first and second frame of emg has a sum of more than 20.
        /// else assign 0. It means that much variation(100 to -100) was observed propotional to higher tension in muscle. 
        /// 
        /// </summary>
        /// <param name="e"></param>
        void CheckEmg(EmgDataEventArgs e)
        {
            int[] emgTension = new int[8];
            Int32 avgTension = 0;
            for (int i = 0; i < 7; i++)
            {
                try
                {
                    if ((firstPreEmgValue[i] + secPreEmgValue[i] + Math.Abs(e.EmgData.GetDataForSensor(i))) <= 20)
                    {
                        emgTension[i] = 0;

                    }
                    else
                    {
                        emgTension[i] = 1;
                    }

                }
                catch
                {
                    Debug.WriteLine("Myo not connceted");
                }
            }

            //add all value from emgTension and assign it to avgTension
            Array.ForEach(emgTension, delegate (int i) { avgTension += i; });
            mWindow.UpdateEmg(avgTension);
            LoadEmg(e);
        }

        /// <summary>
        /// load and save the first and second frame of EMG[] value for comparision
        /// </summary>
        /// <param name="e"></param>
        private void LoadEmg(EmgDataEventArgs e)
        {
            try
            {
                for (int i = 0; i < 7; i++)
                {
                    secPreEmgValue[i] = firstPreEmgValue[i];
                    firstPreEmgValue[i] = Math.Abs(e.EmgData.GetDataForSensor(i));
                }
            }
            catch
            {
                Debug.WriteLine("No emg value");
            }

        }


        public void CheckOrientation(OrientationDataEventArgs e)
        {
            //mWindow.UpdateOrientation(e.Roll.ToString(), e.Yaw.ToString(), e.Pitch.ToString());
        }
    }
}
