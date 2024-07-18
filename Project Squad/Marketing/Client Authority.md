I found the easiest way to make co-op multiplayer games like Lethal Company. It all comes down to "Client Authority".

  

#gamedev 1/4🧵
![[Pasted image 20240630224700.png]]

When you are developing a multiplayer game, you have to consider the other players. A lot of games do this by using "Server Authority". Basically, most things that the player does have to be verified by the server, which takes time because of lag. If you've ever opened a chest in Minecraft with a poor internet connection, you'll notice this.\


2/4🧵
![[Pasted image 20240630225858.png]]

"Client Authority" is just ignoring the server, bypassing this lag. So why don't all games do this? Many games use Server Authority to account for hacking. Since player actions go through the server, we can stop certain actions taken by malicious users. But, not all games need to care about this. Games like Lethal Company, and Rogue Squad (my game) get to assume that you are playing with people you trust.


3/4🧵

So next time you are making a game, consider using Client Authority more often. And before you post "erm actually", there are other methods like Deterministic Netcode with Client Side Prediction that I'm ignoring because it's a very complicated topic.

And don't forget to follow if this was informative! 👉 https://x.com/CloudOuter


4/4🥳
![[200w 1.gif]]