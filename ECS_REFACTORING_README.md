# TramQuy Network - ECS Architecture Refactoring

## Overview

This project has been refactored from a traditional Object-Oriented Programming (OOP) networking architecture to an **Entity-Component-System (ECS)** architecture. This refactoring makes the networking code more modular, scalable, and suitable for MMO-style real-time multiplayer applications.

## Architecture Comparison

### Before (OOP Architecture)
```
NetworkManager (Class)
├── TCP Connection Management
├── Send/Receive Threads
├── Packet Processing
└── Session Management

LoginManager (Singleton)
├── NetworkManager Instance
└── Event Handling

Packets (Classes)
├── ClientPacket (Abstract)
├── ServerPacket (Abstract)
└── Specific Implementations
```

### After (ECS Architecture)
```
ECS World
├── Entities (Data Containers)
├── Components (Pure Data)
│   ├── NetworkConnectionComponent
│   ├── PacketBufferComponent
│   ├── SessionComponent
│   ├── LoginStateComponent
│   └── DeviceInfoComponent
└── Systems (Logic Processors)
    ├── ConnectionSystem
    ├── NetworkReceiveSystem
    ├── NetworkSendSystem
    ├── PacketDispatchSystem
    └── LoginSystem
```

## ECS Components

### NetworkConnectionComponent
- **Purpose**: Stores TCP connection state and configuration
- **Data**: TcpClient, NetworkStream, host, port, connection status, reconnection settings
- **Replaces**: NetworkManager connection fields

### PacketBufferComponent
- **Purpose**: Manages packet buffering and queuing
- **Data**: Receive buffer, send queue, receive queue, thread status flags
- **Replaces**: NetworkManager buffer management

### SessionComponent
- **Purpose**: Holds session-specific data
- **Data**: SessionId, initialization status, session start time
- **Replaces**: NetworkManager.SessionId

### LoginStateComponent
- **Purpose**: Tracks login flow state
- **Data**: LoginState enum, failure messages, state change timestamps
- **Replaces**: Implicit login state in original code

### DeviceInfoComponent
- **Purpose**: Contains device information for authentication
- **Data**: Platform, memory size, device name
- **Replaces**: Static DeviceInfo class usage

## ECS Systems

### ConnectionSystem
- **Purpose**: Manages TCP connection lifecycle
- **Responsibilities**:
  - Establish connections
  - Handle disconnections
  - Retry logic
  - Connection health monitoring
- **Replaces**: NetworkManager.Connect() and connection management

### NetworkReceiveSystem
- **Purpose**: Handles incoming network data
- **Responsibilities**:
  - Background thread for receiving data
  - Buffer management and expansion
  - Packet parsing and queuing
- **Replaces**: NetworkManager.ReceiveLoop() and ProcessReadBuffer()

### NetworkSendSystem
- **Purpose**: Handles outgoing network data
- **Responsibilities**:
  - Background thread for sending data
  - Send queue processing
  - Error handling during send operations
- **Replaces**: NetworkManager.SendLoop() and SendPacket()

### PacketDispatchSystem
- **Purpose**: Routes received packets to appropriate handlers
- **Responsibilities**:
  - Dequeue received packets
  - Create packet instances via factory
  - Execute packet logic (ECS-aware or legacy)
- **Replaces**: NetworkManager.HandlePacket()

### LoginSystem
- **Purpose**: Orchestrates the login flow
- **Responsibilities**:
  - Auto-authentication after session init
  - Login timeout handling
  - State management
- **Replaces**: Implicit login logic in SM_INIT packet

## Key Benefits of ECS Architecture

### 1. **Separation of Concerns**
- **Data** (Components) separated from **Logic** (Systems)
- Each system has a single, well-defined responsibility
- Components are pure data structures

### 2. **Scalability**
- Easy to add new components for additional features
- Systems can be added/removed without affecting others
- Multiple entities can share the same components

### 3. **Testability**
- Systems can be tested in isolation
- Components can be mocked easily
- Clear dependencies between systems

### 4. **Performance**
- Data-oriented design improves cache locality
- Systems process entities in batches
- Minimal object allocation during runtime

### 5. **Modularity**
- Each system is self-contained
- Easy to disable/enable specific functionality
- Clean interfaces between systems

## Backward Compatibility

The refactored system maintains backward compatibility:

- `LoginManager` still exists with the same interface
- Legacy packet classes still work
- Events are still fired for UI integration
- Same connection methods and properties

## Usage Example

