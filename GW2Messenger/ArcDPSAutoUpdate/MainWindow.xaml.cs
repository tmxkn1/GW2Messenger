using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace ArcDPSAutoUpdate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DirectoryTBox.Text = Properties.Settings.Default.Directory;
        }

        private void BrowseBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            var result = fbd.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK || string.IsNullOrWhiteSpace(fbd.SelectedPath)) return;
            if (!fbd.SelectedPath.EndsWith("\\bin64") && !fbd.SelectedPath.EndsWith("\\bin"))
            {
                MessageBox.Show(@"Dude, please select the ""bin64""/""bin"" folder!", "Wrong folder", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Properties.Settings.Default.Directory = fbd.SelectedPath;
            Properties.Settings.Default.Save();
            DirectoryTBox.Text = Properties.Settings.Default.Directory;
        }

        private void CheckNowBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(Properties.Settings.Default.Directory))
            {
                if (!Directory.Exists(Properties.Settings.Default.Directory+"\\"))
                {
                    MessageBox.Show(@"Dude, please select the CORRECT folder!", "Wrong folder", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
            }

            if (Properties.Settings.Default.Directory.EndsWith("\\"))
                DirectoryTBox.Text = DirectoryTBox.Text.Substring(0, DirectoryTBox.Text.Length - 1);

            if (!Properties.Settings.Default.Directory.EndsWith("\\bin64") && !Properties.Settings.Default.Directory.EndsWith("\\bin"))
            {
                MessageBox.Show(@"Dude, please select the ""bin64""/""bin"" folder!", "Wrong folder",MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }

            // arc dps
            var fileStatus = AutoUpdateArcDps.IsLocalArcDpsUpToDate(Properties.Settings.Default.Directory);
            switch (fileStatus)
            {
                case AutoUpdateArcDps.FileStatus.None:
                    ConsoleLbx.Items.Add(new TextBlock
                    {
                        Text = $"Fail to check {Properties.Resources.ArcDpsName} status. Most likely the web server hosting it is not responding."
                    });
                    break;
                case AutoUpdateArcDps.FileStatus.UpToDate:
                    ConsoleLbx.Items.Add(new TextBlock { Text = $"{Properties.Resources.ArcDpsName} is update to date." });
                    break;
                case AutoUpdateArcDps.FileStatus.OutDated:
                case AutoUpdateArcDps.FileStatus.Missing:
                    ConsoleLbx.Items.Add(new TextBlock { Text = $"{Properties.Resources.ArcDpsName} {(fileStatus == AutoUpdateArcDps.FileStatus.OutDated ? "needs update" : "does not exist")}." });
                    if (AutoUpdateArcDps.IsGw2Running())
                    {
                        ConsoleLbx.Items.Add(new TextBlock
                        {
                            Text = "Guild wars 2 is running. Please exit the game and try again."
                        });
                    }
                    else
                    {
                        var ex = AutoUpdateArcDps.DownloadArcDps(Properties.Settings.Default.Directory);
                        if (ex != null)
                        {
                            ConsoleLbx.Items.Add(new TextBlock { Text = $"An exception has occured: {ex.Message}." });
                            ConsoleLbx.Items.Add(new TextBlock { Text = "The operation is not complete. All changes are reverted." });
                        }
                        else
                        {
                            ConsoleLbx.Items.Add(new TextBlock { Text = $"{Properties.Resources.ArcDpsName} has been updated." });
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // build templates
            if (BuildTemCkb.IsChecked == false)
                return;

            fileStatus = AutoUpdateArcDps.IsLocalBuildTempUpToDate(Properties.Settings.Default.Directory);
            switch (fileStatus)
            {
                case AutoUpdateArcDps.FileStatus.None:
                    ConsoleLbx.Items.Add(new TextBlock
                    {
                        Text = $"Fail to check {Properties.Resources.BuildTempName} status. Most likely the web server hosting it is not responding."
                    });
                    break;
                case AutoUpdateArcDps.FileStatus.UpToDate:
                    ConsoleLbx.Items.Add(new TextBlock { Text = $"{Properties.Resources.BuildTempName} is up to date." });
                    break;
                case AutoUpdateArcDps.FileStatus.OutDated:
                case AutoUpdateArcDps.FileStatus.Missing:
                    ConsoleLbx.Items.Add(new TextBlock { Text = $"{Properties.Resources.BuildTempName} {(fileStatus == AutoUpdateArcDps.FileStatus.OutDated ? "needs update" : "does not exist")}." });
                    if (AutoUpdateArcDps.IsGw2Running())
                    {
                        ConsoleLbx.Items.Add(new TextBlock
                        {
                            Text = "Guild wars 2 is running. Please exit the game and try again."
                        });
                    }
                    else
                    {
                        var ex = AutoUpdateArcDps.DownloadBuildTemp(Properties.Settings.Default.Directory);
                        if (ex != null)
                        {
                            ConsoleLbx.Items.Add(new TextBlock { Text = $"An exception has occured: {ex.Message}." });
                            ConsoleLbx.Items.Add(new TextBlock { Text = "The operation is not complete. All changes are reverted." });
                        }
                        else
                        {
                            ConsoleLbx.Items.Add(new TextBlock { Text = $"{Properties.Resources.BuildTempName} has been updated." });
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DirectoryTBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.Directory = DirectoryTBox.Text;
            Properties.Settings.Default.Save();
        }
    }
}
