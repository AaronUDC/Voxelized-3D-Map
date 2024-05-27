using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    
	public int rotation = 0;

	public bool canRotate = true;

	public virtual void SetRotation(int rotation){
		if(canRotate){
			this.rotation = rotation;
			gameObject.transform.eulerAngles = Vector3.up * rotation;
		}
	}

	public virtual void BreakSelf(){
		Destroy(gameObject);
	}
}
