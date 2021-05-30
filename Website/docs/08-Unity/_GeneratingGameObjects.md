# Generating Game Objects

In previous sections, you have seen how Sceelix is able to send its generated data to a format that Unity can understand. This section will go deeper on how the data from Sceelix translates into Unity objects and what possibilities there are.

## Default Entities

Sceelix operates on several different types of entities, such as meshes, surfaces and billboards. For most of these entities, there is a Unity counterpart, and one of the tasks of this plugin is to perform this conversion. The currently supported entity types are as follows:

* **Mesh**: Converted into a Game Object with a Mesh Filter and Mesh Renderer.
* **Mesh Instance**: Converted into a Game Object with a Mesh Filter and Mesh Renderer, using references, not copies, of the same mesh and materials.
* **Surface**: Converted into a Game Object with a Terrain and a Terrain Collider.
* **Billboard**: Converted into a Game Object with a BillboardComponent (defined inside Sceelix).

## Game Objects and prefabs

In addition to the default entity types, Sceelix supports the **Unity Entity**, which