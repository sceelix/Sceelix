# Node Overview

Getting the hang of a new tool is never easy. Getting acquainted with one that requires understanding of a large list of operations is even tougher. Sceelix seems no different, yet it provides several structural features that make the learning process much easier than it seems.

Before going deeper into each node’s description, be aware or some important considerations, since they should facilitate their understanding and use:

## 1. Nodes represent high-level operations

In many node-based systems, you’ll get nodes to do pretty much any kind of operations, including those for adding numbers, setting up vectors or building strings of characters. In other words, it provides a low-level control for all your tasks, which is great!… And also not so great. This means you’ll probably need some solid programming knowledge to set them together, you’ll sometimes need a lot of nodes to do the simplest operations, you’ll need to learn a lot of nodes to get things done.

While Sceelix graphs do certainly allow this kind of control in theory, Sceelix actually works on a higher level, meaning that nodes are meant to do bigger and more complex operations for you, like turning a bunch of lines into meshes or setting several objects on a surface, all in one node. In other words, you need to learn fewer nodes and you’ll probably have simpler and more straightforward graphs.

You can still build mathematical expressions on the parameters and attributes, but that will probably constitute a small part of the work, anyway.

Still, if you are a programming expert and would like to do some lower-level tasks, you can always do so very easily using the C# API.

## 2. Nodes have categories, tags and descriptions

![Node Tags](NodeTags.png)

When you open the node list on a graph, you can see that they are grouped in a tree-like structure. Nodes have categories, which, at this point, have been set as the main entity type that the nodes are handling. When you select an entry, you’ll see that each features a small description of their function, together with a list a tags that should help identifying their exact purpose.

## 3. Node names have a particular structure

In order to maintain consistency, a big effort was to name nodes after their in a homogeneous way. Nodes typically start by the name of the main entity type that they are handling, followed by the verb or noun that indicates what they are doing on this entity. “Actor Group” means that a grouping operation is performed on actor entities, “Mesh Create” creates meshes and “Surface Modify” modifies has operations that somehow change surfaces.

Here is a description of some common operations that are applicable to several entities:

* **Create:** Creates new entities from scratch, based on a pattern, a known geometrical model, randomness, or even by influence/transformation of another entity. “Mesh Create”, for instance, can create cubes, spheres, cones, but also create polygons from converting paths, too.
* **Load:** Loads entities from some kind of source format, like a file, folder, database, etc.  For example, “Surface Load” loads image files, “Mesh Load” loads fbx, dae or obj files, for instance.
* **Modify:** Performs some kind of modification on the entity and the entity alone (no extra inputs). One entities comes in, another comes out at a time. Does not create or divide entities into more. Just a simple internal modification of the entity. For instance, “Surface Modify” including smoothing, resolution change or normal recalculation.
* **Subselect:** Allows a part of an entity to be extracted/separated from the original entity, according to a certain condition. For example, “Mesh Subselect” allows faces to be extracted based on certain properties, so that it can be handled differently.
* **Divide:** Similar to the subselect, but does actually divides the original entity into many entities, according to certain criteria. The “Mesh Divide” can divide meshes into groups of faces that share a certain size or vertex count, for instance.
* **Decompose:** Not to be confused with previous one. Decomposing an object allows us to access its subparts, but without actually destroying their connections, like the division would do. This allows, for example in the “Path Decompose”, one to access the vertices of a path without breaking their edge links. This is very useful if you want to simply set attributes to the individual parts of an entity (set different colors to its vertices, number the edges, etc.)
* **Merge:** The opposite of the “Divide”, as it allows entities of the same type to be joined into one. For example, the “Path Merge” joins different sets of edges, yet does not force vertices to be shared.
* **Unify:** Often used together with the “Merge”, it forces, for example, vertices that lie on the same position to be joined into one, so as to enforce connectivity inside meshes faces or path edges.
* **Save:** The opposite of the Load”, as the name suggests. Saves entities to a file, folder, database, etc. “Mesh Save”, for instance, allows meshes to be saved to an obj or fbx file format.


There are some nodes that do not follow this naming, and that’s because they operate on all and any kinds of entities (“Copy”, “Log”, “Property”…), so including the “Entity” before was redundant.

## 4. Nodes group several operations, for easy access and understanding

One thing to note is that nodes typically aggregate several operations, meaning that a node such as the “Mesh Create” can actually create many different kinds of meshes, which you can choose from a list. Each choice presents new suboptions and, in turn, each suboption can encompass new ones. There are several reasons for this approach. One the one hand, this reduces the amount of different nodes, which makes the whole range of possibilities much less overwhelming. On the other hand, it allows for easy switch between choices (for example, for replacing a cube with a sphere) while making use of parameters that are common.

![Node Operations](NodeOperations.png)

Since in theory this could be used to form one big node that would aggregate all the operations, that there should be a balance, or else nodes could become very complex. This is also why some nodes with growing numbers of operations have been “unfolded” into new independent nodes.