# Golemancy
Running a cult is hard, so lets have a golem do it for us. An attempt at automating Cultist Simulator.

Let's get this show on the road.

## The Plan (tm)

I don't want to have some weird black box machine that looks at the screen and says to move the mouse 3 pixels to the right because it felt like that was the best course of action. So to start off, the source of information is going to be the memory of the process itself. Since memory is inherently hierarchical, this will require some form of memory sampler that can slowly build up a kind of hierarchical memory map. It would be possible to slowly build up this information by taking entire memory snapshots, but those are going to be huge, so instead the focus is going to be on the AI "requesting" memory regions.
