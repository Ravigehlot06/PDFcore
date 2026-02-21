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
        private string _filePath;

        public ViewerForm(string filePath)
        {
            InitializeComponent();
            _filePath = filePath;

            this.Load += ViewerForm_Load;
        }

        private async void ViewerForm_Load(object sender, EventArgs e)
        {
            await webView21.EnsureCoreWebView2Async();
            webView21.Source = new Uri(_filePath);
        }
    }
}
