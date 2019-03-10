using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Craft2Git;
using Newtonsoft;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Security.Permissions;

namespace Craft2Git
{
    public class PackList : ObservableCollection<PackEntry>
    {
        public PackList()
        {
          
            
        }
    }

    public partial class MainWindow : Window
    {
        #region Class-wide Variables
        PackList[] leftListGroup;
        PackList[] rightListGroup;
        string defaultLeftFilePath = "";
        string defaultRightFilePath = "";
        string leftFilePath = "";   
        string rightFilePath = "";
        int leftTabSelected = 0;
        int rightTabSelected = 0;
        System.Windows.Data.Binding leftBinding1;
        System.Windows.Data.Binding leftBinding2;
        System.Windows.Data.Binding leftBinding3;
        System.Windows.Data.Binding leftBinding4;
        System.Windows.Data.Binding rightBinding1;
        System.Windows.Data.Binding rightBinding2;
        System.Windows.Data.Binding rightBinding3;
        System.Windows.Data.Binding rightBinding4;
        FileSystemWatcher leftWatcher;
        FileSystemWatcher rightWatcher;
        #endregion

        public MainWindow()
        {
            #region Load in defaults
            ////////////////////
            //Load in defaults//
            ////////////////////
            if (File.Exists(@"settings.txt"))
            {
                string[] lines = File.ReadAllLines(@"settings.txt");
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("leftDefaultPath="))
                    {
                        string[] stringSeparators = new string[] { "=" };
                        string[] splitLine = lines[i].Split(stringSeparators, StringSplitOptions.None);
                        
                        defaultLeftFilePath = splitLine[1];
                    }
                    else if (lines[i].StartsWith("rightDefaultPath="))
                    {
                        string[] stringSeparators = new string[] { "=" };
                        string[] splitLine = lines[i].Split(stringSeparators, StringSplitOptions.None);

                        defaultRightFilePath = splitLine[1];
                    }
                }
                
            }
            #endregion

            #region Left Side Init
            ////////////////////
            //Left Side init////
            ////////////////////

            leftListGroup = new PackList[4];
            for (int i = 0; i < leftListGroup.Length; i++)
            {
                leftListGroup[i] = new PackList();
            }

            leftBinding1 = new System.Windows.Data.Binding();
            leftBinding2 = new System.Windows.Data.Binding();
            leftBinding3 = new System.Windows.Data.Binding();
            leftBinding4 = new System.Windows.Data.Binding();
            leftBinding1.Source = leftListGroup[0];
            leftBinding2.Source = leftListGroup[1];
            leftBinding3.Source = leftListGroup[2];
            leftBinding4.Source = leftListGroup[3];


            #endregion

            #region Right Side Init
            ////////////////////
            //Right Side init///
            ////////////////////

            rightListGroup = new PackList[4];
            for (int i = 0; i < rightListGroup.Length; i++)
            {
                rightListGroup[i] = new PackList();
            }

            rightBinding1 = new System.Windows.Data.Binding();
            rightBinding2 = new System.Windows.Data.Binding();
            rightBinding3 = new System.Windows.Data.Binding();
            rightBinding4 = new System.Windows.Data.Binding();
            rightBinding1.Source = rightListGroup[0];
            rightBinding2.Source = rightListGroup[1];
            rightBinding3.Source = rightListGroup[2];
            rightBinding4.Source = rightListGroup[3];

            #endregion

            InitializeComponent();

            leftFilePath = defaultLeftFilePath;
            rightFilePath = defaultRightFilePath;
            leftList.SelectedIndex = leftTabSelected;
            rightList.SelectedIndex = rightTabSelected;

            UpdateLeftFocus();
            UpdateRightFocus();

            #region Left File Watcher
            //This region is based on an example from MSDN
            //https://docs.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher?redirectedfrom=MSDN&view=netframework-4.7.2
            leftWatcher = new FileSystemWatcher();
            if (Directory.Exists(leftFilePath) && leftFilePath != "")
            {
                leftWatcher.Path = leftFilePath;
            }
            else
            {
                leftFilePath = Directory.GetCurrentDirectory();
                leftWatcher.Path = Directory.GetCurrentDirectory();
            }
                
