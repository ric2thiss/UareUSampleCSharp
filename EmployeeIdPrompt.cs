using System;
using System.Windows.Forms;

namespace UareUSampleCSharp
{
    public partial class EmployeeIdPrompt : Form
    {
        // Property to get the entered employee ID
        public string EmployeeId { get; private set; }

        public EmployeeIdPrompt()
        {
            InitializeComponent();
        }

        private void EmployeeIdPrompt_Load(object sender, EventArgs e)
        {
            // Optional: focus the input when form loads
            txtEmployeeId.Focus();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmployeeId.Text))
            {
                MessageBox.Show("Please enter a valid Employee ID.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            EmployeeId = txtEmployeeId.Text.Trim();
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void txtEmployeeId_TextChanged(object sender, EventArgs e)
        {
            // You can leave this empty unless you want to handle live typing events.
        }
    }
}
