# Encapsulation

Encapsulation refers to turning full graphs into single nodes that can be used inside other graphs. We call these resulting new nodes **Component nodes**.

[Image of graph next one image of encapsulated node]

Advantages of encapsulation include:
* **Reuse**: If you are repeatedly using the same combination of nodes together, you can turn them into a single node, where you only expose the parameters that change.
* **Organization**: As your project grows more complex, the graphs get bigger with more nodes, more connections, more parameters and more attributes. You can group nodes into subgraphs, according to their purpose, making them easier to understand and manage.
* **Simplification**: When you encapsulate a set a nodes, you build a more powerful building block that hides the complexity of a process. This allows you or other users of your new node to operate on a higher level, without having to worry about how it works internally.

Encapsulation can work recursively, i.e. components nodes can be encapsulated together with other nodes into new component nodes. When using component nodes, we refer to the graphs behind the components as "subgraphs". From the point of view of the subgraph, the containing graph is called the "supergraph".

## Encapsulation Process

Any graph is, at any moment, a potential component nodes, so encapsulation occurs automatically. Suppose you create two new graphs, "A" and "B". Open the "A" file and drag and drop the file "B" from the Project Explorer into A's graph canvas. A new hexagonal node, called "B" will appear. It's that simple! 

[Image illustrating the process]

## Parameters

The parameters of a component node are those that are defined as global graph parameters of the subgraph (see [Parameters](Parameters)). So if you add, edit or remove parameters in the subgraph, the parameters of the component node will be updated automatically.

[Image illustrating the process]

## Ports

In order to create ports in your component nodes, you need to define **gates** somewhere in your subgraph's node ports. Doing so is easy: inside the subgraph, click a node to select it and view its options in the inspector. You will see a selection box with the label "State". If you change its value to "Gate", the field below ("Gate name") will become enabled, allowing you to set a custom name, if you wish. If you save the graph and go to your supergraph, you'll see that a new port has appeared in the component node, named after the value in "Gate Name").

[Image illustrating the process]

By marking a port as a gate, we are basically mapping the ports of the component node to the subgraph's node ports. As a result, the data passed to the ports of the component nodes is sent directly to the ports of the nodes in the subgraph.

The gate name is important to make a distinction between ports. If 2 ports of the same nature (input/output), have the same entity type (see [Entities](Entities)) and have the same name, the ports will be "joined", i.e. appearing as one in the component node.

A quick way to turn a port into a gate is to Alt + left mouse click it, twice.

## Working as a Subgraph

When a graph is executed as a subgraph of another graph, its working can be set to be slightly different. It is not unusual that, when we decide to use a graph as a subgraph of another, that we prefer to replace the source nodes with input coming from the supergraph. Suppose the following case:

[Image of simple graph with source node that will be replaced]

In the case above, the source node is the starting point, which provides the mesh that we are transforming. But if we are getting the mesh from a supergraph, this data is no longer necessary - in fact, it shouldn't be even be generated. To disable this execution, simply click the source node and make sure the "Disable in subgraph" option is enabled (which is, by default).