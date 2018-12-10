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

        private DateTime TimerStart { get; set; }
        #endregion

        public MyoViewModel()
        {
            this.TimerStart = DateTime.Now;
            myConnector = new ConnectorHub.ConnectorHub();
            myFeedback = new ConnectorHub.FeedbackHub();
            myConnector.init();
            myFeedback.init();
            myConnector.sendReady();
            myConnector.startRecordingEvent += MyConnector_startRecordingEvent;
            myConnector.stopRecordingEvent += MyConnector_stopRecordingEvent;
            myFeedback.feedbackReceivedEvent += MyFeedback_feedbackReceivedEvent;

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
                MyoManager.PingMyo();

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

        /// <summary>
        /// Temporary holders for emg data to be stored 
        /// </summary>
        List<double> EMGPod0data = new List<double>();
        List<double> EMGPod1data = new List<double>();
        List<double> EMGPod2data = new List<double>();
        List<double> EMGPod3data = new List<double>();
        List<double> EMGPod4data = new List<double>();
        List<double> EMGPod5data = new List<double>();
        List<double> EMGPod6data = new List<double>();
        List<double> EMGPod7data = new List<double>();
        /// <summary>
        /// holder for average emg data of each second
        /// </summary>
        double[] EMGdata = new double[8];

        private void UpdateEMG(object sender, MyoManager.EMGChangedEventArgs e)
        {
            EMGPod0data.Add(e.EMGPod_0);
            EMGPod1data.Add(e.EMGPod_1);
            EMGPod2data.Add(e.EMGPod_2);
            EMGPod3data.Add(e.EMGPod_3);
            EMGPod4data.Add(e.EMGPod_4);
            EMGPod5data.Add(e.EMGPod_5);
            EMGPod6data.Add(e.EMGPod_6);
            EMGPod7data.Add(e.EMGPod_7);

            if((DateTime.Now - TimerStart).Seconds > 0.5)
            {
                EMGdata[0] = RootMeanSquare(EMGPod0data.ToArray());
                EMGdata[1] = RootMeanSquare(EMGPod1data.ToArray());
                EMGdata[2] = RootMeanSquare(EMGPod2data.ToArray());
                EMGdata[3] = RootMeanSquare(EMGPod3data.ToArray());
                EMGdata[4] = RootMeanSquare(EMGPod4data.ToArray());
                EMGdata[5] = RootMeanSquare(EMGPod5data.ToArray());
                EMGdata[6] = RootMeanSquare(EMGPod6data.ToArray());
                EMGdata[7] = RootMeanSquare(EMGPod7data.ToArray());

                CalculateGripPressureAsync(EMGdata, 15);

                for(int i = 0; i < EMGdata.Count()-1; i++)
                {
                    Debug.WriteLine("EMGdata " + i + " = " + EMGdata[i]);
                }

                if (Globals.IsRecording == true)
                {
                    SendDataAsync();
                }

                TimerStart = DateTime.Now;
                EMGPod0data.Clear();
                EMGPod1data.Clear();
                EMGPod2data.Clear();
                EMGPod3data.Clear();
                EMGPod4data.Clear();
                EMGPod5data.Clear();
                EMGPod6data.Clear();
                EMGPod7data.Clear();
            }

        }

        /// <summary>
        /// calculate the RootMeanSquare from the array
        /// </summary>
        /// <param name="doubleList"></param>
        /// <returns></returns>
        private double RootMeanSquare(double[] doubleList)
        {
            double sum = 0;
            for (int i = 0; i < doubleList.Length; i++)
            {
                sum += (doubleList[i] * doubleList[i]);
            }
            return Math.Sqrt(sum / doubleList.Length);
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
                values.Add(EMGdata[0].ToString());
                values.Add(EMGdata[1].ToString());
                values.Add(EMGdata[2].ToString());
                values.Add(EMGdata[3].ToString());
                values.Add(EMGdata[4].ToString());
                values.Add(EMGdata[5].ToString());
                values.Add(EMGdata[6].ToString());
                values.Add(EMGdata[7].ToString());
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

        private async void CalculateGripPressureAsync(double[] currentEmgValues, double emgThreshold)
        {
            await Task.Run(() => CalculateGripPressure(currentEmgValues, emgThreshold));
        }

        /// <summary>
        /// Iterate through each emg sensor in myo and assign 1 if the sum of the first and second frame of emg has a sum of more than 20.
        /// else assign 0. It means that much variation(100 to -100) was observed propotional to higher tension in muscle. 
        /// </summary>
        /// <param name="e"></param>
        private void CalculateGripPressure(double[] currentEmgValues, double emgThreshold)
        {
            int[] emgTension = new int[8];
            int gripEMG = 0;

            //iterate through all the sensors and store the 1/0  in emg tension depending if the sum of previous frame of data and current frame is less than threshold
            // 0 meaning no tension and 100 meaning lots of tension
            for (int i = 0; i <= 7; i++)
            {
                try
                {
                    if (currentEmgValues[i] >= emgThreshold)
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
                    Debug.WriteLine("Error Calculating GripPressure");
                }
            }

            //add all value from emgTension and assign it to gripEmg
            Array.ForEach(emgTension, delegate (int i) { gripEMG += i; });
            GripPressure = gripEMG;
            if (gripEMG >=5)
            {
                MyoManager.PingMyo();
                Debug.WriteLine("GripPressure = " + gripEMG);
            }
        }

    }
}
