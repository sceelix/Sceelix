==============================================
Nature Sample
Author: Sceelix
==============================================

This samples demonstrates how to create, all from scratch, natural environments, from terrains to trees and grass, as well as combine them. This does not mean that we see this strategy as the most efficient for games (for instance, grass is better drawn within the terrain using shaders), but serve for the purpose of this example.

Tree.slxg
This graph is particularly interesting as it features graph cycles. This allows for easy recursive creation of new branches from old ones. The leaves texture is loaded from a file, but is procedurally transformed to customize the hue color component, which gives us the power to create color variations of the leaves.

Model Instancing.slxg
In this graph, the Tree graph is used as a component and called with 3 different parameter variations. These 3 models are then transformed into 'Mesh Instances', then cloned and later spread throughout the terrain. This means that the models themselves are not expressively copied, but only references of them. This greatly improves the generation and rendering performance. Although with Sceelix we can easily achieve a scene with every tree being unique and different from another, in practice this would take too many memory and graphic resources. So tricks like this ones are often employed.

Billboards.slxg
Often, a alternative for rendering trees and other objects is to display them as Billboards, which are simple textures rendered always facing the user. Because they look less realistic, they are often used for objects viewed from a greater distance.

Types of Entities manipulated in this sample:
- Mesh Entity
- Billboard Entity
- Mesh Instance Entity
- 3D Entity (Supertype)
- Entity  (Supertype)
- Image