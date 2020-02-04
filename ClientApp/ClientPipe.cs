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
        private StreamWriter _writer;
        private StreamReader _reader;
        private bool _close;
        public event EventHandler ClientClosed;

        public ClientPipe(MainWindow parent)
        {
            this._parent = parent;
            _close = false;
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
                PipeUtilities.SendPipedMessage(_writer, message);
                return true;
            }
            return false;
        }

        public void StopClient()
        {
            _close = true;
        }

        private void StartClient()
        {
            try
            {
                Task.Factory.StartNew(async () =>
                {
                    await RunClientAsync();
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task RunClientAsync()
        {
            try
            {
                _clientPipe = new NamedPipeClientStream(".", Constants.PipeName, Constants.PipeDirection, Constants.PipeOptions);
                _clientPipe.Connect();
                _clientPipe.Flush();

                _writer = new StreamWriter(_clientPipe);
                _writer.AutoFlush = true;
                _reader = new StreamReader(_clientPipe);

                Application.Current.Dispatcher.Invoke(() =>
                 {
                     _parent.TextArea.Text = "";
                 });

                do
                {
                    if (_clientPipe != null && _clientPipe.IsConnected)
                    {
                        string line = await _reader.ReadLineAsync();

                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            if (line == "end")
                            {
                                SendMessage("end");
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
                            _close = true;
                        }
                    }

                } while (!_close);

                _clientPipe.Close();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnClientClosed(EventArgs.Empty);
                });
            }
            catch (IOException)
            {
                _close = true;
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
