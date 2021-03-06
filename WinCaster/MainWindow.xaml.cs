using System;
using System.Collections.Generic;
using System.Configuration;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CSCore;
using CSCore.CoreAudioAPI;

namespace WinCaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        // This is surprisingly harder to do that I expected, let's just do it manually for now
        private string version = "0.0.2-debug";
        private static AudioHandler theAudio;
        private static bool weAreRecording = false;
        private static bool guestPicked = false;
        private static bool voicePicked = false;
        private static FolderBrowserDialog folderDialog;
        private static Configuration configFile;
        public MainWindow()
        {
            InitializeComponent();

            VersionField.Content = "Version: " + version;
            configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Figure out our devices
            MMDeviceEnumerator theDevices = new MMDeviceEnumerator();

            // Microphones
            MMDeviceCollection inputDevices = theDevices.EnumAudioEndpoints(DataFlow.Capture, DeviceState.Active);
            VoicePicker.ItemsSource = inputDevices;

            // Speakers
            MMDeviceCollection outputDevices = theDevices.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);
            GuestPicker.ItemsSource = outputDevices;

            theAudio = new AudioHandler();
            folderDialog = new FolderBrowserDialog();

            if (configFile.AppSettings.Settings["OutputDirectory"] != null)
            {
                folderDialog.SelectedPath = configFile.AppSettings.Settings["OutputDirectory"].Value;
                OutputPath.Text = configFile.AppSettings.Settings["OutputDirectory"].Value;
            } else {
                string outDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                configFile.AppSettings.Settings.Add("OutputDirectory", outDir);
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);

                folderDialog.SelectedPath = outDir;
                OutputPath.Text = outDir;
            }

            RecordButton.IsEnabled = false;

        }


        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            theAudio.Exit(sender, e);
        }

        private void Record_Click(object sender, RoutedEventArgs e)
        {
            if (weAreRecording == true)
            {
                RecordButton.Content = "Record";
                weAreRecording = false;
                theAudio.StopRecording();
                RecordingLabel.Visibility = Visibility.Hidden;
            }
            else
            {
                weAreRecording = true;
                RecordButton.Content = "Stop";
                VoicePicker.IsEnabled = false;
                GuestPicker.IsEnabled = false;
                theAudio.StartRecording(OutputPath.Text);
                RecordingLabel.Visibility = Visibility.Visible;
            }
        }

        private void VoicePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MMDevice pickedDevice = (MMDevice)VoicePicker.SelectedItem;
            theAudio.addInputSource(pickedDevice, VoiceMeter);

            voicePicked = true;
            if (guestPicked == true)
            {
                RecordButton.IsEnabled = true;
            }
        }

        private void VoiceMeter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void GuestPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MMDevice pickedDevice = (MMDevice)GuestPicker.SelectedItem;
            theAudio.addOutputSource(pickedDevice, GuestMeter);
            guestPicked = true;
            if (voicePicked == true)
            {
                RecordButton.IsEnabled = true;
            }
        }

        private void GuestMeter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void Output_Click(object sender, RoutedEventArgs e)
        {
            folderDialog.ShowDialog();
            OutputPath.Text = folderDialog.SelectedPath;
            configFile.AppSettings.Settings["OutputDirectory"].Value = OutputPath.Text;
            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/joshbressers/WinCaster");
        }
    }
}
