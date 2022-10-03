# Zero Game Development Kit

![zero-game-demo](.repo/game-demo.gif)

**Zero Gdk** is a C# client/server game world development framework.

# Features

- Entity-Component-System for worlds
- Client connection handling
- Entity data networking and view transmitting
- Distributed world server support
- Multithreaded operations for complete CPU usage
- Client received simulation support (simulate a client's data on the server)

# Install

## Nuget

You can install Gdk packages via nuget. There are four packages available.

- **ZeroServices.Game.Shared** - Client/Server shared code
- **ZeroServices.Game.Server** - Server integrations
- **ZeroServices.Game.Client** - Client integrations
- **ZeroServices.Game.Local** - Setting up a locally hosted server

## Unity Client

There is a Unity package available that install the gdk libraries with some Unity setup classes

To install, add a package via git url
```
https://github.com/Unnamed-Studios-LLC/Zero.Unity.git
```

# Getting started

## Plugin classes

Server and client integration are centered around the **ServerPlugin** and **ClientPlugin** classes.
Override these classes to add your implementation.
### ServerPlugin
```csharp
// server options
ServerOptions Options { get; set; }

// add the connection to it's world (connections may be added/remove from worlds without load/unload being called)
void AddToWorld(Connection connection);

// build data definitions
void BuildData(DataBuilder builder);

// a deferred world start has completed
Task DeferredWorldResponseAsync(StartWorldResponse response);

// load a connection's data (load from database, etc.)
Task<bool> LoadConnectionAsync(Connection connection);

// load a world's data (database, files, etc.)
Task<bool> LoadWorldAsync(World world);

// remove a connection from a world
void RemoveFromWorld(Connection connection);

// starts a server deployment, typically houses initial Deployment.* calls.
Task StartDeploymentAsync();

// starts a server worker, called once per worker before any logic, typically sets up a workers environment
Task StartWorkerAsync();

// called when a deployment has stopped
Task StopDeploymentAsync();

// called when a worker has stopped
Task StopWorkerAsync();

// unloads a connection (save to database, etc.)
Task UnloadConnectionAsync(Connection connection);

// unloads a world
Task UnloadWorldAsync(World world);
```

### ClientPlugin:
```csharp
// client options
ClientOptions Options { get; set; }

// build data definitions
void BuildData(DataBuilder builder);

// connected to server
void Connected();

// copnnecting to server
void Connecting();

// disconnected from server
void Disconnected();
```

## Quick start guide

### Running a local server

1. Create a new console application project
2. Install **ZeroServices.Game.Local** package
3. Create a subclass of ServerPlugin
4. Call ZeroServer.Run, passing your plugin type as the generic parameter

```csharp
class Program
{
    static async Task Main(string[] args)
    {
        await ZeroLocal.RunAsync<MockPlugin>(args);
    }
}
```

5. Next steps
    - [Adding your first world](#adding-your-first-world)
    
That's it! Your server is up and running on localhost

### Connecting a client
 
1. Install **ZeroServices.Game.Client** package (https://github.com/Unnamed-Studios-LLC/Zero.Unity.git if using unity)
2. Create a subclass of ClientPlugin
3. Create an implementation of ILoggingProvider
4. Execute a POST HTTP request to the local server with a body of StartConnectionRequest class

Url
```
https://localhost:4001/api/v1/connnection
```
Request Body (request body should be in json format)
```csharp
public class StartConnectionRequest
{
    public uint WorldId { get; set; }
    public string ClientIp { get; set; }
    public Dictionary<string, string> Data { get; set; }
}
```
Response Body (response will be transmitted in json)
```csharp
public class StartConnectionResponse
{
    public ConnectionFailReason? FailReason { get; set; }
    public string WorkerIp { get; set; }
    public int Port { get; set; }
    public string Key { get; set; }
}
```

5. Use response information to call ZeroClient.Create, passing in the response information, [message handler](#message-handler), logging, and client plugin implementation.

```csharp
ZeroClient Create(IPAddress address, int port, string key, IMessageHandler messageHandler, ILoggingProvider loggingProvider, ClientPlugin plugin);
```

6. Stored the returned **ZeroClient** instance and call **Update** every network update
    - Network update is typically less than your frame/client update. Typically synced with your servers update rate.
    - For Unity, you can use FixedUpdate for this

# Architecture and Overview

## Entities

A collection of related components and data. Entities do not have any object representation,
instead entities are acted on by methods using a passed in **entity id**.

## Components

An "information chunk". Typically houses data that is closely related or used by a given system.

## Systems

The logic and processes for components and entities. Systems update the game state and alter entity structure.

## Data

Data is information about an entity that is transmitted to connections. Data can be attached to an entity as **persistent** or sent
in a fire and forget **event**.

## World

A container for entities, systems, components, data, and connections.

*An entity id is created for each world.
You may use the entity id as usual, however, the GDK will handle create/destroy for the id*

## Connection

A connected client and its information.

*An entity id is created for each connection.
You may use the entity id as usual, however, the GDK will handle create/destroy for the id*

## View

A list of entity ids that a connection will receive data about.

## Message Handler

Processes received data, required for both the client and server.

# Adding your first world

# Adding your first system, entity, and component

# Transmitting entities to a connection

# Misc

## Update loop and order of operations
<img src=".repo/update-loop.png" width="700">
