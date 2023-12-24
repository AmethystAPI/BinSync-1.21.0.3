## Responsiveness
Since entities are networked objects, we need to keep in mind how to keep player interaction responsive. I have a few strategies in mind.
1. Attacks are triggered through Rpcs but the result of those attacks are simulated on the client
2. Taking damage causes the entity to transfer authority to the damaging client allowing for responsive knockback

Certain attacks being run on the client could have some interesting consequences. If an attack has the entity moving like the slime jump, we'll probably relinquish position syncing during the attack and the attack Rpc will send a start and end position for the slime to transition between.