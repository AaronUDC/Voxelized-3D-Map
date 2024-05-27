using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OutputSocket : MonoBehaviour
{
    [SerializeField]
    private InputMechanism _childMechanism;
    
    [SerializeField]
    private Mechanism _mechanism;

    void Awake(){
        _mechanism = GetComponentInParent<Mechanism>();
    }

    public bool IsConnected(){
        return _childMechanism != null;
    }

    public bool ConnectSocket(InputMechanism child, float startingRotation){
        if(IsConnected()){
            return false;
        }

        _childMechanism = child;
        _childMechanism.transform.SetPositionAndRotation(transform.position, transform.rotation);

        _mechanism.UpdateStatus();
        return true;

    }

    public void ReconnectSocket(){
        if(!IsConnected()){
            return;
        }
        _childMechanism.transform.SetPositionAndRotation(transform.position, transform.rotation);
        

    }

    public bool DisconnectSocket(){
        if(!IsConnected()){
            return false;
        }
        
        //Debug.Log("disconect");
        _childMechanism.ShutdownMechanism();
        _childMechanism = null;

        return true;

    }

    public void PropagateStatus(Mechanism.MechanismStatus status){
        if(IsConnected()){
            _childMechanism.SetStatus(status);
        }
    }
}

public interface IInputMechanism{


    public bool IsConnected{ get;}
    public bool ConnectMechanism(OutputSocket newParent);
    public bool DisconnectMechanism();

}
