# Blade-of-the-Shogun
3d stealth-action game made in Unity. General Idea: Metal Gear Solid with swords.  Low health for player.  Adept sword-fighting ability crucial to win.

Current Build Features:
  Player:
    * Camera has full rotation capabilities, and player moves with respect to how camera is rotated using vector math.
  Enemies:
    * Patrol to random points on map (that do not intersect with walls).  Wait a random amount of seconds, and patrol again to a new location.
    * Vision of player is calculated based on many factors, such as; If anything solid intersects with line of sight, distance from player, how centered the player is in enemie's vision, how long player stays in enemies, vision. Shadows soon to be implemented.
    * When mulitple enemies have spotted player and are engaging, up to 4 will spread out around the player evenly and engage in combat. The rest will stand back until one has been killed.
  Other:
    * System of what I call "pathing objects" have been created and will be laid out around map in doorways/turns in the level (check scr_pObj).  They raycast to each other and build an array that stores all other 'pObjs' in the level in an array element, and determines the quickest distance to all other 'pObjs'.  This will be implemented soon to allow the enemies to travel back to their spawn location if they should lose track of the player while pursuing, using the quickest route to get back to their spawn.
    
