using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        //private ObservableCollection<DirectoryInfo> folders;
        //private ObservableCollection<FileInfo> images;

        private DirectoryManager directoryManager;

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

            directoryManager = new DirectoryManager();

            checkBox.IsChecked = showFolders;

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

            folderListBox.ItemsSource = directoryManager.SelectedDirectories;
            imageListBox.ItemsSource = directoryManager.SelectedFiles;

            folderCountLabel.DataContext = directoryManager.SelectedDirectories;
            imageCountLabel.DataContext = directoryManager.SelectedFiles;

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
                System.Windows.MessageBox.Show("Error: Could not find the logo image. Please select a new logo.", "Logo not found.", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Cancel = true;
                return;
            }

            ObservableCollection<FileInfo> images = directoryManager.SelectedFiles;
            string tagPosition = (string)e.Argument;

            FileInfo destination;
            int percentageCount = 0;

            // Tag each image and update the report the current progress.
            foreach (FileInfo image in images)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                destination = new FileInfo(Path.Combine(image.DirectoryName, "Tagged", image.Name));

                ImageTagger.TagImage(image, destination, logoInfo, tagPosition);

                // Calculate the percentage and report it so it can be retreived outside the worker.
                percentageCount++;
                worker.ReportProgress((int)(((double)(percentageCount) / images.Count) * 100));
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
                    directoryManager.OpenDirectories();
                }
                directoryManager.ClearAll();
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

        private void selectImages()
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();

            dialog.Multiselect = true;
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            dialog.Filter = "Image Files (*.jpeg, *.jpg, *.png)|*.jpeg;*.png;*.jpg";

            Nullable<bool> result = dialog.ShowDialog();

            if (result != null)
            {
                directoryManager.AddFiles(dialog.FileNames);
            }
        }

        private void selectDirectory()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                directoryManager.AddDirectory(dialog.SelectedPath);
            }
        }

        private void removeImages()
        {
            var selectedFiles = imageListBox.SelectedItems;

            for (int i = selectedFiles.Count - 1; i >= 0; i--)
            {
                directoryManager.RemoveFile((FileInfo)selectedFiles[i]);
            }
        }

        private void removeDirectories()
        {
            var selectedDirectories = folderListBox.SelectedItems;

            for (int i = selectedDirectories.Count - 1; i >= 0; i--)
            {
                DirectoryInfo directory = (DirectoryInfo)selectedDirectories[i];
                directoryManager.RemoveDirectory(directory);
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
            selectDirectory();
        }

        private void selectImagesButton_Click(object sender, RoutedEventArgs e)
        {
            selectImages();
        }


        private void deleteFolderButton_Click(object sender, RoutedEventArgs e)
        {
            removeDirectories();
        }

        private void deleteImageButton_Click(object sender, RoutedEventArgs e)
        {
            removeImages();
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
