using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Configuration;

/// <summary>
/// Bind a collection of folders to the folder listbox and a collection of images to the listbox.
/// When a new folder is added, add all the images in that folder to the images.
/// When a folder is deleted, remove all the images located in that folder (e.g. store the image name, path and folder)
/// If no more images from a folder are in the list, remove the folder (try a LINQ query?)
/// </summary>

namespace Photo_Tagger_V2_0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<DirectoryInfo> folders;
        private ObservableCollection<FileInfo> images;

        private bool showFolders;
        private FileInfo logoInfo;

        private readonly BackgroundWorker worker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string savedPosition;

            // Try to load values from the configuration file, if any cannot be found reset the file.
            try
            {
                showFolders = config.AppSettings.Settings["showFolders"].Value == "True";
                logoInfo = new FileInfo(config.AppSettings.Settings["logoPath"].Value);
                savedPosition = config.AppSettings.Settings["position"].Value;
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Error loading configuration file.\nUser settings have been reset.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                showFolders = true;
                logoInfo = new FileInfo("logo.png");
                savedPosition = "Bottom Left";

                resetConfig();
            }
            

            checkBox.IsChecked = showFolders;

            folders = new ObservableCollection<DirectoryInfo>();
            images = new ObservableCollection<FileInfo>();

            List<string> positions = new List<string>
            {
                "Watermark Large",
                "Watermark Tiled",
                "Centre",
                "Top Left",
                "Top",
                "Top Right",
                "Right",
                "Bottom Right",
                "Bottom",
                "Bottom Left",
                "Left"
            };

            positionComboBox.ItemsSource = positions;

            // If the stored value does not match (e.g. corrupted or set to invalid option by user)
            // set the default value and update the configuration file.
            if (!positions.Contains(savedPosition))
            {
                savedPosition = "Bottom Left";

                config.AppSettings.Settings["position"].Value = savedPosition;
                config.Save(ConfigurationSaveMode.Modified, true);
            }
            
            positionComboBox.SelectedIndex = positions.IndexOf(savedPosition);

            folderListBox.ItemsSource = folders;
            imageListBox.ItemsSource = images;

            folderCountLabel.DataContext = folders;
            imageCountLabel.DataContext = images;

            // Setup the background worker
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
        }

        /// <summary>
        /// Adds default values to the application's configuration file.
        /// </summary>
        private void resetConfig()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            config.AppSettings.Settings.Clear();
            config.AppSettings.Settings.Add("showFolders", "True");
            config.AppSettings.Settings.Add("logoPath", "logo.png");
            config.AppSettings.Settings.Add("position", "Bottom Left");

            config.Save(ConfigurationSaveMode.Modified, true);
        }

        #region Worker Functions

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            // If the logo cannot be found display an error message and cancel the task.
            if (!logoInfo.Exists)
            {
                System.Windows.MessageBox.Show("Error: Could not find logo to tag image with. Please ensure file exists, or select new logo.", "Could not find tag.", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Cancel = true;
                return;
            }

            ImageTagger tagger = new ImageTagger((string)e.Argument);

            for (int i = 0; i < images.Count; i++)
            {
                // Calculate the percentage and report it so it can be retreived outside the worker.
                worker.ReportProgress((int)(((double)(i + 1) / images.Count) * 100));

                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                System.Drawing.Image newImage = tagger.TagImage(images[i], logoInfo);
                
                saveImage(newImage, new FileInfo(System.IO.Path.Combine(images[i].DirectoryName, "Tagged", images[i].Name)));

                newImage.Dispose();
            }
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            System.Media.SystemSounds.Beep.Play();

            if (e.Cancelled)
            {
                System.Windows.MessageBox.Show("Tag operation cancelled.", "Cancelled.", MessageBoxButton.OK, MessageBoxImage.Stop);
            }

            else if(e.Error != null)
            {
                System.Windows.MessageBox.Show(string.Format("Error: {0}", "Error message....."), "Error", MessageBoxButton.OKCancel);
            }

            else
            {
                if (checkBox.IsChecked == true)
                {
                    openFolders();
                }
                images.Clear();
                folders.Clear();
            }

            
            cancelButton.Visibility = Visibility.Collapsed;
            tagButton.Visibility = Visibility.Visible;
            progressBar.Value = 0;
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Starts tagging images with a background worker if it is not already running.
        /// </summary>
        private void startTask()
        {
            if (worker.IsBusy != true)
            {
                tagButton.Visibility = Visibility.Collapsed;
                cancelButton.Visibility = Visibility.Visible;

                progressBar.Value = 0;

                worker.RunWorkerAsync((string)positionComboBox.SelectedValue);
            }
        }

        /// <summary>
        /// Tells the background worker to cancel it's current task.
        /// </summary>
        private void cancelTask()
        {
            if (worker.WorkerSupportsCancellation)
            {
                tagButton.Visibility = Visibility.Visible;
                cancelButton.Visibility = Visibility.Collapsed;

                worker.CancelAsync();
            }
        }


        private void saveImage(System.Drawing.Image image, FileInfo fileInfo)
        {
            ImageFormat imgFormat = ImageFormat.Jpeg;

            if (fileInfo.Extension == ".png")
                imgFormat = ImageFormat.Png;

    
            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }
            
            image.Save(fileInfo.FullName, imgFormat);
        }

        /// <summary>
        /// Opens a file explorer window for each of the directories currently selected. 
        /// </summary>
        private void openFolders()
        {
            foreach (DirectoryInfo directory in folders)
            {
                Process.Start(System.IO.Path.Combine(directory.FullName, "Tagged"));
            }
        }
  

        /// <summary>
        /// Adds all the images from the specifed directory to the list of selected images.
        /// And appends the directory to the list of selected directories.
        /// </summary>
        /// <param name="directory">Directory to add images from.</param>
        private void addImagesFromFolder(DirectoryInfo directory)
        {
            string[] extensions = { "*.jpg", "*.jpeg", "*.png" };

            List<FileInfo> imagesInDir = new List<FileInfo>();

            // Iterate through all the desired file types and add the images in that directory to a list.
            foreach (string ext in extensions)
            {
                imagesInDir.AddRange(directory.EnumerateFiles(ext));
            }
             
            // For each of the images in the directory, if it is not already in the list of selected images add it to the list.
            foreach(FileInfo image in imagesInDir)
            {
                if (!isImageInList(image, images))
                {
                    images.Add(image);
                }
            }
        }

        /// <summary>
        /// Adds the directory of an image to the selected directories list if it is not already in there.
        /// </summary>
        /// <param name="image">The image you wish to add the directory for.</param>
        private void addImageFolder(FileInfo image)
        {
            if (!isFolderInList(image.Directory, folders))
            {
                folders.Add(image.Directory);
            }
        }


        /// <summary>
        /// Returns true if the directory specified is already in the list specified.
        /// </summary>
        /// <param name="newDirectory">Directory to check</param>
        /// <param name="directories">List of directories to check against.</param>
        /// <returns></returns>
        private bool isFolderInList(DirectoryInfo newDirectory, ObservableCollection<DirectoryInfo> directories)
        {
            foreach (DirectoryInfo directory in directories)
            {
                if (directory.FullName == newDirectory.FullName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if the image specified is already in the list of images (file paths).
        /// </summary>
        /// <param name="newImage">Image to check.</param>
        /// <param name="imageList">List of images to check against.</param>
        /// <returns></returns>
        private bool isImageInList(FileInfo newImage, ObservableCollection<FileInfo> imageList)
        {
            foreach (FileInfo image in imageList)
            {
                if (image.FullName == newImage.FullName)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Removes a directory from the list of selected directories if it exists.
        /// </summary>
        /// <param name="folder">Directory to remove.</param>
        private void removeFolder(DirectoryInfo folder)
        {
            for (int i = 0; i < folders.Count; i++)
            {
                if (folder.FullName == folders[i].FullName)
                {
                    folders.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Removes all the images within a directory from the list of selected images.
        /// </summary>
        /// <param name="folder">Directory containing the images to remove.</param>
        private void removeImagesInFolder(DirectoryInfo folder)
        {
            for (int i = images.Count - 1; i >= 0; i--)
            {
                if (images[i].DirectoryName == folder.FullName)
                {
                    images.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Removes an image from the list of selected images if it exists.
        /// </summary>
        /// <param name="image">Image to remove.</param>
        private void removeImage(FileInfo image)
        {
            for (int i = images.Count - 1; i >= 0; i--)
            {
                if (image.FullName == images[i].FullName)
                {
                    images.RemoveAt(i);
                    break;
                }
            }
        }


        /// <summary>
        /// Removes any directories from the selected directories list if there are no longer any images
        /// in the selected images list which are children of them.
        /// </summary>
        private void checkFoldersStillNeeded()
        {
            bool used;

            // For each directory check if there are any images with it as a parent. If not, remove the directory.
            for (int i = folders.Count - 1; i >= 0; i--)
            {
                used = false;

                foreach (FileInfo image in images)
                {
                    if (image.DirectoryName == folders[i].FullName)
                    {
                        used = true;
                        break;
                    }
                }

                if (!used)
                {
                    folders.RemoveAt(i);
                }
            }
        }

        #endregion

        #region Event Handlers

        private void tagButton_Click(object sender, RoutedEventArgs e)
        {
            startTask();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            cancelTask();
        }


        private void selectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                DirectoryInfo newFolder = new DirectoryInfo(dialog.SelectedPath);

                if (!isFolderInList(newFolder, folders))
                {
                    folders.Add(newFolder);
                }
                addImagesFromFolder(newFolder);
            }
        }

        private void selectImagesButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();

            dialog.Multiselect = true;
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            dialog.Filter = "Image Files (*.jpeg, *.jpg, *.png)|*.jpeg;*.png;*.jpg";

            Nullable<bool> result = dialog.ShowDialog();

            if (result != null)
            {
                foreach (string fileName in dialog.FileNames)
                {
                    FileInfo newImage = new FileInfo(fileName);

                    if (!isImageInList(newImage, images))
                    {
                        images.Add(newImage);
                        addImageFolder(newImage);

                    }
                }
            }
        }


        private void deleteFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedFolders = folderListBox.SelectedItems;

            for (int i = selectedFolders.Count - 1; i >= 0; i--)
            {
                DirectoryInfo folder = (DirectoryInfo)selectedFolders[i];
                removeFolder(folder);
                removeImagesInFolder(folder);
            }
            
        }

        private void deleteImageButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedImages = imageListBox.SelectedItems;
            
            for (int i = selectedImages.Count - 1; i >= 0; i--)
            {
                removeImage((FileInfo)selectedImages[i]);
            }
            checkFoldersStillNeeded();

        }
        

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            showFolders = true;

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["showFolders"].Value = true.ToString();
            config.Save(ConfigurationSaveMode.Modified, true);
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            showFolders = false;

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["showFolders"].Value = false.ToString();
            config.Save(ConfigurationSaveMode.Modified, true);
        }


        private void changeLogoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.InitialDirectory = logoInfo.DirectoryName;
            dialog.Filter = "Image Files (*.jpeg, *.jpg, *.png)|*.jpeg;*.png;*.jpg";

            DialogResult result = dialog.ShowDialog();

            if(result == System.Windows.Forms.DialogResult.OK)
            {
                logoInfo = new FileInfo(dialog.FileName);

                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["logoPath"].Value = logoInfo.FullName;
                config.Save(ConfigurationSaveMode.Modified, true);
            }
        }

        private void resetLogoButton_Click(object sender, RoutedEventArgs e)
        {
            logoInfo = new FileInfo("logo.png");

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["logoPath"].Value = "logo.png";
            config.Save(ConfigurationSaveMode.Modified, true);

            System.Windows.MessageBox.Show("The logo has been reset.", "Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void positionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["position"].Value = (string)positionComboBox.SelectedValue;
            config.Save(ConfigurationSaveMode.Modified, true);
        }

        #endregion
    }
}
