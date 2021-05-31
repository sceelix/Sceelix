==============================================
Parking Lot Sample
Author: Sceelix
==============================================

This samples demonstrates how to use Sceelix to manipulate and organize models that have been built with external tools. For now, Sceelix only supports .OBJ format, but more formats will be introduced in a near future. 

Given that we have few car models and several parking lot locations to place them, our goal is to perform copies of the same model, to as to fill a certain percentage of the parking lot (controlled by a graph parameter). Our copies are instances of the same model, so as to make the generation process faster and more memory efficient. The 'Wrap' node is very important, creating as many copies of the list of passed cars as need so as to fill the desired parking spots. The 'Insert' node is responsible to actually take the model instance and adjust its translation, rotation and scale (the scope) to the one defined by the scope of the parking spot.

Types of Entities manipulated in this sample:
- Path
- Mesh Entity
- Mesh Entity Instance
- 3D Entity (Supertype)
- Entity  (Supertype)