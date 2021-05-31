==============================================
Cards Sample
Author: Sceelix
==============================================

This sample that features falling cards focuses more on game specific behaviors. After procedurally loading the 52 different front card textures, they are shuffled, organized into a spiral and assigned collision and physics properties, so that fall on the ground. 

Deck Trick (Sceelix).slxg
In this graph, Sceelix-specific Game Entities are created. These can be interpreted by Sceelix and are therefore shown on the Renderer.

Deck Trick (Unity).slxg
In this graph, Unity-specific Game Entities are created. These are only meant for the Unity Game Engine, meaning that they won't show up on the 3D Renderer. If you have a Unity instance open with the plugin installed (and the connection established), the result will appear there. The process might take some time, as new Unity assets (textures, materials and meshes) have to be created.

Types of Entities manipulated in this sample:
- Mesh Entity
- Game Entity
- Game Object