# 2D Game Tukuruyo

Unity 2D platform action game prototype.

## Environment

- Unity `6000.3.24f1`
- Universal Render Pipeline 2D
- Input System

## Prototype controls

- Move: `A/D` or left/right arrow keys
- Jump: `Space`, `W`, or the up arrow key
- Shoot: `J`, `X`, or right `Ctrl`

The current prototype generates temporary player and platform objects at runtime.

## Prefabs

Open the Unity project and run `Prototype > Generate Prototype Prefabs` to create the
editable prototype assets under `Assets/Prefabs` and `Assets/Materials`. This tool does
not modify `Assets/Scenes/SampleScene.unity`.

The generated `Player`, `Platform`, and `Projectile` prefabs can be dragged directly
from the Project window into an authored scene. `Player` contains its controller,
camera-follow component, and `ProjectileSpawnPoint`; it uses the `Projectile` prefab
when shooting.
