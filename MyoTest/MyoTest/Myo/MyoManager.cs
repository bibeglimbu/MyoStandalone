using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using MyoSharp.Communication;
using MyoSharp.Device;
using MyoSharp.Exceptions;

namespace MyoHub.Myo
{
    class MyoManager
    {
        #region VAR
        IChannel channel;
        public static IHub hub;

        private int gripEMG = 0;
        private float accelaration = 0.0f;

        private DateTime lastExecutionEmg;
        private DateTime lastExecutionVibrate;
        private DateTime lastExecutionOrientation;
        

        int[] preEmgValue = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        int[] storeEmgValue = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

        private bool vibrateMyo=true;

        #endregion

        #region events
        public event EventHandler<GripPressureChangedEventArgs> GripPressureChanged;
        protected virtual void OnGripPressureChanged(GripPressureChangedEventArgs g)
        {
            EventHandler<GripPressureChangedEventArgs> handler = GripPressureChanged;
            if (handler != null)
            {
                handler(this, g);
            }
        }

        public class GripPressureChangedEventArgs : EventArgs
        {
            public int gripPressure { get; set; }
        }

        public event EventHandler<AccelerometerChangedEventArgs> AccelerometerChanged;
        protected virtual void OnAccelerometerChanged(AccelerometerChangedEventArgs a)
        {
            EventHandler<AccelerometerChangedEventArgs> handler = AccelerometerChanged;
            if (handler != null)
            {
                handler(this, a);
            }
        }

        public class AccelerometerChangedEventArgs : EventArgs
        {
            public float accelerometerMag { get; set; }
        }
        #endregion


        public MyoManager()
        {
            InitMyoManagerHub();
        }

        public void InitMyoManagerHub()
        {
            lastExecutionEmg = DateTime.Now;
            lastExecutionVibrate = DateTime.Now;
            channel = Channel.Create( ChannelDriver.Create(ChannelBridge.Create(),
                MyoErrorHandlerDriver.Create(MyoErrorHandlerBridge.Create())));
            hub = Hub.Create(channel);

            // listen for when the Myo connects
            hub.MyoConnected += (sender, e) =>
            {
                Debug.WriteLine("Myo {0} has connected!", e.Myo.Handle);
                e.Myo.Vibrate(VibrationType.Short);
                e.Myo.EmgDataAcquired += Myo_EmgDataAcquired;
                e.Myo.AccelerometerDataAcquired += Myo_AccelerometerDataAcquired;
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

        #region MyoEvents
        private void Myo_EmgDataAcquired(object sender, EmgDataEventArgs e)
        {
            if ((DateTime.Now - lastExecutionEmg).TotalSeconds >= 0.5)
            {
                //there is no need to send emg data
                CalculateGripPressure(e);
                GripPressureChangedEventArgs args = new GripPressureChangedEventArgs();
                args.gripPressure = gripEMG;
                OnGripPressureChanged(args);
                lastExecutionEmg = DateTime.Now;
            }

                //vibrate only twice a sec
                if (vibrateMyo == true)
                {
                    if (gripEMG >= 4)
                    {
                        Debug.WriteLine("gripEmg" + gripEMG);
                        pingMyo();
                        try
                        {
                            HubConnector.myConnector.sendFeedback("Read Grip the pen gently");
                        }
                        catch
                        {
                            Debug.WriteLine("feedback not sent");
                        }

                        lastExecutionVibrate = DateTime.Now;
                        vibrateMyo = false;
                    }
                }
                if ((DateTime.Now - lastExecutionVibrate).TotalSeconds >= 0.5)
                {
                    vibrateMyo = true;

                }

            gripEMG = 0;
        }
        private void Myo_AccelerometerDataAcquired(object sender, AccelerometerDataEventArgs a)
        {
            AccelerometerChangedEventArgs args = new AccelerometerChangedEventArgs();
            args.accelerometerMag = a.Accelerometer.Magnitude();
            OnAccelerometerChanged(args);
            accelaration = a.Accelerometer.Magnitude();
            //Debug.WriteLine(a.Accelerometer.Magnitude());
        }

        #endregion


        /// <summary>
        /// Iterate through each emg sensor in myo and assign 1 if the sum of the first and second frame of emg has a sum of more than 20.
        /// else assign 0. It means that much variation(100 to -100) was observed propotional to higher tension in muscle. 
        /// </summary>
        /// <param name="e"></param>
        void CalculateGripPressure(EmgDataEventArgs e)
        {
            //Threshold to determind the fluctuation
            int emgThreshold = 15;
            int[] currentEmgValue = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            int[] emgTension = new int[8];

            //iterate through all the sensors and store the 1/0  in emg tension depending if the sum of previous frame of data and current frame is less than threshold
            // 0 meaning no tension and 100 meaning lots of tension
            for (int i = 0; i <= 7; i++)
            {
                storeEmgValue[i] = e.EmgData.GetDataForSensor(i);
                currentEmgValue[i] = Math.Abs(e.EmgData.GetDataForSensor(i));
                //Debug.WriteLine("MyoManager/" + i + " " + Math.Abs(e.EmgData.GetDataForSensor(i)));
                try
                {
                    if (currentEmgValue[i] >= emgThreshold)
                    {
                        emgTension[i] = 1;

                    }
                    else
                    {
                        emgTension[i] = 0;
                    }

                }
                catch
                {
                    Debug.WriteLine("Myo not connceted");
                }
            }

            //add all value from emgTension and assign it to gripEmg
            Array.ForEach(emgTension, delegate (int i) { gripEMG += i; });

            try
            {
                for (int i = 0; i < 7; i++)
                {
                    preEmgValue[i] = currentEmgValue[i];
                }
            }
            catch
            {
                Debug.WriteLine("No emg value");
            }
        }

        public static void pingMyo()
        {
            hub.Myos.Last().Vibrate(VibrationType.Short);
        }

        private void MyFeedback_feedbackReceivedEvent(object sender, string feedback)
        {
            //mWindow.UpdateDebug("Myo: Learninghublistener feedback received: " + feedback);
            //Debug.WriteLine("Myo: Learninghublistener feedback received: " + feedback);

            ReadStream(feedback);
        }

        private void ReadStream(String s)
        {
            if (s.Contains("Myo"))
            {
                pingMyo();
                //mWindow.UpdateDebug(s);
            }

        }
    }
}
