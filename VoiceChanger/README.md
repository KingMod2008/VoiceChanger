# KingVoiceChanger

A powerful real-time voice changer application built with C# and WPF.

## Features

*   **Works with any game or app!** Uses a virtual audio device to act as a microphone.
*   Modern, dark-themed user interface.
*   Select audio input (microphone) and output (speaker) devices.
*   Over 10 preset voice effects (Chipmunk, Ogre, Robot, etc.).

---

## How to Use in Games and Other Apps

To use KingVoiceChanger in games, Discord, or any other application, you need to install a free virtual audio driver called **VB-CABLE**.

**1. Install VB-CABLE:**
   - Download VB-CABLE from [https://vb-audio.com/Cable/](https://vb-audio.com/Cable/).
   - Unzip the downloaded file.
   - Right-click `VBCABLE_Setup_x64.exe` and select **"Run as administrator"** to install it.

**2. Configure KingVoiceChanger:**
   - Start KingVoiceChanger.
   - **Input Device:** Select your **real microphone**.
   - **Output Device:** Select **CABLE Input (VB-Audio Virtual Cable)**.
   - Select a voice effect and click **"APPLY EFFECT"**. The status will change to "Active".

**3. Configure Your Game/App:**
   - In your game or other application's audio settings, set your **microphone/input device** to **CABLE Output (VB-Audio Virtual Cable)**.

> **IMPORTANT NOTE:** The virtual microphone created by VB-CABLE is named **"CABLE Output"**. It cannot be renamed to "KingVoiceChanger" by the app. When you select your mic in a game, be sure to choose **"CABLE Output"**.

Now, your voice will be processed by KingVoiceChanger and sent to your game through the virtual cable!

---

## How to Build and Run

1.  Open `VoiceChanger.sln` in Visual Studio 2022.
2.  Build the solution (`Ctrl+Shift+B`).
3.  Run the `KingVoiceChanger` project (`F5`).

## How to Add a Custom App Icon

The project is ready to build, but without an icon. To add your own:

1.  Find or create a valid icon file and name it `icon.ico`.
2.  Place `icon.ico` inside the `VoiceChanger/` folder.
3.  Open `VoiceChanger.csproj` and add this line inside the first `<PropertyGroup>` section:
    ```xml
    <ApplicationIcon>icon.ico</ApplicationIcon>
    ```
4.  Rebuild the project.
