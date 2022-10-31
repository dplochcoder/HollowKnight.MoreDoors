# MoreDoors

A rando 4 connection mod which adds more locked doors to Hallownest with unique keys.

Thirty unique doors, each with their own unique key, are added to the game by this mod.
The keys also have vanilla locations, which can optionally be added to the locations pool.
See SPOILER.md for the full list of doors and vanilla key locations if you wish.

## Global Settings

MoreDoors has both randomization settings, and global settings. The latter are specific to your instance of Hollow Knight
and have no impact on other players you are racing with or against. The global settings are accessible through `Options > Mods > MoreDoors Options`.

### Enable in Vanilla

Setting this to 'yes' (default: 'no') will add all MoreDoors doors to classic, non-rando save files.
The keys are placed such that a normal save can be completed without doing any skips, though you may have to adapt your usual routes!

### Show Key Shinies

Setting this to 'yes' (default: 'yes') will put a shiny hint-marker on enemies that contain a MoreDoors key check, whether it's vanilla or randomized.
The hint markers are meant as a guide for finding the checks and/or more easily remembering exactly where they are; because they are displaced at a fixed offset from the enemy's position, and not integrated into the animation, they are not the most pleasant to look at.
Disabling the shiny hint has no effect on gameplay, vanilla or rando, so it is not a rando setting and it does not affect rando hashes.

## Rando Settings

* Doors Level: The number of doors to add
  * Some Doors: About a third of the doors are added
  * More Doors: About two-thirds of the doors are added
  * All Doors: All of the doors are added
* Randomize Door Transitions
  * All enabled door transitions are randomized with each other
  * Respects 'coupled' and 'matching' settings from transition rando settings, but requires 'None' settings otherwise
    * If you enabled map area, full area, or room-rando, this setting will do nothing
  * Exiting through a door you don't have a key for automatically opens the door, but only on that side. You still need the key to use the other side.
* Add Key Locations: Adds the vanilla key locations for the added doors to the locations pool
  * Matching Doors: Only adds the key locations for the doors that were actually added
  * All Doors: Add all key locations, even for doors that weren't added

### Customize Doors

You can customize the set of doors you want to allow on the 'Customize Doors' page.
Simply deselect a door to exclude it from randomization, even under 'All Doors'. This also removes its key location.

MoreDoors integrates with RandoSettingsManager, so it needs no manual configuration when saving or loading profiles, and will automatically disable itself when loading a profile from a game which didn't have MoreDoors installed/enabled.

### Interop

All key items are placed in the 'Keys' pool for randomization, and for All Major Items tracking.
If 'Duplicate Unique Keys' is set, every key gets a duplicate
