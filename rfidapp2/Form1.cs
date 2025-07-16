using RFID;
using System;
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

        private string _lastWrittenEPC = "";
        private bool _isConnected = false;
        private bool _isDiscovering = false;
        private string _readerIP = "";
        private bool _disposed = false; // Track form disposal
        private bool _isWriting = false;

        private RFID.SWNetApi.callBackHandler _callbackHandler;

        private System.Threading.Timer _tagPresenceTimer;
        private DateTime _lastTagDetectionTime = DateTime.MinValue;
        private const int TAG_TIMEOUT_MS = 500;

        private bool _expectingNewTag = false;
        private string _writtenEPC = "";

        #region Setup
        public Form1()
        {
            InitializeComponent();
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
            this.Text = "Simple RFID App";
            this.Size = new Size(800, 300);

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
                MessageBox.Show("No RFID reader found on network");
            }
        }
        #endregion

        #region Connection
        private bool ConnectToReader()
        {
            if (_isConnected) return true;

            _callbackHandler = new RFID.SWNetApi.callBackHandler(TagDetectionCallback);

            if (RFID.SWNetApi.SWNet_OpenDevice(_readerIP, 60000))
            {
                _isConnected = true;
                UpdateConnectionStatus(connected: true);

                // Set to Answer mode
                RFID.SWNetApi.SWNet_SetDeviceOneParam(DEVICE_ADDRESS, 0x02, 0x01);

                // Set callback
                RFID.SWNetApi.SWNet_SetCallback(TagDetectionCallback);

                return true;
            }

            return false;
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
                labelConnectionStatus.Text = "Connected";
                labelConnectionStatus.ForeColor = Color.Green;
            }
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
            if (_disposed || _isWriting || param2 == null) return; // Skip if form is disposed
            if (msg == 2) // Tag detection
            {
                _lastTagDetectionTime = DateTime.Now;

                int position = 0;
                if (param3 > 3)
                {
                    byte packLength = param2[position];
                    byte tagType = param2[position + 1];
                    int idLength = (tagType & 0x80) == 0x80 ? packLength - 7 : packLength - 1;

                    // Extract EPC (more accurate)
                    StringBuilder epc = new StringBuilder();
                    for (int j = 0; j < idLength; j++)
                    {
                        epc.Append(param2[position + 3 + j].ToString("X2"));
                    }

                    string currentEPC = epc.ToString().Substring(0, 24);

                    try
                    {
                        if (!_disposed && this.IsHandleCreated)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                if (!_disposed && !_isWriting)
                                {
                                    if (_expectingNewTag)
                                    {
                                        
                                        string labelEPC = labelLastEPC.Text.Replace("Current EPC: ", "");


                                        if (currentEPC == _writtenEPC || 
                                        currentEPC == labelEPC)
                                        {;
                                            labelLastEPC.Text = "Current EPC: " + currentEPC;
                                            UpdateMatchStatus(currentEPC);

                                            textBoxNewEPC.Enabled = true;
                                            textBoxNewEPC.BackColor = SystemColors.Window;
                                            textBoxNewEPC.Focus();
                                            _expectingNewTag = false;
                                        }
                                        return;
                                    }

                                    labelLastEPC.Text = "Current EPC: " + currentEPC;
                                    UpdateMatchStatus(currentEPC);
                                    textBoxNewEPC.Enabled = true;
                                    textBoxNewEPC.BackColor = SystemColors.Window;
                                    textBoxNewEPC.Focus();
                                }
                            });
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        // Form is disposed, ignore
                    }
                }
            }
        
        }
        #endregion

        #region Writing
        private void textBoxNewEPC_TextChanged(object sender, EventArgs e)
        {
            if (_isWriting || !textBoxNewEPC.Enabled) return;

            // Filter to hex characters only
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

            // Auto-write when 9+ characters entered
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
        #endregion

        #region Update Status
        private void UpdateMatchStatus(string currentEPC)
        {
            if (string.IsNullOrEmpty(_lastWrittenEPC))
            {
                labelMatchStatus.Text = "NO MATCH";
                labelMatchStatus.ForeColor = Color.Gray;
            }
            else if (currentEPC == _lastWrittenEPC)
            {
                labelMatchStatus.Text = "MATCH";
                labelMatchStatus.ForeColor = Color.Green;
            }
            else if (!string.IsNullOrEmpty(currentEPC))
            {
                labelMatchStatus.Text = "MISMATCH";
                labelMatchStatus.ForeColor = Color.Red;
            }
            else
            {
                labelMatchStatus.Text = "NO TAG";
                labelMatchStatus.ForeColor = Color.Gray;
            }
        }

        private void TagPresenceCheck(object state)
        {
            if (_isWriting || _expectingNewTag) return;

            if ((DateTime.Now - _lastTagDetectionTime).TotalMilliseconds > TAG_TIMEOUT_MS)
            {
                try
                {
                    if (!_disposed && this.IsHandleCreated)
                    {
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            if (!_disposed && !_isWriting && !_expectingNewTag)
                            {
                                labelLastEPC.Text = "Current EPC: [No Tag]";
                                UpdateMatchStatus(null);

                                textBoxNewEPC.Text = "";
                                textBoxNewEPC.Enabled = false;
                                textBoxNewEPC.BackColor = SystemColors.Control;

                            }
                        });
                    }
                }
                catch (InvalidOperationException)
                {

                }

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
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _disposed = true;
            var timer = _tagPresenceTimer;
            _tagPresenceTimer = null;
            timer?.Dispose();

            try
            {
                if (_isConnected)
                {

                    CleanupRFID();
                }
            }
            catch
            {

            }


            base.OnFormClosed(e);
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _disposed = true; // Mark as disposed
            CleanupRFID();
            base.OnFormClosing(e);
        }

        private void CleanupRFID()
        {
            if (_isConnected)
            {
                RFID.SWNetApi.SWNet_StopRead(DEVICE_ADDRESS);
                RFID.SWNetApi.SWNet_CloseDevice();
                _isConnected = false;
            }
        }
        #endregion
        
    }
}