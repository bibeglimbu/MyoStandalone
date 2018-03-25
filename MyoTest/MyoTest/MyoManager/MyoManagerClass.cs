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
using System.Net.Sockets;
using System.Net;
using System.Windows.Media.Media3D;

namespace MyoTest.MyoManager
{
    class MyoManager
    {
        IChannel channel;
        public static IHub hub;
        MainWindow mWindow;

        private int gripEMG = 0;
        private float orientationW=0;
        private float orientationX=0;
        private float orientationY=0;
        private float orientationZ=0;

        private DateTime lastExecutionEmg;
        private DateTime lastExecutionVibrate;


        int[] preEmgValue = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };


        public void InitMyoManagerHub(MainWindow m)
        {
            lastExecutionEmg = DateTime.Now;
            lastExecutionVibrate = DateTime.Now;
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

            try
            {
                HubConnector.myConnector.startRecordingEvent += MyConnector_startRecordingEvent;
                HubConnector.myConnector.stopRecordingEvent += MyConnector_stopRecordingEvent;
                setValueNames();
            }
            catch (Exception e)
            {
                Debug.WriteLine("MyoManager error at connecting the hub");
            }

            // start listening for Myo data
            channel.StartListening();

        }

        #region Send data
        public void setValueNames()
        {
            List<string> names = new List<string>();
            names.Add("GripPressure");
            HubConnector.SetValuesName(names);

        }

        private void MyConnector_startRecordingEvent(Object sender)
        {

        }

        private void MyConnector_stopRecordingEvent(Object sender)
        {

        }
        #endregion

        #region MyoEvents
        private void Myo_EmgDataAcquired(object sender, EmgDataEventArgs e)
        {
            if ((DateTime.Now - lastExecutionEmg).TotalSeconds >= 0.2)
            {
                CalculateGripPressure(e);
                //SendData();
               
            }
            if((DateTime.Now - lastExecutionVibrate).TotalSeconds >= 1)
            {
                if (gripEMG >= 6)
                {
                    Debug.WriteLine("gripEmg" + gripEMG);
                    pingMyo();
                }
            }

            gripEMG = 0;
        }

        private void Myo_OrientationAcquired(object sender, OrientationDataEventArgs e)
        {
            if ((DateTime.Now - lastExecutionEmg).TotalSeconds >= 0.2)
            {
                CalculateOrientation(e);
                //SendData();
            }
        }
        #endregion

        /// <summary>
        /// Method to broadcast packets of data
        /// </summary>
        /// <param name="pressure"></param>
        public void SendData()
        {
            try
            {
                List<string> values = new List<string>();
                values.Add(gripEMG.ToString());
                HubConnector.SendData(values);
                Debug.WriteLine("MyoManager/ The last value sent: " + values[values.Count - 1].ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
            
    }

        /// <summary>
        /// Iterate through each emg sensor in myo and assign 1 if the sum of the first and second frame of emg has a sum of more than 20.
        /// else assign 0. It means that much variation(100 to -100) was observed propotional to higher tension in muscle. 
        /// </summary>
        /// <param name="e"></param>
        void CalculateGripPressure(EmgDataEventArgs e)
        {
            //Threshold to determind the fluctuation
            int emgThreshold = 40;
            int[] currentEmgValue = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            int[] emgTension = new int[8];

            //iterate through all the sensors and store the 1/0  in emg tension depending if the sum of previous frame of data and current frame is less than threshold
            // 0 meaning no tension and 100 meaning lots of tension
            for (int i = 0; i <= 7; i++)
            {
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
            mWindow.UpdateGripPressure(gripEMG);

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

        /// <summary>
        /// Method called upon receiving the even myodata received. It passes on the orientation data to the UpdateOrientation class in Mainwindow
        /// </summary>
        /// <param name="e"></param>
        public void CalculateOrientation(OrientationDataEventArgs e)
        {
            orientationW = e.Orientation.W;
            orientationX = e.Orientation.X;
            orientationY = e.Orientation.Y;
            orientationZ = e.Orientation.Z;
            mWindow.UpdateOrientation(orientationW, orientationX, orientationY, orientationZ);

            //myoRoll = e.Roll;
            //mWindow.UpdateRoll(myoRoll);
            //myoPitch = e.Pitch;
            //mWindow.UpdatePitch(myoPitch);
            //myoYaw = e.Yaw;
            //mWindow.UpdateYaw(myoYaw);
        }


        public static void pingMyo()
        {
            hub.Myos.Last().Vibrate(VibrationType.Short);
        }
    }
}
