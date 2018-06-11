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

        private float _acceleration = 0.0f;
        public float Acceleration
        {
            get { return _acceleration; }
            set
            {
                if (value != this._acceleration)
                {
                    _acceleration = value;
                    NotifyPropertyChanged();
                    //UpdateAcceleration(_acceleration);
                }
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

        #endregion

        public MyoViewModel()
        {
            HubConnector.StartConnection();
            HubConnector.myConnector.startRecordingEvent += MyConnector_startRecordingEvent;
            HubConnector.myConnector.stopRecordingEvent += MyConnector_stopRecordingEvent;

            myoManager.GripPressureChanged += UpdateGripPressure;
            myoManager.AccelerometerChanged += UpdateAccelerometer;

            try
            {
                setValueNames();
            }
            catch (Exception e)
            {
                DebugText = "MyoManager error at connecting the hub";
            }
        }

        private void UpdateGripPressure(object sender, MyoManager.GripPressureChangedEventArgs grip)
        {
            GripPressure = grip.gripPressure;
        }

        private void UpdateAccelerometer(object sender, MyoManager.AccelerometerChangedEventArgs acceleration)
        {
            Acceleration = acceleration.accelerometerMag;

            try
            {
                if (Globals.IsRecording == true)
                {
                    SendData();
                }
            }
            catch
            {
                DebugText = "Error at MyoViewModel/UpdateAccelerometer- couldnt send data";
            }
        }
        #region Events

        private void MyConnector_stopRecordingEvent(object sender)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            StartRecordingData();
                        }));
        }

        private void MyConnector_startRecordingEvent(object sender)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            StartRecordingData();
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
                names.Add("GripPressure");
                names.Add("Acceleration");
                HubConnector.myConnector.setValuesName(names);
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
                values.Add(GripPressure.ToString());
                values.Add(Acceleration.ToString());
                HubConnector.myConnector.storeFrame(values);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }

        }
        #endregion
    }
}
