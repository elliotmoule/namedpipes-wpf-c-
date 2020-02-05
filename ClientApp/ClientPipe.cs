using Shared.Utilities;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Windows;

namespace ClientApp
{
    public class ClientPipe
    {
        private MainWindow _parent;
        private NamedPipeClientStream _clientPipe;
        private StreamString _streamString;
        private bool _serverClose;
        public event EventHandler ClientClosed;

        public ClientPipe(MainWindow parent)
        {
            this._parent = parent;
            _serverClose = false;
            StartClient();
        }

        private void OnClientClosed(EventArgs e)
        {
            EventHandler handler = ClientClosed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public bool SendMessage(string message)
        {
            if (_clientPipe != null && _clientPipe.IsConnected)
            {
                PipeUtilities.SendPipedMessage(_streamString, message);
                return true;
            }
            return false;
        }

        public void StopClient()
        {
            if (_clientPipe != null && _clientPipe.IsConnected)
            {
                _clientPipe.Close();
                _serverClose = true;
            }
        }

        private void StartClient()
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    RunClient();
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void RunClient()
        {
            try
            {
                _clientPipe = new NamedPipeClientStream(".", Constants.PipeName, Constants.PipeDirection, Constants.PipeOptions);
                _clientPipe.Connect();
                _clientPipe.Flush();
                _streamString = new StreamString(_clientPipe);

                Application.Current.Dispatcher.Invoke(() =>
                 {
                     _parent.TextArea.Text = "";
                 });

                do
                {
                    if (_clientPipe != null && _clientPipe.IsConnected)
                    {
                        string line = _streamString.ReadString();

                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            if (line == Constants.DisconnectKeyword)
                            {
                                SendMessage(Constants.DisconnectKeyword);
                            }
                            else
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    _parent.TextArea.Text = line;
                                });
                            }
                        }
                        else
                        {
                            _serverClose = true;
                        }
                    }

                } while (!_serverClose);

                _clientPipe.Close();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnClientClosed(EventArgs.Empty);
                });
            }
            catch (IOException)
            {
                _serverClose = true;
                _clientPipe.Flush();
                _clientPipe.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
