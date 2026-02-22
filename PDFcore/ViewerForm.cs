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
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using System.IO;

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

        private void splitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                MessageBox.Show("No PDF is open.");
                return;
            }

            string input = "";

            // Custom small input dialog
            using (Form prompt = new Form())
            {
                prompt.Width = 300;
                prompt.Height = 150;
                prompt.Text = "Split PDF";

                Label textLabel = new Label()
                {
                    Left = 10,
                    Top = 20,
                    Width = 260,
                    Text = "Enter page range (Example: 2-5)"
                };

                TextBox inputBox = new TextBox()
                {
                    Left = 10,
                    Top = 50,
                    Width = 260
                };

                Button confirmation = new Button()
                {
                    Text = "OK",
                    Left = 100,
                    Width = 80,
                    Top = 80
                };

                confirmation.Click += (s, ev) => { prompt.Close(); };

                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(inputBox);
                prompt.Controls.Add(confirmation);

                prompt.ShowDialog();
                input = inputBox.Text;
            }

            if (string.IsNullOrWhiteSpace(input))
                return;

            try
            {
                string[] parts = input.Split('-');
                int startPage = int.Parse(parts[0]);
                int endPage = int.Parse(parts[1]);

                // 🔥 Create temporary file (no SaveFileDialog)
                string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf");

                using (PdfReader reader = new PdfReader(_currentFilePath))
                using (PdfWriter writer = new PdfWriter(tempFile))
                using (PdfDocument srcDoc = new PdfDocument(reader))
                using (PdfDocument destDoc = new PdfDocument(writer))
                {
                    srcDoc.CopyPagesTo(startPage, endPage, destDoc);
                }

                // 🔥 Open new window with split result
                ViewerForm newViewer = new ViewerForm(tempFile);
                newViewer.Show();
            }
            catch
            {
                MessageBox.Show("Invalid page range.");
            }
        }
    }
}
