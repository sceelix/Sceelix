# Extending the plugin

The Unity Plugin is extensible in many ways. It is free and open source, so you can check out the code and adjust it to your needs directly. However, the plugin has been designed to support new additions and changes without the need to alter the original files or change the folder of the plugin in any way. You can place your files in any location of the Unity project, and they will be automatically read, overriding the default scripts if so configured.

Doing so facilitates later upgrades to the plugin: you can simply replace the main plugin folder without having to reproduce your custom changes later.

# Examples of possible additions and changes

Complete override of the current pipeline
Instantiation of custom components



## Plugin workflow

The plugin's main task is to interpret data that is sent from the Sceelix Editor. This data comes in json format, so that is quick and easy to process on the Unity side. The [Json.NET](http://www.newtonsoft.com/json) library is used help in this process, but a manual interpretation/deserialization, both to improve performance and customization possibilities.

In order to reduce the amount of data that needs to be sent, there are certain considerations. 

Is self-contained. All necessary data comes in one go, containing all data, some of which may have internal references, such as mesh instances that refer to other meshes.

Often, reference to ids are sent 

For instance, when sending references to the same mesh, 
