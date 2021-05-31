==============================================
Detailed Building Sample
Author: Sceelix
==============================================

This samples demonstrates how to create a more geometrically complex building with indented windows, sills and ornaments, as well as a staircase in front. The use of several new 3D modeling operations are hereby presented.

An important concept in Sceelix's mesh manipulation is the 'Scope', an oriented bounding box that defines individual axis and reference systems. This allows modeling operations to be performed based on the 'direction' of certain mesh parts, such as facades or windows.

An important and recurrent node in these samples is the 'Split' node, which slices and divides meshes. This allows smaller and individual parts to be handled more easily. Using 'Merge' nodes, these parts can be joined again later.

Types of Entities manipulated in this sample:
- Mesh Entity
- 3D Entity (Supertype)
- Entity  (Supertype)