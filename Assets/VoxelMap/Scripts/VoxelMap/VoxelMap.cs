using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class VoxelMap : MonoBehaviour,ISaveableMap
{   
    public float voxelSize = 1.0f;

    public int chunkResolution = 8;

    public int mapResolution = 2;

    public VoxelChunk voxelGridPrefab;
	
	[SerializeField]
    private VoxelChunk[]chunks;

    private float chunkSize, mapsize, halfSize;

    public int startingBlockID = 0;
    public Vector3 startingPosition;

	public PartDB partDB;
	private (int, VoxelChunk.NeighbourID)[] neighbourDeltas; //Deltas to get the index of neighbours of the chunk.
    
    // Start is called before the first frame update
    void Awake()
    {
        Initialize();
    }
	

    // Update is called once per frame
    void Start()
    {   
    }

	private void Initialize(){
		chunkSize =  voxelSize * chunkResolution;
        mapsize = chunkSize * mapResolution;
        halfSize = mapsize * 0.5f;

		/*neighbourDeltas = new(int, VoxelChunk.NeighbourID)[]{
			(-1,VoxelChunk.NeighbourID.X), 
			(-mapResolution,VoxelChunk.NeighbourID.Y),
			(-mapResolution * mapResolution,VoxelChunk.NeighbourID.Z),
			(-mapResolution - 1,VoxelChunk.NeighbourID.XY), 
			(-mapResolution * mapResolution - mapResolution,VoxelChunk.NeighbourID.YZ), 
			(-mapResolution * mapResolution - 1,VoxelChunk.NeighbourID.XZ),
			(-mapResolution * mapResolution - mapResolution - 1,VoxelChunk.NeighbourID.XYZ)}; */
        chunks = new VoxelChunk[mapResolution * mapResolution * mapResolution];
        
        for(int i = 0, z = 0; z < mapResolution; z++){
            for(int y = 0; y < mapResolution; y++){
                for(int x = 0; x < mapResolution; x++, i++){
                    CreateChunk(i,x,y,z);
                }  
            }  
        }
	}

    private void CreateChunk(int i, int x, int y, int z){
        VoxelChunk chunk = Instantiate(voxelGridPrefab);
        chunk.Initialize(chunkResolution,voxelSize, new Vector3Int(
			Mathf.FloorToInt(x * chunkSize - halfSize),
			Mathf.FloorToInt( y * chunkSize - halfSize), 
			Mathf.FloorToInt(z * chunkSize - halfSize)
		), partDB);

        chunk.transform.parent = transform;
        chunk.transform.localPosition = new Vector3(x * chunkSize - halfSize, y * chunkSize - halfSize, z * chunkSize - halfSize);
		chunk.transform.name = string.Format("Chunk_{0}_{1}_{2}",x,y,z);
        chunks[i] = chunk;
		BecomeNeighbour(chunk,i,x,y,z);
    }

	private void BecomeNeighbour(VoxelChunk chunk, int i, int x, int y, int z){

		//Fills this chunk on the neighbours list from the chunks created before.		
		if(x != 0 && i > 0) 
			chunks[i-1].AddNeighbour(VoxelChunk.NeighbourID.X,chunk);
		if(y != 0 && i - mapResolution >= 0) 
			chunks[ i - mapResolution].AddNeighbour(VoxelChunk.NeighbourID.Y,chunk);
		if(z != 0 && i - mapResolution*mapResolution >= 0) 
			chunks[i - mapResolution*mapResolution].AddNeighbour(VoxelChunk.NeighbourID.Z,chunk);
		if(x != 0 && y != 0 && i - mapResolution - 1 >= 0) 
			chunks[i - mapResolution - 1].AddNeighbour(VoxelChunk.NeighbourID.XY,chunk);
		if(y != 0 && z != 0 && i - mapResolution - mapResolution*mapResolution >= 0) 
			chunks[i - mapResolution - mapResolution*mapResolution].AddNeighbour(VoxelChunk.NeighbourID.YZ,chunk);
		if(x != 0 && z != 0 && i - mapResolution*mapResolution - 1 >=0) 
			chunks[i - mapResolution*mapResolution - 1].AddNeighbour(VoxelChunk.NeighbourID.XZ,chunk);
		if(x != 0 && y != 0 && z != 0 && i -mapResolution * mapResolution - mapResolution - 1>=0) 
			chunks[i -mapResolution * mapResolution - mapResolution - 1].AddNeighbour(VoxelChunk.NeighbourID.XYZ,chunk);

	}

    public Vector3 SnapToGrid(Vector3 point){
        Vector3 halfVoxelSize = Vector3.one * voxelSize/2;
        
        Vector3 center = (point - halfVoxelSize)/voxelSize;

        center = new Vector3(Mathf.Round(center.x), Mathf.Round(center.y), Mathf.Round(center.z)) * voxelSize;

        //Debug.Log("Point X Y Z: " + point + "Center X Y Z: " + center);
        return center + halfVoxelSize;
    }



	public Vector3Int MapCoordFromPoint(Vector3 point){

		int centerX = (int) ((point.x + halfSize)/ voxelSize);
        int centerY = (int) ((point.y + halfSize)/ voxelSize);
        int centerZ = (int) ((point.z + halfSize)/ voxelSize);

        //Debug.Log("Center X Y Z: " + new Vector3Int(centerX,centerY,centerZ));

        return new Vector3Int(centerX,centerY,centerZ);
	}

	public Vector3Int ChunkCoordFromMapCoord(Vector3Int mapCoord){
		// Sums half the chunk resolution when map resolution is an ood number. 
		// Because the world 0,0,0 on the chunk (0,0,0)is in its center instead of the edge.
		return new Vector3Int(
			Mathf.FloorToInt((float)mapCoord.x  / chunkResolution),
			Mathf.FloorToInt((float)mapCoord.y  / chunkResolution),
			Mathf.FloorToInt((float)mapCoord.z  / chunkResolution)
		);
		//Debug.Log("ChunkX: " + mapX + "ChunkY: " + mapY + "ChunkZ: " + mapZ);
		//(Vector3Int.one *(chunkResolution/2) * (mapResolution % 2) )
	}

	public bool IsCoordOutOfBounds(Vector3Int mapCoord){
		//Check if is out of bounds
		 // Debug.Log("Out of bounds!");
		int maxCoord= chunkResolution * mapResolution;
        return mapCoord.x <= 0 || mapCoord.x >= maxCoord -1
            || mapCoord.y <= 0 || mapCoord.y >= maxCoord -1
            || mapCoord.z <= 0 || mapCoord.z >= maxCoord -1;

		//Blocking the placement of things on the edges of the grid allows for clean borders on the voxel geometry.
	}

	public Vector3Int LocalChunkCoordfromMapCoord(Vector3Int mapCoord){
		return new Vector3Int(
            mapCoord.x % chunkResolution,
            mapCoord.y % chunkResolution,
            mapCoord.z % chunkResolution
        );
	}

	public int ChunkIndexfromChunkCoord(Vector3Int chunkCoord){
		return chunkCoord.x + (chunkCoord.y * mapResolution) + (chunkCoord.z * mapResolution * mapResolution);
	}

	private bool GetChunkIndexAndLocalChunkCoord(Vector3 point, out int chunkIndex, out Vector3Int localChunkCoord){
		Vector3Int mapCoord = MapCoordFromPoint(point);
		Vector3Int chunkCoord = ChunkCoordFromMapCoord(mapCoord);
		
		Debug.Log("Map XYZ: " + mapCoord + "\nChunk XYZ: " + chunkCoord);

		//Check if is out of bounds
        if (IsCoordOutOfBounds(mapCoord)){

            Debug.Log("Out of bounds!");
			chunkIndex = -1;
			localChunkCoord = Vector3Int.zero;
            return false;
        }

		chunkIndex = ChunkIndexfromChunkCoord(chunkCoord);
        localChunkCoord = LocalChunkCoordfromMapCoord(mapCoord);
        Debug.Log("ChunkI: " + chunkIndex + "\nLocalChunkCoord XYZ: " + localChunkCoord);
		return true;
	}

	public void PlaceBlock(Vector3 point, int rotation, int blockID){
		
		if(GetChunkIndexAndLocalChunkCoord(point, out int chunkIndex,out Vector3Int localChunkCoord)){

        	chunks[chunkIndex].PlaceBlock(localChunkCoord, rotation, blockID);
		}
	}
	
    public void PlaceVoxel(Vector3 point, int voxelId){

		if(GetChunkIndexAndLocalChunkCoord(point, out int chunkIndex,out Vector3Int localChunkCoord)){

        	chunks[chunkIndex].PlaceVoxel(localChunkCoord,voxelId);
			if (localChunkCoord.x == 0 || localChunkCoord.x == chunkResolution-1 ||
				localChunkCoord.y == 0 || localChunkCoord.y == chunkResolution-1 ||
				localChunkCoord.z == 0 || localChunkCoord.z == chunkResolution-1 ) 
				//Only update neighbours when trying to place or remove on the edges of the chunk
				UpdateChunkNeighbours(chunkIndex, voxelId);
		}
    }

	public void BreakAt(Vector3 point){
		
		if(GetChunkIndexAndLocalChunkCoord(point, out int chunkIndex,out Vector3Int localChunkCoord)){

        	int id = chunks[chunkIndex].BreakAt(localChunkCoord);
			if (id >= 0 && (
				localChunkCoord.x == 0 || localChunkCoord.x == chunkResolution-1 ||
				localChunkCoord.y == 0 || localChunkCoord.y == chunkResolution-1 ||
				localChunkCoord.z == 0 || localChunkCoord.z == chunkResolution-1 )){ 
				//Only need to update neighbours when trying to place or remove on the edges of the chunk
				UpdateChunkNeighbours(chunkIndex, id);
			}
		}
	}

	public void UpdateChunkNeighbours(int i, int voxelId){

		int x = i % mapResolution;
		int y = (i / mapResolution) % mapResolution;
		int z = i/mapResolution/mapResolution;
		//Fills this chunk on the neighbours list from the chunks created before.
		if(x != 0 && i > 0) 
			chunks[i-1].UpdateVoxelMesh(voxelId);
		if(y != 0 && i - mapResolution >= 0) 
			chunks[ i - mapResolution].UpdateVoxelMesh(voxelId);
		if(z != 0 && i - mapResolution*mapResolution >= 0) 
			chunks[i - mapResolution*mapResolution].UpdateVoxelMesh(voxelId);
		if(x != 0 && y != 0 && i - mapResolution - 1 >= 0) 
			chunks[i - mapResolution - 1].UpdateVoxelMesh(voxelId);
		if(y != 0 && z != 0 && i - mapResolution - mapResolution*mapResolution >= 0) 
			chunks[i - mapResolution - mapResolution*mapResolution].UpdateVoxelMesh(voxelId);
		if(x != 0 && z != 0 && i - mapResolution*mapResolution - 1 >=0) 
			chunks[i - mapResolution*mapResolution - 1].UpdateVoxelMesh(voxelId);
		if(x != 0 && y != 0 && z != 0 && i -mapResolution * mapResolution - mapResolution - 1>=0) 
			chunks[i -mapResolution * mapResolution - mapResolution - 1].UpdateVoxelMesh(voxelId);
	}

	public void PopulateSaveData(MapSaveData saveData)
	{
		saveData.voxelSize = voxelSize;
		saveData.chunkResolution = chunkResolution;
		saveData.mapResolution = mapResolution;

		List<MapSaveData.VoxelChunkData> chunkDatas = new List<MapSaveData.VoxelChunkData>();

		foreach (VoxelChunk chunk in chunks){

			MapSaveData.VoxelChunkData chunkData = new();
			//voxelIDs = chunk.GetVoxels();
			int[] voxelIds = new int[chunkResolution*chunkResolution*chunkResolution];
			ArrayList blockDatas = new();
			
			int i = 0;
			foreach (Voxel voxel in chunk.GetVoxels()){
				switch(voxel.voxelStatus){
					case Voxel.VoxelStatus.VOXEL:
						voxelIds[i] = voxel.voxelId;
						
						break;
					case Voxel.VoxelStatus.BLOCK:
						MapSaveData.BlockData blockData = new MapSaveData.BlockData
						{
							id = voxel.blockId,
							index = i,
							rotation = voxel.block.rotation
						};
						Debug.Log("BlockSaved ID:" + voxel.voxelId + " at " + i);
						blockDatas.Add(blockData);
						voxelIds[i] = -1;
						break;
					default:
						voxelIds[i] = -1;
						break;
				}
				i++;
			}
			chunkData.chunkName = chunk.name;
			chunkData.voxelIDs = voxelIds;
			chunkData.blocks = (blockDatas as ArrayList).ToArray(typeof(MapSaveData.BlockData)) as MapSaveData.BlockData[];
			Debug.Log(chunkData.blocks);
			chunkDatas.Add(chunkData);
			
		}
		saveData.chunks = chunkDatas;
	}

	public void LoadFromSaveData(MapSaveData saveData)
	{
		//Delete current map
		foreach (VoxelChunk chunk in chunks){
			Destroy(chunk.gameObject);
		}
		//Set new map parameters

		voxelSize = saveData.voxelSize;
		chunkResolution = saveData.chunkResolution;
		mapResolution = saveData.mapResolution;
		
		//Init map and create new chunks

		Initialize();

		//Fill chunks with voxel data and place blocks
		for(int i = 0; i < chunks.Length; i++){
			chunks[i].LoadChunkData(saveData.chunks[i]);
		}

		//Recreate voxel mesh
		for(int i = 0; i< chunks.Length; i++){
			chunks[i].UpdateVoxelMesh();
		}

		//throw new System.NotImplementedException();
	}
}
