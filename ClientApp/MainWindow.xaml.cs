using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClientApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ClientPipe _clientPipe;
        public ClientPipe ClientPipe
        {
            get { return _clientPipe; }
            set
            {
                _clientPipe = value;
                ClientStartButton.IsEnabled = _clientPipe == null ? true : false;
                ClientStopButton.IsEnabled = !ClientStartButton.IsEnabled;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ClientPipe?.SendMessage(string.Copy(UserInput.Text));
            UserInput.Text = "";
        }

        private void ClientStartButton_Click(object sender, RoutedEventArgs e)
        {
            ClientPipe = new ClientPipe(this);
            ClientPipe.ClientClosed += ClientPipe_ClientClosed;
        }

        private void ClientPipe_ClientClosed(object sender, EventArgs e)
        {
            ClientPipe.ClientClosed -= ClientPipe_ClientClosed;
            ClientPipe = null;
        }

        private void ClientStopButton_Click(object sender, RoutedEventArgs e)
        {
            ClientPipe?.StopClient();
            ClientStopButton.IsEnabled = false;
        }
    }
}
