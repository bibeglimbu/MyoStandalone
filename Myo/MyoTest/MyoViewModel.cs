using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MyoHub.Myo;
using MyoTest;

namespace MyoHub
{
    public class MyoViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region Variables

        MyoManager myoManager = new MyoManager();

        private string _debugText = " ";
        public string DebugText
        {
            get { return _debugText; }
            set
            {
                    _debugText = value;
                    NotifyPropertyChanged();
                    //UpdateDebug(_debugText);
            }
        }

        private float _orientationW = 0;
        public float OrientationW
        {
            get { return _orientationW; }
            set
            {
                _orientationW = value;
                NotifyPropertyChanged();
            }
        }
        private float _orientationX = 0;
        public float OrientationX
        {
            get { return _orientationX; }
            set
            {
                _orientationX = value;
                NotifyPropertyChanged();
            }
        }
        private float _orientationY = 0;
        public float OrientationY
        {
            get { return _orientationY; }
            set
            {
                _orientationY = value;
                NotifyPropertyChanged();
            }
        }
        private float _orientationZ = 0;
        public float OrientationZ
        {
            get { return _orientationZ; }
            set
            {
                _orientationZ = value;
                NotifyPropertyChanged();
            }
        }
        private float _accelerometerX = 0;
        public float AccelerometerX
        {
            get { return _accelerometerX; }
            set
            {
                _accelerometerX = value;
                NotifyPropertyChanged();
            }
        }
        private float _accelerometerY = 0;
        public float AccelerometerY
        {
            get { return _accelerometerY; }
            set
            {
                _accelerometerY = value;
                NotifyPropertyChanged();
            }
        }
        private float _accelerometerZ = 0;
        public float AccelerometerZ
        {
            get { return _accelerometerZ; }
            set
            {
                _accelerometerZ = value;
                NotifyPropertyChanged();
            }
        }
        private float _gyroscopeX = 0;
        public float GyroscopeX
        {
            get { return _gyroscopeX; }
            set
            {
                _gyroscopeX = value;
                NotifyPropertyChanged();
            }
        }
        private float _gyroscopeY = 0;
        public float GyroscopeY
        {
            get { return _gyroscopeY; }
            set
            {
                _gyroscopeY = value;
                NotifyPropertyChanged();
            }
        }
        private float _gyroscopeZ = 0;
        public float GyroscopeZ
        {
            get { return _gyroscopeZ; }
            set
            {
                _gyroscopeZ = value;
                NotifyPropertyChanged();
            }
        }

        private int _gripPressure = 0;
        public int GripPressure
        {
            get { return _gripPressure; }
            set
            {
                if (value != this._gripPressure)
                {
                    _gripPressure = value;
                    NotifyPropertyChanged();
                    //UpdateGripPressure(_gripPressure);
                }
            }
        }