```csharp
// Create ECS network manager
var ecsNetwork = new EcsNetworkManager();

// Set up event handlers
ecsNetwork.OnConnected += () => Console.WriteLine("Connected!");
ecsNetwork.OnLoginSuccess += () => Console.WriteLine("Login successful!");

// Connect to server
ecsNetwork.Connect("127.0.0.1", 1906);

// Check status
bool connected = ecsNetwork.IsConnected();
LoginState state = ecsNetwork.GetLoginState();
int sessionId = ecsNetwork.GetSessionId();
```

## Extending the System

### Adding New Components
```csharp
public class PlayerDataComponent : IComponent
{
    public int EntityId { get; set; }
    public string PlayerName { get; set; }
    public int Level { get; set; }
    public Vector3 Position { get; set; }
}
```

### Adding New Systems
```csharp
public class PlayerUpdateSystem : ISystem
{
    public void Update(World world, float deltaTime)
    {
        var players = world.GetEntitiesWith<PlayerDataComponent, NetworkConnectionComponent>();
        foreach (var player in players)
        {
            // Process player updates
        }
    }
}
```

### Adding New Packets
```csharp
public class SM_PLAYER_UPDATE_Ecs : ServerPacket, IEcsPacket
{
    public void ProcessInEcs(Entity entity, PacketReader reader)
    {
        var playerData = entity.GetComponent<PlayerDataComponent>();
        if (playerData != null)
        {
            playerData.Level = reader.ReadInt();
            playerData.Position = reader.ReadVector3();
        }
    }
}
```

## Migration Guide

### For Existing Packet Handlers
1. Implement `IEcsPacket` interface
2. Add `ProcessInEcs(Entity entity, PacketReader reader)` method
3. Update `ServerPacketFactory` to return ECS version
4. Access data through entity components instead of singletons

### For New Features
1. Define data as components (no logic)
2. Create systems to process the data
3. Add systems to the world in `EcsNetworkManager`
4. Use `world.GetEntitiesWith<>()` to query entities

## File Structure

```
TramQuyNetwork/
├── arriety/
│   ├── ecs/                          # ECS Framework
│   │   ├── Entity.cs                 # Entity container
│   │   ├── IComponent.cs             # Component interface
│   │   ├── ISystem.cs                # System interface  
│   │   ├── World.cs                  # ECS world manager
│   │   ├── EcsNetworkManager.cs      # Main ECS network manager
│   │   ├── components/               # All ECS components
│   │   │   ├── NetworkConnectionComponent.cs
│   │   │   ├── PacketBufferComponent.cs
│   │   │   ├── SessionComponent.cs
│   │   │   ├── LoginStateComponent.cs
│   │   │   └── DeviceInfoComponent.cs
│   │   ├── systems/                  # All ECS systems
│   │   │   ├── ConnectionSystem.cs
│   │   │   ├── NetworkReceiveSystem.cs
│   │   │   ├── NetworkSendSystem.cs
│   │   │   ├── PacketDispatchSystem.cs
│   │   │   └── LoginSystem.cs
│   │   └── packets/                  # ECS-compatible packets
│   │       ├── CM_AuthGG_Ecs.cs
│   │       ├── SM_INIT_Ecs.cs
│   │       └── SM_LOGIN_RESPONSE_Ecs.cs
│   ├── login/                        # Legacy login system (updated)
│   └── utils/                        # Utility classes (unchanged)
├── Form1.cs                          # Updated to show ECS status
├── Program.cs                        # Updated to use ECS
└── ECS_REFACTORING_README.md         # This documentation
```

## Performance Considerations

### Thread Safety
- ECS systems use thread-safe collections for packet queues
- Background threads for network I/O
- Main thread for ECS logic updates

### Memory Management
- Component data is stored in entity dictionaries
- Packet buffers are reused and expanded as needed
- Minimal allocations during packet processing

### Update Frequency
- ECS world updates at ~60 FPS (16ms intervals)
- Network I/O threads run continuously with small delays
- Status checking happens every second for UI updates

## Future Enhancements

The ECS architecture enables easy addition of:

1. **Multiple Connections**: Each connection as a separate entity
2. **Player Management**: Player entities with position, stats, etc.
3. **Game State**: World state, NPCs, items as entities
4. **Networking Optimization**: Packet compression, delta compression
5. **Event System**: Decoupled event handling between systems
6. **Serialization**: Save/load game state from components
7. **Debugging Tools**: Runtime inspection of entities and components

## Conclusion

This ECS refactoring transforms the networking code from a monolithic OOP design to a modular, data-driven architecture that's well-suited for MMO development. The separation of data and logic, combined with the entity-based approach, provides a solid foundation for building complex multiplayer game systems.

The refactored system maintains full backward compatibility while providing significant benefits in terms of maintainability, testability, and scalability. 