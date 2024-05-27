
# Voxelized-3D-Map
This is the project for a 3D voxelized map on Unity 2022.3.9f1. It uses an atlas of 3D parts to build a voxel mesh and has support for placing other gameobjects on the grid.

## Terminology
Before explaining this project characteristics, there are some terms that need to be defined.

**VoxelMap**: The **VoxelMap** is what stores the information of the map. The map is subdivided into **VoxelChunks** to be able to divide the processes into smaller manageable parts.

**Voxel**: Each **VoxelChunk** is defined as a list of **Voxels** on a 3-dimensional grid. Each **Voxel** can be free (air) or occupied by either a **VoxelMaterial**, or a **Block**.

**VoxelMaterial**: It is an atlas of 3D parts that can be used to build a mesh, by placing parts between **Voxels**.

**Block**: It is a 3D part that is snapped to the grid of **Voxels**. It can be rotated only on the vertical axis, and it can be used for various purposes, from just static decoration parts, to more complex parts with functionality.

## Playing with the tool
On the project, inside VoxelMap/Scenes, there is a sample scene already set for editing a map.
After hitting play, a block is placed in front of us on the center of the map. We can move the camera by holding right click and using the WASD keys.

### Build mode GUI
On the left of the screen we have a GUI that lets us modify the **VoxelMap**



### Saving/Loading GUI
On the right of the screen there is a GUI that lets us save our map, by entering a name, or load the map with the given name. During saving, if there is a map with the given name already saved, it will be overwritten. Also, when loading, the current map is lost.

## Setting up a VoxelMap
