
# Properties and Attributes

Entities area complex objects, featuring various properties. The color of a vertex, the scale or a surface or the position of a mesh are all examples of properties. While some of them are explicitly stored in the entity, others are the result of calculations - such as area, volume or number of vertices in a mesh - and hence are not stored unless requested. 

Different entities have different sets of properties, depending on their nature. Paths have length (the sum of all lengths of all edges), but no volume nor area, unlike meshes do.
 
A property does not need to be a number, it can be of other types as well, like those of [parameters](Parameters#types).

You can consult the list of available properties of a generated entity in the Data Explorer.

![Properties Data Explorer](images/PropertiesDataExplorer.png)


## Attributes

Users can assign custom properties to entities, which are called **attributes**. Attributes that have been assigned to an entity can be viewed on the Data Explorer as well.


It is possible to assign and read custom data to entities in the form of **attributes**. 

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