        private string _buttonText = "Start Recording";
        public string ButtonText
        {
            get { return _buttonText; }
            set
            {
                if (value != this._buttonText)
                {
                    _buttonText = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private SolidColorBrush _buttonBrush =new SolidColorBrush(Colors.White);
        public SolidColorBrush ButtonBrush
        {
            get { return _buttonBrush; }
            set
            {
                if (value != this._buttonBrush)
                {
                    _buttonBrush = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private ICommand _buttonClicked;

        public ICommand ButtonClicked
        {
            get
            {
                if (_buttonClicked == null)
                {
                           _buttonClicked = new RelayCommand(
                               param => this.StartRecordingData(),
                               null
                               );  
                }

                return _buttonClicked;
            }
        }

        ConnectorHub.ConnectorHub myConnector;
        ConnectorHub.FeedbackHub myFeedback;

        private double EMGPod_0, EMGPod_1, EMGPod_2, EMGPod_3, EMGPod_4, EMGPod_5, EMGPod_6, EMGPod_7;

        #endregion

        public MyoViewModel()
        {
            myConnector = new ConnectorHub.ConnectorHub();
            myFeedback = new ConnectorHub.FeedbackHub();
            myConnector.init();
            myFeedback.init();
            myConnector.sendReady();
            myConnector.startRecordingEvent += MyConnector_startRecordingEvent;
            myConnector.stopRecordingEvent += MyConnector_stopRecordingEvent;
            myFeedback.feedbackReceivedEvent += MyFeedback_feedbackReceivedEvent;

            myoManager.GripPressureChanged += UpdateGripPressure;
            myoManager.AccelerometerChanged += UpdateAccelerometer;
            myoManager.GyroscopeChanged += UpdateGyroscope;
            myoManager.OrientationChanged += UpdateOrientation;
            myoManager.EMGChanged += UpdateEMG;

            setValueNames();

        }

        private void MyFeedback_feedbackReceivedEvent(object sender, string feedback)
        {
            ReadStream(feedback);
        }

        private void MyConnector_stopRecordingEvent(object sender)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            Globals.IsRecording = true;
                            this.StartRecordingData();
                        }));
        }

        private void MyConnector_startRecordingEvent(object sender)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            Globals.IsRecording = false;
                            this.StartRecordingData();
                        }));
        }

        private void ReadStream(String s)
        {
            if (s.Contains("Myo"))
            {
                MyoManager.pingMyo();

            }

        }

        #region Events handlers

        private void UpdateOrientation(object sender, MyoManager.OrientationChangedEventArgs o)
        {
            OrientationW = o.OrientationW;
            OrientationX = o.OrientationX;
            OrientationY = o.OrientationY;
            OrientationZ = o.OrientationZ;
            if (Globals.IsRecording == true)
            {
                SendDataAsync();
            }
        }

        private void UpdateGyroscope(object sender, MyoManager.GyroscopeChangedEventArgs e)
        {
            GyroscopeX = e.gyroscopeX;
            GyroscopeY = e.gyroscopeY;
            GyroscopeZ = e.gyroscopeZ;
            if (Globals.IsRecording == true)
            {
                SendDataAsync();
            }
        }

        private void UpdateAccelerometer(object sender, MyoManager.AccelerometerChangedEventArgs a)
        {
            AccelerometerX = a.accelerometerX;
            AccelerometerY = a.accelerometerY;
            AccelerometerZ = a.accelerometerZ;
            if (Globals.IsRecording == true)
            {
                SendDataAsync();
            }
        }

        private void UpdateEMG(object sender, MyoManager.EMGChangedEventArgs e)
        {
            EMGPod_0 = e.EMGPod_0;
            EMGPod_1 = e.EMGPod_1;
            EMGPod_2 = e.EMGPod_2;
            EMGPod_3 = e.EMGPod_3;
            EMGPod_4 = e.EMGPod_4;
            EMGPod_5 = e.EMGPod_5;
            EMGPod_6 = e.EMGPod_6;
            EMGPod_7 = e.EMGPod_7;
            if (Globals.IsRecording == true)
            {
                SendDataAsync ();
            }
        }

        private void UpdateGripPressure(object sender, MyoManager.GripPressureChangedEventArgs grip)
        {
            GripPressure = grip.gripPressure;
            Task.Run(() =>
            {
                if (GripPressure >= 4)
                {
                    try
                    {
                        if ((DateTime.Now - Globals.LastExecution).TotalSeconds > 1)
                        {
                            Debug.WriteLine("gripEmg" + GripPressure);
                        }
                        //myConnector.sendFeedback("Read Grip the pen gently");
                        MyoManager.pingMyo();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("feedback not sent" + e.ToString());
                    }

                }
            });
            if (Globals.IsRecording == true)
            {
                SendDataAsync();
            }
        }

        #endregion

        #region UI

        public void StartRecordingData()
        {
            Debug.WriteLine("ButtonClicked" + Globals.IsRecording);
            if (Globals.IsRecording == false)
            {
                Globals.IsRecording = true;
                ButtonText = "Stop Recoding";
                ButtonBrush = new SolidColorBrush(Colors.Green);

            }
            else if (Globals.IsRecording == true)
            {
                Globals.IsRecording = false;
                ButtonText = "Start Recoding";
                ButtonBrush = new SolidColorBrush(Colors.White);
            }
        }

        #endregion

        #region Send data
        private async void setValueNames()
        {
            await Task.Run(() =>
            {
                try
                {
                    List<string> names = new List<string>();
                    names.Add("OrientationW");
                    names.Add("OrientationX");
                    names.Add("OrientationY");
                    names.Add("OrientationZ");
                    names.Add("AccelerometerX");
                    names.Add("AccelerometerY");
                    names.Add("AccelerometerZ");
                    names.Add("GyroscopeX");
                    names.Add("GyroscopeY");
                    names.Add("GyroscopeZ");
                    names.Add("GripPressure");
                    names.Add("EMGPod_0");
                    names.Add("EMGPod_1");
                    names.Add("EMGPod_2");
                    names.Add("EMGPod_3");
                    names.Add("EMGPod_4");
                    names.Add("EMGPod_5");
                    names.Add("EMGPod_6");
                    names.Add("EMGPod_7");

                    myConnector.setValuesName(names);
                }
                catch (Exception ex)
                {
                    if (DebugText != ex.ToString())
                    {
                        DebugText = ex.ToString();
                    }
                }
            });

        }
        /// <summary>
        /// method for sending data Async
        /// </summary>
        public async void SendDataAsync()
        {
            await Task.Run(() => SendData());
        }
        /// <summary>
        /// Method to send the data to the learning hub
        /// </summary>
        private void SendData()
        {
            try
            {
                List<string> values = new List<string>();
                values.Add(OrientationW.ToString());
                values.Add(OrientationX.ToString());
                values.Add(OrientationY.ToString());
                values.Add(OrientationZ.ToString());
                values.Add(AccelerometerX.ToString());
                values.Add(AccelerometerY.ToString());
                values.Add(AccelerometerZ.ToString());
                values.Add(GyroscopeX.ToString());
                values.Add(GyroscopeY.ToString());
                values.Add(GyroscopeZ.ToString());
                values.Add(GripPressure.ToString());
                values.Add(EMGPod_0.ToString());
                values.Add(EMGPod_1.ToString());
                values.Add(EMGPod_2.ToString());
                values.Add(EMGPod_3.ToString());
                values.Add(EMGPod_4.ToString());
                values.Add(EMGPod_5.ToString());
                values.Add(EMGPod_6.ToString());
                values.Add(EMGPod_7.ToString());
                myConnector.storeFrame(values);
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(ex.StackTrace);
                if (DebugText != ex.ToString())
                {
                    DebugText = ex.ToString();
                }
            }

        }
        #endregion

        #region filterData
        /// <summary>
        /// This function returns the data filtered. Converted to C# 2 July 2014.
        /// Original source written in VBA for Microsoft Excel, 2000 by Sam Van
        /// Wassenbergh (University of Antwerp), 6 june 2007.
        /// </summary>
        /// <param name="indata"></param>
        /// <param name="deltaTimeinsec"></param>
        /// <param name="CutOff"></param>
        /// <returns></returns>
        public static double[] Butterworth(double[] indata, double deltaTimeinsec, double CutOff)
        {
            if (indata == null) return null;
            if (CutOff == 0) return indata;

            double Samplingrate = 1 / deltaTimeinsec;
            long dF2 = indata.Length - 1;        // The data range is set with dF2
            double[] Dat2 = new double[dF2 + 4]; // Array with 4 extra points front and back
            double[] data = indata; // Ptr., changes passed data

            // Copy indata to Dat2
            for (long r = 0; r < dF2; r++)
            {
                Dat2[2 + r] = indata[r];
            }
            Dat2[1] = Dat2[0] = indata[0];
            Dat2[dF2 + 3] = Dat2[dF2 + 2] = indata[dF2];

            const double pi = 3.14159265358979;
            double wc = Math.Tan(CutOff * pi / Samplingrate);
            double k1 = 1.414213562 * wc; // Sqrt(2) * wc
            double k2 = wc * wc;
            double a = k2 / (1 + k1 + k2);
            double b = 2 * a;
            double c = a;
            double k3 = b / k2;
            double d = -2 * a + k3;
            double e = 1 - (2 * a) - k3;

            // RECURSIVE TRIGGERS - ENABLE filter is performed (first, last points constant)
            double[] DatYt = new double[dF2 + 4];
            DatYt[1] = DatYt[0] = indata[0];
            for (long s = 2; s < dF2 + 2; s++)
            {
                DatYt[s] = a * Dat2[s] + b * Dat2[s - 1] + c * Dat2[s - 2]
                           + d * DatYt[s - 1] + e * DatYt[s - 2];
            }
            DatYt[dF2 + 3] = DatYt[dF2 + 2] = DatYt[dF2 + 1];

            // FORWARD filter
            double[] DatZt = new double[dF2 + 2];
            DatZt[dF2] = DatYt[dF2 + 2];
            DatZt[dF2 + 1] = DatYt[dF2 + 3];
            for (long t = -dF2 + 1; t <= 0; t++)
            {
                DatZt[-t] = a * DatYt[-t + 2] + b * DatYt[-t + 3] + c * DatYt[-t + 4]
                            + d * DatZt[-t + 1] + e * DatZt[-t + 2];
            }

            // Calculated points copied for return
            for (long p = 0; p < dF2; p++)
            {
                data[p] = DatZt[p];
            }

            return data;
        }
        #endregion

    }
}
