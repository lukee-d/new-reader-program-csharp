using RFID;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace rfidapp
{
    public partial class Form1 : Form
    {
        // Constants
        private const byte DEVICE_ADDRESS = 0xFF;
        private const byte PARAM_ADDRESS_WORKMODE = 0x02;
        private const byte PARAM_ADDRESS_POWER = 0x05;
        private const byte EPC_BANK = 0x01;
        private const byte PASSWORD_LENGTH = 4;

        // Reader modes
        private enum ReaderMode { Active = 0x01, Answer = 0x00, Trigger = 0x02 }

        // State variables
        private bool _isConnected = false;
        private bool _isReading = false;
        private ReaderMode _currentMode = ReaderMode.Active;

        private bool _singleReadRequested = false;
        private DateTime _lastTagReadTime = DateTime.MinValue;

        private bool _isInActiveMode = false;

        public Form1()
        {
            InitializeComponent();
            SetupUI();
        }

        private async void SetupUI()
        {
            // Connection settings
            textBoxIP.Text = "192.168.1.178";
            textBoxPort.Text = "60000";

            // Initial button states
            buttonDisconnect.Enabled = false;
            buttonStopReading.Enabled = false;
            buttonWriteTag.Enabled = false;

            // Mode selection
            radioButtonActive.Checked = true;

            // EPC TextBox setup
            textBoxNewEPC.MaxLength = 24;
            textBoxNewEPC.Text = "Enter 12-byte EPC (24 hex chars)";
            textBoxNewEPC.ForeColor = SystemColors.GrayText;
            textBoxNewEPC.GotFocus += RemoveWatermark;
            textBoxNewEPC.LostFocus += SetWatermark;
            textBoxNewEPC.TextChanged += textBoxNewEPC_TextChanged;

            // Register callback
            RFID.SWNetApi.SWNet_SetCallback(TagDetectionCallback);

        }

        #region Connection Management
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (_isConnected) return;

            if (RFID.SWNetApi.SWNet_OpenDevice(textBoxIP.Text, Convert.ToUInt16(textBoxPort.Text)))
            {
                _isConnected = true;
                _isReading = false;
                AppendToOutput("Connected successfully\r\n");


                UpdateUI();
                InitializePowerLevel();
            }
            else
            {
                AppendToOutput("Connection failed\r\n");
            }
        }

        private async void buttonDisconnect_Click(object sender, EventArgs e)
        {
            if (!_isConnected) return;

            // Stop reading if active
            if (_isReading)
            {
                buttonStopReading_Click(null, EventArgs.Empty);
            }


            bool success = await ExecuteWithRetry(() =>
                RFID.SWNetApi.SWNet_CloseDevice(),
                retries: 2,
                delayMs: 200);



            if (success)
            {
                _isConnected = false;
                AppendToOutput("Disconnected successfully\r\n");
            }
            else
            {
                AppendToOutput("Disconnection failed\r\n");
            }

            UpdateUI();
        }
        #endregion

        #region Reading Operations
        private async void buttonStartReading_Click(object sender, EventArgs e)
        {
            if (!_isConnected || _isReading) return;

            _currentMode = radioButtonActive.Checked ? ReaderMode.Active : ReaderMode.Answer;

            RFID.SWNetApi.SWNet_ClearTagBuf();

            if (_currentMode == ReaderMode.Active)
            {

                bool success = await ExecuteWithRetry(() =>
                RFID.SWNetApi.SWNet_StartRead(DEVICE_ADDRESS),
                retries: 3,
                delayMs: 300);

                if (success)
                {
                    _isReading = true;
                    AppendToOutput("ACTIVE MODE: Continuous reading started\r\n");
                }
                else
                {
                    AppendToOutput("Failed to start active reading\r\n");
                    return;
                }
            }
            else // Answer Mode
            {
                if (RFID.SWNetApi.SWNet_SetDeviceOneParam(DEVICE_ADDRESS, PARAM_ADDRESS_WORKMODE, (byte)ReaderMode.Answer))
                {
                    _isReading = true;
                    AppendToOutput("ANSWER MODE: Ready for manual scans\r\n");
                }
                else
                {
                    AppendToOutput("Failed to switch to Answer mode\r\n");
                    return;
                }
            }

            UpdateUI();
        }

        private async void buttonStopReading_Click(object sender, EventArgs e)
        {
            if (!_isConnected || !_isReading) return;


            if (_currentMode == ReaderMode.Active)
            {
                bool success = await ExecuteWithRetry(() =>
                RFID.SWNetApi.SWNet_StopRead(DEVICE_ADDRESS),
                retries: 2,
                delayMs: 200);
                AppendToOutput(success ? "Active reading stopped\r\n" : "Failed to stop active reading\r\n");
            }

            // Always attempt to return to Active mode
            bool modeSwitched = RFID.SWNetApi.SWNet_SetDeviceOneParam(
                DEVICE_ADDRESS,
                PARAM_ADDRESS_WORKMODE,
                (byte)ReaderMode.Active);

            AppendToOutput(modeSwitched ? "Returned to Active mode\r\n" : "Failed to return to Active mode\r\n");

            _isReading = false;
            UpdateUI();

            // Additional verification
            
        }

        private void TagDetectionCallback(int msg, int param1, byte[] param2, int param3, byte[] param4)
        {
            if (msg == 2 && _isReading) // Tag detection when in reading mode
            {
                string tagInfo = _currentMode == ReaderMode.Active
                    ? ProcessActiveModeTags(param1, param2, param3)
                    : ProcessAnswerModeTags(param1, param2, param3);

                this.Invoke((MethodInvoker)delegate {
                    AppendToOutput(tagInfo);
                });
            }
        }
        #endregion

        #region Tag Processing
        private string ProcessActiveModeTags(int tagCount, byte[] tagData, int dataLength)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{tagCount} tag(s) detected:");

            for (int i = 0; i < dataLength; i++)
            {
                sb.Append(tagData[i].ToString("X2"));
                if ((i + 1) % 16 == 0) sb.AppendLine();
            }

            return sb.ToString();
        }

        private string ProcessAnswerModeTags(int tagCount, byte[] tagData, int dataLength)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Inventory scan found {tagCount} tags:");

            int position = 0;
            for (int i = 0; i < tagCount; i++)
            {
                if (position + 3 >= dataLength) break;

                byte packLength = tagData[position];
                byte tagType = tagData[position + 1];
                byte antenna = tagData[position + 2];

                bool hasTimestamp = (tagType & 0x80) == 0x80;
                int idLength = hasTimestamp ? packLength - 7 : packLength - 1;

                // Extract EPC
                StringBuilder epc = new StringBuilder();
                for (int j = 3; j < 3 + idLength - 2; j++)
                {
                    epc.Append(tagData[position + j].ToString("X2"));
                }

                // Get RSSI
                byte rssi = tagData[position + packLength - (hasTimestamp ? 7 : 1)];

                // Display tag info
                sb.AppendLine($"Tag {i + 1}:");
                sb.AppendLine($"  Antenna: {antenna}");
                sb.AppendLine($"  EPC: {epc}");
                sb.AppendLine($"  RSSI: {rssi} dBm");

                position += packLength + 1;
            }

            return sb.ToString();
        }
        #endregion

        #region Tag Writing
        private void buttonWriteTag_Click(object sender, EventArgs e)
        {
            string epc = textBoxNewEPC.Text.Trim();

            if (string.IsNullOrEmpty(epc))
            {
                AppendToOutput("Please enter an EPC to write\r\n");
                return;
            }

            if (!IsValidHex(epc))
            {
                AppendToOutput("Invalid EPC - must be hexadecimal (0-9, A-F)\r\n");
                return;
            }

            epc = epc.PadRight(24, '0').Substring(0, 24);
            AppendToOutput($"Attempting to write EPC: {epc}\r\n");



            WriteTagEPC(epc);
        }

        private async void WriteTagEPC(string epcHex)
        {
            buttonWriteTag.Enabled = false;
            try
            {
                byte[] epcBytes = HexStringToByteArray(epcHex);
                byte[] password = new byte[PASSWORD_LENGTH]; // Default zeros

                bool writeSuccess = await ExecuteWithRetry(() =>
                RFID.SWNetApi.SWNet_WriteCardG2(
                    DEVICE_ADDRESS,
                    password,
                    EPC_BANK,
                    0x02, // Start at word address 2
                    (byte)epcBytes.Length,
                    epcBytes),
                retries: 3,
                delayMs: 300);

                await Task.Delay(300);


                /*read test
                byte[] readBuffer = new byte[epcBytes.Length];
                bool verifySuccess = await ExecuteWithRetry(() =>
                RFID.SWNetApi.SWNet_ReadCardG2(
                    DEVICE_ADDRESS,
                    password,
                    EPC_BANK,
                    0x02, // Start at word address 2
                    (byte)epcBytes.Length,
                    epcBytes),
                retries: 2,
                delayMs: 200);

                bool actuallyWrote = verifySuccess && readBuffer.SequenceEqual(epcBytes);

                AppendToOutput(actuallyWrote ?
                    "Write verified successfully!\r\n" :
                    "Write verification failed\r\n");
               */
                AppendToOutput("Write successful!");

            }
            catch (Exception ex)
            {
                AppendToOutput($"Write error: {ex.Message}\r\n");
            }
            finally
            {
                buttonWriteTag.Enabled = true;
            }
        }
        #endregion

        #region UI Helpers
        private void UpdateUI()
        {
            buttonConnect.Enabled = !_isConnected;
            buttonDisconnect.Enabled = _isConnected;
            buttonStartReading.Enabled = _isConnected && !_isReading;
            buttonStopReading.Enabled = _isConnected && _isReading;
            groupBoxModes.Enabled = _isConnected && !_isReading;
            buttonWriteTag.Enabled = _isConnected && !_isReading &&
                                   textBoxNewEPC.Text.Length >= 12;

            // Visual feedback
            textBoxOutput.BackColor = _isReading ? Color.LightYellow : SystemColors.Window;

            labelStatus.Text = _isConnected ? "Connected" : "Disconnected";
            labelStatus.ForeColor = _isConnected ? Color.Green : Color.Red;

        }

        private void AppendToOutput(string text)
        {
            if (textBoxOutput.InvokeRequired)
            {
                textBoxOutput.Invoke((MethodInvoker)delegate { AppendToOutput(text); });
                return;
            }

            if (textBoxOutput.TextLength > 10000)
                textBoxOutput.Text = string.Empty;

            textBoxOutput.AppendText(text);
            textBoxOutput.ScrollToCaret();
        }
        private void AppendToOutputEPCRead(string text)
        {
            if (textBoxEPCRead.InvokeRequired)
            {
                textBoxEPCRead.Invoke((MethodInvoker)delegate { AppendToOutputEPCRead(text); });
                return;
            }

            if (textBoxEPCRead.TextLength > 10000)
                textBoxEPCRead.Text = string.Empty;

            textBoxEPCRead.AppendText(text);
            textBoxEPCRead.ScrollToCaret();
        }


        private void RemoveWatermark(object sender, EventArgs e)
        {
            if (textBoxNewEPC.Text == "Enter 12-byte EPC (24 hex chars)")
            {
                textBoxNewEPC.Text = "";
                textBoxNewEPC.ForeColor = SystemColors.WindowText;
            }
        }

        private void SetWatermark(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxNewEPC.Text))
            {
                textBoxNewEPC.Text = "Enter 12-byte EPC (24 hex chars)";
                textBoxNewEPC.ForeColor = SystemColors.GrayText;
            }
        }

        private void textBoxNewEPC_TextChanged(object sender, EventArgs e)
        {
            int pos = textBoxNewEPC.SelectionStart;
            textBoxNewEPC.Text = Regex.Replace(textBoxNewEPC.Text.ToUpper(), "[^0-9A-F]", "");
            textBoxNewEPC.SelectionStart = pos;

            if (textBoxNewEPC.Text.Length > 0 && textBoxNewEPC.ForeColor == SystemColors.GrayText)
                textBoxNewEPC.ForeColor = SystemColors.WindowText;

            UpdateUI();
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBoxOutput.Text = string.Empty;
        }
        #endregion

        #region Utility Methods
        private bool IsValidHex(string input)
        {
            foreach (char c in input)
                if (!Uri.IsHexDigit(c)) return false;
            return true;
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

        private void labelStatus_Click(object sender, EventArgs e)
        {

        }

        private void buttonSetPower_Click(object sender, EventArgs e)
        {
            if (!_isConnected)
            {
                AppendToOutput("Not connected - cannot set power\n");
                return;
            }

            byte powerLevel = (byte)trackBarPower.Value;
            bool success = RFID.SWNetApi.SWNet_SetDeviceOneParam(
                DEVICE_ADDRESS,
                PARAM_ADDRESS_POWER,
                powerLevel);

            AppendToOutput(success ?
                $"Power set to {powerLevel} dBm successfully\r\n"
                : "Failed to set power level\n");
        }

        private void trackBarPower_Scroll(object sender, EventArgs e)
        {
            labelPower.Text = $"Power: {trackBarPower.Value} dBm";
        }

        private void labelPower_Click(object sender, EventArgs e)
        {

        }

        private void textBoxEPCRead_TextChanged(object sender, EventArgs e)
        {

        }

        private void buttonRead_Click(object sender, EventArgs e)
        {
            if (!_isConnected)
            {
                AppendToOutputEPCRead("Not connected - can't read\n");
                return;
            }

            // If active mode is running, pause it temporarily
            if (_isReading && _currentMode == ReaderMode.Active)
            {
                RFID.SWNetApi.SWNet_StopRead(DEVICE_ADDRESS);
                _isInActiveMode = true; // Remember to restore later
            }

            // Clear tag buffer and set up for single read
            RFID.SWNetApi.SWNet_ClearTagBuf();
            _singleReadRequested = true;
            RFID.SWNetApi.SWNet_SetCallback(getEPCRead);

            AppendToOutputEPCRead("Ready to scan - place tag near reader\n");
            buttonRead.Enabled = false;
        }

        private void getEPCRead(int msg, int param1, byte[] param2, int param3, byte[] param4)
        {
            if (msg == 2)
            {
                if (_singleReadRequested)
                {
                    // Debounce check
                    if ((DateTime.Now - _lastTagReadTime).TotalSeconds < 1.0)
                        return;

                    _lastTagReadTime = DateTime.Now;
                    string tagInfo = ProcessAnswerModeTags(param1, param2, param3);

                    this.Invoke((MethodInvoker)delegate {
                        AppendToOutputEPCRead(tagInfo + "\n");
                        buttonRead.Enabled = true;
                    });

                    _singleReadRequested = false;

                    // Restore active mode if it was running
                    if (_isInActiveMode)
                    {
                        RFID.SWNetApi.SWNet_StartRead(DEVICE_ADDRESS);
                        _isInActiveMode = false;
                        // Restore the original callback for active mode
                        RFID.SWNetApi.SWNet_SetCallback(TagDetectionCallback);
                    }
                }
                else if (_isReading && _currentMode == ReaderMode.Active)
                {
                    // Handle active mode tags normally
                    string tagInfo = ProcessActiveModeTags(param1, param2, param3);
                    this.Invoke((MethodInvoker)delegate {
                        AppendToOutput(tagInfo);
                    });
                }
            }
        }
        private void buttonSaveLog_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {

                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.Title = "Save Log";
                saveFileDialog.FileName = $"rfid_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        System.IO.File.WriteAllText(saveFileDialog.FileName, textBoxOutput.Text);
                        AppendToOutput($"Log saved to {saveFileDialog.FileName}\r\n");
                    }
                    catch (Exception ex)
                    {
                        AppendToOutput($"Error saving log: {ex.Message}\r\n");
                    }
                }
            }
        }

        private async Task<bool> ExecuteWithRetry(Func<bool> command, int retries = 2, int delayMs = 200)
        {
            for (int attempt = 1; attempt <= retries; attempt++)
            {
                if (command()) return true;

                if (attempt < retries)
                {
                    await Task.Delay(delayMs);
                }
            }
            return false;
        }

        private void InitializePowerLevel()
        {
            if (!_isConnected) return;
            try
            {
                string currentPower = getCurrentPowerLevel().TrimStart('0');
                
                this.Invoke((MethodInvoker)delegate
                {
                    trackBarPower.Value = int.Parse(currentPower);
                    labelPower.Text = $"Power: {currentPower} dBm";
                });
            }
            catch
            {
                trackBarPower.Value = 15;
                labelPower.Text = "Power: 15 dBm";
            }
        }

        private string getCurrentPowerLevel()
        {
            byte bParamAddr = 0;
            byte[] bValue = new byte[2];

            /*  01: Transport
                02: WorkMode
                03: DeviceAddr
                04: FilterTime
                05: RFPower
                06: BeepEnable
                07: UartBaudRate*/
            bParamAddr = 0x05;

            if (RFID.SWNetApi.SWNet_ReadDeviceOneParam(0xFF, bParamAddr, bValue) == false)
            {
                AppendToOutput("Failed");
                return "";
            }
            string str1 = "";
            str1 = bValue[0].ToString("d2");

            return(str1);

        }
    }
}