//using DPUruNet;
//using System;
//using System.Collections.Generic;
//using System.Net.Http;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//using Newtonsoft.Json.Linq;

//namespace UareUSampleCSharp
//{
//    public class ServerResponse
//    {
//        public string message { get; set; }
//    }

//    public partial class Identification : Form
//    {
//        public Form_Main _sender;
//        private Fmd anyFinger;
//        private int count;

//        public Identification()
//        {
//            InitializeComponent();
//        }

//        private void Identification_Load(object sender, EventArgs e)
//        {
//            txtIdentify.Text = string.Empty;
//            anyFinger = null;
//            count = 0;

//            SendMessage(Action.SendMessage, "Place any finger on the reader.");

//            if (!_sender.OpenReader() || !_sender.StartCaptureAsync(OnCaptured))
//            {
//                this.Close();
//            }
//        }

//        private async void OnCaptured(CaptureResult captureResult)
//        {
//            try
//            {
//                if (!_sender.CheckCaptureResult(captureResult)) return;

//                SendMessage(Action.SendMessage, "A finger was captured.");

//                var resultConversion = FeatureExtraction.CreateFmdFromFid(captureResult.Data, Constants.Formats.Fmd.ANSI);
//                if (resultConversion.ResultCode != Constants.ResultCode.DP_SUCCESS)
//                {
//                    _sender.Reset = true;
//                    throw new Exception(resultConversion.ResultCode.ToString());
//                }

//                anyFinger = resultConversion.Data;

//                using (var client = new HttpClient())
//                {
//                    //http://localhost/attendance-system/api/services.php?resource=templates
//                    //http://localhost:8000/api/templates

//                    var response = await client.GetAsync("http://localhost/attendance-system/api/services.php?resource=templates");
//                    var jArray = JArray.Parse(await response.Content.ReadAsStringAsync());

//                    var storedTemplates = new List<StoredTemplate>();
//                    foreach (var jToken in jArray)
//                    {
//                        storedTemplates.Add(new StoredTemplate
//                        {
//                            employee_id = jToken["employee_id"]?.ToString(),
//                            template = jToken["template"]?.ToString()
//                        });
//                    }

//                    bool matchFound = false;
//                    SendMessage(Action.SendMessage, $"Current PC time: {DateTime.Now:HH:mm:ss}");

//                    foreach (var item in storedTemplates)
//                    {
//                        try
//                        {
//                            byte[] storedBytes = Convert.FromBase64String(item.template);
//                            string storedXml = System.Text.Encoding.UTF8.GetString(storedBytes);
//                            Fmd storedFmd = Fmd.DeserializeXml(storedXml);

//                            var compareResult = Comparison.Compare(anyFinger, 0, storedFmd, 0);

//                            if (compareResult.ResultCode == Constants.ResultCode.DP_SUCCESS &&
//                                compareResult.Score < (0x7fffffff / 100000))
//                            {
//                                // Step 1: Determine active time window
//                                //http://localhost:8000/api/attendance-window
//                                var timeWindowResponse = await client.GetAsync("http://localhost/attendance-system/api/services.php?resource=attendance-windows");
//                                var windows = JArray.Parse(JObject.Parse(await timeWindowResponse.Content.ReadAsStringAsync())["windows"].ToString());

//                                DateTime now = DateTime.Now;
//                                string currentWindow = null;

//                                foreach (var win in windows)
//                                {
//                                    var start = TimeSpan.Parse(win["start"].ToString());
//                                    var end = TimeSpan.Parse(win["end"].ToString());

//                                    if (now.TimeOfDay >= start && now.TimeOfDay <= end)
//                                    {
//                                        currentWindow = win["label"].ToString();
//                                        break;
//                                    }
//                                }

//                                if (currentWindow == null)
//                                {
//                                    string result = await new HttpClient().GetStringAsync("http://localhost/attendance-system/admin/attendance.php");
//                                    MessageBox.Show("❌ No active attendance window found. Please try again later.", "No Active Window", MessageBoxButtons.OK, MessageBoxIcon.Warning);
//                                    return;
//                                }

//                                // Step 2: Check if already logged
//                                //var checkUri = $"http://localhost:8000/api/attendance/check?employee_id={item.employee_id}&window={currentWindow}";
//                                //var checkResp = await client.GetAsync(checkUri);
//                                //bool alreadyLogged = JObject.Parse(await checkResp.Content.ReadAsStringAsync())["already_logged"].ToObject<bool>();

//                                //if (alreadyLogged)
//                                //{
//                                //    MessageBox.Show($"⚠️ Already logged attendance in this window ({currentWindow}).", "Already Logged", MessageBoxButtons.OK, MessageBoxIcon.Warning);
//                                //    return;
//                                //}

//                                // Step 3: Log attendance
//                                var attendanceContent = new FormUrlEncodedContent(new Dictionary<string, string>
//                                {
//                                    { "employee_id", item.employee_id },
//                                    { "window", currentWindow }
//                                });

