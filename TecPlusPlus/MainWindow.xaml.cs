using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.Win32;

namespace TecPlusPlus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Socket _sockClient;
        private IPAddress _ipAddress;
        private IPEndPoint _ipEndPoint;
        private IAsyncResult _asyncResult;
        private AsyncCallback _asyncCallback;
        //private Byte[] _buffer = new byte[1024];
        private bool _isSocketConnected;
        
        private Byte[] _hexToSend;
        
        private readonly List<string> _commandHistory;
        private Int32 _currentCommandHistoryIndex;

        private String _lastCommandEntered;
        private String _lastDataReceived;

        private String _appendData;
        private readonly Object _cmdHistLock = new Object();

        private readonly StreamWriter _gameLogWriter;
        private readonly StreamWriter _errorLogWriter;
        private MudCfg mudCfg;

//        [Obsolete]
        public MainWindow()
        {
            InitializeComponent();
            Unloaded += new RoutedEventHandler(MainWindow_Unloaded);
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            
            _commandHistory = new List<string>();
            _lastCommandEntered = String.Empty;
            _currentCommandHistoryIndex = 0;           

            txtInput.Focus();

            _gameLogWriter = new StreamWriter(new FileStream("datalog.txt", FileMode.Create, FileAccess.Write, FileShare.Read));
            _errorLogWriter = new StreamWriter(new FileStream("errorlog.txt", FileMode.Create, FileAccess.Write, FileShare.Read));

            txtOutput.Background = new SolidColorBrush(Colors.Black);
        }

//        [Obsolete]
        private bool LoadMudCfg(string filename,ref MudCfg cfg)
        {
            try
            {
                string text = System.IO.File.ReadAllText(filename);
                cfg = JsonConvert.DeserializeObject<MudCfg>(text);
                return true;
            }
            catch (Exception ex)
            {
                var s = string.Format("Error occured in LoadMudCfg: {0}", ex.Message);
                txtOutput.AppendText(s);
                _errorLogWriter.WriteLine(s);
                return false;
            }
        }

//        [Obsolete]
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isSocketConnected == false)
            {
                _hexToSend = StringToByteArray("0d0a");
            }
        }

        void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_isSocketConnected == true)
            {
                CloseSocket();
            }
        }

