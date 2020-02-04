using Shared.Utilities;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Windows;

namespace ServerApp
{
    public class ServerPipe
    {
        private MainWindow _parent;
        private StreamWriter _writer;
        private StreamReader _reader;
        private NamedPipeServerStream _serverPipe;
        public event EventHandler ServerClosed;

        public ServerPipe(MainWindow parent)
        {
            this._parent = parent;
            StartServer();
        }

        private void OnServerClosed(EventArgs e)
        {
            EventHandler handler = ServerClosed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public bool SendMessage(string message)
        {
            if (_serverPipe != null && _serverPipe.IsConnected)
            {
                PipeUtilities.SendPipedMessage(_writer, message);
                return true;
            }
            return false;
        }

        public void StopServer()
        {
            SendMessage("end");
        }

        private void StartServer()
        {
            try
            {
                PipeSecurity security = new PipeSecurity();
                security.AddAccessRule(
                    new PipeAccessRule($"{Environment.UserDomainName}\\{Environment.UserName}",
                    PipeAccessRights.ReadWrite,
                    System.Security.AccessControl.AccessControlType.Allow));
                _serverPipe = new NamedPipeServerStream(
                    Constants.PipeName,
                    Constants.PipeDirection,
                    1,
                    Constants.PipeTransmissionMode,
                    Constants.PipeOptions,
                    4096,
                    4096,
                    security);

                Task.Factory.StartNew(async () =>
                {
                    await RunServerAsync();
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task RunServerAsync()
        {
            try
            {
                if (_serverPipe != null)
                {
                    await Task.Factory.FromAsync(
                                                (cb, state) => _serverPipe.BeginWaitForConnection(cb, state),
                                                ar => _serverPipe.EndWaitForConnection(ar),
                                                null);

                    _writer = new StreamWriter(_serverPipe);
                    _writer.AutoFlush = true;
                    _reader = new StreamReader(_serverPipe);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _parent.TextArea.Text = "";
                    });

                    SendMessage("initialised");

                    while (true)
                    {
                        if (_serverPipe != null && _serverPipe.IsConnected)
                        {
                            string line = string.Empty;
                            try
                            {
                                line = await _reader.ReadLineAsync();
                            }
                            catch (ObjectDisposedException ex)
                            {
                                Console.WriteLine(ex);
                            }

                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                if (line == "end")
                                {
                                    break;
                                }
                                else
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        _parent.TextArea.Text = line;
                                    });
                                }
                            }
                            else if (string.IsNullOrWhiteSpace(line))
                            {
                                break;
                            }
                        }
                        else if (!_serverPipe.IsConnected)
                        {
                            Console.WriteLine("Client disconnected");
                            break;
                        }
                    }

                    PipeFlushClose(_serverPipe);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        OnServerClosed(EventArgs.Empty);
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void PipeFlushClose(NamedPipeServerStream namedPipeServerStream)
        {
            try
            {
                if (namedPipeServerStream != null)
                {
                    if (!namedPipeServerStream.IsConnected)
                    {
                        using (NamedPipeClientStream npcs = new NamedPipeClientStream(Constants.PipeName))
                        {
                            npcs.Connect(100);
                        }
                    }
                    namedPipeServerStream.Flush();
                    namedPipeServerStream.Close();

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
