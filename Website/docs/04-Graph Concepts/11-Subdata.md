# Subdata

Because the data we generate and operate is often a little more complex, we need to know how to read and write data from its subparts. In this section, we will address the topic of _subentities_, _subattributes_ and _subexpressions_.

## Subentities

A subentity is simply an entity that is contained inside an entity. 

Vertices, edges and faces are examples of entities that are contained inside another entity, the Mesh. In practice, we tend not to manipulate these subentities directly, but only the Meshes as a whole. Still, these entities are there and carry important information.

Likewise, when we group entities into Group entities, they become subentities of the created group entity. Still, in such occasions, there is still the need to access data from such subentities.

## Subattributes

A subattribute is an attribute of a subentity.



## Subexpressions

In many cases, we need to be able to access and operate based on subattributes in the nodes. Imagine the following use cases:

 * We would like to extract from a mesh the faces that have a certain subattributes (such as the largest area or is lying on the boundary)
 * We would like to create mesh faces out of path edges, whereas each edge has a different associated width.
 * We would like to divide a set of entities according to common attributes.

For such occasions, subexpressions come in handy.

Subexpression textboxes are slightly differently colored

Parameter expressions are 

In many cases, 