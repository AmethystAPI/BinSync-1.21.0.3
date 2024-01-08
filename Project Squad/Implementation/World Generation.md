See the [[Design/World Generation|Design]]

> [!warning]
> This implementation is out of date! See new implementation in [[Day 18-11]]

An initial spawn room is placed. This spawn room will have no entrance and will always have an exit in the up direction.

Rooms have doors at exits. These doors can be destroyed by attacking them. Once a door is destroyed, all other doors lock and the next room is generated.

Once the next player enters, all other players will have a short amount of time before they start suffering from loneliness, an effect that damages the player over time.

After all players enter the next room, combat starts. and the previous room is despawned.

Rooms may have a "hallway" that extends out of the entrance. This hallway will contain a door which shuts once combat has started in the room to prevent backtracking.