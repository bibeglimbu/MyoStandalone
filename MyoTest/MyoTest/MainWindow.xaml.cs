using System;
using System.Windows;
using System.Windows.Threading;

namespace MyoTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        MyoManager.MyoManager myoManager = new MyoManager.MyoManager();

        public MainWindow()
        {
            InitializeComponent();
            myoManager.InitMyoManagerHub(this);
            
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
    }
}
