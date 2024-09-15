## Main Path
The main path is generated first using specific rooms that lead forward.
The final room of each location is on the main path
## Side Paths
main path rooms can spawn with extra exits that can lead to spawning side paths.
Rooms may never collide. If a room can not be placed, a wall will simply be filled in.

Along these side paths, extra loot and challenges can be found.
## Backtracking
Rooms will rest over time
Defeating these rooms also increases difficulty
Allows advanced players to take on a greater challenge while collecting extra loot
























# Old

See the [[Implementation/World Generation|Implementation]]
## Goals
- Non linearity (forking paths)
- No backtracking
- Diegetic

There should not be teleportation between rooms. Players should be actually traversing physically to new rooms.


## Rooms
What is slow about rooms?
1. Placing the border
	1. Lining up the border
	2. Making Elements connect
2. Placing decorations