//        [Obsolete]
        public void OpenConnection(MudCfg cfg)
        {
            try
            {
                _sockClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPHostEntry ipHostInfo = Dns.GetHostEntry(cfg.Address);
                _ipAddress = ipHostInfo.AddressList[0];
                if (_ipAddress != null) _ipEndPoint = new IPEndPoint(_ipAddress, cfg.Port);
                _sockClient.Connect(_ipEndPoint);

                if (_sockClient.Connected)
                {
                    _isSocketConnected = true;

                    txtOutput.AppendText("Connected at: " + Convert.ToString(DateTime.Now) + "...");
                    this.Title = "TEC++  [ Connected ]";

                    //Login
                    ProcessSendData("/\\/Connect: n/a!!n/a");

                    WaitForData();
                }
                else
                {
                    txtOutput.AppendText("Connection unsuccessful");
                }
            }
            catch (Exception ex)
            {
                var s = string.Format("Error occured in OpenConnection: {0}", ex.Message);
                txtOutput.AppendText(s);
                _errorLogWriter.WriteLine(s);
            }
        }

        public void CloseSocket()
        {
            if (_sockClient != null)
            {
                _sockClient.Close();
                _sockClient = null;

                txtOutput.AppendText("\r\nConnection closed at: " + Convert.ToString(DateTime.Now) + "...\r\n");
                this.Title = "TEC++ [ Disconnected ]";
            }
        }

        public void WaitForData()
        {
            try
            {
                if (_asyncCallback == null)
                {
                    _asyncCallback = new AsyncCallback(OnDataReceived);
                }

                byte[] buffer = new byte[1024];
                // Start listening for data...
                if(_sockClient != null)
                {
                    _asyncResult = _sockClient.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, _asyncCallback, buffer);
                }
            }
            catch (Exception ex)
            {
                _errorLogWriter.WriteLine(string.Format("Error occured in WaitForData: {0}", ex.Message));
            }
        }

        public void OnDataReceived(IAsyncResult ar)
        {
            try
            {
                //end receive...
                int iRx = 0;
                if(_sockClient != null)
                {
                    iRx = _sockClient.EndReceive(ar);
                }
                byte[] buffer = (byte[])ar.AsyncState;
                //string data = System.Text.Encoding.UTF8.GetString(buffer,0,iRx);
                //string data = System.Text.Encoding.GetEncoding("gb2312").GetString(buffer, 0, iRx);
                string data = System.Text.Encoding.GetEncoding(mudCfg.Encode).GetString(buffer, 0, iRx);
                ProcessReceivedData(data);
                                
                WaitForData();
            }
            catch (Exception ex)
            {
                _errorLogWriter.WriteLine(string.Format("Error occured in OnDataReceived: {0}", ex.Message)); 
            }
        }

        public void ProcessReceivedData(string data)
        {
            _gameLogWriter.Write(data);
            _lastDataReceived = data;
           Regex regx = new Regex("[A-Za-z0-9 ,\'\"><\\[\\]\\=\\-\\u001B]");
           if (!regx.IsMatch(data))
           {
               Debug.WriteLine("Found unhandled character.");
           }
           
            String[] receivedLines = _lastDataReceived.Split(new string[] {"\r\n","\n\r"},StringSplitOptions.None);
            if (_appendData != null && receivedLines.Length > 0)
            {
                receivedLines[0] = receivedLines[0].Insert(0, _appendData);
                _appendData = null;
            }
            if (!_lastDataReceived.EndsWith("\r\n") || !_lastDataReceived.EndsWith("\n\r"))
            {
                _appendData = receivedLines.LastOrDefault();
                receivedLines = receivedLines.Take(receivedLines.Length - 1).ToArray();
            }
            foreach (string line in receivedLines)
            {  
                ProcessReceivedLine(line);
            }

        }

        public void ProcessReceivedLine(string data)
        {
            if (data.StartsWith(@"/\/"))
            {
                ProcessCommandData(data);
                return;
            }

            txtOutput.Dispatcher.Invoke(DispatcherPriority.Send, (Action)(() => 
            {
                string[] param = data.Split(new char[] {(char)27});

                Color textColor = Colors.White;
                Paragraph textParagraph = new Paragraph();
                textParagraph.Margin = new Thickness(0);

                foreach (string p in param)
                {
                    if (p.Contains("ci="))
                    {
                        string[] rgb = p.Split('=')[1].Split(',');
                        textColor = Color.FromRgb(byte.Parse(rgb[0]), byte.Parse(rgb[1]), byte.Parse(rgb[2]));
                    }
                    else
                    {
                        AnsiColor ansiColor = new AnsiColor();
                        int pos = AnsiColor.Parse(p, ref ansiColor);
                        if(pos > 0)
                        {
                            if(ansiColor.Ground == AnsiColor.EnumGround.Forground)
                            {
                                textColor = ansiColor.ColorValue;
                            }
                        }
                        Run textRun = new Run(p.Substring(pos));
                        textRun.Foreground = new SolidColorBrush(textColor);
                        if(pos > 0 && ansiColor.Ground == AnsiColor.EnumGround.Background)
                        {
                            textRun.Background = new SolidColorBrush(ansiColor.ColorValue);
                        }
                        textParagraph.Inlines.Add(textRun);
                    }
                }
                txtOutput.Document.Blocks.Add(textParagraph);
                
                txtOutput.ScrollToEnd(); 
                 
            }));
        }

        public void ProcessCommandData(string data)
        {

        }

        public void TxtInputKeyDown(object sender, KeyEventArgs e)
        {
            lock (_cmdHistLock)
            {
                if (e.Key == Key.Enter)
                {
                    _currentCommandHistoryIndex = -1;
                    _lastCommandEntered = txtInput.Text;
                    ProcessSendData(_lastCommandEntered);
                    if (_commandHistory.Count != 0)
                    {
                        int tempCmHistCount;
                        if ((tempCmHistCount = _commandHistory.IndexOf(_lastCommandEntered)) != -1)
                        {
                            string item = _commandHistory[tempCmHistCount];
                            _commandHistory.RemoveAt(tempCmHistCount);
                            _commandHistory.Insert(0, item);
                        }
                        else
                        {
                            _commandHistory.Insert(0, _lastCommandEntered);
                        }
                    }
                    else
                    {
                        _commandHistory.Insert(0, _lastCommandEntered);
                    }

                    txtInput.Clear();
                }
                if (e.Key == Key.Up)
                {
                    if (_currentCommandHistoryIndex <= _commandHistory.Count - 2)
                    {
                        //Debug.WriteLine(String.Format("Command History {0} of {1}", _currentCommandHistoryIndex, _commandHistory.Count));
                        txtInput.Text = _commandHistory[++_currentCommandHistoryIndex];
                    }
                }
                else if (e.Key == Key.Down)
                {
                    if (_currentCommandHistoryIndex > 0)
                        txtInput.Text = _commandHistory[--_currentCommandHistoryIndex];
                }
            }
        }

        public void ProcessSendData(string input)
        {
            try
            {
                //byte[] byteData = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] byteData = System.Text.Encoding.GetEncoding(mudCfg.Encode).GetBytes(input);

                _sockClient.Send(ConcatenateByteArrays(byteData, _hexToSend));

            }
            catch (SocketException ex)
            {
                _errorLogWriter.WriteLine(string.Format("SocketException error occured in ProcessSendData: {0}", ex.Message));
            }
        }

        public static byte[] ConcatenateByteArrays(byte[] byteAry1, byte[] byteAry2)
        {
            byte[] merged = new byte[byteAry1.Length + byteAry2.Length];
            System.Buffer.BlockCopy(byteAry1, 0, merged, 0, byteAry1.Length);
            System.Buffer.BlockCopy(byteAry2, 0, merged, byteAry1.Length, byteAry2.Length);
            return merged;
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length).
                   Where(x => 0 == x % 2).
                   Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).
                   ToArray();
        }

