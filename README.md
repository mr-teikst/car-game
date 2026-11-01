# Night Drive

Open the project with Unity 6000.6.6f1, then open `Assets/Scenes/NightDrive.unity` and press Play. This scene is also selected in Build Settings. Use A/D or arrow keys to change lanes, Space to jump and barrel roll, and Escape to pause.

## Scene and Inspector

The scene contains the player and its visual children, camera, lighting, Game Manager, Road and Object Pools, Audio and Effects, UI, and EventSystem. Select the Game Manager to see its connected player, road, UI, and effects components and change run settings. Select the player to adjust lane and jump settings and its camera reference. Select Road and Object Pools to inspect the connected prefabs, pool sizes, and spacing. The UI's panels, labels, and button actions are scene objects; the prefabs and materials are under `Assets/NightDriveGenerated`.

Road segments and interactable instances are prewarmed from the assigned prefabs when the game starts, then reused. `GameManager` is a singleton; `Coin`, `FuelCan`, `MovingObstacle`, and `StationaryObstacle` inherit from `Interactable`. Audio tones are generated at runtime unless clips are assigned to Audio and Effects.

`Night Drive > Create or rebuild game scene` regenerates the scene and generated assets. It overwrites the generated scene and its prefabs, so use it only when you intend to discard edits to those generated assets.

For submission, create and test a standalone build, include the complete build directory, a repository link, and a report with screenshots. The scene uses a procedural starry skybox; final visual polish still needs work. The scene uses OBJ models from the included Kenney car, road, and commercial building packs: https://kenney.nl/assets/car-kit and https://kenney.nl/assets/city-kit-roads.

Roadside buildings use `RoadsideBuildings` on the pooled road prefab. Each segment creates three buildings per side once; they move with the segment. Change the model references, scale, and distance from the road in the prefab Inspector. Source: https://kenney.nl/assets/city-kit-commercial