            leftWatcher.Filter = "*.*";
            leftWatcher.EnableRaisingEvents = true;
            leftWatcher.IncludeSubdirectories = true;

            leftWatcher.Created += OnLeftDirectoryChange;
            leftWatcher.Changed += OnLeftDirectoryChange;
            leftWatcher.Renamed += OnLeftDirectoryChange;
            leftWatcher.Deleted += OnLeftDirectoryChange;
            #endregion

            #region Right File Watcher
            rightWatcher = new FileSystemWatcher();
            if (Directory.Exists(rightFilePath) && rightFilePath != "")
            {
                rightWatcher.Path = rightFilePath;
            }
            else
            {
                rightFilePath = Directory.GetCurrentDirectory();
                rightWatcher.Path = Directory.GetCurrentDirectory();
            }

            rightWatcher.Filter = "*.*";
            rightWatcher.EnableRaisingEvents = true;
            rightWatcher.IncludeSubdirectories = true;

            rightWatcher.Created += OnRightDirectoryChange;
            rightWatcher.Changed += OnRightDirectoryChange;
            rightWatcher.Renamed += OnRightDirectoryChange;
            rightWatcher.Deleted += OnRightDirectoryChange;
            #endregion

            leftText.Text = leftFilePath;
            rightText.Text = rightFilePath;
        }

        private void LeftCopy(object sender, RoutedEventArgs e)
        {
            if (leftList.SelectedIndex > -1)
            {


                string sourceFilePath = System.IO.Path.GetDirectoryName(leftListGroup[leftTabSelected][leftList.SelectedIndex].filePath);
                string[] stringSeparators = new string[] { "\\" };
                string[] splitEntryPath = leftListGroup[leftTabSelected][leftList.SelectedIndex].filePath.Split(stringSeparators, StringSplitOptions.None);

                string destFilePath;
                if (leftTabSelected == 3)
                {
                    destFilePath = System.IO.Path.Combine(rightFilePath, splitEntryPath[splitEntryPath.Length - 2]);
                }
                else
                {
                    destFilePath = System.IO.Path.Combine(rightFilePath, splitEntryPath[splitEntryPath.Length - 3], splitEntryPath[splitEntryPath.Length - 2]);
                }
                rightWatcher.EnableRaisingEvents = false;
                DirectoryCopy(sourceFilePath, destFilePath, true);
                rightWatcher.EnableRaisingEvents = true;
                LoadRightPacks(rightFilePath);
                if (rightList.SelectedIndex == -1)
                {
                    rightList.SelectedIndex = 0;
                }
            }
        }

        private void RightCopy(object sender, RoutedEventArgs e)
        {
            if (rightList.SelectedIndex > -1)
            {


                string sourceFilePath = System.IO.Path.GetDirectoryName(rightListGroup[rightTabSelected][rightList.SelectedIndex].filePath);
                string[] stringSeparators = new string[] { "\\" };
                string[] splitEntryPath = rightListGroup[rightTabSelected][rightList.SelectedIndex].filePath.Split(stringSeparators, StringSplitOptions.None);

                string destFilePath;
                if (rightTabSelected == 3)
                {
                    destFilePath = System.IO.Path.Combine(leftFilePath, splitEntryPath[splitEntryPath.Length - 2]);
                }
                else
                {
                    destFilePath = System.IO.Path.Combine(leftFilePath, splitEntryPath[splitEntryPath.Length - 3], splitEntryPath[splitEntryPath.Length - 2]);
                }
                leftWatcher.EnableRaisingEvents = false;
                DirectoryCopy(sourceFilePath, destFilePath, true);
                leftWatcher.EnableRaisingEvents = true;
                LoadLeftPacks(leftFilePath);
                if (leftList.SelectedIndex == -1)
                {
                    leftList.SelectedIndex = 0;
                }
            }
        }

        private void LoadLeftPacks(string filePath)
        {
            
            #region Behavior Packs
            ////////////////////
            //Behavior packs////
            ////////////////////
            string[] subDirectories;
            leftListGroup[0].Clear();
            try
            {
                subDirectories = Directory.GetDirectories(System.IO.Path.Combine(filePath, "development_behavior_packs"));

                for (int i = 0; i < subDirectories.Length; i++)
                {
                    string filePathAppended = System.IO.Path.Combine(subDirectories[i], "manifest.json");
                    if (File.Exists(filePathAppended))
                    {

                        string contents = File.ReadAllText(filePathAppended);
                        PackEntry newEntry = Newtonsoft.Json.JsonConvert.DeserializeObject<PackEntry>(contents);
                        newEntry.filePath = filePathAppended;

                        newEntry.iconPath = System.IO.Path.Combine(subDirectories[i], "pack_icon.png");

                        newEntry.loadIcon();

                        //Handing name/desc stored in lang files
                        if (newEntry.header.name == "pack.name")
                        {
                            filePathAppended = System.IO.Path.Combine(subDirectories[i], "texts/en_US.lang");
                            if (File.Exists(filePathAppended))
                            {
                                string[] langLines = File.ReadAllLines(filePathAppended);
                                for (int j = 0; j < langLines.Length; j++)
                                {
                                    string[] stringSeparators = new string[] { "=" };
                                    string[] splitLine = langLines[j].Split(stringSeparators, StringSplitOptions.None);
                                    if (splitLine[0] == "pack.name" && splitLine.Length > 1)
                                    {
                                        newEntry.header.name = splitLine[1];
                                    }
                                    if (splitLine[0] == "pack.description" && splitLine.Length > 1)
                                    {
                                        newEntry.header.description = splitLine[1];
                                    }
                                }
                            }
                            
                        }
                        
                        leftListGroup[0].Add(newEntry);
                    }
                }
            }
            catch (Exception)
           {
            }
            
            #endregion
            #region Resource Packs
            ////////////////////
            //Resource packs////
            ////////////////////
            leftListGroup[1].Clear();
            try
            {
                subDirectories = Directory.GetDirectories(System.IO.Path.Combine(filePath, "development_resource_packs"));

                for (int i = 0; i < subDirectories.Length; i++)
                {
                    string filePathAppended = System.IO.Path.Combine(subDirectories[i], "manifest.json");
                    if (File.Exists(filePathAppended))
                    {

                        string contents = File.ReadAllText(filePathAppended);
                        PackEntry newEntry = Newtonsoft.Json.JsonConvert.DeserializeObject<PackEntry>(contents);
                        newEntry.filePath = filePathAppended;

                        newEntry.iconPath = System.IO.Path.Combine(subDirectories[i], "pack_icon.png");

                        newEntry.loadIcon();

                        //Handing name/desc stored in lang files
                        if (newEntry.header.name == "pack.name")
                        {
                            filePathAppended = System.IO.Path.Combine(subDirectories[i], "texts/en_US.lang");
                            if (File.Exists(filePathAppended))
                            {
                                string[] langLines = File.ReadAllLines(filePathAppended);
                                for (int j = 0; j < langLines.Length; j++)
                                {
                                    string[] stringSeparators = new string[] { "=" };
                                    string[] splitLine = langLines[j].Split(stringSeparators, StringSplitOptions.None);
                                    if (splitLine[0] == "pack.name" && splitLine.Length > 1)
                                    {
                                        newEntry.header.name = splitLine[1];
                                    }
                                    if (splitLine[0] == "pack.description" && splitLine.Length > 1)
                                    {
                                        newEntry.header.description = splitLine[1];
                                    }
                                }
                            }

                        }

                        leftListGroup[1].Add(newEntry);
                    }
                }
            }
            catch (Exception)
            {


            }

            #endregion

            #region Worlds
            ////////////////////
            //Worlds////////////
            ////////////////////
            leftListGroup[2].Clear();
            try
            {
                subDirectories = Directory.GetDirectories(System.IO.Path.Combine(filePath, "minecraftWorlds"));

                for (int i = 0; i < subDirectories.Length; i++)
                {
                    string filePathAppended = System.IO.Path.Combine(subDirectories[i], "levelname.txt");
                    if (File.Exists(filePathAppended))
                    {

                        string contents = File.ReadAllText(filePathAppended);
                        PackEntry newEntry = new PackEntry();
                        newEntry.header.name = contents;
                        newEntry.filePath = filePathAppended;

                        newEntry.iconPath = System.IO.Path.Combine(subDirectories[i], "world_icon.jpeg");

                        newEntry.loadIcon();

                        leftListGroup[2].Add(newEntry);
                    }
                }
            }
            catch (Exception)
            {


            }

            #endregion

            #region Uncategorized
            ////////////////////
            //Uncategorized/////
            ////////////////////
            leftListGroup[3].Clear();
            try
            {
                subDirectories = Directory.GetDirectories(filePath);

                for (int i = 0; i < subDirectories.Length; i++)
                {
                    string filePathAppended = System.IO.Path.Combine(subDirectories[i], "manifest.json");
                    if (File.Exists(filePathAppended))
                    {

                        string contents = File.ReadAllText(filePathAppended);
                        PackEntry newEntry = Newtonsoft.Json.JsonConvert.DeserializeObject<PackEntry>(contents);
                        newEntry.filePath = filePathAppended;

                        newEntry.iconPath = System.IO.Path.Combine(subDirectories[i], "pack_icon.png");

                        newEntry.loadIcon();

                        leftListGroup[3].Add(newEntry);
                    }
                }
            }
            catch (Exception)
            {

                
            }
            
            #endregion
        }

        private void LoadRightPacks(string filePath)
        {

            #region Behavior Packs
            ////////////////////
            //Behavior packs////
            ////////////////////
            string[] subDirectories;
            rightListGroup[0].Clear();
            try
            {
                subDirectories = Directory.GetDirectories(System.IO.Path.Combine(filePath, "development_behavior_packs"));

                for (int i = 0; i < subDirectories.Length; i++)
                {
                    string filePathAppended = System.IO.Path.Combine(subDirectories[i], "manifest.json");
                    if (File.Exists(filePathAppended))
                    {

                        string contents = File.ReadAllText(filePathAppended);
                        PackEntry newEntry = Newtonsoft.Json.JsonConvert.DeserializeObject<PackEntry>(contents);
                        newEntry.filePath = filePathAppended;

                        newEntry.iconPath = System.IO.Path.Combine(subDirectories[i], "pack_icon.png");

                        newEntry.loadIcon();

                        //Handing name/desc stored in lang files
                        if (newEntry.header.name == "pack.name")
                        {
                            filePathAppended = System.IO.Path.Combine(subDirectories[i], "texts/en_US.lang");
                            if (File.Exists(filePathAppended))
                            {
                                string[] langLines = File.ReadAllLines(filePathAppended);
                                for (int j = 0; j < langLines.Length; j++)
                                {
                                    string[] stringSeparators = new string[] { "=" };
                                    string[] splitLine = langLines[j].Split(stringSeparators, StringSplitOptions.None);
                                    if (splitLine[0] == "pack.name" && splitLine.Length > 1)
                                    {
                                        newEntry.header.name = splitLine[1];
                                    }
                                    if (splitLine[0] == "pack.description" && splitLine.Length > 1)
                                    {
                                        newEntry.header.description = splitLine[1];
                                    }
                                }
                            }

                        }

                        rightListGroup[0].Add(newEntry);
                    }
                }
            }
            catch (Exception)
            {

            }

            #endregion
            #region Resource Packs
            ////////////////////
            //Resource packs////
            ////////////////////
            rightListGroup[1].Clear();
            try
            {
                subDirectories = Directory.GetDirectories(System.IO.Path.Combine(filePath, "development_resource_packs"));

                for (int i = 0; i < subDirectories.Length; i++)
                {
                    string filePathAppended = System.IO.Path.Combine(subDirectories[i], "manifest.json");
                    if (File.Exists(filePathAppended))
                    {

                        string contents = File.ReadAllText(filePathAppended);
                        PackEntry newEntry = Newtonsoft.Json.JsonConvert.DeserializeObject<PackEntry>(contents);
                        newEntry.filePath = filePathAppended;

                        newEntry.iconPath = System.IO.Path.Combine(subDirectories[i], "pack_icon.png");

                        newEntry.loadIcon();

                        //Handing name/desc stored in lang files
                        if (newEntry.header.name == "pack.name")
                        {
                            filePathAppended = System.IO.Path.Combine(subDirectories[i], "texts/en_US.lang");
                            if (File.Exists(filePathAppended))
                            {
                                string[] langLines = File.ReadAllLines(filePathAppended);
                                for (int j = 0; j < langLines.Length; j++)
                                {
                                    string[] stringSeparators = new string[] { "=" };
                                    string[] splitLine = langLines[j].Split(stringSeparators, StringSplitOptions.None);
                                    if (splitLine[0] == "pack.name" && splitLine.Length > 1)
                                    {
                                        newEntry.header.name = splitLine[1];
                                    }
                                    if (splitLine[0] == "pack.description" && splitLine.Length > 1)
                                    {
                                        newEntry.header.description = splitLine[1];
                                    }
                                }
                            }

                        }

                        rightListGroup[1].Add(newEntry);
                    }
                }
            }
            catch (Exception)
            {


            }

            #endregion

            #region Worlds
            ////////////////////
            //Worlds////////////
            ////////////////////
            rightListGroup[2].Clear();
            try
            {
                subDirectories = Directory.GetDirectories(System.IO.Path.Combine(filePath, "minecraftWorlds"));

                for (int i = 0; i < subDirectories.Length; i++)
                {
                    string filePathAppended = System.IO.Path.Combine(subDirectories[i], "levelname.txt");
                    if (File.Exists(filePathAppended))
                    {

                        string contents = File.ReadAllText(filePathAppended);
                        PackEntry newEntry = new PackEntry();
                        newEntry.header.name = contents;
                        newEntry.filePath = filePathAppended;

                        newEntry.iconPath = System.IO.Path.Combine(subDirectories[i], "world_icon.jpeg");

                        newEntry.loadIcon();

                        rightListGroup[2].Add(newEntry);
                    }
                }
            }
            catch (Exception)
            {


            }

            #endregion

            #region Uncategorized
            ////////////////////
            //Uncategorized/////
            ////////////////////
            rightListGroup[3].Clear();
            try
            {
                subDirectories = Directory.GetDirectories(filePath);

                for (int i = 0; i < subDirectories.Length; i++)
                {
                    string filePathAppended = System.IO.Path.Combine(subDirectories[i], "manifest.json");
                    if (File.Exists(filePathAppended))
                    {

                        string contents = File.ReadAllText(filePathAppended);
                        PackEntry newEntry = Newtonsoft.Json.JsonConvert.DeserializeObject<PackEntry>(contents);
                        newEntry.filePath = filePathAppended;

                        newEntry.iconPath = System.IO.Path.Combine(subDirectories[i], "pack_icon.png");

                        newEntry.loadIcon();

                        rightListGroup[3].Add(newEntry);
                    }
                }
            }
            catch (Exception)
            {


            }

            #endregion
        }

        private void LeftText_TextChanged(object sender, TextChangedEventArgs e)
        {
            leftFilePath = leftText.Text;
            LoadLeftPacks(leftFilePath);
            if (leftWatcher != null)
            {
                if (Directory.Exists(leftFilePath) && leftFilePath != "")
                {
                    leftWatcher.Path = leftFilePath;
                }
                
            }
        }

        private void RightText_TextChanged(object sender, TextChangedEventArgs e)
        {
            rightFilePath = rightText.Text;
            LoadRightPacks(rightFilePath);
            if (rightWatcher != null)
            {
                if (Directory.Exists(rightFilePath) && rightFilePath != "")
                {
                    rightWatcher.Path = rightFilePath;
                }
            }
        }

        private void LeftTabChanged(object sender, RoutedEventArgs e)
        {

            if (leftBehaviorTab.IsSelected)
            {
                leftTabSelected = 0;
                rightTabSelected = 0;
                rightTabControl.SelectedIndex = 0;
            }
            else if (leftResourceTab.IsSelected)
            {
                leftTabSelected = 1;
                rightTabSelected = 1;
                rightTabControl.SelectedIndex = 1;
            }
            else if (leftWorldTab.IsSelected)
            {
                leftTabSelected = 2;
                rightTabSelected = 2;
                rightTabControl.SelectedIndex = 2;
            }
            else if (leftUncatTab.IsSelected)
            {
                leftTabSelected = 3;
                rightTabSelected = 3;
                rightTabControl.SelectedIndex = 3;
            }
            UpdateLeftFocus();
            UpdateRightFocus();
        }

        private void RightTabChanged(object sender, RoutedEventArgs e)
        {

            if (rightBehaviorTab.IsSelected)
            {
                rightTabSelected = 0;
                leftTabSelected = 0;
                leftTabControl.SelectedIndex = 0;
            }
            else if (rightResourceTab.IsSelected)
            {
                rightTabSelected = 1;
                leftTabSelected = 1;
                leftTabControl.SelectedIndex = 1;
            }
            else if (rightWorldTab.IsSelected)
            {
                rightTabSelected = 2;
                leftTabSelected = 2;
                leftTabControl.SelectedIndex = 2;
            }
            else if (rightUncatTab.IsSelected)
            {
                rightTabSelected = 3;
                leftTabSelected = 3;
                leftTabControl.SelectedIndex = 3;
            }
            UpdateRightFocus();
        }

        private void UpdateLeftFocus()
        {
            switch (leftTabSelected)
            {
                case 0:
                    leftList.SetBinding(System.Windows.Controls.ListBox.ItemsSourceProperty, leftBinding1);
                    break;
                case 1:
                    leftList.SetBinding(System.Windows.Controls.ListBox.ItemsSourceProperty, leftBinding2);
                    break;
                case 2:
                    leftList.SetBinding(System.Windows.Controls.ListBox.ItemsSourceProperty, leftBinding3);
                    break;
                case 3:
                    leftList.SetBinding(System.Windows.Controls.ListBox.ItemsSourceProperty, leftBinding4);
                    break;
                default:
                    leftList.SetBinding(System.Windows.Controls.ListBox.ItemsSourceProperty, leftBinding1);
                    break;
            }
            if (leftListGroup[leftTabSelected].Count > 0)
            {
                leftList.SelectedIndex = 0;
            }
            else
            {
                leftList.SelectedIndex = -1;
            }
        }

        private void UpdateRightFocus()
        {
            switch (rightTabSelected)
            {
                case 0:
                    rightList.SetBinding(System.Windows.Controls.ListBox.ItemsSourceProperty, rightBinding1);
                    break;
                case 1:
                    rightList.SetBinding(System.Windows.Controls.ListBox.ItemsSourceProperty, rightBinding2);
                    break;
                case 2:
                    rightList.SetBinding(System.Windows.Controls.ListBox.ItemsSourceProperty, rightBinding3);
                    break;
                case 3:
                    rightList.SetBinding(System.Windows.Controls.ListBox.ItemsSourceProperty, rightBinding4);
                    break;
                default:
                    rightList.SetBinding(System.Windows.Controls.ListBox.ItemsSourceProperty, rightBinding1);
                    break;
            }
            if (rightListGroup[rightTabSelected].Count > 0)
            {
                rightList.SelectedIndex = 0;
            }
            else
            {
                rightList.SelectedIndex = -1;
            }
        }
        
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            //This function is based on an example from MSDN
            //https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories

            // Get the subdirectories for the specified directory.
            DirectoryInfo sourceDir = new DirectoryInfo(sourceDirName);
            DirectoryInfo destDir = new DirectoryInfo(destDirName);
            if (!sourceDir.Exists)
            {
                return;
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            if (destDir.Exists)
            {
                Directory.Delete(destDirName, true);
            }

            DirectoryInfo[] dirs = sourceDir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = sourceDir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = System.IO.Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = System.IO.Path.Combine(destDirName, subdir.Name);
                    
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private void LeftOpenDialog(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

            if ((int)result == 1)
            { 
                leftFilePath = dialog.SelectedPath;
                leftText.Text = leftFilePath;
                LoadLeftPacks(leftFilePath);
            }
        }

        private void RightOpenDialog(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

            if ((int)result == 1)
            {
                rightFilePath = dialog.SelectedPath;
                rightText.Text = rightFilePath;
                LoadRightPacks(rightFilePath);
            }
        }

        private void LeftRefreshClick(object sender, RoutedEventArgs e)
        {
            LoadLeftPacks(leftFilePath);
            leftList.SelectedIndex = 0;
        }

        private void RightRefreshClick(object sender, RoutedEventArgs e)
        {
            LoadRightPacks(rightFilePath);
            leftList.SelectedIndex = 0;
        }

        private void LeftDeleteClick(object sender, RoutedEventArgs e)
        {
            if (leftList.SelectedIndex > -1)
            {
                string filePath = System.IO.Path.GetDirectoryName(leftListGroup[leftTabSelected][leftList.SelectedIndex].filePath);
                int storedIndex = rightList.SelectedIndex;

                DirectoryInfo dir = new DirectoryInfo(filePath);
                if (!dir.Exists)
                {
                    return;
                    throw new DirectoryNotFoundException(
                        "Source directory does not exist or could not be found: "
                        + filePath);
                }

                if (dir.Exists)
                {
                    leftWatcher.EnableRaisingEvents = false;
                    Directory.Delete(filePath, true);
                    leftWatcher.EnableRaisingEvents = true;
                    LoadLeftPacks(leftFilePath);
                }
                if (leftListGroup[leftTabSelected].Count() - 1 >= storedIndex)
                {
                    leftList.SelectedIndex = storedIndex;
                }
                else if (leftListGroup[leftTabSelected].Count() - 1 < storedIndex)
                {
                    leftList.SelectedIndex = storedIndex - 1;
                }
                Console.WriteLine(leftList.SelectedIndex);
            }
        }

        private void RightDeleteClick(object sender, RoutedEventArgs e)
        {
            if (rightList.SelectedIndex > -1)
            {
                string filePath = System.IO.Path.GetDirectoryName(rightListGroup[rightTabSelected][rightList.SelectedIndex].filePath);
                int storedIndex = rightList.SelectedIndex;

                DirectoryInfo dir = new DirectoryInfo(filePath);
                if (!dir.Exists)
                {
                    return;
                    throw new DirectoryNotFoundException(
                        "Source directory does not exist or could not be found: "
                        + filePath);
                }

                if (dir.Exists)
                {
                    rightWatcher.EnableRaisingEvents = false;
                    Directory.Delete(filePath, true);
                    rightWatcher.EnableRaisingEvents = true;
                    LoadRightPacks(rightFilePath);
                }

                if (rightListGroup[rightTabSelected].Count() - 1 >= storedIndex)
                {
                    rightList.SelectedIndex = storedIndex;
                }
                else if (rightListGroup[rightTabSelected].Count() - 1 < storedIndex)
                {
                    rightList.SelectedIndex = storedIndex - 1;
                }
            }
        }

        private void SetLeftDefault(object sender, RoutedEventArgs e)
        {
            defaultLeftFilePath = leftText.Text;
            WriteSettings();
        }

        private void SetRightDefault(object sender, RoutedEventArgs e)
        {
            defaultRightFilePath = rightText.Text;
            WriteSettings();
        }

        private void WriteSettings()
        {
            string[] contents = new string[] { "leftDefaultPath=" + defaultLeftFilePath, "rightDefaultPath=" + defaultRightFilePath };
            File.WriteAllLines(@"settings.txt", contents);
        }

        private void OnLeftDirectoryChange(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                LoadLeftPacks(leftFilePath);
            });
        }

        private void OnRightDirectoryChange(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                LoadRightPacks(rightFilePath);
            });
        }
    }
}
