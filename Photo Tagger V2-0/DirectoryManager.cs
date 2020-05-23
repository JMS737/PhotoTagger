using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace Photo_Tagger_V2_0
{
    class DirectoryManager
    {
        public ObservableCollection<DirectoryInfo> SelectedDirectories;
        public ObservableCollection<FileInfo> SelectedFiles;

        public DirectoryManager()
        {
            SelectedDirectories = new ObservableCollection<DirectoryInfo>();
            SelectedFiles = new ObservableCollection<FileInfo>();
        }

        /// <summary>
        /// Opens a file explorer window for each of the directories currently selected. 
        /// </summary>
        public void OpenDirectories()
        {
            foreach (DirectoryInfo directory in SelectedDirectories)
            {
                Process.Start(Path.Combine(directory.FullName, "Tagged"));
            }
        }

        public void AddFiles(string[] fileNames)
        {
            foreach (string fileName in fileNames)
            {
                FileInfo newFile = new FileInfo(fileName);
                AddFile(newFile);
            }
        }

        public void AddFile(FileInfo file)
        {
            if (isNewFile(file))
            {
                SelectedFiles.Add(file);
                addParentDirectory(file);
            }
        }

        public void AddFiles_fromDirectory(DirectoryInfo directory)
        {
            string[] extensions = { "*.jpg", "*.jpeg", "*.png" };

            List<FileInfo> filesInDir = new List<FileInfo>();

            // Iterate through all the desired file types and add the images in that directory to a list.
            foreach (string ext in extensions)
            {
                filesInDir.AddRange(directory.EnumerateFiles(ext));
            }

            // For each of the images in the directory, if it is not already in the list of selected images add it to the list.
            foreach (FileInfo file in filesInDir)
            {
                if (isNewFile(file))
                {
                    SelectedFiles.Add(file);
                }
            }
        }

        public void AddDirectory(DirectoryInfo directory)
        {
            if (isNewDirectory(directory))
            {
                SelectedDirectories.Add(directory);
            }
            AddFiles_fromDirectory(directory);
        }

        public void AddDirectory(string directory)
        {
            AddDirectory(new DirectoryInfo(directory));
        }

        private void addParentDirectory(FileInfo file)
        {
            if (isNewDirectory(file.Directory))
            {
                SelectedDirectories.Add(file.Directory);
            }
        }
        
        private bool isNewDirectory(DirectoryInfo newDirectory)
        {
            foreach (DirectoryInfo directory in SelectedDirectories)
            {
                if (directory.FullName == newDirectory.FullName)
                {
                    return false;
                }
            }

            return true;
        }

        private bool isNewFile(FileInfo newFile)
        {
            foreach (FileInfo file in SelectedFiles)
            {
                if (file.FullName == newFile.FullName)
                {
                    return false;
                }
            }

            return true;
        }
        
        public void RemoveDirectory(DirectoryInfo directory)
        {
            for (int i = 0; i < SelectedDirectories.Count; i++)
            {
                if (directory.FullName == SelectedDirectories[i].FullName)
                {
                    SelectedDirectories.RemoveAt(i);
                    removeAllChildFiles(directory);
                    return;
                }
            }
        }

        public void RemoveDirectory(string directory)
        {
            RemoveDirectory(new DirectoryInfo(directory));
        }

        public void RemoveDirectories(string[] directories)
        {
            foreach (string directory in directories)
            {
                RemoveDirectory(directory);
            }
        }

        public void RemoveDirectories(DirectoryInfo[] directories)
        {
            foreach (DirectoryInfo directory in directories)
            {
                RemoveDirectory(directory);
            }
        }
        
        private void removeAllChildFiles(DirectoryInfo directory)
        {
            for (int i = SelectedFiles.Count - 1; i >= 0; i--)
            {
                if (SelectedFiles[i].DirectoryName == directory.FullName)
                {
                    SelectedFiles.RemoveAt(i);
                }
            }
        }
        
        public void RemoveFile(FileInfo file)
        {
            for (int i = SelectedFiles.Count - 1; i >= 0; i--)
            {
                if (file.FullName == SelectedFiles[i].FullName)
                {
                    SelectedFiles.RemoveAt(i);
                    break;
                }
            }
            checkDirectoryStillNeeded();
        }

        public void RemoveFile(string file)
        {
            RemoveFile(new FileInfo(file));
        }

        public void RemoveFiles(FileInfo[] files)
        {
            foreach (FileInfo file in files)
            {
                RemoveFile(file);
            }
        }
        
        private void checkDirectoryStillNeeded()
        {
            bool used;

            // For each directory check if there are any images with it as a parent. If not, remove the directory.
            for (int i = SelectedDirectories.Count - 1; i >= 0; i--)
            {
                used = false;

                foreach (FileInfo file in SelectedFiles)
                {
                    if (file.DirectoryName == SelectedDirectories[i].FullName)
                    {
                        used = true;
                        break;
                    }
                }

                if (!used)
                {
                    SelectedDirectories.RemoveAt(i);
                }
            }
        }

        public void ClearAll()
        {
            SelectedDirectories.Clear();
            SelectedFiles.Clear();
        }
    }
}
