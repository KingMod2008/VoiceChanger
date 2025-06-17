# 🎤 KingVoiceChanger

<p align="center">
  <img src="https://img.shields.io/badge/Version-1.0-blue" alt="Version">
  <img src="https://img.shields.io/badge/License-MIT-green" alt="License">
  <img src="https://img.shields.io/badge/Platform-Windows-yellow" alt="Platform">
</p>

A powerful real-time voice changer application built with C# and WPF. 🚀

## 🌟 Features

- 🎮 Works with ANY game or app! Uses a virtual audio device to act as a microphone.
- 🎨 Modern, dark-themed user interface with smooth animations.
- 🎯 Select audio input (microphone) and output (speaker) devices easily.
- 🎭 Over 10 preset voice effects (Chipmunk, Ogre, Robot, etc.).
- 🎯 Real-time voice processing with minimal latency.
- 🎯 Customizable effect parameters.
- 🎯 Save and load your favorite presets.
- 🎯 Hotkey support for quick effect switching.

## 🎮 How to Use in Games and Other Apps

To use KingVoiceChanger in games, Discord, or any other application, you'll need a free virtual audio driver called VB-CABLE.

### 1️⃣ Install VB-CABLE

1. Download VB-CABLE from [vb-audio.com/Cable](https://vb-audio.com/Cable/)
2. Unzip the downloaded file
3. Right-click `VBCABLE_Setup_x64.exe` and select "Run as administrator" to install it

### 2️⃣ Configure KingVoiceChanger

1. Start KingVoiceChanger
2. Input Device: Select your real microphone
3. Output Device: Select `CABLE Input (VB-Audio Virtual Cable)`
4. Select a voice effect and click "APPLY EFFECT". The status will change to "Active"

### 3️⃣ Configure Your Game/App

1. In your game or other application's audio settings, set your microphone/input device to `CABLE Output (VB-Audio Virtual Cable)`

**⚠️ IMPORTANT NOTE:** The virtual microphone created by VB-CABLE is named "CABLE Output". It cannot be renamed to "KingVoiceChanger" by the app. When you select your mic in a game, be sure to choose "CABLE Output".

## 💻 How to Build and Run

1. Open [VoiceChanger.sln](cci:7://file:///c:/Users/M/Desktop/vc/VoiceChanger/VoiceChanger.sln:0:0-0:0) in Visual Studio 2022
2. Build the solution (Ctrl+Shift+B)
3. Run the KingVoiceChanger project (F5)

## 🎨 How to Add a Custom App Icon

The project is ready to build, but without an icon. To add your own:

1. Find or create a valid icon file and name it `icon.ico`
2. Place `icon.ico` inside the `VoiceChanger/` folder
3. Open `VoiceChanger.csproj` and add this line inside the first `<PropertyGroup>` section:
   ```xml
   <ApplicationIcon>icon.ico</ApplicationIcon>
