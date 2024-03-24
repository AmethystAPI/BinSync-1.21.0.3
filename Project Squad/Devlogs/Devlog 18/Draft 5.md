## Intro
*Holds up "your game"* When the success on your game hinges on your prototype, *Tosses aside 
"your game", Flicks on switch and monitor Turns on* how do you know what to prototype, what to not, when to finish?

We're going *Looks to side and Cuts to where I'm looking* to learn three techniques *Three monitors with numbers Turn on* that the developers of Outer Wilds, Spelunky, and Spore use to make sure their prototype is successful. *the symbol of each game Replaces number as their are said*

## Technique #1: Outer Wilds
First up we have the technique Alex Beachum used when developing the amazing game, Outer Wilds. The technique can be boiled down to prototyping the riskiest element first. This means don't just prototype by building out a simple version of the game, which is a very common way of thinking, but focus specifically on the parts of your game that are most likely going to cause issues in development. Some examples could be prototyping the character movement in a platformer, the multiplayer support in an online shooter. For Outer Wilds, this element was the narrative.

In fact, the Outer Wilds team didn't even start with a digital prototype, they started with paper.

"\[The paper prototype] completely abstracted away the space travel and focused on what we wanted to test: the underlying narrative structure. " - Alex Beachum

This technique saves you time in two ways. First, it helps prevent issues about core elements of the game later in development that could be costly in development time. For example, if you discover that your platformer character's jump doesn't feel responsive enough so you add coyote time, All of your level design will be messed up which is pretty costly. Or if you are working on the online shooter and you realize the network architecture you were using has too much latency, you'll have to rewrite a lot of code. But also secondly, this technique save you time by clearly displaying a core element of your game. This makes it easier to test and explore without other secondary mechanics slowing you down, like the spaceflight in outer wilds.

"We needed a way for players to test the game's entire narrative structure in a fraction of the time it would take during a normal playthrough." - Alex Beachum

This technique undoubtable helped Alex develop a game that won best game, game design, and narrative in 2020. 

Now, before we go on to technique two, I want to tell you that to prove that these techniques actually work I'm going to be going over how implementing these techniques into my own video game supercharged my development speed and creativity at the end of the video.

Onto the second technique that developer, Chris Hecker and Chaim Gingold used to prototype Spore. And this technique is unique in that I've never really heard it before but is actually golden advice: Prototypes should answer a specific yes or no question. For example, when starting a prototype you shouldn't ask your self, "What makes a platformer fun?", instead ask "Does coyote time make the character controller feel responsive?" The difference between these is kind of subtle but a specific yes or no question guides your prototype to a clear success or failure.

"Good prototypes make some kind of claim. Once you have the prototype written you should be able to tell if it actually worked." - Chaim Gingold

Using this technique will help you save time by clearly defining when your prototype as finished and if your prototype worked. As soon as you have developed enough to answer yes or no to the question you started out with, you've finished a prototype that's going to give you valuable insight. And you can combine this with the first technique by asking specific questions about your core game elements, which Chris Hecker has found to be extremely important.

"The ambitious projects I had undertaken in the past failed because I made the mistake of not proving out the core ideas in prototypes." - Chris Hecker

These last two techniques are really useful if you already have a good idea of what you want to prototype and specific things that you want to test, but what do you do if you don't know where to start? The third technique third and final technique we are going to talk about helps solve this issue. When Derek Yu prototypes, he makes lots of little prototypes to utilize his creativity.

"When I approach prototyping, I like to treat it like \[doodle]." - Derek Yu

Just making little things without putting to much analysis into the ideas is going help you come up with specific things to use the previous two techniques on. For example, you might start by making a simple character that runs and jump, and then a character that bounces of surfaces, and then a character that uses a grappling hook. Now, you can ask yourself a specific question to test like "Does using a grappling hook and not touching the ground lead to interesting decisions?" So this technique ultimately helps you come up with ideas, and apply the last to techniques I talked about.

But to talk about the techniques is one thing. I want to show you that these actually work. I've been developing a multiplayer, cooperative rougelike, codenamed Project Squad. When I started prototyping the game I started with technique one: identify the riskiest element of your game. From experience I decided that this would probably be the multiplayer. I then continued with technique two: ask testable questions. I asked, "Does Network Rollback lead to responsive actions" and the answer was almost yes, except for some issues with physics. So, I pivoted and asked "Does Entity Interpolation lead to responsive actions", and the answer was yes. So, now, I knew I was on the right track. And during this process I had been implement technique three, doodle, by doodling art, and thinking about different enemies and potential environments.

So, all in all, this has led me to today with a promising game, and three key take aways for you:
1. Identify the risky element
2. Ask testable questions
3. Doodle

I'll be linking some of the resources I used in the description. If you want to learn more about multiplayer check out this video and as always thank you to my subscribers, I'll see you in the next one.