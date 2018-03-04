# Maid Fiddler Core

Maid Fiddler Core is the COM3D2 plug-in that modifies in-game values in real time.

Maid Fiddler Core runs a [ZeroRPC service](http://www.zerorpc.io/) (a *zeroservice*) and exposes an API that allows to

* Capture in-game information (maid info, current in-game phase, names, etc.)
* Modify in-game information
* Apply specialized patches provided by Maid Fiddler Core

The zeroservice is run locally on `127.0.0.1` over TCP, so you can communicate with it using any programming language/framework that has an implementation of ZeroRPC (currently [Python](https://github.com/0rpc/zerorpc-python), [Node.js](https://github.com/0rpc/zerorpc-node) and [.NET](https://github.com/denikson/zerorpc-dotnet)).

## Alpha status

Note that Maid Fiddler is still in early development stage. Core's API **will** change, so do not rely on it.


## Building

Currently only building through Visual Studio is supported -- a build script will come later.

Thus, to build the core:

1. Clone the repository with its dependencies:

```
git clone --recurse-submodules https://github.com/denikson/zerorpc-dotnet.git
```

2. Open the solution in Visual Studio.

3. Restore NuGet packages. You might need to manually re-add them, as zerorpc-net has its own dependencies.

4. Build the project.