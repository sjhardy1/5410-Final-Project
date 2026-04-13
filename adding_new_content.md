## New Content Guide

This guide documents the correct way to add new Units, Buildings, and Technologies into the game.

### Scene file

1. Create a scene with Node2D as the base
2. Attach to it the script at Scripts/Placement/GridPlaceable
3. Add tilemaps, sprites, or animations to determine how it will look

### Resource file

1. Go into Resources/Definitions and select the correct folder.
2. Create a new generic resource file.
3. Attach one of the definitions in Scripts/Resources
4. Fill out all of the blanks in the Godot inspector
5. Attach your previously made scene to the resource

### Add to game database

1. Go to Resources/Definitions/GameDatabase.tres
2. Add your resource file to the corresponding section
