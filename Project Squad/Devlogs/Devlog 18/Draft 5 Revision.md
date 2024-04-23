## Intro
*Holds up "your game"* When the success on your game hinges on your prototype, *Tosses aside 
"your game", Flicks on switch and monitor Turns on* how do you know what to prototype, what to not, when to finish?

We're going *Looks to side and Cuts to where I'm looking* to learn three techniques *Three monitors with numbers Turn on* that the developers of Outer Wilds, Spelunky, and Spore use to make sure their prototype is successful. *the symbol of each game Replaces number as their are said*

## Technique #1: Outer Wilds
First up, let's look at the technique that Alex Beachum used when prototyping the amazing Outer Wilds. This technique can be boiled down to: prototype the riskiest element first. It is a common idea to prototype by just building out a simple version of the game, but it is far more effective when your prototype focuses specifically on the parts of your game that are most likely to cause issues. Some examples could be prototyping the character movement in a platformer or the multiplayer support in an online shooter. For Outer Wilds, this element was the narrative.   

In fact, the Outer Wilds team didn't even start with a digital prototype, they started with paper.

"\[The paper prototype] completely abstracted away the space travel and focused on what we wanted to test: the underlying narrative structure. " - Alex Beachum

This technique saves time in two ways. First, it helps you discover issues with the core elements of your game which could have been costly in development time. For example, if you discover that your platformer character's jump doesn't feel quite responsive enough, you might add coyote time, but then all of your level design will be messed up. Or, if you are working on an online shooter, you could realize that the network architecture you were using has too much latency, which means you'll have to rewrite a lot of code. But also secondly, this technique save you time by isolating a core element of your game. This makes it easier to test and explore without other secondary mechanics slowing you down, like the spaceflight in outer wilds.

"We needed a way for players to test the game's entire narrative structure in a fraction of the time it would take during a normal playthrough." - Alex Beachum

This technique undoubtably helped Alex develop a game that won best game, game design, and narrative in 2020. 

Now, before we go on to technique two, I want to mention that to prove to you that these techniques actually work, I'm going to be going over how implementing these techniques into my own video game supercharged my development speed and creativity at the end of the video.

But before that, we have to talk about the second technique that developer, Chris Hecker and Chaim Gingold used to prototype Spore. And this technique is unique in that I've never really heard it before but is actually golden advice: Prototypes should answer a specific, testable, yes or no question. For example, when starting a prototype you shouldn't ask your self, "What makes a platformer fun?", instead ask "Does coyote time make the character controller feel responsive?" The difference between these is kind of subtle but a testable question will guide your prototype, making it easier to tell when it is finished and whether or not it was a clear success or failure.

"Good prototypes make some kind of claim. Once you have the prototype written you should be able to tell if it actually worked." - Chaim Gingold

Using this technique will help you save time by clearly defining an endpoint a result for your prototype. As soon as you have developed enough to answer yes or no to the question you started out with, you've finished a prototype and it's going to give you valuable insight. And you can combine this with the first technique by asking specific questions about your core game elements, which Chris Hecker has found to be extremely important.

"The ambitious projects I had undertaken in the past failed because I made the mistake of not proving out the core ideas in prototypes." - Chris Hecker

These last two techniques are really useful if you already have a good idea of what you want to prototype and specific things that you want to test, but what do you do if you don't know where to start? The third and final technique we are going to talk about from Derek Yu, the developer of Spelunky, helps solve this issue. When Derek prototypes, he makes a lot of little prototypes to explore ideas.

"When I approach prototyping, I like to treat it like \[doodle]." - Derek Yu

Skipping analysis and just making little things helps you come up with the specific ideas to test and expand on. For example, you might start by making a simple character that runs and jumps, and then a character that bounces of surfaces, and then a character that uses a grappling hook. Now, you have ideas to expand on using the last two techniques like asking the testable question "Does using a grappling hook and not touching the ground lead to interesting decisions?"

This technique will ultimately help you come up with ideas that you can apply the last to techniques I talked about on.

But to talk about the techniques is one thing. I want to show you that these actually work. I've been developing a multiplayer, teamwork based rougelike, codenamed Project Squad. And when I started prototyping, I started with technique one: identify the riskiest element of your game. From experience I knew that this would probably be the multiplayer code. So, I moved on to technique two: ask testable questions. I question I wanted to test was, "Does Network Rollback lead to responsive actions" and the answer was almost yes, except for some big issues with physics. So, realizing that it wasn't going to work, I came up with a new idea to test: "Does Entity Interpolation lead to responsive actions", and the answer was yes. So, now, I knew I was on the right track. And throughout this process I had been implement technique three, doodle, by doing small little art pieces or thinking about enemy designs.

So, all in all, this has led me to today with a promising game, and three key take aways for you:
1. Identify the risky element
2. Ask testable questions
3. Doodle

I'll be linking some of the resources I used in the description. If you want to learn more about multiplayer check out this video and as always thank you to my subscribers, I'll see you in the next one.