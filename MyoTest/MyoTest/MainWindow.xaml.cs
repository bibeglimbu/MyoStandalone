using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace MyoTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        MyoManager.MyoManager myoManager = new MyoManager.MyoManager();
        //bool value for switching the record button text and the color
        public static bool isRecordingData = false;

        public MainWindow()
        {
            InitializeComponent();
            myoManager.InitMyoManagerHub(this);
            MyoManager.MyoManager.myConnector.startRecordingEvent += MyConnector_startRecordingEvent;
            MyoManager.MyoManager.myConnector.stopRecordingEvent += MyConnector_stopRecordingEvent;

        }

        private void MyConnector_stopRecordingEvent(object sender)
        {
            myoManager.IsRecording = false;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            StartRecordingData();
                        }));
        }

        private void MyConnector_startRecordingEvent(object sender)
        {
            myoManager.IsRecording = true;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            StartRecordingData();
                        }));
        }

        /// <summary>
        /// Method to update the grip textbox and assign the value to gripPressure var
        /// </summary>
        /// <param name="g"></param>
        public void UpdateGripPressure(Int32 g)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            GripTxt.Text = g.ToString();
                        }));
        }

        /// <summary>
        /// Method to update the orientation textbox and assign the value of orientation
        /// </summary>
        /// <param name="ww"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void UpdateOrientation(float w, float x, float y, float z)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            OrientationTxt.Text = w.ToString()+" "+x.ToString()+" "+y.ToString() + " " + z.ToString(); ;
                        }));
        }

        public void UpdateDebug(String s)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            DebugTxt.Text = s;
                        }));
        }

        private void RecordingButton_Click(object sender, RoutedEventArgs e)
        {
            StartRecordingData();
        }

        public void StartRecordingData()
        {
            if (isRecordingData == false)
            {
                isRecordingData = true;
                RecordingButton.Content = "Stop Recording";
                RecordingButton.Background = new SolidColorBrush(Colors.Green);

            }
            else if (isRecordingData == true)
            {
                isRecordingData = false;
                RecordingButton.Content = "Start Recording";
                RecordingButton.Background = new SolidColorBrush(Colors.White);
            }
            Debug.WriteLine("isRecordingData= " + isRecordingData);
        }
    }
}
