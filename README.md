<div align="center">

# DISMANTLE

**A brutal 3D first-person horror shooter built in Unity**

*Survive. Destroy. Dismantle.*

[![Unity](https://img.shields.io/badge/Unity-2022.3.62f1-black?logo=unity)](https://unity.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Mac%20%7C%20Linux-blue)]()
[![License](https://img.shields.io/badge/License-MIT-red)]()
[![Status](https://img.shields.io/badge/Status-In%20Development-orange)]()

</div>

---

## 🩸 ScreenShots and About the Game
<img width="960" height="600" alt="1" src="https://github.com/user-attachments/assets/ef4c4226-a14c-46ca-a26f-84f88122dba9" />
<img width="960" height="600" alt="2" src="https://github.com/user-attachments/assets/d2196830-5c55-4cdb-bcaf-b97c2bf79ac9" />
<img width="960" height="600" alt="4" src="https://github.com/user-attachments/assets/b24d7d79-b41b-4ffc-9e71-b3db829952df" />
<img width="960" height="600" alt="5" src="https://github.com/user-attachments/assets/5096d0ab-41cf-426f-a117-99424339890b" />

**DISMANTLE** is a fast-paced first-person horror shooter set in dark, industrial corridors filled with enemies that want you dead. Armed with multiple weapons, limited ammo, and your survival instincts — fight through waves of ground troops, flying threats, turrets, and a terrifying multi-phase boss.

The game blends **classic retro FPS gameplay** with **modern horror atmosphere** — dynamic post-processing, blood flash effects, camera shake on every hit, and an adaptive boss music system that kicks in the moment danger arrives.

---

## 🎮 Gameplay Features

### 🔫 Combat System
- **Multiple weapon types** — each with unique fire rates, spread, and ammo pools
- **Realistic ammo management** — reload tracking, magazine limits, must-reload system
- **Projectile-based shooting** with spread variance and fire delay
- **Hitscan + Projectile** hybrid support

### 👾 Enemy AI

| Enemy Type | Behaviour |
|---|---|
| **Ground Enemy** | Patrols, chases, attacks in melee or ranged |
| **Flying Enemy** | Aerial pursuit, swoops on the player |
| **Turret Enemy** | Static, high-accuracy ranged fire |
| **Explosive Enemy** | Charges and detonates near the player |
| **Boss Enemy** | Multi-phase, aerial slam attacks, melee + ranged, phase transitions with unique music |

- Enemy **spawner system** — configurable wave spawning
- **Ragdoll physics** on death
- **Enemy item drops** — ammo, health, pickups

### 🗺️ Levels
- Multiple handcrafted maps with industrial/horror atmosphere
- Key & door system — some doors require keys to unlock
- Checkpoint system
- Destructible environment elements

### 🧰 Player Systems
- **Full character controller** — move, jump, look
- **Health + Lives system** with respawn
- **Score system** with animated count-up display
- **Ammo tracker** — per-gun ammo management with UI
- **Dynamic crosshair** — expands on shoot/move, turns red on enemy aim

---

## ✨ Visual & Audio Polish

### 🎥 Camera & Screen Effects
- **Camera Shake** — scales with hit intensity
- **Damage Flash** — full-screen blood-red vignette on player hit
- **Dynamic Post-Processing** (URP):
  - Vignette darkens as health drops
  - Chromatic aberration spikes on damage
  - Lens distortion pulses on boss impact

### 🎵 Adaptive Music
- **MusicManager** with smooth crossfade between normal and boss tracks
- Boss music triggers automatically on boss activation
- Crossfade duration configurable per track

### 🖥️ Main Menu
- **Cinematic video background** (two-clip system: load clip + looping ambient)
- **Animated title logo** — slide-in entrance, floating breathe, glow pulse, horror flicker, rotation sway
- **Menu button animations** — hover scale, colour shift, click feedback
- Level select, settings (mouse sensitivity), controls reference

---

## 🛠️ Built With

| Technology | Purpose |
|---|---|
| **Unity 2022.3 LTS** | Game engine |
| **Universal Render Pipeline (URP)** | Rendering & post-processing |
| **TextMeshPro** | All in-game text & UI |
| **Unity Input System** | Player input handling |
| **Unity Video Player** | Menu video backgrounds |
| **C#** | All game scripting |

---

## 📁 Project Structure

```
Assets/
├── Art/
│   ├── Fonts/          # Custom game fonts (KnightWarrior, Blade)
│   ├── UI/             # UI sprites, logo, button assets
│   └── ...             # Textures, models, effects
├── _Scenes/
│   ├── MainMenu.unity  # Title screen & menu
│   ├── Map_v1.unity    # Level 1
│   └── Map_v2.unity    # Level 2
├── Scripts/
│   ├── Player/         # PlayerController, CameraShake
│   ├── Enemies/        # Enemy AI, Boss, Spawners, Attackers
│   ├── Shooter/        # Gun, Shooter, AmmoTracker, GunSmoke
│   ├── Health&Damage/  # Health, Damage, Destructable
│   ├── UI/             # UIManager, TitleAnimator, DamageFlash, DynamicCrosshair
│   ├── Utility/        # GameManager, MusicManager, DynamicPostProcessing
│   ├── Pickups/        # Health, Ammo, Life pickups
│   ├── Score/          # Score tracking
│   └── Keys&Doors/     # Key/door interaction system
└── Prefabs/
    ├── Enemies/
    ├── Guns/
    ├── Pickups/
    ├── Effects/
    └── Utility/        # LevelManagement prefab
```

---

## 🚀 Getting Started

### Prerequisites
- [Unity 2022.3 LTS](https://unity.com/releases/lts) or later
- Universal Render Pipeline (installed via Package Manager)
- TextMeshPro (installed via Package Manager)

### Installation

```bash
# Clone the repository
git clone https://github.com/YOUR_USERNAME/dismantle.git
```

1. Open **Unity Hub**
2. Click **Add** → select the cloned project folder
3. Open with **Unity 2022.3** or later
4. Let Unity import all assets *(first import may take a few minutes)*
5. Open `Assets/_Scenes/MainMenu.unity`
6. Press ▶ **Play**

> **Note:** If post-processing effects don't appear, go to your URP Renderer asset and enable **Post Processing** in the renderer settings.

---

## ⌨️ Controls

| Key | Action |
|---|---|
| `WASD` | Move |
| `Mouse` | Look around |
| `Left Click` | Shoot |
| `Space` | Jump |
| `R` | Reload |
| `Scroll Wheel` | Switch weapon |
| `Esc` | Pause / Menu |

---

## 🗺️ Roadmap

- [ ] Save system (high scores, settings persistence)
- [ ] Additional enemy types
- [ ] More levels / maps
- [ ] Controller support
- [ ] Difficulty settings
- [ ] Intro cinematic / loading screen

---

## 🤝 Contributing

This is a solo student project but contributions and feedback are welcome!

1. Fork the repository
2. Create your feature branch: `git checkout -b feature/amazing-feature`
3. Commit your changes: `git commit -m 'Add amazing feature'`
4. Push to the branch: `git push origin feature/amazing-feature`
5. Open a Pull Request

---

## 📜 License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

> Assets, fonts, and third-party resources may have their own licenses. See individual asset folders for attribution.

---

## 🙏 Acknowledgements

- Font: *KnightWarrior* & *Blade 2*
- Level art assets from the Unity Asset Store
- Built with Unity 2022.3 LTS

---

<div align="center">

Made with 🩸 and Unity

**[⬆ Back to top](#dismantle)**

</div>
