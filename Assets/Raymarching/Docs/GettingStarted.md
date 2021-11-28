# Getting Started
## URP Renderer Setup
Change the URP renderer to use the Deferred rendering path - Raymarching is not supported on the Forward renderer. Enable post-processing here too if you are going to be using it within your game.

The [[RaymarchRenderFeature]] needs to be added to the renderer (this is at the bottom, click 'Add Renderer Feature'). The render feature includes any settings that are used during the render path.