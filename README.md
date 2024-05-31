


# Voxelized-3D-Map
This is the project for a 3D voxelized map on Unity 2022.3.9f1. It uses an atlas of 3D parts to build a voxel mesh and has support for placing other gameobjects on the grid.

![Imagen](https://github.com/AaronUDC/Voxelized-3D-Map/assets/103149928/2ed03ff9-6e36-4cdb-ac7c-88894e5c38c1)


## Terminology
Before explaining this project characteristics, there are some terms that need to be defined.

**VoxelMap**: The **VoxelMap** is what stores the information of the map. The map is subdivided into **VoxelChunks** to be able to divide the processes into smaller manageable parts.

**Voxel**: Each **VoxelChunk** is defined as a list of **Voxels** on a 3-dimensional grid. Each **Voxel** can be free (air) or occupied by either a **VoxelMaterial**, or a **Block**.

**VoxelMaterial**: It is an atlas of 3D parts that can be used to build a mesh, by placing parts between **Voxels**.

**Block**: It is a 3D part that is snapped to the grid of **Voxels**. It can be rotated only on the vertical axis, and it can be used for various purposes, from just static decoration parts, to more complex parts with functionality.

## Playing with the tool
On the project, inside *VoxelMap/Scenes*, there is a sample scene already set for editing a map.
After hitting play, the map will be initialize. We can control the camera by holding right click and using the WASD keys or moving the mouse.

### Build mode GUI

| ![GUI](https://github.com/AaronUDC/Voxelized-3D-Map/assets/103149928/215f595a-0920-49aa-b9e1-6cbb0a0a77ea) |On the left of the screen we have a GUI that lets us modify the **VoxelMap**. We can change the build mode between destroy and place using de topmost buttons. When place mode is selected, a list of available parts will be shown below, divided into **Blocks** and **VoxelMaterials**.  |
|--|--|

### Saving/Loading GUI
|![GUISave](https://github.com/AaronUDC/Voxelized-3D-Map/assets/103149928/dc3fd534-253e-4eaf-84d4-e53f0d611c24) |On the right of the screen there is a GUI that lets us save our map, by entering a name, or load the map with the given name. During saving, if there is a map with the given name already saved, it will be overwritten. Also, when loading, the current map is lost. |
|--|--|
## Setting up a VoxelMap

### 1. Configuring the VoxelMap prefab
 After making a new scene, we just need to place the VoxelMap prefab available on VoxelMap/Prefabs/Map. Inside the script there are some parameters we can set up before anything.
 
![VoxelMap](https://github.com/AaronUDC/Voxelized-3D-Map/assets/103149928/b111c797-953d-4b4c-bc51-5d6a895ff686)

**Voxel size**: The size of each **Voxel**. This will scale the **VoxelMaterial** parts to be able to fit them between **Voxels**.
 
 **Chunk resolution**: How many **Voxels** a chunk will have per axis. Each chunk will have (chunk resolution)^3  **Voxels**.
 
 **Map resolution**: How many **VoxelChunks** the map will have per axis. The map will grow in every direction, centered at the origin of the block.
 
 **PartDB**: A scriptable object where we will store the list of the **VoxelMaterials** and **Blocks** our map will be able to use.
###  2. Setting up the parts database
An example database is already stored in the VoxelMap folder which already contains a **VoxelMaterial** and a **Block**.
This scriptable object contains two dictionaries that stores an unique identifier for each **VoxelMaterial** and **Block**. 
New parts can be added on the inspector of the database, or we can create our **PartDB** by adding an asset under *ScriptableObjects/VoxelMap*.
  
### 3. Creating new parts
#### Blocks
To create a new block, we just need to add the **Block** script (Under *VoxelMap/Script/Blocks*) to the thing that we want to be snapped into the grid. The origin of the GameObject containing the script will be snapped to the **Voxel** it is placed into.

Try to make the object around the same size as the voxel size of the map for best results.

#### Voxel Materials
The **VoxelMaterial**, just like the Parts database, is a ScriptableObject, and can be created the same way.
To set up a **VoxelMaterial**, we need to prepare a total of 34 meshes in our 3D software of choice and each mesh mus be named accordingly. An example is available at (*VoxelMap/Models*), remember to allow read/write on the import settings of the model in order to the mesh merging to work. 

To fill the data on the scriptable object more easily, there is a button to automatically do it by providing a "tileset" of parts, like the one from the example. A warning will pop up for any part not named correctly on the set.

### 4. Map editing and save and load
To be able to edit the map, or save and load it, we need to add a Placer or a SaveLoad prefab on the scene. After placing them in the scene, add the **VoxelMap** on the inspector of each object. And each GUI should pop up when playing. Of course, you can also implement your own system.

## Credits
Serializable Dictionary licensed under MIT License [Copyright (c) 2017 Mathieu Le Ber](https://github.com/azixMcAze/Unity-SerializableDictionary)
