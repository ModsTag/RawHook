using System.Windows.Forms;

namespace RawHook
{
    /// <summary>
    /// using Pipe to connect and w/r data
    /// </summary>
    public class ServerForm : Form
    {
        public ServerForm()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            SuspendLayout();
            Hide();
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            ClientSize = new System.Drawing.Size(128, 128);

            Name = "Program";
            ResumeLayout(false);

        }
    }
}
