using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Windows.Forms;
using System.Windows.Interactivity;
using System.Drawing;
using System.Diagnostics;
using System.Security.Permissions;
using System.ComponentModel;
using TabControl = System.Windows.Controls.TabControl;
using Button = System.Windows.Controls.Button;
using TextBox = System.Windows.Controls.TextBox;
using ComboBox = System.Windows.Controls.ComboBox;
using ListBox = System.Windows.Controls.ListBox;

namespace Craft2Git
{
    // Packhub class
    public class PackHub
    {
        // Array of lists that holds the data of each pack. One for each tab of the list box
        PackList[] packLists;
        // Array of bindings. One for each tab of the list box
        System.Windows.Data.Binding[] listBoxBindings;
        string defaultFilePath, filePath;
        int selectedTab;
        FileSystemWatcher watcher;
        StructureType structureType;


        // UI objects
        TabControl tabControl;
        ListBox listBox;
        Button copyButton;
        Button deleteButton;
        TextBox directoryTextBox;
        Button browseButton;
        ComboBox projectComboBox;
        ComboBox structureComboBox;

        // Properties
        public string Filepath
        {
            get { return filePath; }
            set
            {
                filePath = value;
                LoadLeftPacks(filePath, true);
                Console.WriteLine("leftFilePath changed to: " + filePath);
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("LeftFilePath");
            }
        }

        // Constructor
        public PackHub(TabControl _tabControl, ListBox _listBox, Button _copyButton, Button _deleteButton, TextBox _directoryTextBox,
                        Button _browseButton, ComboBox _projectComboBox, ComboBox _structureComboBox)
        {
            #region UI object assignment
            tabControl = _tabControl;
            copyButton = _copyButton;
            deleteButton = _deleteButton;
            directoryTextBox = _directoryTextBox;
            browseButton = _browseButton;
            projectComboBox = _projectComboBox;
            structureComboBox = _structureComboBox;
            #endregion

            packLists = new PackList[4];
            for (int i = 0; i < packLists.Length; i++)
            {
                packLists[i] = new PackList();
                listBoxBindings[i] = new System.Windows.Data.Binding();
                listBoxBindings[i].Source = packLists[i];
            }

            filePath = defaultFilePath;
            directoryTextBox.Text = filePath;

            

            #region Setup File Watcher
            // This region is based on an example from MSDN
            //https://docs.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher?redirectedfrom=MSDN&view=netframework-4.7.2
            watcher = new FileSystemWatcher();
            if (Directory.Exists(filePath) && filePath != "")
            {
                watcher.Path = filePath;
            }
            else
            {
                filePath = Directory.GetCurrentDirectory();
                watcher.Path = Directory.GetCurrentDirectory();
            }

            watcher.Filter = "*.*";
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;

            // TODO: Figure out how to either pass in an event to supply to the watcher or assign the event from outside the constructor. 
            // Also need to see if you can pass in parameters from the watcher (eg. "left" or "right")
            //watcher.Created += OnLeftDirectoryChange;
            //watcher.Changed += OnLeftDirectoryChange;
            //watcher.Renamed += OnLeftDirectoryChange;
            //watcher.Deleted += OnLeftDirectoryChange;
            #endregion
        }

