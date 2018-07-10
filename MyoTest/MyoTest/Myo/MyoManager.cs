﻿using System;
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

        int[] preEmgValue = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        int[] storeEmgValue = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

        #endregion

        #region events
        public event EventHandler<GripPressureChangedEventArgs> GripPressureChanged;
        protected virtual void OnGripPressureChanged(GripPressureChangedEventArgs gripEvent)
        {
            EventHandler<GripPressureChangedEventArgs> handler = GripPressureChanged;
            if (handler != null)
            {
                handler(this, gripEvent);
            }
        }

        public class GripPressureChangedEventArgs : EventArgs
        {
            public int gripPressure { get; set; }
        }

        public event EventHandler<AccelerometerChangedEventArgs> AccelerometerChanged;
        protected virtual void OnAccelerometerChanged(AccelerometerChangedEventArgs accEvent)
        {
            EventHandler<AccelerometerChangedEventArgs> handler = AccelerometerChanged;
            if (handler != null)
            {
                handler(this, accEvent);
            }
        }

        public class AccelerometerChangedEventArgs : EventArgs
        {
            public float accelerometerMag { get; set; }
            public float accelerometerX { get; set; }
            public float accelerometerY { get; set; }
            public float accelerometerZ { get; set; }
        }

        public event EventHandler<GyroscopeChangedEventArgs> GyroscopeChanged;
        protected virtual void OnGyroscopeChanged(GyroscopeChangedEventArgs gyroEvent)
        {
            EventHandler<GyroscopeChangedEventArgs> handler = GyroscopeChanged;
            if (handler != null)
            {
                handler(this, gyroEvent);
            }
        }

        public class GyroscopeChangedEventArgs : EventArgs
        {
            public float gyroscopeX { get; set; }
            public float gyroscopeY { get; set; }
            public float gyroscopeZ { get; set; }
        }

        public event EventHandler<OrientationChangedEventArgs> OrientationChanged;
        protected virtual void OnOrientationChanged(OrientationChangedEventArgs OrientationEvent)
        {
            EventHandler<OrientationChangedEventArgs> handler = OrientationChanged;
            if (handler != null)
            {
                handler(this, OrientationEvent);
            }
        }

        public class OrientationChangedEventArgs : EventArgs
        {
            public float OrientationW { get; set; }
            public float OrientationX { get; set; }
            public float OrientationY { get; set; }
            public float OrientationZ { get; set; }
        }
        #endregion


        public MyoManager()
        {
            InitMyoManagerHub();
        }

        public void InitMyoManagerHub()
        {
            Globals.LastExecution = DateTime.Now;
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
                e.Myo.GyroscopeDataAcquired += Myo_GyroscopeDataAcquired;
                e.Myo.OrientationDataAcquired += Myo_OrientationDataAcquired;
                e.Myo.SetEmgStreaming(true);
            };

            // listen for when the Myo disconnects
            hub.MyoDisconnected += (sender, e) =>
            {
                Debug.WriteLine("Oh no! It looks like {0} arm Myo has disconnected!", e.Myo.Arm);
                e.Myo.SetEmgStreaming(false);
                e.Myo.EmgDataAcquired -= Myo_EmgDataAcquired;
                e.Myo.AccelerometerDataAcquired -= Myo_AccelerometerDataAcquired;
                e.Myo.GyroscopeDataAcquired -= Myo_GyroscopeDataAcquired;
                e.Myo.OrientationDataAcquired -= Myo_OrientationDataAcquired;
            };

            // start listening for Myo data
            channel.StartListening();
            
        }


        #region MyoEvents

        private void Myo_EmgDataAcquired(object sender, EmgDataEventArgs e)
        {
                CalculateGripPressure(e);
                GripPressureChangedEventArgs args = new GripPressureChangedEventArgs();
                args.gripPressure = gripEMG;
                OnGripPressureChanged(args);
                gripEMG = 0;
        }

        private void Myo_OrientationDataAcquired(object sender, OrientationDataEventArgs e)
        {
            OrientationChangedEventArgs args = new OrientationChangedEventArgs();
            args.OrientationW = e.Orientation.W;
            args.OrientationX = e.Orientation.X;
            args.OrientationY = e.Orientation.Y;
            args.OrientationZ = e.Orientation.Z;
            OnOrientationChanged(args);
        }

        private void Myo_GyroscopeDataAcquired(object sender, GyroscopeDataEventArgs e)
        {
            //there is no need to send emg data
            GyroscopeChangedEventArgs args = new GyroscopeChangedEventArgs();
            args.gyroscopeX = e.Gyroscope.X;
            args.gyroscopeY = e.Gyroscope.Y;
            args.gyroscopeZ = e.Gyroscope.Z;
            OnGyroscopeChanged(args);
        }
        private void Myo_AccelerometerDataAcquired(object sender, AccelerometerDataEventArgs a)
        {
                AccelerometerChangedEventArgs args = new AccelerometerChangedEventArgs();
                args.accelerometerMag = a.Accelerometer.Magnitude();
                args.accelerometerX = a.Accelerometer.X;
                args.accelerometerY = a.Accelerometer.Y;
                args.accelerometerZ = a.Accelerometer.Z;
                OnAccelerometerChanged(args);
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
            if ((DateTime.Now - Globals.LastExecution).TotalSeconds > 1)
            {
                hub.Myos.Last().Vibrate(VibrationType.Short);
                Globals.LastExecution = DateTime.Now;
            }
            
        }


    }
}