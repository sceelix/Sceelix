==============================================
City Sample
Author: Sceelix
==============================================

City.slxg
This is a simple, but very complete sample to illustrate the basic concepts of Sceelix's graph language. It shows how to create an urban landscape composed of roads, sidewalks and tall buildings. The graph starts by creating a path entity for the street layout and converts that to a set of meshes that are progressively transformed to create sidewalks and buildings of random heights and with a random selection of textures.

Unity Export.slxg
This graph uses the previous as a component node and focuses only on creating Game Objects for use in Unity. Once executed, nothing will appear on the Rendering Window, as it is not capable of understanding Unity Objects. Instead, if you have a Unity instance open with the plugin installed (and the connection established), the result will appear there. The process might take some time, as new Unity assets (textures, materials and meshes) have to be created.


Types of Entities manipulated in this sample:
- Path Entity
- Mesh Entity
- 3D Entity (Supertype)
- Game Object
- Entity  (Supertype)