        private void LoadLeftPacks(string filePath, bool resetIndex)
        {
            string[] folderNames = new string[3];

            switch (structureType)
            {
                case StructureType.comMojang:

                    
                    folderNames[0] = comMojangStructure.BPFolder;
                    folderNames[1] = comMojangStructure.RPFolder;
                    folderNames[2] = comMojangStructure.worldsFolder;
                    break;
                case StructureType.singleRepo:
                    folderNames[0] = repoStructure.BPFolder;
                    folderNames[1] = repoStructure.RPFolder;
                    folderNames[2] = repoStructure.worldsFolder;
                    break;
                case StructureType.solvedStructure:
                    throw new NotImplementedException();
                    break;
            }


            #region Behavior Packs
            // Behavior pack section
            string[] packFolders;
            packLists[0].Clear();
            try
            {
                packFolders = Directory.GetDirectories(System.IO.Path.Combine(filePath, folderNames[0]));

                for (int i = 0; i < packFolders.Length; i++)
                {
                    string manifestFilePath = System.IO.Path.Combine(packFolders[i], "manifest.json");
                    if (File.Exists(manifestFilePath))
                    {

                        string manifestContents = File.ReadAllText(manifestFilePath);
                        PackEntry newEntry = Newtonsoft.Json.JsonConvert.DeserializeObject<PackEntry>(manifestContents);
                        newEntry.filePath = manifestFilePath;
                        newEntry.iconPath = System.IO.Path.Combine(packFolders[i], "pack_icon.png");
                        newEntry.loadIcon();

                        //Handing name/desc stored in lang files
                        if (newEntry.header.name == "pack.name")
                        {
                            manifestFilePath = System.IO.Path.Combine(packFolders[i], "texts/en_US.lang");
                            if (File.Exists(manifestFilePath))
                            {
                                string[] langLines = File.ReadAllLines(manifestFilePath);
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
                        packLists[0].Add(newEntry);
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
            packLists[1].Clear();
            try
            {
                packFolders = Directory.GetDirectories(System.IO.Path.Combine(filePath, folderNames[1]));

                for (int i = 0; i < packFolders.Length; i++)
                {
                    string filePathAppended = System.IO.Path.Combine(packFolders[i], "manifest.json");
                    if (File.Exists(filePathAppended))
                    {

                        string contents = File.ReadAllText(filePathAppended);
                        PackEntry newEntry = Newtonsoft.Json.JsonConvert.DeserializeObject<PackEntry>(contents);
                        newEntry.filePath = filePathAppended;

                        newEntry.iconPath = System.IO.Path.Combine(packFolders[i], "pack_icon.png");

                        newEntry.loadIcon();

                        //Handing name/desc stored in lang files
                        if (newEntry.header.name == "pack.name")
                        {
                            filePathAppended = System.IO.Path.Combine(packFolders[i], "texts/en_US.lang");
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
                packFolders = Directory.GetDirectories(System.IO.Path.Combine(filePath, folderNames[2]));

                for (int i = 0; i < packFolders.Length; i++)
                {
                    string filePathAppended = System.IO.Path.Combine(packFolders[i], "levelname.txt");
                    if (File.Exists(filePathAppended))
                    {

                        string contents = File.ReadAllText(filePathAppended);
                        PackEntry newEntry = new PackEntry();
                        newEntry.header.name = contents;
                        newEntry.filePath = filePathAppended;

                        newEntry.iconPath = System.IO.Path.Combine(packFolders[i], "world_icon.jpeg");

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
            packLists[3].Clear();
            try
            {
                packFolders = Directory.GetDirectories(filePath);

                for (int i = 0; i < packFolders.Length; i++)
                {
                    string filePathAppended = System.IO.Path.Combine(packFolders[i], "manifest.json");
                    if (File.Exists(filePathAppended))
                    {

                        string contents = File.ReadAllText(filePathAppended);
                        PackEntry newEntry = Newtonsoft.Json.JsonConvert.DeserializeObject<PackEntry>(contents);
                        newEntry.filePath = filePathAppended;

                        newEntry.iconPath = System.IO.Path.Combine(packFolders[i], "pack_icon.png");

                        newEntry.loadIcon();

                        packLists[3].Add(newEntry);
                    }
                }
            }
            catch (Exception)
            {


            }

            #endregion

            if (resetIndex && listBox.SelectedIndex > -1)
            {
                listBox.SelectedIndex = packLists[tabControl.SelectedIndex].Count > 0 ? 0 : -1;
            }
        }
    }

    public class PackList : ObservableCollection<PackEntry>
    {
        public PackList()
        {
        }
    }

    public class DirectoryStructure
    {
        public string BPFolder { get; set; }
        public string RPFolder { get; set; }
        public string worldsFolder { get; set; }

        public DirectoryStructure(string _BPFolder, string _RPFolder, string _worldsFolder)
        {
            BPFolder = _BPFolder;
            RPFolder = _RPFolder;
            worldsFolder = _worldsFolder;
        }
    }
    enum Side
    {
        Left = 0,
        Right = 1
    }

    enum PackType
    {
        BP = 0,
        RP = 1,
        World = 2
    }

    enum StructureType
    {
        comMojang = 0,
        singleRepo = 1,
        solvedStructure = 2
    }

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Class-wide Variables
        PackList[] leftListGroup, rightListGroup;
        string defaultLeftFilePath = "", defaultRightFilePath = "", leftFilePath = "";
        public string LeftFilePath
        {
            get { return leftFilePath; }
            set
            {
                leftFilePath = value;
                LoadLeftPacks(leftFilePath, true);
                Console.WriteLine("leftFilePath changed to: " + leftFilePath);
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged("LeftFilePath");
            }
        }
        public string rightFilePath { get; set; }
        System.Windows.Data.Binding leftBinding1, leftBinding2, leftBinding3, leftBinding4, rightBinding1, rightBinding2, rightBinding3, rightBinding4;
        FileSystemWatcher leftWatcher, rightWatcher;
        public DirectoryStructure comMojangStructure = new DirectoryStructure("development_behavior_packs", "development_resource_packs", "minecraftWorlds");
        public DirectoryStructure ComMojangStructure
        {
            get { return comMojangStructure; }
        }
        public DirectoryStructure repoStructure = new DirectoryStructure("BPs", "RPs", "Worlds");
        StructureType leftStructureType = 0;
        StructureType rightStructureType = 0;
        bool shouldMakeBackups = false;
        bool ignoreNextTabChange = false;
        #endregion
        #region Commands
        public static RoutedCommand DeletePackCmd = new RoutedCommand();
        public static RoutedCommand CopyPackCmd = new RoutedCommand();
        public static RoutedCommand TabChangedCmd = new RoutedCommand();
        public static RoutedCommand DirectoryChangedCmd = new RoutedCommand();
        public static RoutedCommand OpenDialogCmd = new RoutedCommand();
        public static RoutedCommand SetDefaultPathCmd = new RoutedCommand();
        public static RoutedCommand SetStructureTypeCmd = new RoutedCommand();
        #endregion
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            #region Load in defaults
            // Reading settings file
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
            // Init the left side
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

            // Left PackHub
            PackHub LeftHub = new PackHub(leftTabControl, leftList, leftCopyButton, leftDeleteButton, leftText, leftOpen, leftProjectCombo, leftStructureCombo);

            InitializeComponent();
            // This DataContext assignment is used for property bindings
            this.DataContext = this;

            leftFilePath = defaultLeftFilePath;
            rightFilePath = defaultRightFilePath;

            ChangePackTypeFocus(0);

            leftText.Text = leftFilePath;
            rightText.Text = rightFilePath;

            Refresh(true);

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
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void LeftCopy(object sender, RoutedEventArgs e)
        {
            
            if (leftList.SelectedIndex <= -1) /*Prevent out of range exception*/
            {
                return;
            }

            string[] destFolderNames = new string[3];
            switch (rightStructureType)
            {
                case StructureType.comMojang:
                    destFolderNames[0] = comMojangStructure.BPFolder;
                    destFolderNames[1] = comMojangStructure.RPFolder;
                    destFolderNames[2] = comMojangStructure.worldsFolder;
                    break;
                case StructureType.singleRepo:
                    destFolderNames[0] = repoStructure.BPFolder;
                    destFolderNames[1] = repoStructure.RPFolder;
                    destFolderNames[2] = repoStructure.worldsFolder;
                    break;
                case StructureType.solvedStructure:
                    // TODO
                    throw new NotImplementedException();
                    break;
            }

            string sourceFilePath = System.IO.Path.GetDirectoryName(leftListGroup[leftTabControl.SelectedIndex][leftList.SelectedIndex].filePath);
            string[] stringSeparators = new string[] { "\\" };
            string[] splitEntryPath = leftListGroup[leftTabControl.SelectedIndex][leftList.SelectedIndex].filePath.Split(stringSeparators, StringSplitOptions.None);

            string destFilePath = rightFilePath;
            if (leftTabControl.SelectedIndex != 3) /*For categorized packs*/
            {
                destFilePath = System.IO.Path.Combine(destFilePath, destFolderNames[leftTabControl.SelectedIndex]);    
            }
            else /*For uncategorized packs*/
            {
            }
            destFilePath = System.IO.Path.Combine(destFilePath, splitEntryPath[splitEntryPath.Length - 2]);

            rightWatcher.EnableRaisingEvents = false;
            DirectoryCopy(sourceFilePath, destFilePath, true);
            rightWatcher.EnableRaisingEvents = true;
            LoadRightPacks(rightFilePath, true);
            if (rightList.SelectedIndex == -1)
            {
                rightList.SelectedIndex = 0;
            } 
        }

        private void RightCopy(object sender, RoutedEventArgs e)
        {
            if (rightList.SelectedIndex <= -1) /*Prevent out of range exception*/
            {
                return;
            }

            string[] destFolderNames = new string[3];
            switch (leftStructureType)
            {
                case StructureType.comMojang:
                    destFolderNames[0] = comMojangStructure.BPFolder;
                    destFolderNames[1] = comMojangStructure.RPFolder;
                    destFolderNames[2] = comMojangStructure.worldsFolder;
                    break;
                case StructureType.singleRepo:
                    destFolderNames[0] = repoStructure.BPFolder;
                    destFolderNames[1] = repoStructure.RPFolder;
                    destFolderNames[2] = repoStructure.worldsFolder;
                    break;
                case StructureType.solvedStructure:
                    throw new NotImplementedException();
                    break;
            }

            string sourceFilePath = System.IO.Path.GetDirectoryName(rightListGroup[rightTabControl.SelectedIndex][rightList.SelectedIndex].filePath);
            string[] stringSeparators = new string[] { "\\" };
            string[] splitEntryPath = rightListGroup[rightTabControl.SelectedIndex][rightList.SelectedIndex].filePath.Split(stringSeparators, StringSplitOptions.None);

            string destFilePath = leftFilePath;
            if (rightTabControl.SelectedIndex != 3) /*For categorized packs*/
            {
                destFilePath = System.IO.Path.Combine(destFilePath, destFolderNames[rightTabControl.SelectedIndex]);
            }
            else /*For uncategorized packs*/
            {
            }
            destFilePath = System.IO.Path.Combine(destFilePath, splitEntryPath[splitEntryPath.Length - 2]);

            leftWatcher.EnableRaisingEvents = false;
            DirectoryCopy(sourceFilePath, destFilePath, true);
            leftWatcher.EnableRaisingEvents = true;
            LoadLeftPacks(leftFilePath, true);
            if (leftList.SelectedIndex == -1)
            {
                leftList.SelectedIndex = 0;
            }     
        }

        private void LoadLeftPacks(string filePath, bool resetIndex)
        {
            string[] folderNames = new string[3];

            switch (leftStructureType)
            {
                case StructureType.comMojang:
                    folderNames[0] = comMojangStructure.BPFolder;
                    folderNames[1] = comMojangStructure.RPFolder;
                    folderNames[2] = comMojangStructure.worldsFolder;
                    break;
                case StructureType.singleRepo:
                    folderNames[0] = repoStructure.BPFolder;
                    folderNames[1] = repoStructure.RPFolder;
                    folderNames[2] = repoStructure.worldsFolder;
                    break;
                case StructureType.solvedStructure:
                    throw new NotImplementedException();
                    break;
            }


            #region Behavior Packs
            // Behavior pack section
            string[] packFolders;
            leftListGroup[0].Clear();
            try
            {
                packFolders = Directory.GetDirectories(System.IO.Path.Combine(filePath, folderNames[0]));

                for (int i = 0; i < packFolders.Length; i++)
                {
                    string manifestFilePath = System.IO.Path.Combine(packFolders[i], "manifest.json");
                    if (File.Exists(manifestFilePath))
                    {

                        string manifestContents = File.ReadAllText(manifestFilePath);
                        PackEntry newEntry = Newtonsoft.Json.JsonConvert.DeserializeObject<PackEntry>(manifestContents);
                        newEntry.filePath = manifestFilePath;
                        newEntry.iconPath = System.IO.Path.Combine(packFolders[i], "pack_icon.png");
                        newEntry.loadIcon();

                        //Handing name/desc stored in lang files
                        if (newEntry.header.name == "pack.name")
                        {
                            manifestFilePath = System.IO.Path.Combine(packFolders[i], "texts/en_US.lang");
                            if (File.Exists(manifestFilePath))
                            {
                                string[] langLines = File.ReadAllLines(manifestFilePath);
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
                packFolders = Directory.GetDirectories(System.IO.Path.Combine(filePath, folderNames[1]));

                for (int i = 0; i < packFolders.Length; i++)
                {
                    string filePathAppended = System.IO.Path.Combine(packFolders[i], "manifest.json");
                    if (File.Exists(filePathAppended))
                    {

                        string contents = File.ReadAllText(filePathAppended);
                        PackEntry newEntry = Newtonsoft.Json.JsonConvert.DeserializeObject<PackEntry>(contents);
                        newEntry.filePath = filePathAppended;

                        newEntry.iconPath = System.IO.Path.Combine(packFolders[i], "pack_icon.png");

                        newEntry.loadIcon();

                        //Handing name/desc stored in lang files
                        if (newEntry.header.name == "pack.name")
                        {
                            filePathAppended = System.IO.Path.Combine(packFolders[i], "texts/en_US.lang");
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
                packFolders = Directory.GetDirectories(System.IO.Path.Combine(filePath, folderNames[2]));

                for (int i = 0; i < packFolders.Length; i++)
                {
                    string filePathAppended = System.IO.Path.Combine(packFolders[i], "levelname.txt");
                    if (File.Exists(filePathAppended))
                    {

                        string contents = File.ReadAllText(filePathAppended);
                        PackEntry newEntry = new PackEntry();
                        newEntry.header.name = contents;
                        newEntry.filePath = filePathAppended;

                        newEntry.iconPath = System.IO.Path.Combine(packFolders[i], "world_icon.jpeg");

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
                packFolders = Directory.GetDirectories(filePath);

                for (int i = 0; i < packFolders.Length; i++)
                {
                    string filePathAppended = System.IO.Path.Combine(packFolders[i], "manifest.json");
                    if (File.Exists(filePathAppended))
                    {

                        string contents = File.ReadAllText(filePathAppended);
                        PackEntry newEntry = Newtonsoft.Json.JsonConvert.DeserializeObject<PackEntry>(contents);
                        newEntry.filePath = filePathAppended;

                        newEntry.iconPath = System.IO.Path.Combine(packFolders[i], "pack_icon.png");

                        newEntry.loadIcon();

                        leftListGroup[3].Add(newEntry);
                    }
                }
            }
            catch (Exception)
            {

                
            }

            #endregion

            if (resetIndex && leftList.SelectedIndex > -1)
            {
                leftList.SelectedIndex = leftListGroup[leftTabControl.SelectedIndex].Count > 0 ? 0 : -1;
            }
        }

        private void LoadRightPacks(string filePath, bool resetIndex)
        {
            string[] folderNames = new string[3];

            switch (rightStructureType)
            {
                case StructureType.comMojang:
                    folderNames[0] = comMojangStructure.BPFolder;
                    folderNames[1] = comMojangStructure.RPFolder;
                    folderNames[2] = comMojangStructure.worldsFolder;
                    break;
                case StructureType.singleRepo:
                    folderNames[0] = repoStructure.BPFolder;
                    folderNames[1] = repoStructure.RPFolder;
                    folderNames[2] = repoStructure.worldsFolder;
                    break;
                case StructureType.solvedStructure:
                    // TODO
                    throw new NotImplementedException();
                    break;
            }

            #region Behavior Packs
            ////////////////////
            //Behavior packs////
            ////////////////////
            string[] packFolders;
            rightListGroup[0].Clear();
            try
            {
                packFolders = Directory.GetDirectories(System.IO.Path.Combine(filePath, folderNames[0]));

                for (int i = 0; i < packFolders.Length; i++)
                {
                    string filePathAppended = System.IO.Path.Combine(packFolders[i], "manifest.json");
                    if (File.Exists(filePathAppended))
                    {

                        string contents = File.ReadAllText(filePathAppended);
                        PackEntry newEntry = Newtonsoft.Json.JsonConvert.DeserializeObject<PackEntry>(contents);
                        newEntry.filePath = filePathAppended;

                        newEntry.iconPath = System.IO.Path.Combine(packFolders[i], "pack_icon.png");

                        newEntry.loadIcon();

                        //Handing name/desc stored in lang files
                        if (newEntry.header.name == "pack.name")
                        {
                            filePathAppended = System.IO.Path.Combine(packFolders[i], "texts/en_US.lang");
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
                packFolders = Directory.GetDirectories(System.IO.Path.Combine(filePath, folderNames[1]));

                for (int i = 0; i < packFolders.Length; i++)
                {
                    string filePathAppended = System.IO.Path.Combine(packFolders[i], "manifest.json");
                    if (File.Exists(filePathAppended))
                    {

                        string contents = File.ReadAllText(filePathAppended);
                        PackEntry newEntry = Newtonsoft.Json.JsonConvert.DeserializeObject<PackEntry>(contents);
                        newEntry.filePath = filePathAppended;

                        newEntry.iconPath = System.IO.Path.Combine(packFolders[i], "pack_icon.png");

                        newEntry.loadIcon();

                        //Handing name/desc stored in lang files
                        if (newEntry.header.name == "pack.name")
                        {
                            filePathAppended = System.IO.Path.Combine(packFolders[i], "texts/en_US.lang");
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
                packFolders = Directory.GetDirectories(System.IO.Path.Combine(filePath, folderNames[2]));

                for (int i = 0; i < packFolders.Length; i++)
                {
                    string filePathAppended = System.IO.Path.Combine(packFolders[i], "levelname.txt");
                    if (File.Exists(filePathAppended))
                    {

                        string contents = File.ReadAllText(filePathAppended);
                        PackEntry newEntry = new PackEntry();
                        newEntry.header.name = contents;
                        newEntry.filePath = filePathAppended;

                        newEntry.iconPath = System.IO.Path.Combine(packFolders[i], "world_icon.jpeg");

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
                packFolders = Directory.GetDirectories(filePath);

                for (int i = 0; i < packFolders.Length; i++)
                {
                    string filePathAppended = System.IO.Path.Combine(packFolders[i], "manifest.json");
                    if (File.Exists(filePathAppended))
                    {

                        string contents = File.ReadAllText(filePathAppended);
                        PackEntry newEntry = Newtonsoft.Json.JsonConvert.DeserializeObject<PackEntry>(contents);
                        newEntry.filePath = filePathAppended;

                        newEntry.iconPath = System.IO.Path.Combine(packFolders[i], "pack_icon.png");

                        newEntry.loadIcon();

                        rightListGroup[3].Add(newEntry);
                    }
                }
            }
            catch (Exception)
            {


            }

            #endregion

            if (resetIndex && rightList.SelectedIndex > -1)
            {
                rightList.SelectedIndex = rightListGroup[rightTabControl.SelectedIndex].Count > 0 ? 0 : -1;
            }
        }

        private void MenuRefreshClick(object sender, RoutedEventArgs e)
        {
            Refresh(true);
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
                try
                {
                    string temppath = System.IO.Path.Combine(destDirName, file.Name);
                    file.CopyTo(temppath, false);
                }
                catch (PathTooLongException)
                {
                    System.Windows.MessageBox.Show("PathTooLongException");
                    throw;
                }
                
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

        private void Refresh(bool resetIndex)
        {
            LoadLeftPacks(leftFilePath, resetIndex);
            LoadRightPacks(rightFilePath, resetIndex);
        }

        private void SetStructureType(PackType packType, Side side)
        {
            if (side.ToString() == "left")
            {
                
            }
            else if (side.ToString() == "right")
            {
                
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(leftFilePath); 
        }

        private void WriteSettings()
        {
            string[] contents = new string[] { "leftDefaultPath=" + defaultLeftFilePath, "rightDefaultPath=" + defaultRightFilePath };
            File.WriteAllLines(@"settings.txt", contents);
        }


        private void OnLeftDirectoryChange(object source, FileSystemEventArgs e)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                LoadLeftPacks(leftFilePath, true);
            });
        }

        private void OnRightDirectoryChange(object source, FileSystemEventArgs e)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                LoadRightPacks(rightFilePath, true);
            });
        }

        #region Command definitions
        // Definitions for my command events
        private void TabChangedCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (ignoreNextTabChange) //This disgusting bit of code is to prevent an infinite loop of the left and right tabs affecting each othger due to wpf weirdness.
            {
                ignoreNextTabChange = false;
                return;
            }
            else
            {
                ignoreNextTabChange = true;
            }
            Console.WriteLine("Tab Changed");
            if ((int)e.Parameter >= 0 && (int)e.Parameter <= 4)
            {
                ChangePackTypeFocus((int)e.Parameter);
                leftTabControl.SelectedIndex = (int)e.Parameter;
                rightTabControl.SelectedIndex = (int)e.Parameter;
            }
        }

        private void DirectoryChangedCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if ((String)e.Parameter == "left")
            {
                //Console.WriteLine("LeftText change to:" + ((TextBox)e.Source).Text);
                leftFilePath = leftText.Text;
                LoadLeftPacks(leftFilePath, true);

                try
                {
                    leftWatcher.Path = leftFilePath;
                    if (leftWatcher.EnableRaisingEvents == false)
                    {
                        leftWatcher.EnableRaisingEvents = true;
                    }
                }
                catch (System.ArgumentException)
                {
                    leftWatcher.EnableRaisingEvents = false;
                }

            }
            else if ((String)e.Parameter == "right")
            {
                rightFilePath = rightText.Text;
                LoadRightPacks(rightFilePath, true);

                try
                {
                    rightWatcher.Path = rightFilePath;
                    if (rightWatcher.EnableRaisingEvents == false)
                    {
                        rightWatcher.EnableRaisingEvents = true;
                    }
                }
                catch (System.ArgumentException)
                {
                    rightWatcher.EnableRaisingEvents = false;
                }
            }
        }

        private void OpenDialogCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

            if ((int)result == 1)
            {
                if ((String)e.Parameter == "left")
                {
                    leftFilePath = dialog.SelectedPath;
                    leftText.Text = leftFilePath;
                    LoadLeftPacks(leftFilePath, true);
                }
                else if ((String)e.Parameter == "right")
                {
                    rightFilePath = dialog.SelectedPath;
                    rightText.Text = rightFilePath;
                    LoadRightPacks(rightFilePath, true);
                }
            } 
        }

        private void DeletePackCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if ((String)e.Parameter == "left")
            {
                DeletePack(ref leftListGroup[leftTabControl.SelectedIndex], leftList);
            }
            else if ((String)e.Parameter == "right")
            {
                DeletePack(ref rightListGroup[rightTabControl.SelectedIndex], rightList);
            }
            else
            {
                System.Windows.MessageBox.Show("in DeletePackCmdExecuted \n Parameter: " + e.Parameter);
            }
        }

        private void DeletePackCmdCanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((String)e.Parameter == "left")
            {
                e.CanExecute = leftListGroup[leftTabControl.SelectedIndex].Count > 0 ? true : false;
            }
            else if ((String)e.Parameter == "right")
            {
                e.CanExecute = rightListGroup[rightTabControl.SelectedIndex].Count > 0 ? true : false;
            }
        }

        private void SetDefaultPathCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if ((String)e.Parameter == "left")
            {
                defaultLeftFilePath = leftText.Text;
            }
            else if ((String)e.Parameter == "right")
            {
                defaultRightFilePath = rightText.Text;
            }
            WriteSettings();
        }

        private void SetStructureTypeCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {

            //SetStructureType();
        }
        #endregion

        #region Stand-alone methods
        private void ChangePackTypeFocus(int index)
        {
            switch (index)
            {
                case 0:
                    leftList.SetBinding(System.Windows.Controls.ListBox.ItemsSourceProperty, leftBinding1);
                    rightList.SetBinding(System.Windows.Controls.ListBox.ItemsSourceProperty, rightBinding1);
                    break;
                case 1:
                    leftList.SetBinding(System.Windows.Controls.ListBox.ItemsSourceProperty, leftBinding2);
                    rightList.SetBinding(System.Windows.Controls.ListBox.ItemsSourceProperty, rightBinding2);
                    break;
                case 2:
                    leftList.SetBinding(System.Windows.Controls.ListBox.ItemsSourceProperty, leftBinding3);
                    rightList.SetBinding(System.Windows.Controls.ListBox.ItemsSourceProperty, rightBinding3);
                    break;
                case 3:
                    leftList.SetBinding(System.Windows.Controls.ListBox.ItemsSourceProperty, leftBinding4);
                    rightList.SetBinding(System.Windows.Controls.ListBox.ItemsSourceProperty, rightBinding4);
                    break;
                default:
                    leftList.SetBinding(System.Windows.Controls.ListBox.ItemsSourceProperty, leftBinding1);
                    rightList.SetBinding(System.Windows.Controls.ListBox.ItemsSourceProperty, rightBinding1);
                    break;
            }

            leftList.SelectedIndex = leftListGroup[leftTabControl.SelectedIndex].Count > 0 ? 0 : -1;
            rightList.SelectedIndex = rightListGroup[rightTabControl.SelectedIndex].Count > 0 ? 0 : -1;
        }

        private void DeletePack(ref PackList packlist, ListBox listbox)
        {
            if (listbox.SelectedIndex <= -1)
            {
                System.Windows.MessageBox.Show("Delete index is too small");
                return;
            }

            string filePath = System.IO.Path.GetDirectoryName(packlist[listbox.SelectedIndex].filePath);

            DirectoryInfo dir = new DirectoryInfo(filePath);
            int storedIndex = listbox.SelectedIndex;
            if (dir.Exists)
            {
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {

                    leftWatcher.EnableRaisingEvents = false;
                    rightWatcher.EnableRaisingEvents = false;
                    Directory.Delete(filePath, true);
                    LoadLeftPacks(leftFilePath, false);
                    LoadRightPacks(rightFilePath, false);
                    leftWatcher.EnableRaisingEvents = true;
                    rightWatcher.EnableRaisingEvents = true;
                }
            }
            else
            {
                throw new DirectoryNotFoundException(
                        "Source directory does not exist or could not be found: "
                        + filePath);
            }
            if (packlist.Count() - 1 >= storedIndex)
            {
                listbox.SelectedIndex = storedIndex;
            }
            else if (packlist.Count() - 1 < storedIndex)
            {
                listbox.SelectedIndex = storedIndex - 1;
            }
        }
        #endregion
    }
}
