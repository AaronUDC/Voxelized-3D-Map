using System;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class Voxel
{

	public enum VoxelStatus{ FREE, VOXEL, BLOCK}
    public int voxelId;
	public Block block;

	public int blockId;

    public Vector3 position;

	public VoxelStatus voxelStatus{
		get{
			if((block == null) && (voxelId != -1)){
				return VoxelStatus.VOXEL;
			}
			if((block != null) && (blockId != -1)){
				return VoxelStatus.BLOCK;
			}
			return VoxelStatus.FREE;
		}
	}
	
	/*
    public bool IsFree{
        get{return (voxelId == -1) && (block == null);}
    }

	public bool IsVoxel{
		get{return (block == null) && (voxelId != -1);}
	}
	public bool IsBlock{
		get{return (block != null) && (voxelId == -1);}
	}*/

    public Voxel(int x, int y, int z, float size){
        position.x = (x + 0.5f) * size;
        position.y = (y + 0.5f) * size;
        position.z = (z + 0.5f) * size;
        
        voxelId = -1;
		block = null;
    }

    public Voxel(){}

    public bool SetVoxel(int voxelId){

        if(voxelStatus ==  VoxelStatus.FREE){
            this.voxelId = voxelId;
			return true;
        }
		return false;
    }

	public bool SetBlock(Block block, int blockID){
		if(voxelStatus == VoxelStatus.FREE){
            this.block = block;
			this.blockId = blockID;
			return true;
        }
		return false;
	}

	public void BreakThis(bool destroyBlock){
		if(voxelStatus == VoxelStatus.VOXEL){
			BreakVoxel();

		}else if(voxelStatus == VoxelStatus.BLOCK){
			BreakBlock(destroyBlock);
		}
	}

	public int BreakVoxel(){
		int id = -1;
		if(voxelStatus == VoxelStatus.VOXEL){
			id = this.voxelId;
			this.voxelId = -1;
		}
		return id;
	}

	public Block BreakBlock(bool destroyBlock){
		if(voxelStatus == VoxelStatus.BLOCK){
			Block tempBlock = block;
			if(destroyBlock){
				//Mandar que se destruya. Probablemente haya que crear un script para los bloques.
				block.BreakSelf();			
			}
			block = null;
			blockId = -1;
			return tempBlock;
		}
		return null;
	}

	

}
