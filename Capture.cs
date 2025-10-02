using DPUruNet;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace UareUSampleCSharp
{
    public partial class Capture : Form
    {
        public Form_Main _sender;

        public Capture()
        {
            InitializeComponent();
        }

        private void Capture_Load(object sender, EventArgs e)
        {
            pbFingerprint.Image = null;

            if (!_sender.OpenReader())
            {
                this.Close();
            }

            if (!_sender.StartCaptureAsync(this.OnCaptured))
            {
                this.Close();
            }
        }

        public async void OnCaptured(CaptureResult captureResult)
        {
            try
            {
                if (!_sender.CheckCaptureResult(captureResult)) return;

                foreach (Fid.Fiv fiv in captureResult.Data.Views)
                {
                    SendMessage(ActionType.SendBitmap, _sender.CreateBitmap(fiv.RawImage, fiv.Width, fiv.Height));

                    // ✅ Call API to log attendance
                    await SendAttendanceAsync();
                }
            }
            catch (Exception ex)
            {
                SendMessage(ActionType.SendMessage, "Error: " + ex.Message);
            }
        }

        private async Task SendAttendanceAsync()
        {
            var data = new
            {
                employee_id = "123456", // You can make this dynamic later
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            var json = JsonConvert.SerializeObject(data); // Newtonsoft.Json
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.PostAsync("http://127.0.0.1:8000/api/attendance", content);

                    if (response.IsSuccessStatusCode)
                    {
                        SendMessage(ActionType.SendMessage, "✅ Attendance sent successfully.");
                    }
                    else
                    {
                        SendMessage(ActionType.SendMessage, $"❌ Failed to send attendance. Status: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    SendMessage(ActionType.SendMessage, "❌ Error sending data: " + ex.Message);
                }
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Capture_Closed(object sender, EventArgs e)
        {
            _sender.CancelCaptureAndCloseReader(this.OnCaptured);
        }

        #region SendMessage
        private enum ActionType
        {
            SendBitmap,
            SendMessage
        }

        private delegate void SendMessageCallback(ActionType action, object payload);

        private void SendMessage(ActionType action, object payload)
        {
            try
            {
                if (this.pbFingerprint.InvokeRequired)
                {
                    SendMessageCallback d = new SendMessageCallback(SendMessage);
                    this.Invoke(d, new object[] { action, payload });
                }
                else
                {
                    switch (action)
                    {
                        case ActionType.SendMessage:
                            MessageBox.Show((string)payload);
                            break;
                        case ActionType.SendBitmap:
                            pbFingerprint.Image = (Bitmap)payload;
                            pbFingerprint.Refresh();
                            break;
                    }
                }
            }
            catch (Exception)
            {
                // Optional: log or handle error
            }
        }
        #endregion
    }
}
