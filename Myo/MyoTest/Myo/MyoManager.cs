using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using MyoSharp.Communication;
using MyoSharp.Device;
using MyoSharp.Exceptions;
using System.Threading.Tasks;

namespace MyoHub.Myo
{
    class MyoManager
    {
        #region VAR
        IChannel channel;
        public static IHub hub;

        #endregion

        #region events
        
        public event EventHandler<GripPressureChangedEventArgs> GripPressureChanged;
        /// <summary>
        /// Event raised when the grip pressure has changed over the last iteration
        /// </summary>
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
        /// <summary>
        /// Event raised when accelerometer data has changed
        /// </summary>
        /// <param name="accEvent"></param>
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
        /// <summary>
        /// event raised when the gyroscope data has changed
        /// </summary>
        /// <param name="gyroEvent"></param>
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
        /// <summary>
        /// event raise when the oreientation data has changed
        /// </summary>
        /// <param name="OrientationEvent"></param>
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

        public event EventHandler<EMGChangedEventArgs> EMGChanged;
        /// <summary>
        /// event raised when the EMG data has changed
        /// </summary>
        /// <param name="EMGEvent"></param>
        protected virtual void OnEMGChanged(EMGChangedEventArgs EMGEvent)
        {
            EventHandler<EMGChangedEventArgs> handler = EMGChanged;
            if (handler != null)
            {
                handler(this, EMGEvent);
            }
        }

        public class EMGChangedEventArgs : EventArgs
        {
            public double EMGPod_0, EMGPod_1, EMGPod_2, EMGPod_3, EMGPod_4, EMGPod_5, EMGPod_6, EMGPod_7;
        }
        #endregion

        /// <summary>
        /// Consturctor for <see cref="MyoManager"/>
        /// </summary>
        public MyoManager()
        {
            InitMyoManagerHub();
        }

        /// <summary>
        /// Initializer Myo armband
        /// </summary>
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
            EMGChangedEventArgs emgArgs = new EMGChangedEventArgs();
            emgArgs.EMGPod_0 = e.Myo.EmgData.GetDataForSensor(0);
            emgArgs.EMGPod_1 = e.Myo.EmgData.GetDataForSensor(1);
            emgArgs.EMGPod_2 = e.Myo.EmgData.GetDataForSensor(2);
            emgArgs.EMGPod_3 = e.Myo.EmgData.GetDataForSensor(3);
            emgArgs.EMGPod_4 = e.Myo.EmgData.GetDataForSensor(4);
            emgArgs.EMGPod_5 = e.Myo.EmgData.GetDataForSensor(5);
            emgArgs.EMGPod_6 = e.Myo.EmgData.GetDataForSensor(6);
            emgArgs.EMGPod_7= e.Myo.EmgData.GetDataForSensor(7);
            //raise the event for receiving EMG data
            OnEMGChanged(emgArgs);
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
        /// Method for vibrating myo
        /// </summary>
        public static void PingMyo()
        {
            if ((DateTime.Now - Globals.LastExecution).TotalSeconds > 1)
            {
                hub.Myos.Last().Vibrate(VibrationType.Short);
                Globals.LastExecution = DateTime.Now;
            }
            
        }


    }
}
