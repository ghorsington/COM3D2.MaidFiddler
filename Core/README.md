# Maid Fiddler Core

Maid Fiddler Core is the COM3D2 plug-in that modifies in-game values in real time.

Maid Fiddler Core communicates with the GUI via [named pipes](https://docs.microsoft.com/en-us/windows/desktop/ipc/named-pipes).  
Namely, the Core exposes the following capabilities via its API:

* Capturing in-game information (maid info, current in-game phase, names, etc.)
* Modifying in-game information
* Applying other cheats provided by Maid Fiddler Core

Core runs two separate pipes: `MaidFiddlerService` for general IPC and `MaidFildderEventEmitter` for sending in-game events (i.e. maid created, property changed) to the GUI.

## Beta status

Note that Maid Fiddler is still in development. Core's API is more or less stable, but might change in the future.

## Building

Currently only building through Visual Studio is supported.

Thus, to build the core:

1. Clone the repository with its dependencies:

```
git clone https://github.com/denikson/COM3D2.MaidFiddler.git
```

2. Put required assemblies into `Core/Libs` folder (refer to `Core/Libs/README.md` for a list)

2. Open the solution in Visual Studio.

5. Build the project