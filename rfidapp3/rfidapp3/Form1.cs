using RFID;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace rfidapp3
{
    public partial class Form1 : Form
    {
        private const byte DEVICE_ADDRESS = 0xFF;
        private const int DEFAULT_POWER = 26;
        private const byte PARAM_ADDRESS_TRANSPORT = 0x01;
        private const int DEFAULT_TRANSPORT = 2; //RJ45
        private bool _isConnected = false;
        private bool _isDiscovering = false;
        private string _readerIP = "";
        private bool _disposed = false;
        private RFID.SWNetApi.callBackHandler _callbackHandler;
        private string _lastEPC = "";
        private DateTime _lastUpdateTime = DateTime.MinValue;
        private const int UI_UPDATE_THROTTLE_MS = 100;
        private HashSet<string> _currentTags = new HashSet<string>();
        private HashSet<string> _reportedTags = new HashSet<string>();
        private Timer timerTagCheck;

        public Form1()
        {
            InitializeComponent();
            _callbackHandler = new RFID.SWNetApi.callBackHandler(TagDetectionCallback);
            SetupUI();

            timerTagCheck = new Timer();
            timerTagCheck.Interval = 500;
            timerTagCheck.Tick += (s, e) => CheckForRemovedTags();
            timerTagCheck.Start();

            StartRFIDProcess();
        }

        private void SetupUI()
        {
            this.Text = "NRU Reader Log";
            this.Size = new Size(750, 400);
            this.FormClosing += Form1_FormClosing;
        }

        #region Connection and Discovery
        private async void StartRFIDProcess()
        {
            _readerIP = await DiscoverReaderIP();
            if (!string.IsNullOrEmpty(_readerIP))
            {
                if (ConnectToReader())
                {
                    StartReading();
                }
            }
            else
            {
                MessageBox.Show("No RFID reader found on network. Please Reconnect.");
            }
        }

        private bool ConnectToReader()
        {
            if (_isConnected) return true;

            if (RFID.SWNetApi.SWNet_OpenDevice(_readerIP, 60000))
            {
                _isConnected = true;
                RFID.SWNetApi.SWNet_SetDeviceOneParam(DEVICE_ADDRESS, 0x02, 0x01);
                RFID.SWNetApi.SWNet_SetCallback(_callbackHandler);
                RFID.SWNetApi.SWNet_SetDeviceOneParam(DEVICE_ADDRESS, 0x05, DEFAULT_POWER);
                RFID.SWNetApi.SWNet_SetDeviceOneParam(DEVICE_ADDRESS, PARAM_ADDRESS_TRANSPORT, DEFAULT_TRANSPORT);
                UpdateConnectionStatus(connected: true);
                return true;
            }
            return false;
        }

        private async Task<string> DiscoverReaderIP()
        {
            UpdateConnectionStatus(discovering: true);
            string localIP = GetLocalIPAddress();
            if (string.IsNullOrEmpty(localIP)) return null;

            string subnet = localIP.Substring(0, localIP.LastIndexOf('.') + 1);

            var tasks = Enumerable.Range(1, 254).Select(async i =>
            {
                string testIP = $"{subnet}{i}";
                bool isOpen = await CheckRFIDPort(testIP, 60000);
                return (ip: testIP, success: isOpen);
            }).ToList();

            var results = await Task.WhenAll(tasks);
            var match = results.FirstOrDefault(r => r.success);
            UpdateConnectionStatus(connected: false, discovering: false);
            return match.ip;
        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork && ip.ToString().StartsWith("192.168."))
                {
                    return ip.ToString();
                }
            }
            return host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString();
        }

        private async Task<bool> CheckRFIDPort(string ip, int port)
        {
            try
            {
                using (var tcpClient = new TcpClient())
                {
                    var connectTask = tcpClient.ConnectAsync(ip, port);
                    if (await Task.WhenAny(connectTask, Task.Delay(50)) == connectTask)
                    {
                        return tcpClient.Connected;
                    }
                    return false;
                }
            }
            catch { return false; }
        }
        #endregion

        #region Reading and Tag Processing
        private void StartReading()
        {
            RFID.SWNetApi.SWNet_ClearTagBuf();
            RFID.SWNetApi.SWNet_StartRead(DEVICE_ADDRESS);
        }

        private void TagDetectionCallback(int msg, int param1, byte[] param2, int param3, byte[] param4)
        {
            if (_disposed || msg != 2 || param2 == null || param3 <= 3) return;

            try
            {
                int position = 0;
                byte packLength = param2[position];
                byte tagType = param2[position + 1];
                int idLength = (tagType & 0x80) == 0x80 ? packLength - 7 : packLength - 1;

                StringBuilder epc = new StringBuilder();
                for (int j = 0; j < idLength; j++)
                {
                    epc.Append(param2[position + 3 + j].ToString("X2"));
                }

                string currentEPC = epc.ToString().Substring(0, 24);


                _currentTags.Add(currentEPC);

                // Throttle UI updates to prevent flooding
                if ((DateTime.Now - _lastUpdateTime).TotalMilliseconds < UI_UPDATE_THROTTLE_MS)
                    return;

                _lastUpdateTime = DateTime.Now;

                this.BeginInvoke((MethodInvoker)delegate
                {
                if (_disposed || !this.IsHandleCreated) return;

                    if (!_reportedTags.Contains(currentEPC))
                    {
                        labelLastEPC.Text = currentEPC.Insert(9, "-");

                        string timestamp = DateTime.Now.ToString("HH:mm:ss");
                        string logEntry = $"{timestamp} - {currentEPC}{Environment.NewLine}";
                        richTextBoxLog.AppendText(logEntry);
                        richTextBoxLog.ScrollToCaret();

                        _lastEPC = currentEPC;
                        labelConnectionStatus.Text = $"Connected to: {_readerIP}";

                        _reportedTags.Add(currentEPC);

                    }

                    
                });
            }
            catch { /* Suppress any errors during processing */ }
        }

        private void CheckForRemovedTags()
        {
            var tagsToRemove = _reportedTags.Except(_currentTags).ToList();
            foreach (var tag in tagsToRemove)
            {
                _reportedTags.Remove(tag);
            }

            _currentTags.Clear();
        }

        #endregion

        #region UI Events
        private void buttonReconnect_Click(object sender, EventArgs e)
        {
            if (_isDiscovering) return;
            StartRFIDProcess();
        }

        private void UpdateConnectionStatus(bool connected = false, bool discovering = false)
        {
            _isConnected = connected;
            _isDiscovering = discovering;

            labelConnectionStatus.Text = discovering ? "Scanning for nearby readers..." :
                              connected ? $"Connected to: {_readerIP}" :
                              "Could not find reader";
            labelConnectionStatus.ForeColor = discovering ? Color.SteelBlue :
                                    connected ? Color.Green :
                                    Color.Red;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _disposed = true;
            if (_isConnected)
            {
                RFID.SWNetApi.SWNet_SetCallback(null);
                RFID.SWNetApi.SWNet_StopRead(DEVICE_ADDRESS);
                RFID.SWNetApi.SWNet_CloseDevice();
            }
        }
        #endregion

    }
}