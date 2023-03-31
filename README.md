<br/>
<p align="center">
  <a href="https://github.com/tpbeldie/UFLVK-Emu">
    <img src="https://i.imgur.com/tcvzKR8.png" alt="Logo" width="80" height="80">
  </a>
  <h3 align="center">UFLVK-Emu</h3>

  <p align="center">
    Universal FL Studio Virtual Keyboard Emulator for all DAWs
    <br/>
    <a href="https://github.com/tpbeldie/UFLVK-Emu">View Demo</a>
    .
    <a href="https://github.com/tpbeldie/UFLVK-Emu/issues">Report Bug</a>
    .
    <a href="https://github.com/tpbeldie/UFLVK-Emu/issues">Request Feature</a>
  </p>
</p>

## Contents

* [About](#about)
* [Requirements](#requirements)
* [Getting Started](#getting-started)
* [License](#license)
* [Authors](#authors)

## About

![Screen Shot](https://cdn.discordapp.com/attachments/121395249003233280/1091135962383392892/UFLVK_Emu_MYXk78YhSe.png)

Initially this project was meant to be a personal project for personal needs. As an (ex) FL Studio user, my favorite part of the DAW was the in-built ability to use your typing keyboard as a MIDI keyboard, it supported most of the buttons from the centric area (37 buttons), unlike other programs that doesn't offer this feature or the number of keys is greatly reduced to a minimal amount, where a series of buttons remain for general control and switching octaves to the few keys using to sing, that is MEH. FL Studio has by far the best native MIDI emulation a DAW could offer for people who lack a MIDI keyboard device or simply prefer using the typing keyboard. Thus, it made the potential jump from one program to another really hard for me. 

It supports most of the features of FL Studio's virtual piano, with other bonus features, including:

* **Root Note Selection** - Allows you to easily select the root note of your musical composition, which is essential for creating harmonic progressions and melodies, which determines the pitch of all the other notes and musical elements in your composition. 

* **Velocity Mapping** - Maps the velocity of the notes you click based on the location of the cursor, allowing for more realistic and nuanced performance by emulating the velocity sensitivity of physical instruments. The behavior of this feature is consistent with FL Studio's default behavior.

* **Global Velocity** - This knob allows you to adjust the default velocity of the regular played keys, which is set at its maximum value (127) by default, giving you control over the overall loudness and intensity of your musical composition.

* **Identical FL Studio Key Mapping** - Use *Z*, *S*, *X*, *D*, *C*, *V*, *G*, *B*, *H*, *N*, *J*, *M*, *Oemcomma*, *L*, *OemPeriod*, *Oem1*, *OemQuestion*, *Q*, *D2*, *W*, *D3*, *E*, *R*, *D5*, *T*, *D6*, *Y*, *D7*, *U*, *I*, *D9*, *O*, *D0*, *P*, *OemOpenBrackets*, *Oemplus*, *Oem6*, to play. 

* **Beep?** - When this feature is enabled, the application will generate a beep or boop sound to indicate when it is activated or deactivated, providing audio feedback that helps to confirm changes in the program's status.

* **Steal Focus?** - Enabling this feature ensures that every time a key is pressed, the application will take exclusive control of global inputs and prevent conflicts with other software by stealing the focus from any other active applications instances the initial key was sent to. This helps to ensure smooth and uninterrupted workflow while using your DAW. (It also works while minimized).

* **Input Feedback** - his feature provides real-time display of details such as the current root key & note, as well as the currently pressed key(s) and corresponding notes, making it a powerful tool for learning and improving musical proficiency, especially for new people using the typing keyboard as MIDI keyboard.

* **Steal Focus?** - No brainer. 

* **And More** - You can discover them yourself. :smile:

Here is a quick video demonstration in Bitwig. If I had FL STudio or another DAW opened, it would've worked on both simultaneously. 

https://user-images.githubusercontent.com/122232758/229094232-23a8123b-ac8c-4ae7-a22e-0ae0407fd4be.mp4

## Requirements

- NET Framework 4.6 or a higher version
- This program uses loopMIDI as gateway of communicating with the DAW, which can only perceive loopMIDI's ports as MIDI Keyboards. So UFLVK-Emu will send the data to loopMIDI's port and loopMIDI will forward the data to the DAW. So you need to have it installed.

You can download it from here: https://www.tobias-erichsen.de/software/loopmidi.html

## Getting Started

Here is a step-by step guide on how to get started.

1. Download the latest compiled assembly from the releases page: https://github.com/tpbeldie/UFLVK-Emu/releases
2. Extract the archive anywhere you want. It requires no installation.
3. In loopMIDI create a new port. Give it any name of choice. 
4. In your DAW, set a new MIDI Keyboard device and pick the freshly created port in loopMIDI.
5. In the UFLVK-Emu's folder, create a new text file called loopMIDI.txt where you will put the index number (starting from 0) that will specify the index of the MIDI Keyboard (loopMIDI Port) in the list of global midi devices.
6. Run UFLVK-Emu. Enjoy!

* **IMPORTANT**: If step 6 doesn't work, close the application, go back to step 5 and increment the index. And remember, UFLVK-Emu only listens to inputs and passes the data when **Caps Lock** is enabled, this button is the main toggle that will mark UFLVK-Emu either as ACTIVE or INACTIVE.

## License

Distributed under the MIT License. See [LICENSE](https://github.com/tpbeldie/UFLVK-Emu/blob/main/LICENSE.md) for more information.

## Authors

* **tpbeldie**  - [Github Profile](https://github.com/tpbeldie) - *Main Developer*
