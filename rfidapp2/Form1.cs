using RFID;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace rfidapp2
{
    public partial class Form1 : Form
    {
        private const byte DEVICE_ADDRESS = 0xFF;
        private const byte EPC_BANK = 0x01;
        private const byte PASSWORD_LENGTH = 4;
        private const byte PARAM_ADDRESS_POWER = 0x05;
        private const int DEFAULT_POWER = 6;

        private string _lastWrittenEPC = "";
        private bool _isConnected = false;
        private bool _isDiscovering = false;
        private string _readerIP = "";
        private bool _disposed = false; 
        private bool _isWriting = false;
        private bool _multipleTags = false;

        private RFID.SWNetApi.callBackHandler _callbackHandler;

        private System.Threading.Timer _tagPresenceTimer;
        private DateTime _lastTagDetectionTime = DateTime.MinValue;
        private const int TAG_TIMEOUT_MS = 500;

        private bool _expectingNewTag = false;
        private string _writtenEPC = "";

        private readonly Dictionary<string, DateTime> _recentTagTimestamps = new Dictionary<string, DateTime>();
        private readonly TimeSpan _tagTimeout = TimeSpan.FromMilliseconds(1000);


        #region Setup
        public Form1()
        {
            InitializeComponent();
            _callbackHandler = new RFID.SWNetApi.callBackHandler(TagDetectionCallback);
            SetupUI();

            this.Load += (s, e) =>
            {
                _tagPresenceTimer = new System.Threading.Timer(
                    TagPresenceCheck,
                    null,
                    dueTime: 100,
                    period: 100);
            };
            StartRFIDProcess();
        }

        private void SetupUI()
        {
            this.Text = "NRU Assignation Utility";
            this.Size = new Size(750, 300);

            labelLastEPC.Text = "Current EPC: [No Tag]";
            UpdateMatchStatus(null);
            UpdateConnectionStatus();

            textBoxNewEPC.Enabled = false;
            textBoxNewEPC.BackColor = SystemColors.Control;
        }

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
        #endregion

        #region Connection
        private bool ConnectToReader()
        {
            if (_isConnected) return true;

            if (_callbackHandler == null)
                _callbackHandler = new RFID.SWNetApi.callBackHandler(TagDetectionCallback);

            if (RFID.SWNetApi.SWNet_OpenDevice(_readerIP, 60000))
            {
                _isConnected = true;
                UpdateConnectionStatus(connected: true);

                RFID.SWNetApi.SWNet_SetDeviceOneParam(DEVICE_ADDRESS, 0x02, 0x01);
                RFID.SWNetApi.SWNet_SetCallback(_callbackHandler);

                //set rf power to 6/default
                bool success = RFID.SWNetApi.SWNet_SetDeviceOneParam(
                DEVICE_ADDRESS,
                PARAM_ADDRESS_POWER,
                DEFAULT_POWER);

                return true;
            }
            return false;
        }

        private void buttonReconnect_Click(object sender, EventArgs e)
        {
            if (_isDiscovering || _isWriting) return;

            StartRFIDProcess();
        }

        #endregion

        #region IP Logic
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
            if (match.success)
            {
                UpdateConnectionStatus(connected: false, discovering: false);
                return match.ip;
            }

            UpdateConnectionStatus(connected: false, discovering: false);
            return null;
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
            catch
            {
                return false;
            }
        }
        #endregion

        #region Reading

        private async void StartReading()
        {
            RFID.SWNetApi.SWNet_ClearTagBuf();
            bool success = await ExecuteWithRetry(() =>
                RFID.SWNetApi.SWNet_StartRead(DEVICE_ADDRESS),
                retries: 3,
                delayMs: 300);
            UpdateConnectionStatus(connected: true);
        }

        private void TagDetectionCallback(int msg, int param1, byte[] param2, int param3, byte[] param4)
        {
            if (_disposed || _isWriting || param2 == null || this.IsDisposed) return;

            if (msg == 2) 
            {
                _lastTagDetectionTime = DateTime.Now;

                int position = 0;
                if (param3 > 3 && this.IsHandleCreated)
                {
                    byte packLength = param2[position];
                    byte tagType = param2[position + 1];
                    int idLength = (tagType & 0x80) == 0x80 ? packLength - 7 : packLength - 1;

                    StringBuilder epc = new StringBuilder();

                    for (int j = 0; j < idLength; j++)
                    {
                        epc.Append(param2[position + 3 + j].ToString("X2"));
                    }

                    string currentEPC = epc.ToString().Substring(0, 24);
                    DateTime now = DateTime.Now;

                    lock (_recentTagTimestamps)
                    {
                        _recentTagTimestamps[currentEPC] = now;

                        // Remove expired tags
                        var expiredKeys = _recentTagTimestamps
                            .Where(kv => (now - kv.Value) > _tagTimeout)
                            .Select(kv => kv.Key)
                            .ToList();

                        foreach (var key in expiredKeys)
                        {
                            _recentTagTimestamps.Remove(key);
                        }

                        _multipleTags = _recentTagTimestamps.Count >= 2;
                    }

                    this.Invoke((MethodInvoker)delegate
                    {
                        if (_disposed || _isWriting || !this.IsHandleCreated) return;

                        if (_multipleTags)
                        {
                            labelLastEPC.Text = "Current EPC: " + currentEPC;
                            UpdateMatchStatus("MULTIPLE TAGS");
                            textBoxNewEPC.Text = "";
                            textBoxNewEPC.Enabled = false;
                            textBoxNewEPC.BackColor = SystemColors.Window;
                            return;
                        }

                        if (_expectingNewTag)
                        {
                            string labelEPC = labelLastEPC.Text.Replace("Current EPC: ", "");
                            if (currentEPC == _writtenEPC || currentEPC == labelEPC)
                            {
                                HandleSuccessfulWrite(currentEPC);
                            }
                            return;
                        }

                        labelLastEPC.Text = "Current EPC: " + currentEPC;
                        UpdateMatchStatus(currentEPC);
                        textBoxNewEPC.Enabled = true;
                    });
                }
            }
        }
        private void TagPresenceCheck(object state)
        {
            if (_disposed || _isWriting || _expectingNewTag) return;

            try
            {
                bool tagsPresent;
                lock (_recentTagTimestamps)
                {
                    tagsPresent = _recentTagTimestamps.Any(kv =>
                        (DateTime.Now - kv.Value) <= _tagTimeout);
                }

                if (!tagsPresent && (DateTime.Now - _lastTagDetectionTime).TotalMilliseconds > TAG_TIMEOUT_MS)
                {
                    // Safe UI update with disposal check
                    if (!_disposed && this.IsHandleCreated)
                    {
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            // Double-check disposal status
                            if (!_disposed && !_isWriting && !_expectingNewTag)
                            {
                                labelLastEPC.Text = "Current EPC: [No Tag]";
                                UpdateMatchStatus(null);
                                textBoxNewEPC.Text = "";
                                textBoxNewEPC.Enabled = false;
                            }
                        });
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // Silently ignore - form is closing
            }
            catch (InvalidOperationException)
            {
                // Handle might not be created yet
            }
        }

        #endregion

        #region Writing
        private void textBoxNewEPC_TextChanged(object sender, EventArgs e)
        {
            if (_isWriting || !textBoxNewEPC.Enabled) return;

            int pos = textBoxNewEPC.SelectionStart;
            string newText = Regex.Replace(textBoxNewEPC.Text.ToUpper(), "[^0-9A-F]", "");

            if (newText.Length > 9)
            {
                newText = newText.Substring(0, 9);
            }

            if (newText != textBoxNewEPC.Text)
            {
                textBoxNewEPC.Text = newText;
                textBoxNewEPC.SelectionStart = Math.Min(pos, newText.Length);
            }

            if (textBoxNewEPC.Text.Length == 9)
            {
                _isWriting = true;
                string epcToWrite = (textBoxNewEPC.Text.PadRight(24, '0').Substring(0, 24));
                textBoxNewEPC.Focus();
                WriteEPC(epcToWrite);
            }
        }

        private async void WriteEPC(string epcHex)
        {
            if (!_isConnected) return;
            try
            {
                textBoxNewEPC.Enabled = false;
                _isWriting = true;
                _expectingNewTag = true;
                _writtenEPC = epcHex.Substring(0, 24);

                byte[] epcBytes = HexStringToByteArray(epcHex.Substring(0, 12));
                byte[] password = new byte[PASSWORD_LENGTH]; // Default zeros


                // Then write the EPC
                bool success = await ExecuteWithRetry(() =>
                    RFID.SWNetApi.SWNet_WriteCardG2(
                        DEVICE_ADDRESS,
                        password,
                        EPC_BANK,
                        0x02, // Start at word address 2 (EPC starts at word 2)
                        (byte)(epcBytes.Length), // Length in words
                        epcBytes),
                    retries: 3,
                    delayMs: 300);

                await Task.Delay(800);

                if (success)
                {
                    _lastWrittenEPC = epcHex.Substring(0, 24);
                }
                else
                {
                    _isWriting = false;
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Write error: {ex.Message}");
                MessageBox.Show($"Write error: {ex.Message}");
            }
            finally
            {
                _isWriting = false;
            }
        }
        private void HandleSuccessfulWrite(string currentEPC)
        {
            labelLastEPC.Text = "Current EPC: " + currentEPC;
            UpdateMatchStatus(currentEPC);
            textBoxNewEPC.Enabled = true;
            _expectingNewTag = false;
        }

        #endregion

        #region Update UI
        private void UpdateMatchStatus(string currentEPC)
        {
            if (string.IsNullOrEmpty(_lastWrittenEPC) && !_multipleTags)
            {
                labelMatchStatus.Text = "NO MATCH";
                labelMatchStatus.ForeColor = Color.Gray;
            }
            else if (!string.IsNullOrEmpty(currentEPC) && currentEPC == _lastWrittenEPC && !_multipleTags)
            {
                labelMatchStatus.Text = "MATCH";
                labelMatchStatus.ForeColor = Color.Green;
            }
            else if (!string.IsNullOrEmpty(currentEPC) && !_multipleTags)
            {
                labelMatchStatus.Text = "MISMATCH";
                labelMatchStatus.ForeColor = Color.Red;
            }
            else if (currentEPC == "MULTIPLE TAGS")
            {
                labelMatchStatus.Text = "MULTIPLE TAGS";
                labelMatchStatus.ForeColor = Color.Red;
            }
            else
            {
                labelMatchStatus.Text = "NO TAG";
                labelMatchStatus.ForeColor = Color.Gray;
            }
        }
        private void UpdateConnectionStatus(bool connected = false, bool discovering = false)
        {
            _isConnected = connected;
            _isDiscovering = discovering;

            if (!connected)
            {
                if (discovering)
                {
                    labelConnectionStatus.Text = "Scanning for nearby readers...";
                    labelConnectionStatus.ForeColor = Color.SteelBlue;
                }
                else
                {
                    labelConnectionStatus.Text = "Could not find reader";
                    labelConnectionStatus.ForeColor = Color.Red;
                }
            }
            else
            {
                labelConnectionStatus.Text = "Connected to: " + _readerIP;
                labelConnectionStatus.ForeColor = Color.Green;
            }
        }
        #endregion

        #region Helpers
        private async Task<bool> ExecuteWithRetry(Func<bool> command, int retries = 2, int delayMs = 200)
        {
            for (int attempt = 1; attempt <= retries; attempt++)
            {
                if (command()) return true;
                if (attempt < retries) await Task.Delay(delayMs);

            }
            return false;
        }

        private byte[] HexStringToByteArray(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new ArgumentException("Hex string must have even length");

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);

            return bytes;
        }

        #endregion

        #region Cleanup
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _disposed = true;

            var timer = _tagPresenceTimer;
            _tagPresenceTimer = null;
            timer?.Dispose();

            if (_isConnected)
            {
                RFID.SWNetApi.SWNet_SetCallback(null);

                Task.Delay(100).Wait();

                RFID.SWNetApi.SWNet_StopRead(DEVICE_ADDRESS);
            }

            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_isConnected)
            {
                RFID.SWNetApi.SWNet_CloseDevice();
                _isConnected = false;
            }
            base.OnFormClosed(e);
        }
        
        
        #endregion
    }
}