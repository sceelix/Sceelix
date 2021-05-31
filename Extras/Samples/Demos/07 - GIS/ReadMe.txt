==============================================
GIS  Sample
Author: Sceelix
==============================================

This samples demonstrates how to load and operate on GIS Data. In practice, it all comes down to the data loading nodes (GIS Load or OSM Load), which will read the existing features into Sceelix ones, such as points, paths and meshes.

OpenStreetMap.slxg
Loads data from a file exported from http://openstreetmap.org, as well as a heightmap previously extracted from Bing maps and transforms this data into a plausible city. Uses the little attribute data associated to each loaded feature to aid in the process (for instance, for turning the points marked as trees into actual tree models).


World.slxg
Loads a Shapefile which contains the boundaries of the world's countries, as well as some associate data. Performs an extrusion based on the relative population amount of each country.
