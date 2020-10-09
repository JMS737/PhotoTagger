using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoTagger_Classic
{
    public partial class PhotoTaggerForm : Form
    {

        private IList<String> folderPaths = new List<String>();
        private IList<String> images = new List<String>();
        private String logoPath;

        public PhotoTaggerForm()
        {
            InitializeComponent();
            updateList();
            logoPath = "logo.png";
        }

        private void PickFolderButton_Click(object sender, EventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                folderPaths.Add(dialog.SelectedPath);

                updateList();
            }
        }

        private IList<String> getImagesInFolders(IList<String> folders)
        {
            IList<String> images = new List<String>();
            folderListText.Text = "";
            foreach (String folderPath in folders)
            {
                images = addList(images, getImagesInFolder(folderPath));

            }
            return images;
        }

        private IList<String> addList(IList<String> list1, IList<String> list2)
        {
            foreach (String item in list2)
            {
                list1.Add(item);
            }
            return list1;
        }

        private IList<String> getImagesInFolder(String folderPath)
        {
            IList<String> imagePaths = new List<String>();

            var jpgs = Directory.EnumerateFiles(folderPath, "*.jpg");
            var jpegs = Directory.EnumerateFiles(folderPath, "*.jpeg");
            var pngs = Directory.EnumerateFiles(folderPath, "*.png");

            foreach (string filePath in jpgs)
            {
                imagePaths.Add(filePath);
            }
            foreach (String filePath in jpegs)
            {
                imagePaths.Add(filePath);
            }
            foreach (String filePath in pngs)
            {
                imagePaths.Add(filePath);
            }
            
            return imagePaths;
        }

        private void updateList()
        {
            if (folderPaths.Count > 0)
            {
                clearButton.Enabled = true;
                tagButton.Enabled = true;

                folderListText.Text = "";

                foreach (String folder in folderPaths)
                {
                    folderListText.Text += (folder + "\n");

                }
            }
            else
            {
                clearButton.Enabled = false;
                tagButton.Enabled = false;
                folderListText.Text = "No folders selected...";
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            folderPaths.Clear();
            updateList();
        }

        private Image applyEXIFRotation(Image image)
        {
            if (Array.IndexOf(image.PropertyIdList, 274) > 1)
            {
                var orientation = (int)image.GetPropertyItem(274).Value[0];

                switch (orientation)
                {
                    case 1:
                        // Not rotation required
                        break;
                    case 2:
                        image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case 3:
                        image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 4:
                        image.RotateFlip(RotateFlipType.Rotate180FlipX);
                        break;
                    case 5:
                        image.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case 6:
                        image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case 7:
                        image.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case 8:
                        image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }
                image.RemovePropertyItem(274);
            }
            return image;
        }

        private Bitmap applyEXIFData(Image inputImage, Bitmap outputImage)
        {
            foreach (var id in inputImage.PropertyIdList)
            {
                outputImage.SetPropertyItem(inputImage.GetPropertyItem(id));
            }
            
            return outputImage;
        }

        private Bitmap tagImage(Image imageToTag)
        {
            Image logo = Image.FromFile(logoPath);
            imageToTag = applyEXIFRotation(imageToTag);
            double scaleFactor = 1;
            double ratio = .2;
            if (imageToTag.Width <= imageToTag.Height)
            {
                ratio = .4;
            }
            int logoW = (int)((double)imageToTag.Width * ratio);
            scaleFactor = (double)logoW / (double)logo.Width;

            Bitmap logoScaled = ResizeImage(logo, scaleFactor);

            Bitmap outputImage = new Bitmap(imageToTag);
            outputImage = applyEXIFData(imageToTag, outputImage);

            using (Graphics graphics = Graphics.FromImage(outputImage))
            {
                graphics.DrawImage(logoScaled, new Point(0, imageToTag.Height - logoScaled.Height));
            }
            logo.Dispose();
            logoScaled.Dispose();
            imageToTag.Dispose();
            return outputImage;
        }

        private Bitmap ResizeImage(Image image, double factor)
        {
            int width = (int)(image.Width * factor);
            int height = (int)(image.Height * factor);
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
            }
            image.Dispose();
            
            return destImage;
        }

        private String getNewFileName(String fileName)
        {
            String newDirectory = fileName.Substring(0, fileName.LastIndexOf('\\') + 1) + "Tagged";
            String newFileName = newDirectory + fileName.Substring(fileName.LastIndexOf('\\'));

            bool exists = System.IO.Directory.Exists(newDirectory);
            if (!exists)
            {
                System.IO.Directory.CreateDirectory(newDirectory);
            }

            return newFileName;
        }

        private void tagButton_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy != true)
            {
                tagButton.Enabled = false;
                cancelTagButton.Enabled = true;
                images = getImagesInFolders(folderPaths);
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                folderListText.Text = "Tagging Images...\n";
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void cancelTagButton_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.WorkerSupportsCancellation)
            {
                tagButton.Enabled = true;
                cancelTagButton.Enabled = false;
                backgroundWorker1.CancelAsync();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            int counter = 0;
            foreach (String fileName in images)
            {
                counter++;
                worker.ReportProgress((int)(((float)counter / images.Count) * 100), counter);
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                Bitmap newImage = tagImage(Image.FromFile(fileName));
                ImageFormat imgFormat = ImageFormat.Jpeg;

                if (fileName.Substring(fileName.LastIndexOf('.')) == ".png")
                {
                    imgFormat = ImageFormat.Png;
                }
                newImage.Save(getNewFileName(fileName), imgFormat);
                newImage.Dispose();
                
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                folderListText.Text = "Cancelled";
            }
            else if (e.Error != null)
            {
                folderListText.Text = "Error: " + e.Error.Message;
            }
            else
            {
                folderListText.Text = "Done!";
                SystemSounds.Beep.Play();
                if (openFolderCheckBox.Checked)
                {
                    foreach (String folderPath in folderPaths)
                    {
                        Process.Start(@"" + folderPath + "\\Tagged");
                    }
                }
                
                
            }
            progressBar1.Value = 0;
            folderPaths.Clear();
            images.Clear();
            cancelTagButton.Enabled = false;
            updateList();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            folderListText.Text = "Tagging image " + (int)e.UserState + "/" + images.Count + "\n";
        }

        private void LogoPreview_Click(object sender, EventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "Image files (*.jpg;*.png;*.jpeg)|*.jpg;*.png;*.jpeg";
            dialog.FilterIndex = 0;
            //dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                logoPath = dialog.FileName;
                LogoPreview.ImageLocation = logoPath;
            }
        }
    }
}
