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
using PdfiumViewer;
using System.Drawing.Imaging;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Pdf.Canvas;


namespace PDFcore
{
    public partial class ViewerForm : Form
    {
        private string _currentFilePath;

        float clickX;
        float clickY;

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
                using (iText.Kernel.Pdf.PdfDocument srcDoc = new iText.Kernel.Pdf.PdfDocument(reader))
                using (iText.Kernel.Pdf.PdfDocument destDoc = new iText.Kernel.Pdf.PdfDocument(writer))
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

        private void pDFToImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                MessageBox.Show("No PDF is open.");
                return;
            }

            FolderBrowserDialog folderDialog = new FolderBrowserDialog();

            if (folderDialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                using (var document = PdfiumViewer.PdfDocument.Load(_currentFilePath))
                {
                    for (int i = 0; i < document.PageCount; i++)
                    {
                        using (var image = document.Render(i, 300, 300, true))
                        {
                            string outputPath = Path.Combine(
                                folderDialog.SelectedPath,
                                $"Page_{i + 1}.png");

                            image.Save(outputPath, ImageFormat.Png);
                        }
                    }
                }

                MessageBox.Show("PDF converted to images successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void mergePDFsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                MessageBox.Show("No PDF is currently open.");
                return;
            }

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "PDF Files|*.pdf";
            ofd.Title = "Select PDF to Merge";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            string secondPdfPath = ofd.FileName;

            try
            {
                // Create temp merged file
                string mergedFilePath = Path.Combine(
                    Path.GetTempPath(),
                    Guid.NewGuid().ToString() + "_merged.pdf");

                using (iText.Kernel.Pdf.PdfWriter writer =
                       new iText.Kernel.Pdf.PdfWriter(mergedFilePath))
                using (iText.Kernel.Pdf.PdfDocument destDoc =
                       new iText.Kernel.Pdf.PdfDocument(writer))
                {
                    // First PDF (currently open)
                    using (iText.Kernel.Pdf.PdfReader reader1 =
                           new iText.Kernel.Pdf.PdfReader(_currentFilePath))
                    using (iText.Kernel.Pdf.PdfDocument srcDoc1 =
                           new iText.Kernel.Pdf.PdfDocument(reader1))
                    {
                        srcDoc1.CopyPagesTo(1, srcDoc1.GetNumberOfPages(), destDoc);
                    }

                    // Second selected PDF
                    using (iText.Kernel.Pdf.PdfReader reader2 =
                           new iText.Kernel.Pdf.PdfReader(secondPdfPath))
                    using (iText.Kernel.Pdf.PdfDocument srcDoc2 =
                           new iText.Kernel.Pdf.PdfDocument(reader2))
                    {
                        srcDoc2.CopyPagesTo(1, srcDoc2.GetNumberOfPages(), destDoc);
                    }
                }

                MessageBox.Show("PDFs merged successfully!");

                // Open merged file in new window
                ViewerForm newViewer = new ViewerForm(mergedFilePath);
                newViewer.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void imageToPDFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            ofd.Multiselect = true;
            ofd.Title = "Select Image(s)";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                string outputPath = Path.Combine(
                    Path.GetTempPath(),
                    Guid.NewGuid().ToString() + "_images.pdf");

                using (iText.Kernel.Pdf.PdfWriter writer =
                       new iText.Kernel.Pdf.PdfWriter(outputPath))
                using (iText.Kernel.Pdf.PdfDocument pdf =
                       new iText.Kernel.Pdf.PdfDocument(writer))
                using (iText.Layout.Document document =
                       new iText.Layout.Document(pdf))
                {
                    foreach (string imagePath in ofd.FileNames)
                    {
                        iText.Layout.Element.Image img =
                            new iText.Layout.Element.Image(
                                iText.IO.Image.ImageDataFactory.Create(imagePath));

                        img.SetAutoScale(true);

                        document.Add(img);
                        document.Add(new iText.Layout.Element.AreaBreak());
                    }
                }

                MessageBox.Show("Images converted to PDF successfully!");

                ViewerForm newViewer = new ViewerForm(outputPath);
                newViewer.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void addTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                MessageBox.Show("No PDF open.");
                return;
            }

            string text = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter text to add:",
                "Add Text",
                "");

            if (string.IsNullOrWhiteSpace(text))
                return;

            string pageInput = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter page number:",
                "Page Number",
                "1");

            int pageNumber = int.Parse(pageInput);

            string tempFile = Path.Combine(Path.GetTempPath(), "addtext.pdf");

            PdfReader reader = new PdfReader(_currentFilePath);
            PdfWriter writer = new PdfWriter(tempFile);
            iText.Kernel.Pdf.PdfDocument pdfDoc = new iText.Kernel.Pdf.PdfDocument(reader, writer);

            PdfPage page = pdfDoc.GetPage(pageNumber);

            PdfCanvas canvas = new PdfCanvas(page);

            canvas.BeginText();
            canvas.SetFontAndSize(
                PdfFontFactory.CreateFont(StandardFonts.HELVETICA),
                20
            );

            float pdfY = page.GetPageSize().GetHeight() - clickY;

            canvas.MoveText(clickX, pdfY);
            canvas.ShowText(text);
            canvas.EndText();

            pdfDoc.Close();
            reader.Close();
            writer.Close();

            _currentFilePath = tempFile;
            webView21.Source = new Uri(_currentFilePath);
        }

        private void webView21_MouseClick(object sender, MouseEventArgs e)
        {
            clickX = e.X;
            clickY = e.Y;

            MessageBox.Show("Text will be placed here");
        }

        private void deletePageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter page number to delete",
                "Delete Page");

            int pageNumber = int.Parse(input);

            string output = Path.Combine(Path.GetTempPath(), "output.pdf");

            PdfReader reader = new PdfReader(_currentFilePath);
            PdfWriter writer = new PdfWriter(output);

            iText.Kernel.Pdf.PdfDocument pdfDoc =
                new iText.Kernel.Pdf.PdfDocument(reader, writer);

            if (pageNumber < 1 || pageNumber > pdfDoc.GetNumberOfPages())
            {
                MessageBox.Show("Invalid page number");
                pdfDoc.Close();
                return;
            }

            pdfDoc.RemovePage(pageNumber);

            pdfDoc.Close();

            _currentFilePath = output;
            webView21.CoreWebView2.Navigate(_currentFilePath);

            MessageBox.Show("Page deleted successfully.");
        }

        private void movePageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string pageInput = Microsoft.VisualBasic.Interaction.InputBox(
            "Enter page number to move:", "Move Page", "");

            string positionInput = Microsoft.VisualBasic.Interaction.InputBox(
            "Enter new position:", "Move Page", "");

            if (string.IsNullOrWhiteSpace(pageInput) || string.IsNullOrWhiteSpace(positionInput))
                return;

            int pageNumber = int.Parse(pageInput);
            int newPosition = int.Parse(positionInput);

            string tempFile = Path.Combine(Path.GetTempPath(), "output_temp.pdf");

            PdfReader reader = new PdfReader(_currentFilePath);
            PdfWriter writer = new PdfWriter(tempFile);

            iText.Kernel.Pdf.PdfDocument pdfDoc = new iText.Kernel.Pdf.PdfDocument(reader, writer);

            pdfDoc.MovePage(pageNumber, newPosition);

            pdfDoc.Close();

            _currentFilePath = tempFile;

            webView21.CoreWebView2.Navigate(_currentFilePath);

            MessageBox.Show("Page moved successfully.");
        }
    }
}
