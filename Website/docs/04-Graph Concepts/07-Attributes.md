# Attributes

It is possible to assign and read custom data to entities in the form of **attributes**. Attributes that have been assigned to an entity can be viewed on the Data Explorer.

[Image of Data explorer with several attributes set]

You can create new attributes using the 'Attribute' Node. Here, you define what value to assign and what will be the name of the attribute.

[Image of Attribute node, showing also the inspector on the node and its view on the data explorer]

Many other nodes can also create attributes. The 'Copy', which creates copies of entities, can save an index of the copy to an attribute, if you specify a name.

[Image of the copy node, the inspector parameters and the data explorer]

Likewise, the random node assigns its random generated values to attributes.

[Image of the random node]

You can use attributes within expressions of parameters so as to achieve different transformations for each entity. In the example below, we are making use of the mesh split node's ability to assign the index of the splitted part to the mesh. We then use this value to extrude each mesh with a different value, creating a staircase.

[Image of splitted rectangle forming a staircase]  

Depending on its type, each entity will have particular **properties** which are tied to their nature (area, material, position). To access them, you must assign them to attributes first. In the case below, we calculate the perimeter of the input meshes and depending if they are bigger than a certain value, we will choose upon what material to use.

[Image that reflects the case above]   