using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MyoSharp.Communication;
using MyoSharp.Device;
using MyoSharp.Exceptions;
using MyoSharp.Poses;
using System.Diagnostics;
using System.Windows.Threading;
using System.Net.Sockets;
using System.Net;

namespace MyoTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Socket sending_socket;
        IPAddress send_to_address;
        MyoManager.MyoManagerClass myoManager = new MyoManager.MyoManagerClass();

        public MainWindow()
        {
            InitializeComponent();
            myoManager.InitMyoManagerHub(this);
            
        }

        public void SendData(Int32 emg)
        {

            sending_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            send_to_address = IPAddress.Parse("127.0.0.1");
            IPEndPoint sending_end_point = new IPEndPoint(send_to_address, 11002);

            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
            socketEventArg.RemoteEndPoint = sending_end_point;

            byte[] send_buffer = Encoding.UTF8.GetBytes(emg.ToString());

            try
            {
                socketEventArg.SetBuffer(send_buffer, 0, send_buffer.Length);
                sending_socket.SendToAsync(socketEventArg);
                Debug.WriteLine("text sent");
            }
            catch
            {
                Debug.WriteLine("not initialized");
            }
        }

        public void UpdateEmg(Int32 emg)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            EmgTxt.Text =emg.ToString();
                        }));
            SendData(emg);
        }
    }
}
