# Quest

**Quest** is a pixelates tile-based adventure game built with a custom engine. It uses simple dynamic connected textures, an integrated level editor, and a modular tile system.

## Features

- **Tile-Based World**  
  Fully customizable grid-based maps with support for different terrain, structures, and entities.

- **Connected Textures**  
  Tiles automatically determine their appearance based on surrounding tiles, smooth connections.

- **NPCs**  
  Non-playable characters with custom dialog and textures can be placed in the world.

- **Level Editor**  
  A built-in lightweight editor to design and export levels.

- **Modular Tile System**  
  Each tile is an object that can define behavior, appearance, and interactions.

- **Optimized Performance**  
  The game runs very fast on any computer and uses up little memory.

## Getting Started

### Prerequisites

- [.NET](https://dotnet.microsoft.com/)
- [MonoGame](https://www.monogame.net/)
- Monogame Extended

### Running the Game
Clone the repository:
   ```bash
   git clone https://github.com/Nektarios1667/Quest.git
   cd Quest
   ```

### Modifying Constants
- Constants for the game, rendering, settings, etc are stored in `Constants.cs`  
- Variables like window size and player movement speed can easily be modifed.  
- Any changes will be reflected throughout the code.  
- Debug flags such as TEXT_INFO can also be used to show/hide realtime debug info.  
- **Warning: Some constants may be used for important rendering and calulations. Changing these can break game code.**  
  
