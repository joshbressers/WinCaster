# WinCaster
Simple tool for recording podcast audio


# About
![image](https://user-images.githubusercontent.com/1692786/110213270-ed6b6580-7e64-11eb-9958-65fad5545d81.png)

WinCaster is a tool that will record the audio from a local microphone as well as the audio being sent to speakers. The intent is to use this to record yourself and guest for podcasting. The idea was taken from a Linux tool called [Pulsecaster](http://stickster.github.io/pulsecaster/)

Yes, the artwork is very bad, but it's grown on me, so I'm keeing it for now.

# How to use
Just pick the audio devices you want to record, choose an output folder, and hit record. The output will be wav files that include a timestamp so you don't accidentally overwrite any old files. It's meant to be very simple.

# Notes
- If the output device has nothing sending it sound, there's nothing to record so the wav files will be empty
- The output wav volume is dependent on your speaker output volume
- I am not a csharp developer, I opted for simple over clever anytime I could in the code
