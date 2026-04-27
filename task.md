# Task Breakdown

This checklist translates the proposal into concrete implementation tasks in dependency order.

## Phase 1: Project Setup and Core Scaffolding
- [X] Create base scenes: MainMenu, GameRoot, HUD, PauseMenu, SettingsMenu, ChoiceScreen.
- [X] Add core scripts: GameFlowManager, RunState, SaveManager, SignalBus.
- [X] Register required autoloads in project settings.
- [X] Define enums/state constants for downtime, raid, paused, victory, defeat.
- [X] Verify scene transitions among menu, game, pause, and settings.

## Phase 2: Play Area and Camera
- [X] Build a top-down playfield with a grid-aligned placement layer.
- [X] Add environmental obstacles (rocks/trees) that block placement and movement.
- [X] Implement camera drag panning and keyboard movement (WASD/arrow keys).
- [X] Add camera bounds and sensible zoom defaults.
- [X] Ensure map generation/decor variation can differ per run.

## Phase 3: Resource Economy and Buildings
- [X] Create resource model for food and wood.
- [X] Implement periodic production and upkeep ticks.
- [X] Add Farm entity and data definition (cost, output, upkeep).
- [X] Add Lumber Mill entity and data definition (cost, output, passive efficiency effect).
- [X] Implement build validation (space blocked, cost affordability, placement rules).
- [ ] Add selling/removal behavior for buildings.

## Phase 4: Unit System and Placement/Repositioning
- [ ] Create UnitBase and at least one allied unit prototype.
- [ ] Implement unit stats (health, speed, attack, range, cost/upkeep).
- [ ] Build sidebar storage UI for units/buildings.
- [ ] Implement drag-and-drop to deploy and recall from sidebar.
- [ ] Prevent illegal drops and overlap with obstacles/buildings.

## Phase 5: Dual-State Game Loop
- [ ] Implement downtime timer and manual start-raid trigger.
- [ ] Lock/unlock allowed player actions by state.
- [ ] Implement raid start/end transitions with clear UI feedback.
- [ ] Persist map/entity state correctly across transitions.
- [ ] Add pause state that freezes gameplay timers and simulation.

## Phase 6: Enemy, Combat, and AI Steering
- [ ] Implement enemy spawning from off-map entry points.
- [ ] Create at least one enemy unit with basic combat behavior.
- [ ] Add target selection and attack resolution for allies and enemies.
- [ ] Implement pursue steering toward hostile targets.
- [ ] Implement obstacle avoidance for units and collision separation.
- [ ] Validate units do not clip through buildings or each other.

## Phase 7: Loot/Choice System and Progression
- [ ] Design data model for buildings/units/tech rewards.
- [ ] Implement weighted random selection for choice cards.
- [ ] Build choice screen UI with configurable number of options.
- [ ] Apply chosen reward and update run state.
- [ ] Add persistent post-run currency reward (win or loss).
- [ ] Implement meta-progression spend/unlock flow in main menu.

## Phase 8: UI and Settings
- [ ] Finalize main menu options: new game, continue, upgrades, settings, quit.
- [ ] Implement keybind remapping support.
- [ ] Add volume controls and visual options in settings.
- [ ] Connect settings persistence to save file.
- [ ] Build UI indicators for downtime timer and resource values.

## Phase 9: Audio Integration
- [ ] Source or compose royalty-free tracks for menu, downtime, and raid.
- [ ] Source SFX for attacks, impacts, placement, destruction, and reward rarity cues.
- [ ] Hook audio events into combat, construction, and UI actions.
- [ ] Route all audio through master/music/SFX buses.
- [ ] Verify volume settings apply globally at runtime.

## Phase 10: Win/Loss, Testing, and Balance
- [ ] Implement loss condition: all buildings destroyed.
- [ ] Implement victory condition: survive enough raids or defeat final encounter.
- [ ] Tune enemy scaling and economy pacing to 15-30 minute runs.
- [ ] Run full-playthrough test passes and log balance issues.
- [ ] Fix blocker bugs and polish feedback clarity.

## Stretch Goal Tasks (Only After Core Is Stable)
- [ ] Add difficulty tiers with unique maps, enemies, and audiovisual themes.
- [ ] Implement activated abilities with cooldown and resource cost UI.
- [ ] Add weather states that modify economy/combat values.
- [ ] Implement adaptive enemy composition that counters player defenses.

## Acceptance Checklist
- [ ] New run starts and reaches raid loop without errors.
- [ ] Downtime and raid transitions are reliable.
- [ ] Player can place/reposition units and buildings legally.
- [ ] Resources, upkeep, and build costs function correctly.
- [ ] Combat resolves with readable outcomes.
- [ ] Choice rewards appear and apply correctly.
- [ ] Save/load preserves relevant run and progression data.
- [ ] Audio and settings function as expected.
