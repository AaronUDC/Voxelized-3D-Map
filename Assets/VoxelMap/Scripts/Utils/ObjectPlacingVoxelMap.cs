using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacingVoxelMap : ObjectPlacing
{	
	private enum PlacementMode{VOXEL,BLOCK}

    public VoxelMap voxelMap;

	[SerializeField]
	private float markerSpeed;

	[SerializeField]
	private PartDB _partsDB;

	public int selectedOption = 0;

	public Color placeColor;
	public Color destroyColor;
	
	private Material markerMaterial;

	

	private static readonly string[] buildModesSt = {"Destroy", "Place"};

	public int buildMode = 0;

	[SerializeField]
	private PlacementMode placementMode = PlacementMode.BLOCK;

	public int blockRotation = 0;

	public int rotationChange = 45;

	protected override void Start(){
		base.Start();
		markerMaterial = marker.GetComponent<MeshRenderer>().sharedMaterial;
	}

	void OnGUI(){
		GUILayout.BeginArea(new Rect(4f,4f,150f,500f));
		
		buildMode = GUILayout.SelectionGrid(buildMode,buildModesSt,2);
		if(buildMode == 1){
			GUILayout.Label("Blocks");
			GUILayout.BeginScrollView(new Vector2(100,200));
			for(int i = 0; i < _partsDB.blocks.Count; i++){
				if(GUILayout.Button(_partsDB.blocks[i].name)){
					//GameObject newPart = Instantiate(_blocksPrefabs[i]);
					selectedOption = i;
					placementMode = PlacementMode.BLOCK;
					blockRotation = 0;
				};
			}
			GUILayout.EndScrollView();
			
			GUILayout.Label("Voxels");
			GUILayout.BeginScrollView(new Vector2(100,200));
			for(int i = 0; i < _partsDB.voxelMaterials.Count; i++){
				if(GUILayout.Button(_partsDB.voxelMaterials[i].name)){
					//GameObject newPart = Instantiate(_blocksPrefabs[i]);
					selectedOption = i;
					placementMode = PlacementMode.VOXEL;
				};
			}
			GUILayout.EndScrollView();
		}else if(buildMode == 1){
			GUILayout.Label("Destroy mode");
		}
        GUILayout.EndArea();
	}
    // Update is called once per frame
    void Update()
    {
        UpdateMaxDistance();
        
        RaycastHit hitInfo;
        Vector3 point;

		if(Input.GetKeyDown(KeyCode.R))
			blockRotation += rotationChange;

        
		bool isSnapped = GetCursorWorldPositionFromCamera(maxDistance,Camera.main,out hitInfo, out point);
	
        PlaceCursor(point,hitInfo,isSnapped);


		
        if(Input.GetMouseButtonDown(0) && GUIUtility.hotControl == 0){
			if(buildMode == 1){
				
				if(placementMode == PlacementMode.VOXEL){
					voxelMap.PlaceVoxel(marker.transform.position, selectedOption);
				}else{
					voxelMap.PlaceBlock(marker.transform.position, blockRotation, selectedOption);
				}
			}else{
				voxelMap.BreakAt(marker.transform.position);
			}
        }
    }

	void PlaceCursor(Vector3 point, RaycastHit hitInfo, bool blockSnap){
		Vector3 newPosition = Vector3.zero;
		
		if(debugMode){
			if(blockSnap){
				newPosition = voxelMap.SnapToGrid( point + (((buildMode * 2)-1) * (voxelMap.voxelSize/2) * hitInfo.normal));
			}else{
				newPosition = voxelMap.SnapToGrid( point);
			}
		}else{
			marker.SetActive(true);
			if(buildMode == 1){ //Place mode
				markerMaterial.SetColor("_Border_color", placeColor);
				marker.transform.localScale = Vector3.one * 0.8f; 
				if(blockSnap){
					newPosition = voxelMap.SnapToGrid(point + (voxelMap.voxelSize/2) * hitInfo.normal);
					
				}else{
					newPosition= voxelMap.SnapToGrid(point);
				}
			}else{	//Destroy mode
				markerMaterial.SetColor("_Border_color", destroyColor);
				marker.transform.localScale = Vector3.one * 1.3f; 
				if(blockSnap){
					newPosition = voxelMap.SnapToGrid(point + (voxelMap.voxelSize/2) * -hitInfo.normal);
					
				}else{
					newPosition = Vector3.zero;
					marker.SetActive(false);
				}
			}
		}
		marker.transform.position = Vector3.Lerp(marker.transform.position, newPosition, markerSpeed* Time.deltaTime);
	}


        
}
