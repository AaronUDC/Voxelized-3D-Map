using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelChunk : MonoBehaviour
{   
	public enum NeighbourID{X,Y,Z,XY,YZ,XZ,XYZ};

    public int chunkResolution = 16;
    private int halfchunkResolution;

	private Vector3Int startingPosition;

    [SerializeField]
    private Voxel[] voxels;

	private Dictionary<NeighbourID,VoxelChunk> chunkNeighbours;
    public float voxelSize,chunkSize,halfVoxelSize;

    //public GameObject markerPrefab;
    //private Transform[] voxelMarkers;

	private Dictionary<int,int> _voxelsCount= new Dictionary<int, int>();

	private PartDB _partDB;

	[SerializeField]
	MeshFilter meshFilter;

	[SerializeField]
	MeshRenderer meshRenderer;

	[SerializeField]
	MeshCollider meshCollider;

	public VoxelMaterial lastVoxelUsed;

	//MARK: Initialization
    public void Initialize(int chunkResolution, float voxelSize, Vector3Int startingPosition, PartDB partDB){
        this.chunkResolution = chunkResolution;
        chunkSize = voxelSize * chunkResolution;
        this.voxelSize = voxelSize;
		this.halfVoxelSize = voxelSize/2;
		this.startingPosition = startingPosition;
        halfchunkResolution = Mathf.FloorToInt(chunkResolution/2);

        voxels = new Voxel[chunkResolution*chunkResolution*chunkResolution];
        //voxelMarkers = new Transform[voxels.Length];

        for(int i = 0, z = 0; z < chunkResolution; z++){
            for(int y = 0; y < chunkResolution; y++){
                for(int x = 0; x < chunkResolution; x++, i++){
                        CreateVoxel(i,x,y,z);
                }
            }
        }

		chunkNeighbours = new Dictionary<NeighbourID, VoxelChunk>();

		this._partDB = partDB;
    }

    private void CreateVoxel(int i, int x, int y, int z){
        /*GameObject marker = Instantiate(markerPrefab) as GameObject;
        marker.transform.parent = transform; 
		marker.name = string.Format("Marker_{3}_{0}_{1}_{2} ",startingPosition.x + x, startingPosition.y + y, startingPosition.z + z,i); 
        marker.transform.localPosition = new Vector3((x + 0.5f) *voxelSize, (y + 0.5f) * voxelSize, (z + 0.5f) * voxelSize);
        marker.transform.localScale = 0.1f * voxelSize * Vector3.one;

        voxelMarkers[i] = marker.transform;
        marker.SetActive(false);
        */
        voxels[i] = new Voxel(x, y, z, voxelSize);
    } 

	public Voxel[] GetVoxels(){
		return (Voxel[])voxels.Clone();
	}

	public void SetVoxels(Voxel[] voxels){
		if(voxels.Length != chunkResolution*chunkResolution*chunkResolution){
			throw new Exception("Attempted to load a chunk of different dimensionsaa");
		}else{
			this.voxels = (Voxel[])voxels.Clone();
		}
		
	}

	public void LoadChunkData(MapSaveData.VoxelChunkData chunkData){

		//Add the voxels
		for(int i = 0; i< voxels.Length; i++){
			if(chunkData.voxelIDs[i]>=0){
				voxels[i].SetVoxel(chunkData.voxelIDs[i]);
				AddToVoxelCount(chunkData.voxelIDs[i]);
			}
		}

		//Place blocks
		
		foreach (MapSaveData.BlockData blockData in chunkData.blocks){
			if(voxels[blockData.index].voxelStatus != Voxel.VoxelStatus.FREE)
				throw new Exception("Map data is not consistent. Atempted placing a block in a non free voxel");
			
			PlaceBlock(blockData.index,blockData.rotation, blockData.id);
		}

		
	}

	//MARK: Utils

	public void AddNeighbour(NeighbourID neighbourID, VoxelChunk neighbour){
		chunkNeighbours.Add(neighbourID,neighbour);
	}

	public int GetIndexFromChunkPosition(Vector3Int position){
		return GetIndexFromChunkPosition(position.x,position.y,position.z);
	}

	public int GetIndexFromChunkPosition(int x, int y, int z){
		return x + (y * chunkResolution) + (z *chunkResolution * chunkResolution);
	}
	private void AddToVoxelCount(int voxelId){
		if(_voxelsCount.ContainsKey(voxelId)){
			_voxelsCount[voxelId]++;
		}else{
			_voxelsCount.Add(voxelId,1);
		}
	}
	private void SubstractToVoxelCount(int voxelId){
		if(_voxelsCount.ContainsKey(voxelId)){
			_voxelsCount[voxelId]--;
		}else{
			throw new Exception("Tried substracting from a nonexistant voxel in the chunk:" + startingPosition);
		}
	}


	//MARK: Mesh updating
	
	public void UpdateVoxelMesh(){

		foreach(int voxelId in _voxelsCount.Keys){
			BuildVoxelMesh(voxelId);
		}
		
	}

	public void UpdateVoxelMesh(int voxelId){
		BuildVoxelMesh(voxelId);
	}

	private void BuildVoxelMesh(int voxelId){

		int cells = chunkResolution -1;

		ArrayList parts = new ArrayList();
		int i = 0;

		//Loop on each cell
		for(int z= 0; z < cells; z++){
			for(int y= 0; y < cells; y++){
				
				for(int x = 0; x < cells; x++, i++){
					byte celltype = GetCellType(voxelId,i);
					Vector3 position = voxels[i].position + (Vector3.one * halfVoxelSize);
					parts.AddRange(ChunkBuilder.AddCellInfo(celltype, position, voxelSize));
				}
				//Gap with X neighbour
				if(chunkNeighbours.ContainsKey(NeighbourID.X)){

					byte celltype = GetCellType(voxelId,new Voxel[]{
						voxels[i],
						chunkNeighbours[NeighbourID.X].voxels[GetIndexFromChunkPosition(0,y,z)],
						chunkNeighbours[NeighbourID.X].voxels[GetIndexFromChunkPosition(0,y,z+1)],
						voxels[i + chunkResolution * chunkResolution],
						voxels[i + chunkResolution],
						chunkNeighbours[NeighbourID.X].voxels[GetIndexFromChunkPosition(0,y+1,z)],
						chunkNeighbours[NeighbourID.X].voxels[GetIndexFromChunkPosition(0,y+1,z+1)],	
						voxels[i + chunkResolution + chunkResolution * chunkResolution]
					});
					Vector3 position = voxels[i].position + (Vector3.one * halfVoxelSize);
					parts.AddRange(ChunkBuilder.AddCellInfo(celltype, position, voxelSize));
				}
				i++;
			}
			if(chunkNeighbours.ContainsKey(NeighbourID.Y)){ 
				for(int x = 0; x < cells; x++, i++){
					//Gap with Y neighbour
					byte celltype = GetCellType(voxelId,new Voxel[]{
						voxels[i],
						voxels[i + 1],
						voxels[i + 1 + chunkResolution * chunkResolution],
						voxels[i + chunkResolution * chunkResolution],
						chunkNeighbours[NeighbourID.Y].voxels[GetIndexFromChunkPosition(x,0,z)],
						chunkNeighbours[NeighbourID.Y].voxels[GetIndexFromChunkPosition(x+1,0,z)],
						chunkNeighbours[NeighbourID.Y].voxels[GetIndexFromChunkPosition(x+1,0,z+1)],	
						chunkNeighbours[NeighbourID.Y].voxels[GetIndexFromChunkPosition(x,0,z+1)]
					});
					Vector3 position = voxels[i].position + (Vector3.one * halfVoxelSize);
					parts.AddRange(ChunkBuilder.AddCellInfo(celltype, position, voxelSize));
				}

			}else{ i+= cells;}
			
			//Gap with XY neighbour
			if(chunkNeighbours.ContainsKey(NeighbourID.Y) && chunkNeighbours.ContainsKey(NeighbourID.X)){
					byte celltype = GetCellType(voxelId,new Voxel[]{
						voxels[i],
						chunkNeighbours[NeighbourID.X].voxels[GetIndexFromChunkPosition(0,cells,z)],
						chunkNeighbours[NeighbourID.X].voxels[GetIndexFromChunkPosition(0,cells,z+1)],
						voxels[i + chunkResolution * chunkResolution],
						chunkNeighbours[NeighbourID.Y].voxels[GetIndexFromChunkPosition(cells,0,z)],
						chunkNeighbours[NeighbourID.XY].voxels[GetIndexFromChunkPosition(0,0,z)],
						chunkNeighbours[NeighbourID.XY].voxels[GetIndexFromChunkPosition(0,0,z+1)],	
						chunkNeighbours[NeighbourID.Y].voxels[GetIndexFromChunkPosition(cells,0,z+1)]
					});
					Vector3 position = voxels[i].position + (Vector3.one * halfVoxelSize);
					parts.AddRange(ChunkBuilder.AddCellInfo(celltype, position, voxelSize));
			}
			i++;			
		}

		if(chunkNeighbours.ContainsKey(NeighbourID.Z)){
			for(int y = 0; y < cells; y++){
				for(int x = 0; x < cells; x++, i++){
					//Gap with Z neighbour
					byte celltype = GetCellType(voxelId,new Voxel[]{
						voxels[i],
						voxels[i + 1],
						chunkNeighbours[NeighbourID.Z].voxels[GetIndexFromChunkPosition(x+1,y,0)],
						chunkNeighbours[NeighbourID.Z].voxels[GetIndexFromChunkPosition(x,y,0)],
						voxels[i + chunkResolution],
						voxels[i + chunkResolution + 1],
						chunkNeighbours[NeighbourID.Z].voxels[GetIndexFromChunkPosition(x+1,y+1,0)],	
						chunkNeighbours[NeighbourID.Z].voxels[GetIndexFromChunkPosition(x,y+1,0)]
					});
					Vector3 position = voxels[i].position + (Vector3.one * halfVoxelSize);
					parts.AddRange(ChunkBuilder.AddCellInfo(celltype, position, voxelSize));
				}
				//Gap with XZ neighbour
				if(chunkNeighbours.ContainsKey(NeighbourID.X)){
					
					byte celltype = GetCellType(voxelId,new Voxel[]{
						voxels[i],
						chunkNeighbours[NeighbourID.X].voxels[GetIndexFromChunkPosition(0,y,cells)],
						chunkNeighbours[NeighbourID.XZ].voxels[GetIndexFromChunkPosition(0,y,0)],
						chunkNeighbours[NeighbourID.Z].voxels[GetIndexFromChunkPosition(cells,y,0)],
						voxels[i + chunkResolution],
						chunkNeighbours[NeighbourID.X].voxels[GetIndexFromChunkPosition(0,y+1,cells)],
						chunkNeighbours[NeighbourID.XZ].voxels[GetIndexFromChunkPosition(0,y+1,0)],	
						chunkNeighbours[NeighbourID.Z].voxels[GetIndexFromChunkPosition(cells,y+1,0)]
					});
					Vector3 position = voxels[i].position + (Vector3.one * halfVoxelSize);
					parts.AddRange(ChunkBuilder.AddCellInfo(celltype, position, voxelSize));
				}
				i++;
			}
			//Debug.Log(i);
			if(chunkNeighbours.ContainsKey(NeighbourID.Y)){
				for(int x = 0; x < cells; x++, i++){
					//Gap with YZ neighbour
					byte celltype = GetCellType(voxelId,new Voxel[]{
						voxels[i],
						voxels[i + 1],
						chunkNeighbours[NeighbourID.Z].voxels[GetIndexFromChunkPosition(x+1,cells,0)],
						chunkNeighbours[NeighbourID.Z].voxels[GetIndexFromChunkPosition(x,cells,0)],
						chunkNeighbours[NeighbourID.Y].voxels[GetIndexFromChunkPosition(x,0,cells)],
						chunkNeighbours[NeighbourID.Y].voxels[GetIndexFromChunkPosition(x+1,0,cells)],
						chunkNeighbours[NeighbourID.YZ].voxels[GetIndexFromChunkPosition(x+1,0,0)],	
						chunkNeighbours[NeighbourID.YZ].voxels[GetIndexFromChunkPosition(x,0,0)]
					});
					Vector3 position = voxels[i].position + (Vector3.one * halfVoxelSize);
					parts.AddRange(ChunkBuilder.AddCellInfo(celltype, position, voxelSize));
				}
				if(chunkNeighbours.ContainsKey(NeighbourID.X) && chunkNeighbours.ContainsKey(NeighbourID.Z)){
				//Gap with XYZ neighbour	
				byte celltype = GetCellType(voxelId,new Voxel[]{
						voxels[i],
						chunkNeighbours[NeighbourID.X].voxels[GetIndexFromChunkPosition(0,cells,cells)],
						chunkNeighbours[NeighbourID.XZ].voxels[GetIndexFromChunkPosition(0,cells,0)],
						chunkNeighbours[NeighbourID.Z].voxels[GetIndexFromChunkPosition(cells,cells,0)],
						chunkNeighbours[NeighbourID.Y].voxels[GetIndexFromChunkPosition(cells,0,cells)],
						chunkNeighbours[NeighbourID.XY].voxels[GetIndexFromChunkPosition(0,0,cells)],
						chunkNeighbours[NeighbourID.XYZ].voxels[0],	
						chunkNeighbours[NeighbourID.YZ].voxels[GetIndexFromChunkPosition(cells,0,0)]
					});
					Vector3 position = voxels[i].position + (Vector3.one * halfVoxelSize);
					parts.AddRange(ChunkBuilder.AddCellInfo(celltype, position, voxelSize));
				i++;
				}
			}
		}

		//Still supports only one voxel Material
		ChunkBuilder.MeshData meshData = ChunkBuilder.BuildChunk(parts.ToArray(typeof(ChunkBuilder.PartInfo)) as ChunkBuilder.PartInfo[], _partDB.voxelMaterials[voxelId]);

		meshFilter.sharedMesh = meshData.mesh;
		meshRenderer.materials = meshData.materials;

		meshCollider.sharedMesh = meshData.mesh;
		
	}

	private byte GetCellType(int voxelId, int i){
		return GetCellType(voxelId, new Voxel[]{
			voxels[i],
			voxels[i + 1],
			voxels[i + 1 + chunkResolution * chunkResolution],
			voxels[i + chunkResolution * chunkResolution],
			voxels[i + chunkResolution],
            voxels[i + chunkResolution + 1],
            voxels[i + chunkResolution + 1 + chunkResolution * chunkResolution],	
            voxels[i + chunkResolution + chunkResolution * chunkResolution]
		});
	}

	
	private byte GetCellType(int voxelId, Voxel[] voxels){
		int type = 0;
		for(int i = 0; i < 8; i++){
			if(voxels[i].voxelId == voxelId)
				type |= 1 << i;
		}

		return (byte)type;
	}

	//MARK: Placing/Breaking
    public void PlaceVoxel(Vector3Int position, int voxelId){

        int i = GetIndexFromChunkPosition(position);
		//Debug.Log(voxels[i].voxelStatus);
        if(voxels[i].voxelStatus == Voxel.VoxelStatus.FREE){
			//voxelMarkers[i].gameObject.SetActive(true);
            voxels[i].SetVoxel(voxelId);
			AddToVoxelCount(voxelId);
			//lastVoxelUsed = voxelDB;
			UpdateVoxelMesh();
		}
		
    }
	public void PlaceBlock(int index, int rotation,int blockID){
		//Debug.Log(voxels[index].voxelStatus);
        if(voxels[index].voxelStatus == Voxel.VoxelStatus.FREE){
			Block blockInstance = Instantiate(_partDB.blocks[blockID].gameObject).GetComponent<Block>();
			blockInstance.SetRotation(rotation);
			voxels[index].SetBlock(blockInstance, blockID);
        	//voxelMarkers[i].gameObject.SetActive(true);
			blockInstance.transform.parent = gameObject.transform; //voxelMarkers[i];
			blockInstance.transform.localPosition =  voxels[index].position;
		}
	}
	public void PlaceBlock(Vector3Int position, int rotation, int blockID){
     	PlaceBlock(GetIndexFromChunkPosition(position),rotation,blockID);
    }

	public int BreakAt(Vector3Int position){
		int i = GetIndexFromChunkPosition(position);
		if(voxels[i].voxelStatus == Voxel.VoxelStatus.BLOCK){
			
			Debug.Log("Block destroyed");
			Destroy(voxels[i].block);
			voxels[i].BreakBlock(true);

		}else if(voxels[i].voxelStatus == Voxel.VoxelStatus.VOXEL){

			Debug.Log("Voxel destroyed");
			//voxelMarkers[i].gameObject.SetActive(false);
			SubstractToVoxelCount(voxels[i].voxelId);
			int id = voxels[i].BreakVoxel();

			UpdateVoxelMesh();	

			return id;
		}
		return -1;
	}

}
