# Final Project: Citybuilder Roguelite

## Team
- Spencer Hardy
- Jaden Thomas

## Overview
This game is a top-down 2D citybuilder roguelite built with Godot 4.5 and C#. The player manages a growing civilization by producing resources, placing buildings, and commanding units between enemy raids.

Each run is designed to last about 15-30 minutes. During downtime, the player reorganizes defenses and invests in growth. During raids, enemies attack in real time and attempt to overwhelm the city. Between waves, the player chooses randomized upgrades (buildings, units, and technologies), creating run-defining synergies and tradeoffs.

The long-term progression loop includes persistent unlocks that carry across runs, whether the player wins or loses.

## Core Gameplay Loop
1. Start a run with basic economy and defenses.
2. Build resource production (food, wood) and expand infrastructure.
3. Enter downtime: reposition units/buildings, buy upgrades, research tech.
4. Survive raid state: enemies spawn, path in, and attack your city.
5. Choose one of several random rewards on a choice screen.
6. Repeat with scaling difficulty until victory or defeat.

## Core Features (Must-Have)
- Resource economy (food, wood, upkeep, spending)
- Dual-state loop (downtime and raid)
- Unit and building placement/repositioning on a grid-like map
- Basic unit AI for movement and combat
- Randomized upgrade choice system
- Win/loss conditions
- Camera controls (drag, WASD, arrow keys)
- Music, SFX, and basic settings

## Stretch Features (Nice-to-Have)
- Higher difficulties with distinct environments and enemies
- Activated combat abilities (cooldowns/costs)
- Weather effects with gameplay modifiers
- Smarter enemy composition/counterplay
- Expanded settings and polish

## Technical Direction
- Engine: Godot 4.5 (.NET)
- Language: C#
- Architecture goals:
	- Data-driven pipelines for units, buildings, technologies, enemies, and abilities
	- Scene-based modular design for menus, gameplay, and overlays
	- Save/load support for run state and meta-progression
	- Decoupled UI and gameplay systems using Godot signals/events

## Current Repository Layout
This repository currently includes:

- `project.godot`
- `proposal.txt`
- `README.md`
- `Scenes/`

Planned structure is documented in `plan.md`.

## Documentation
- `proposal.txt`: original project proposal and scope
- `plan.md`: project implementation plan and target file structure
- `task.md`: concrete task-by-task implementation checklist

## Running the Project
1. Install Godot 4.5 with .NET support.
2. Open this folder in Godot.
3. Build C# project files when prompted.
4. Run the main scene once gameplay scenes are configured.

## Success Criteria
- A full run is playable end-to-end in 15-30 minutes.
- Core loop is stable: build, survive raids, choose upgrades, repeat.
- Player receives persistent currency/unlocks after each run.
- Clear win/loss outcomes and restart flow.
