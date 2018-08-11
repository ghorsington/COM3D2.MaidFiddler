# Maid Fiddler Core

Maid Fiddler Core is the COM3D2 plug-in that modifies in-game values in real time.

Maid Fiddler Core runs a [ZeroRPC service](http://www.zerorpc.io/) (a *zeroservice*) and exposes an API that allows to

* Capture in-game information (maid info, current in-game phase, names, etc.)
* Modify in-game information
* Apply other cheats provided by Maid Fiddler Core

The zeroservice is run locally on `127.0.0.1` over TCP, so you can communicate with it using any programming language/framework that has an implementation of ZeroRPC (currently [Python](https://github.com/0rpc/zerorpc-python), [Node.js](https://github.com/0rpc/zerorpc-node) and [.NET](https://github.com/denikson/zerorpc-dotnet)).

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

5. Build the project (this should automatically restore NuGet packages).