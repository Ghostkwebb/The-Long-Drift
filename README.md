<div align="center">
  <img src="https://github.com/user-attachments/assets/cbd12245-ecac-4b02-b4b9-5f1d67396927" alt="The Long Drift Logo" width="200"/>
  <h1>The Long Drift</h1>
  <p>
    <em>Outsmart the cosmos. A 2D physics puzzler about slingshotting through star systems on a single tank of fuel.</em>
  </p>
  <p>
    <a href="https://unity.com/"><img src="https://img.shields.io/badge/Made%20with-Unity-57b9d3.svg?style=for-the-badge&logo=unity" alt="Made with Unity"></a>
    <a href="https://ghostkwebb.itch.io/the-long-drift"><img src="https://img.shields.io/badge/Play%20on-itch.io-fa5c5c.svg?style=for-the-badge&logo=itchdotio" alt="Play on Itch.io"></a>
  </p>
</div>

---

### Gameplay Screenshots

<table>
  <tr>
    <td align="center">A daring slingshot maneuver planned with the trajectory line.</td>
    <td align="center">A stable, hands-free orbit achieved around the destination.</td>
  </tr>
  <tr>
    <td><img src="https://github.com/user-attachments/assets/79058fdb-be38-472e-accf-a230251955e9" alt="Gameplay screenshot showing a slingshot maneuver"></td>
    <td><img src="https://github.com/user-attachments/assets/19ffd28e-f111-4bff-8b53-a3a160ffd13f" alt="Gameplay screenshot showing a stable orbit"></td>
  </tr>
</table>

---

## About The Game

**The Long Drift** is a game about outsmarting the cosmos. Armed with a low-fuel ship and a powerful physics simulation, you must chart a course through complex gravitational fields to reach your destination.

This is not a game of trial and error. It is a game of planning, precision, and skill. The core philosophy is that gravity is not an obstacle, but a powerful tool to be harnessed. By using the predictive trajectory line, you can visualize your path seconds into the future, turning impossible shots into masterful slingshot maneuvers.

This repository contains the complete Unity project for the polished prototype.

## Core Features

*   **Predictive Trajectory Line:** A custom, manually-implemented physics simulation (Verlet integration) that accurately predicts the ship's future path through multiple gravitational fields.
*   **N-Body Gravity System:** A robust `GravityBody` script that calculates and applies gravitational forces from all `GravitySource` objects in the scene, allowing for complex orbital mechanics.
*   **Dynamic Camera System:** A custom camera rig that intelligently switches between a tight player-follow mode at high speeds and a dynamic group-framing mode to keep both the player and the nearest planet in view.
*   **Stateful Player Controller:** Manages multiple ship states, including a kinematic autopilot for starting orbits, full manual control for slingshotting, and a prograde-locked "autopilot" state as a reward for stable trajectories.
*   **Persistent Game Management:** A singleton `GameManager` and `SoundManager` that persist across scene loads to handle checkpoint data, scene transitions with fades, and global audio settings.
*   **Advanced UI:** Features a dynamic, scrolling minimap that correctly maps world space to UI space and a full-featured pause/settings menu.

## Controls

| Control             | Key(s)                  |
| ------------------- | ----------------------- |
| **Rotate Ship**     | `A` / `D`               |
| **Thrust**          | `W`                     |
| **Super Boost**     | `Space` or `Left Shift` |
| **Camera Zoom**     | `Mouse Scroll Wheel`    |
| **Pause / Menu**    | `Escape`                |

## Built With

*   **Engine:** [Unity](https://unity.com/) (Version 6000.0.50f1)
*   **Language:** [C#](https://docs.microsoft.com/en-us/dotnet/csharp/)
*   **Libraries:** Unity's New Input System

## Getting Started

To run the project locally:

1.  Clone this repository:
    ```sh
    git clone https://github.com/Ghostkwebb/The-Long-Drift
    ```
2.  Open the project folder in the Unity Hub (ensure you have Unity version 6000.0.50f1 or later installed).
3.  Open the `MainMenu` scene from the `Assets/Scenes` folder.
4.  Press Play.
