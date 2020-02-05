using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ServerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ServerPipe _serverPipe;

        public ServerPipe ServerPipe
        {
            get { return _serverPipe; }
            set
            {
                _serverPipe = value;
                ServerStartButton.IsEnabled = _serverPipe == null ? true : false;
                ServerStopButton.IsEnabled = !ServerStartButton.IsEnabled;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ServerStartButton_Click(object sender, RoutedEventArgs e)
        {
            ServerPipe = new ServerPipe(this);
            ServerPipe.ServerClosed += ServerPipe_ServerClosed;
            ServerPipe.ClientDisconnected += ServerPipe_ClientDisconnected;
        }

        private void ServerPipe_ClientDisconnected(object sender, EventArgs e)
        {
            ServerStopButton.IsEnabled = false;
        }

        private void ServerPipe_ServerClosed(object sender, EventArgs e)
        {
            ServerPipe.ServerClosed -= ServerPipe_ServerClosed;
            ServerPipe.ServerClosed -= ServerPipe_ServerClosed;
            ServerPipe = null;
        }

        private void ServerStopButton_Click(object sender, RoutedEventArgs e)
        {
            ServerPipe?.StopServer();
            ServerStopButton.IsEnabled = false;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ServerPipe?.SendMessage(string.Copy(UserInput.Text));
            UserInput.Text = "";
        }
    }
}
