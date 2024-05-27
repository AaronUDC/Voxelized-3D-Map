using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class ObjectPlacing : MonoBehaviour
{
    public Vector2 distanceLimits = new Vector2(1, 1000);
    public float maxDistance = 10.0f;

	[SerializeField]
    private GameObject placerMarkerPrefab;
	
    protected GameObject marker;


	[SerializeField]
	protected bool debugMode = false;
	
	[SerializeField]
	private GameObject placerMarkerDebug;
	
	
	protected virtual void Start()
    {	
		if(debugMode){
			marker = Instantiate(placerMarkerDebug);
		}else{
			marker = Instantiate(placerMarkerPrefab);
		}
        //marker.SetActive(true);
        marker.layer = LayerMask.NameToLayer("Ignore Raycast");

    }

    // Update is called once per frame
    void Update()
    {   
        UpdateMaxDistance();
        
        RaycastHit hitInfo;
        GetCursorWorldPositionFromCamera(maxDistance, Camera.main, out hitInfo, out Vector3 point);
        marker.transform.position = point;

        

    }

    protected void UpdateMaxDistance(){
        maxDistance += Input.mouseScrollDelta.y;
        maxDistance = Mathf.Clamp(maxDistance,distanceLimits[0], distanceLimits[1]);
    }

    public bool GetCursorWorldPositionFromCamera(float maxDistance, Camera camera, out RaycastHit hitInfo, out Vector3 point){

        Ray rayCamera = camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(rayCamera, out hitInfo) && (hitInfo.distance < maxDistance)){

            point =  hitInfo.point;
            return true;
        }else{
            point = rayCamera.GetPoint(maxDistance);
            return false;
        }
    }
}
