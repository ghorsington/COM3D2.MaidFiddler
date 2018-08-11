> ⚠️ **Work in progress** ⚠️
> 
> This tool is not ready!  
> If you are developer, feel free to contribute/fork though.

# Maid Fiddler COM3D2 Edition
*Now with 5% more horses*

Maid Fiddler COM3D2 is a real-time value editor for COM3D2.  
This initially started as a [plug-in for CM3D2](https://github.com/denikson/CM3D2.MaidFiddler), but is now being rewritten for COM3D2.

## Beta status

This tool is in a early development stage. Everything is subject to change!

Currently the GUI version is working, but no prebuilt binaries are available for now.

If you are a developer, you can build Maid Fiddler directly from source code. Refer to instructions in `Core` and `GUI` folders.

## Contents of the repository

#### Core

Contains the core plug-in that is put in the game itself. Uses Sybaris and UnityInjector to get patched into the game.

#### Terminal

A simple terminal written in Python that allows to directly control the core plug-in.

#### GUI

The GUI application of Maid Fiddler made in Qt with Python backend.