//                                //Console.WriteLine($"Sending => employee_id={item.employee_id}, window={currentWindow}");


//                                //http://localhost/attendance-system/api/services.php?resource=attendances
//                                //http://localhost:8000/api/attendance
//                                var logResponse = await client.PostAsync("http://localhost/attendance-system/api/services.php?resource=attendances", attendanceContent);
//                                string logResult = await logResponse.Content.ReadAsStringAsync();

//                                if (logResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
//                                {
//                                    MessageBox.Show($"⚠️ Attendance already logged for Employee ID: {item.employee_id}.", "Already Logged", MessageBoxButtons.OK, MessageBoxIcon.Warning);
//                                }
//                                else if (logResponse.IsSuccessStatusCode)
//                                {
//                                    string result = await new HttpClient().GetStringAsync("http://localhost/attendance-system/admin/attendance.php");
//                                    var message = JObject.Parse(logResult)["message"]?.ToString() ?? "Attendance logged successfully.";
//                                    MessageBox.Show(message, $"✅ Match Found - {item.employee_id}", MessageBoxButtons.OK, MessageBoxIcon.Information);
//                                }
//                                else
//                                {
//                                    MessageBox.Show($"❌ Server error: {logResponse.StatusCode} - {logResult}", "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//                                }

//                                matchFound = true;
//                                break;
//                            }
//                        }
//                        catch (Exception ex)
//                        {
//                            SendMessage(Action.SendMessage, $"⚠️ Failed to process template for {item.employee_id}: {ex.Message}");
//                        }
//                    }

//                    if (!matchFound)
//                    {
//                        MessageBox.Show("❌ No match found. Please try again.", "No Match", MessageBoxButtons.OK, MessageBoxIcon.Warning);
//                    }
//                }

