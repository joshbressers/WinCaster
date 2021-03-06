using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using System.Text;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CSCore;
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using CSCore.Codecs.WAV;

namespace WinCaster
{
    class AudioHandler
    {
        private static System.Timers.Timer theTimer;
        private static MMDevice inputDevice = null;
        private static MMDevice outputDevice = null;
        private static ProgressBar inputProgress;
        private static ProgressBar outputProgress;
        private static WasapiCapture inputCapture;
        private static WasapiCapture outputCapture;
        private static WaveWriter inputWave;
        private static WaveWriter outputWave;
        private static bool areRecording = false;

        public AudioHandler()
        {
            theTimer = new System.Timers.Timer();
            theTimer.Elapsed += TripTimer;
            theTimer.Interval = 100;
            theTimer.Enabled = true;
            theTimer.Start();
        }
        
        public void addInputSource(MMDevice audioDevice, ProgressBar aProgress)
        {

            if (inputDevice != null)
            {
                inputCapture.Stop();
            }
            inputDevice = audioDevice;
            inputProgress = aProgress;

            inputCapture = new WasapiCapture();

            inputCapture.Device = inputDevice;
            inputCapture.Initialize();
            inputCapture.Start();
        }

        public void addOutputSource(MMDevice audioDevice, ProgressBar aProgress)
        {

            if (outputDevice != null)
            {
                outputCapture.Stop();
            }

            outputDevice = audioDevice;
            outputProgress = aProgress;

            outputCapture = new WasapiLoopbackCapture();

            outputCapture.Device = outputDevice;
            outputCapture.Initialize();
            outputCapture.Start();
        }
        private void TripTimer(Object source, System.Timers.ElapsedEventArgs e)
        {
            AudioMeterInformation theInfo;
            float peakValue;

            if (inputDevice != null)
            {
                theInfo = AudioMeterInformation.FromDevice(inputDevice);
                peakValue = theInfo.GetPeakValue();
                inputProgress.Dispatcher.InvokeAsync((Action)(() =>
                {
                    inputProgress.Value = (double)(peakValue * 500);
                }));
            }

            if (outputDevice != null)
            {
                theInfo = AudioMeterInformation.FromDevice(outputDevice);
                peakValue = theInfo.GetPeakValue();
                outputProgress.Dispatcher.InvokeAsync((Action)(() =>
                {
                    outputProgress.Value = (double)(peakValue * 500);
                }));
            }
        }
        public void StartRecording(string thePath)
        {
            areRecording = true;
            string dateString = DateTime.Now.ToString("yyyyMMddHmmss");
            string voiceOutput = Path.Combine(thePath, dateString + "-voice.wav");
            string guestOutput = Path.Combine(thePath, dateString + "-guest.wav");

            inputWave = new WaveWriter(voiceOutput, inputCapture.WaveFormat);
            outputWave = new WaveWriter(guestOutput, outputCapture.WaveFormat);

            inputCapture.DataAvailable += WriteInputData;

            outputCapture.DataAvailable += WriteOutputData;
        }

        private void WriteInputData(object s, DataAvailableEventArgs e)
        {
            Thread.CurrentThread.IsBackground = true;
            inputWave.Write(e.Data, e.Offset, e.ByteCount);
        }

        private void WriteOutputData(object s, DataAvailableEventArgs e)
        {
            Thread.CurrentThread.IsBackground = true;
            outputWave.Write(e.Data, e.Offset, e.ByteCount);
        }

        public void StopRecording()
        {
            areRecording = false;
            outputCapture.DataAvailable -= WriteOutputData;
            inputCapture.DataAvailable -= WriteInputData;

            outputWave.Dispose();
            inputWave.Dispose();
        }

        public void Exit(Object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (areRecording == true)
            {
                if (MessageBox.Show("You are recording, close anyway?", "Recording runs", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    StopRecording();
                }
            }
        }
    }
}
