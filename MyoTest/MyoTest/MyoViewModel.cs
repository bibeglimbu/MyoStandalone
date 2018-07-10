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

        private string _debugText = "";
        public string DebugText
        {
            get { return _debugText; }
            set
            {
                if (value != this._debugText)
                {
                    _debugText = value;
                    NotifyPropertyChanged();
                    //UpdateDebug(_debugText);
                }
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

        DispatcherTimer Dtimer = new DispatcherTimer();

        //private bool _isRecording = false;
        //public bool IsRecording
        //{
        //    get { return _isRecording; }
        //    set
        //    {
        //        _isRecording = value;
        //        NotifyPropertyChanged();
        //    }
        //}

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

            Dtimer.Interval = new TimeSpan(100);
            Dtimer.Tick += Dtimer_Tick;
            Dtimer.Start();

            try
            {
                setValueNames();
            }
            catch (Exception e)
            {
                DebugText = "MyoManager error at connecting the hub";
            }
        }

        private void Dtimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (Globals.IsRecording == true)
                {
                    SendData();
                }
                
            }
            catch
            {
                DebugText = "Failed to send data";
            }
           
        }

        private void MyFeedback_feedbackReceivedEvent(object sender, string feedback)
        {
            ReadStream(feedback);
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
        }

        private void UpdateGyroscope(object sender, MyoManager.GyroscopeChangedEventArgs e)
        {
            GyroscopeX = e.gyroscopeX;
            GyroscopeY = e.gyroscopeY;
            GyroscopeZ = e.gyroscopeZ;
        }

        private void UpdateGripPressure(object sender, MyoManager.GripPressureChangedEventArgs grip)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
            () =>
            {
                GripPressure = grip.gripPressure;
                //vibrate only twice a sec

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
            }));
        }

        private void UpdateAccelerometer(object sender, MyoManager.AccelerometerChangedEventArgs a)
        {
            AccelerometerX = a.accelerometerX;
            AccelerometerY = a.accelerometerY;
            AccelerometerZ = a.accelerometerZ;

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
        private void setValueNames()
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
                myConnector.setValuesName(names);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }

        }

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
                myConnector.storeFrame(values);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }

        }
        #endregion

    }
}