//                SendMessage(Action.SendMessage, "Place another finger on the reader to match again.");
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("⚠️ Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//        }

//        private void btnBack_Click(object sender, EventArgs e)
//        {
//            this.Close();
//        }

//        private void Identification_Closed(object sender, EventArgs e)
//        {
//            _sender.CancelCaptureAndCloseReader(OnCaptured);
//        }

//        #region SendMessage
//        private enum Action
//        {
//            SendMessage
//        }

//        private delegate void SendMessageCallback(Action action, string payload);

//        private void SendMessage(Action action, string payload)
//        {
//            try
//            {
//                if (txtIdentify.InvokeRequired)
//                {
//                    var d = new SendMessageCallback(SendMessage);
//                    Invoke(d, new object[] { action, payload });
//                }
//                else
//                {
//                    if (action == Action.SendMessage)
//                    {
//                        txtIdentify.Text += payload + "\r\n\r\n";
//                        txtIdentify.SelectionStart = txtIdentify.TextLength;
//                        txtIdentify.ScrollToCaret();
//                    }
//                }
//            }
//            catch { }
//        }
//        #endregion
//    }

//    public class StoredTemplate
//    {
//        public string employee_id { get; set; }
//        public string template { get; set; }
//    }
//}


using DPUruNet;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace UareUSampleCSharp
{
    public class ServerResponse
    {
        public string message { get; set; }
    }

    public partial class Identification : Form
    {
        public Form_Main _sender;
        private Fmd anyFinger;
        private int count;
        private ClientWebSocket wsClient; // ✅ Added WebSocket client

        public Identification()
        {
            InitializeComponent();
        }

        private async void Identification_Load(object sender, EventArgs e)
        {
            txtIdentify.Text = string.Empty;
            anyFinger = null;
            count = 0;

            SendMessage(Action.SendMessage, "Place any finger on the reader.");

            if (!_sender.OpenReader() || !_sender.StartCaptureAsync(OnCaptured))
            {
                this.Close();
            }

            //await ConnectWebSocketAsync(); // ✅ Connect to WebSocket server
        }

        private async void OnCaptured(CaptureResult captureResult)
        {
            try
            {
                if (!_sender.CheckCaptureResult(captureResult)) return;

                SendMessage(Action.SendMessage, "A finger was captured.");

                var resultConversion = FeatureExtraction.CreateFmdFromFid(captureResult.Data, Constants.Formats.Fmd.ANSI);
                if (resultConversion.ResultCode != Constants.ResultCode.DP_SUCCESS)
                {
                    _sender.Reset = true;
                    throw new Exception(resultConversion.ResultCode.ToString());
                }

                anyFinger = resultConversion.Data;

                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync("http://localhost/attendance-system/api/services.php?resource=templates");
                    var jArray = JArray.Parse(await response.Content.ReadAsStringAsync());

                    var storedTemplates = new List<StoredTemplate>();
                    foreach (var jToken in jArray)
                    {
                        storedTemplates.Add(new StoredTemplate
                        {
                            employee_id = jToken["employee_id"]?.ToString(),
                            template = jToken["template"]?.ToString()
                        });
                    }

                    bool matchFound = false;
                    SendMessage(Action.SendMessage, $"Current PC time: {DateTime.Now:HH:mm:ss}");

                    foreach (var item in storedTemplates)
                    {
                        try
                        {
                            byte[] storedBytes = Convert.FromBase64String(item.template);
                            string storedXml = System.Text.Encoding.UTF8.GetString(storedBytes);
                            Fmd storedFmd = Fmd.DeserializeXml(storedXml);

                            var compareResult = Comparison.Compare(anyFinger, 0, storedFmd, 0);

                            if (compareResult.ResultCode == Constants.ResultCode.DP_SUCCESS &&
                                compareResult.Score < (0x7fffffff / 100000))
                            {
                                // Step 1: Determine active time window
                                var timeWindowResponse = await client.GetAsync("http://localhost/attendance-system/api/services.php?resource=attendance-windows");
                                var windows = JArray.Parse(JObject.Parse(await timeWindowResponse.Content.ReadAsStringAsync())["windows"].ToString());

                                DateTime now = DateTime.Now;
                                string currentWindow = null;

                                foreach (var win in windows)
                                {
                                    var start = TimeSpan.Parse(win["start"].ToString());
                                    var end = TimeSpan.Parse(win["end"].ToString());

                                    if (now.TimeOfDay >= start && now.TimeOfDay <= end)
                                    {
                                        currentWindow = win["label"].ToString();
                                        break;
                                    }
                                }

                                if (currentWindow == null)
                                {
                                    string result = await new HttpClient().GetStringAsync("http://localhost/attendance-system/admin/attendance.php");
                                    MessageBox.Show("❌ No active attendance window found. Please try again later.", "No Active Window", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                var attendanceContent = new FormUrlEncodedContent(new Dictionary<string, string>
                                {
                                    { "employee_id", item.employee_id },
                                    { "window", currentWindow }
                                });

                                var logResponse = await client.PostAsync("http://localhost/attendance-system/api/services.php?resource=attendances", attendanceContent);
                                string logResult = await logResponse.Content.ReadAsStringAsync();

                                if (logResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
                                {
                                    MessageBox.Show($"⚠️ Attendance already logged for Employee ID: {item.employee_id}.", "Already Logged", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                                else if (logResponse.IsSuccessStatusCode)
                                {
                                    string result = await new HttpClient().GetStringAsync("http://localhost/attendance-system/admin/attendance.php");
                                    var message = JObject.Parse(logResult)["message"]?.ToString() ?? "Attendance logged successfully.";
                                    await ConnectWebSocketAsync();
                                    MessageBox.Show(message, $"✅ Match Found - {item.employee_id}", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else
                                {
                                    MessageBox.Show($"❌ Server error: {logResponse.StatusCode} - {logResult}", "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }

                                matchFound = true;
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            SendMessage(Action.SendMessage, $"⚠️ Failed to process template for {item.employee_id}: {ex.Message}");
                        }
                    }

                    if (!matchFound)
                    {
                        MessageBox.Show("❌ No match found. Please try again.", "No Match", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                SendMessage(Action.SendMessage, "Place another finger on the reader to match again.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("⚠️ Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Identification_Closed(object sender, EventArgs e)
        {
            _sender.CancelCaptureAndCloseReader(OnCaptured);
        }

        #region WebSocket Client Connection
        private async Task ConnectWebSocketAsync()
        {
            try
            {
                wsClient = new ClientWebSocket();
                await wsClient.ConnectAsync(new Uri("ws://localhost:8080"), CancellationToken.None);
                SendMessage(Action.SendMessage, "✅ Connected to WebSocket server.");

                // Optional: listen to incoming messages (not required but useful for debugging)
                //_ = Task.Run(async () =>
                //{
                //    var buffer = new byte[1024];
                //    while (wsClient.State == WebSocketState.Open)
                //    {
                //        var result = await wsClient.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                //        if (result.MessageType == WebSocketMessageType.Text)
                //        {
                //            string msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                //            SendMessage(Action.SendMessage, $"📩 From server: {msg}");
                //        }
                //    }
                //});
            }
            catch (Exception ex)
            {
                SendMessage(Action.SendMessage, $"⚠️ WebSocket connection failed: {ex.Message}");
            }
        }
        #endregion

        #region SendMessage
        private enum Action
        {
            SendMessage
        }

        private delegate void SendMessageCallback(Action action, string payload);

        private void SendMessage(Action action, string payload)
        {
            try
            {
                if (txtIdentify.InvokeRequired)
                {
                    var d = new SendMessageCallback(SendMessage);
                    Invoke(d, new object[] { action, payload });
                }
                else
                {
                    if (action == Action.SendMessage)
                    {
                        txtIdentify.Text += payload + "\r\n\r\n";
                        txtIdentify.SelectionStart = txtIdentify.TextLength;
                        txtIdentify.ScrollToCaret();
                    }
                }
            }
            catch { }
        }
        #endregion
    }

    public class StoredTemplate
    {
        public string employee_id { get; set; }
        public string template { get; set; }
    }
}