#region MenuEvents 

        private void MnuFileClick(object sender, RoutedEventArgs e)
        {

        }

        private void MnuNewCfgClick(object sender, RoutedEventArgs e)
        {
            MudCfgBaseEditDlg dlg = new MudCfgBaseEditDlg(new MudCfg(), "");
            if (dlg.ShowDialog() == true)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "Mud配置|*.mudcfg|所有文件|*.*";
                if (dialog.ShowDialog() == true)
                {
                    string data = JsonConvert.SerializeObject(dlg.mudCfg);
                    File.WriteAllText(dialog.FileName, data);
                }
            }
        }

        private void MnuEditCfgClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Mud配置|*.mudcfg|所有文件|*.*";
            if (dialog.ShowDialog() == true)
            {
                MudCfg cfg = new MudCfg();
                if (LoadMudCfg(dialog.FileName, ref cfg))
                {
                    MudCfgBaseEditDlg dlg = new MudCfgBaseEditDlg(cfg, dialog.FileName);
                    if (dlg.ShowDialog() == true)
                    {
                        string data = JsonConvert.SerializeObject(dlg.mudCfg);
                        File.WriteAllText(dialog.FileName, data);
                    }
                }
            }
        }
        private void MnuConnectClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Mud配置|*.mudcfg|所有文件|*.*";
            if(dialog.ShowDialog() == true)
            {
                if (LoadMudCfg(dialog.FileName, ref mudCfg))
                {
                    CloseSocket();
                    OpenConnection(mudCfg);
                }
            }

        }

        private void MnuDisconnectClick(object sender, RoutedEventArgs e)
        {
            CloseSocket();
        }

        private void MnuExitClick(object sender, RoutedEventArgs e)
        {

        }

        private void MnuEditClick(object sender, RoutedEventArgs e)
        {

        }

        private void MnuScriptsClick(object sender, RoutedEventArgs e)
        {

        }

        private void MnuToolsClick(object sender, RoutedEventArgs e)
        {

        }

        private void MnuOptionsClick(object sender, RoutedEventArgs e)
        {

        }

        private void MnuLayoutClick(object sender, RoutedEventArgs e)
        {

        }

        private void MnuHelpClick(object sender, RoutedEventArgs e)
        {

        }

        private void MnuCopyClick(object sender, RoutedEventArgs e)
        {

        }

        private void MnuPasteClick(object sender, RoutedEventArgs e)
        {

        }

        private void MnuPreferencesClick(object sender, RoutedEventArgs e)
        {

        }
        
        private void MnuHelpFilesClick(object sender, RoutedEventArgs e)
        {

        }

        private void MnuAboutClick(object sender, RoutedEventArgs e)
        {

        }

#endregion

    

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void mnuManageScripts_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
