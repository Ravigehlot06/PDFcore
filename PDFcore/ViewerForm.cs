using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace PDFcore
{
    public partial class ViewerForm : Form
    {
        private string _currentFilePath;

        public ViewerForm(string filePath)
        {
            InitializeComponent();
            _currentFilePath = filePath;

            this.Load += ViewerForm_Load;
        }

        private async void ViewerForm_Load(object sender, EventArgs e)
        {
            await webView21.EnsureCoreWebView2Async();
            webView21.Source = new Uri(_currentFilePath);
        }

        private async void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "PDF Files|*.pdf";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _currentFilePath = ofd.FileName;

                await webView21.EnsureCoreWebView2Async();
                webView21.Source = new Uri(_currentFilePath);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                MessageBox.Show("No file open.");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PDF Files|*.pdf";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.Copy(_currentFilePath, sfd.FileName, true);
                MessageBox.Show("File saved successfully.");
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                MessageBox.Show("No file open.");
                return;
            }

            MessageBox.Show("File is already up to date.");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
