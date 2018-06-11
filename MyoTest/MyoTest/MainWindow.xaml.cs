using System;
using System.Windows;
using System.Windows.Threading;


namespace MyoHub
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {

        }

        #region Update UI
        /// <summary>
        /// Method to update the grip textbox and assign the value to gripPressure var
        /// </summary>
        /// <param name="g"></param>
        public void UpdateGripPressure(int pressure)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            GripTxt.Text = pressure.ToString();
                        }));
        }

        /// <summary>
        /// Method to update the Acceleration textbox and assign the value of orientation
        /// </summary>
        public void UpdateAcceleration(float acceleration)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            AccelerationTxt.Text = acceleration.ToString();
                        }));
        }

        /// <summary>
        /// Method to update the debug text area
        /// </summary>
        /// <param name="s"></param>
        public void UpdateDebug(String s)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            DebugTxt.Text = s;
                        }));
        }

        #endregion

    }
}
