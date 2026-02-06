# BlockPuzzle <br>
<img align = "right" width="295" height="639" alt="IMG_7990" src="https://github.com/user-attachments/assets/e43b3481-b9ae-4ec6-917b-d52475d89b65" />

A Unity-based block puzzle game featuring an intelligent block suggestion system and modular architecture.

![Unity Version](https://img.shields.io/badge/Unity-6000.2.7f2-blue)
![Platform](https://img.shields.io/badge/Platform-iOS%20%7C%20Android%20%7C%20WebGL-blue)
### [Play In Browser](https://play.unity.com/en/games/9b21f18f-8b7a-431d-b6ec-c0ef39316ec2/blockpuzzlewebglbuild)

### Overview

BlockPuzzle is an 8×8 grid puzzle game where player drags and drops blocks to clear rows and columns. The game features a unique algorithm that intelligently suggests blocks based on the current field state, making each playthrough strategic and engaging.

### Key Features

- **Intelligent Block Selection** - Smart algorithm suggests optimal blocks for the current field state
- **Customizable Themes** - Multiple color themes using ScriptableObjects
- **Interactive Tutorial** - Step-by-step guidance for new players
- **iOS Haptic Feedback** - Native haptics integration written in Objective-C
- **Automatic Save System** - Progress saved after every move
- **Adaptive Camera** - Automatically fits all screen resolutions with safe area support
- **Custom Editor Tools** - Built-in tools for creating blocks and tutorials
- **Audio Manager** - Centralized sound effects system

<br clear = "right">


## Quick Start

### Prerequisites

- **Unity 6000.2.7f2** (download from [Unity Hub](https://unity.com/download))

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/BabkenInants/BlockPuzzle.git
   ```

2. Open Unity Hub and click **Add** → select the cloned folder

3. Open the project with Unity 6000.2.7f2

4. Load the game scene: `Assets/Scenes/EndlessGame.unity`

5. Press Play

### Opening the Code

In Unity Editor: **Assets → Open C# Project** to launch your IDE

## Block Suggestion Algorithm

The game uses a sophisticated algorithm to suggest blocks that fit well with the current field state:

1. **Field Simulation** - Creates a temporary copy of the field for testing
2. **Block Evaluation** - Tests each block in all possible positions
3. **Position Scoring** - Rates each position based on:
   - Number of cells the block occupies
   - Connectivity to adjacent free cells
   - Potential for clearing rows/columns
4. **Intelligent Selection** - Chooses blocks that maximize field utility

**Key Functions:**
- `GenerateNextBlocks()` - Main entry point for block generation
- `FindBlockForField()` - Selects best block from available options
- `GetBestPositionForBlock()` - Evaluates all possible placements
- `FieldUtils.RateField()` - Scoring function for field states

**Scoring Formula:**
```
Score = Σ(cellGrade²) × clearMultiplier
where cellGrade = 0 (occupied) or 1 + adjacentFreeCells (free)
```

## Developer Guides

### Adding a New Block <br>
<img align = "right" width="577" height="700" alt="Screenshot 2026-02-06 at 11 02 02" src="https://github.com/user-attachments/assets/f8a54e5a-ee68-4d39-bc91-59e5b1e41991" />

1. Duplicate an existing block prefab in `Assets/Prefabs/Blocks`
2. Double-click the prefab to edit
3. Arrange cells in your desired pattern
4. Ensure BoxCollider2D is square (minimum 2×2) and fits the block perfectly
5. Update the `cells` array in the Block script
6. Adjust Matrix SizeX and SizeY
7. Draw your block pattern (green = occupied, gray = empty)
8. Add the block to the Settings ScriptableObject

<br clear = "right">

### Adding a New Theme <br>
<img align = "right" width="579" height="778" alt="Screenshot 2026-02-06 at 02 28 17" src="https://github.com/user-attachments/assets/667fe8e1-e95b-451d-8765-3c57f7a63821" />

1. Duplicate a theme in `Assets/Themes` or create new: **Right-click → Create → NewTheme**
2. Configure your colors
3. Add the theme to Theme Manager (in Managers on the scene) in the `themes` array

<br clear = "right">

### Implementing IThemeReceiver

```csharp
using Themes;

public class MyComponent : MonoBehaviour, IThemeReceiver
{
    public void ReceiveThemeOnGameStart(Theme theme)
    {
        // Apply theme without animations
        // Called once when game loads
    }
    
    public void ReceiveTheme(Theme theme)
    {
        // Apply theme with animations
        // Called when theme changes during gameplay
    }
}
```

**Tip:** Use `ThemeTools` for color animation coroutines.

### Adding a Tutorial Example <br>
<img align="right" width="577" height="781" alt="Screenshot 2026-02-06 at 02 41 52" src="https://github.com/user-attachments/assets/cb82cdcd-2ae9-4e43-9591-6b0eeb9ffb2a" />

1. Duplicate an existing example in `Assets/Tutorials` or create new: **Right-click → Create → Tutorial Example**
2. Draw the field (leave space for one block)
3. Assign a Block Prefab from `Assets/Prefabs/Blocks`
4. Duplicate that prefab into `Assets/Tutorials/TutorialBlocks`
5. Remove BoxCollider2D and Block components, add TutorialBlock instead
6. Open the prefab and move cells so the pivot is at the top-left corner <br>
Examples:  
<img width="195" height="167" alt="Screenshot 2026-02-06 at 00 32 05" src="https://github.com/user-attachments/assets/08950e5a-5476-41fe-bc3d-e9cc5ffb981a" /> <img width="78" height="167" alt="Screenshot 2026-02-06 at 00 32 33" src="https://github.com/user-attachments/assets/fa8f0731-859f-4d8d-956f-259f5bafb331" /> 
7. Assign the modified prefab to Preview Block Prefab field
8. Click Show and set Target Pos (row, column) to the pivot cell position
9. Add the example to Managers/Tutorial Manager in the scene

<br clear = "right">

### Using Haptic Feedback (iOS)

```csharp
GameEvents.RaisePlayHaptics(HapticManager.HapticType); //Plays haptics of desired HapticType

GameEvents.RaisePlayHapticsInARow(HapticManager.HapticType); //Plays haptics of desired HapticType for n times
```
**Haptic Types:**  
- Light
- Medium
- Heavy

**Haptic Types Used:**
- Block placement (no clear): Light
- Block placement (with clear): Heavy × lines cleared
- UI buttons: Light

### Implementing ISavable

```csharp
using Saves;

public class MyComponent : MonoBehaviour, ISavable
{
    public void Save(SaveData saveData)
    {
        // Save your data
    }
    
    public void Load(SaveData saveData)
    {
        // Load your data
    }
}
```

**Note:** Only use serializable types in SaveData (primitives, strings, arrays, serializable structs).
**Tip:** Use SerializableColor for saving colors

### Safe Zone Preview

**To preview:**
1. Enable the SafeZonePreview object in the scene
2. Adjust Screen Width and Height in the Settings ScriptableObject → Camera Section
3. Change Game window resolution or enter Play mode to see camera updates
4. Disable SafeZonePreview when done

### Audio Manager Implementation
```csharp
RaisePlaySfx(clip);
```
**Note:** For UI actions that trigger scene loads, wait for clip.length + 0.1f before loading to ensure sound completes.
I had audio files but deleted them because I didn't have a license for all of them. Anyway there are fields for basic sound effects in settings ScriptableObject.



## Use Cases

- **Game Development:** Reference implementation for block puzzle mechanics
- **Algorithm Study:** Example of intelligent content suggestion
- **Unity Learning:** Demonstrates editor tools, ScriptableObjects, and interfaces
- **Mobile Development:** Shows iOS native plugin integration

## Contributing

Issues and pull requests are welcome! Please open an issue to discuss major changes.

## Have questions, suggestions, issues or need to get help?

Please [open an issue](https://github.com/BabkenInants/BlockPuzzle/issues).
