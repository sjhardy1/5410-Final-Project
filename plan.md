# Development Plan

## Goal
Build a playable 15-30 minute citybuilder roguelite in Godot/C# that delivers a complete core loop (economy, downtime, raid, reward selection, and run conclusion), with optional stretch systems if time allows.

## Constraints
- Team size: 2 developers
- Time budget: approximately 60-70 combined hours over 5 weeks
- Priority: finish core loop first, then polish, then stretch features

## Milestones

### Milestone 1: Foundation and Project Skeleton
Target outcome: project boots into a navigable menu and gameplay placeholder scene.

- Create baseline folder and scene architecture.
- Set up autoload singletons for run state, save data, and game flow state.
- Implement main menu navigation shell (new game, continue, settings, quit).
- Add camera controls to a simple test map.

### Milestone 2: Economy and Placement Core
Target outcome: player can place/move basic buildings and units during downtime.

- Implement grid-like placement validation and obstacle blocking.
- Add farm and lumber mill prototypes with production/upkeep values.
- Add sidebar inventory/storage behavior for inactive assets.
- Implement drag-and-drop repositioning.

### Milestone 3: Raid State and Combat
Target outcome: downtime transitions to raid; enemies can attack and be fought.

- Implement dual-state flow with timer and manual start.
- Spawn enemies from map boundaries.
- Add basic ally/enemy movement and combat behaviors.
- Implement pursue plus obstacle avoidance for mobile units.
- Add win/loss checks (city destroyed or raid objective complete).

### Milestone 4: Rewards, Progression, and Save
Target outcome: post-raid reward choices and cross-run progression are functional.

- Build weighted choice screen for buildings/units/technologies.
- Implement item definitions and unlock rules.
- Save and load run state and meta currency.
- Add meta-progression spend screen in main menu.

### Milestone 5: Audio, Polish, and Balance
Target outcome: core game is presentation-ready and stable.

- Integrate music for downtime/raid/menu.
- Add key SFX tied to gameplay events.
- Implement pause flow that halts simulation and timers.
- Tune pacing and scaling for a 15-30 minute run.
- Execute bug-fix and balance pass.

### Stretch Milestone: Advanced Systems
Only after core completion.

- Higher difficulty environments and enemy sets
- Activated abilities with cooldown/cost UI
- Weather debuffs and feedback systems
- Smart enemy counter-composition behavior

## File Structure

### Current Structure
```text
final-project/
  icon.svg
  icon.svg.import
  project.godot
  proposal.txt
  README.md
  Scenes/
```

### Target Structure
```text
final-project/
  project.godot
  README.md
  proposal.txt
  plan.md
  task.md
  Scenes/
	MainMenu.tscn
	GameRoot.tscn
	PauseMenu.tscn
	ChoiceScreen.tscn
	SettingsMenu.tscn
	HUD.tscn
	World/
	  Playfield.tscn
	  TileObstacle.tscn
	Buildings/
	  Farm.tscn
	  LumberMill.tscn
	  BuildingBase.tscn
	Units/
	  UnitBase.tscn
	  AllyMelee.tscn
	  AllyRanged.tscn
	  EnemyRaider.tscn
  Scripts/
	Core/
	  GameFlowManager.cs
	  RunState.cs
	  SaveManager.cs
	  SignalBus.cs
	Systems/
	  ResourceSystem.cs
	  PlacementSystem.cs
	  CombatSystem.cs
	  AISteeringSystem.cs
	  LootSystem.cs
	  SpawnSystem.cs
	  AudioSystem.cs
	Entities/
	  BuildingBase.cs
	  UnitBase.cs
	  EnemyBase.cs
	UI/
	  MainMenuController.cs
	  HUDController.cs
	  ChoiceScreenController.cs
	  PauseMenuController.cs
	  SettingsController.cs
  Data/
	Buildings/
	  Farm.tres
	  LumberMill.tres
	Units/
	  AllyMelee.tres
	  AllyRanged.tres
	  EnemyRaider.tres
	Technologies/
	  Tech_Example.tres
	Loot/
	  ChoicePools.tres
  Assets/
	Audio/
	  Music/
	  SFX/
	Sprites/
	  Buildings/
	  Units/
	  UI/
	Fonts/
  Tests/
	Unit/
	Integration/
```

## Division of Work (Suggested)
- Developer A focus:
  - Gameplay systems (resources, placement, combat)
  - AI steering and spawn logic
  - Save/run state integrity
- Developer B focus:
  - UI scenes (menus, HUD, choice screen, pause/settings)
  - Content data authoring and balancing
  - Audio integration and presentation polish

Both:
- Joint balancing sessions
- Weekly integration and bug triage
- Feature demos at each milestone

## Risk Management
- Biggest technical risk: steering around dense obstacles during raid chaos.
- Mitigation: prototype pathing/avoidance early (Milestone 3 starts with isolated movement tests).
- Scope risk: too many content variants too soon.
- Mitigation: ship with a minimal set of units/buildings/technologies first, then expand breadth.

## Definition of Done
- Core loop complete and stable with no blocker bugs.
- Game start to end playable without editor intervention.
- Save/load and meta-progression verified.
- Audio, pause, and settings integrated.
- Stretch goals attempted only after all core acceptance criteria pass.
