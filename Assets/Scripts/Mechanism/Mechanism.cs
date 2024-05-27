using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public abstract class Mechanism : MonoBehaviour
{
    [Serializable]
    public struct MechanismStatus{
        public float speed;

        public float offset;
        public bool isActive;

        public MechanismStatus(float speed, float offset, bool isActive){
            this.speed = speed;
            this.offset = offset;
            this.isActive = isActive;
        }

        public MechanismStatus(MechanismStatus mechanismStatus){
            this.speed = mechanismStatus.speed;
            this.offset = mechanismStatus.offset;
            this.isActive = mechanismStatus.isActive;
        }

    }

    [SerializeField]
    protected MechanismStatus _status = new MechanismStatus(0, 0, false);


    //Set a new status
    public void SetStatus(MechanismStatus status){
        _status = new MechanismStatus(status);

        OnUpdateStatus();
    }
    //Set only isActive
    public virtual void SetActive(bool isActive){
        _status.isActive = isActive;
        
        OnUpdateStatus();
    }

    //Set a new speed
    public virtual void SetSpeed(float speed){
        _status.speed = speed;

        OnUpdateStatus();
    }

    public virtual void SetOffset(float offset){
        _status.offset = offset;
        
        OnUpdateStatus();
    }


    public virtual void UpdateStatus(){
        OnUpdateStatus();
    }

    //Do something when the status has changed
    protected abstract void OnUpdateStatus();
}
public abstract class InputMechanism : Mechanism
{
    [SerializeField]
    private OutputSocket _parent;   //Mechanism connected on the input}

    public bool IsConnected(){
        return _parent != null;
    }

    public virtual bool ConnectMechanism(OutputSocket newParent, float startingRotation){
        if(IsConnected()){
            DisconnectMechanism();
        }
        if(newParent.ConnectSocket(this, startingRotation)){
            _parent = newParent;

            //OnUpdateStatus();

            return true;
        }

        return false;
    }

    public virtual void DisconnectMechanism(){
        if(IsConnected()){
            if(_parent.DisconnectSocket())
                _parent = null;
        }
    }

    public virtual void ShutdownMechanism(){
        SetActive(false);
    }

    public OutputSocket GetParentSocket(){
        return _parent;
    }
}

public abstract class InOutMechanism : InputMechanism
{

    [SerializeField]
    private List<OutputSocket> _outputSockets; //Output sockets

    public override bool ConnectMechanism(OutputSocket newParent, float startingRotation)
    {
        foreach(OutputSocket outputSocket in _outputSockets){
            outputSocket.DisconnectSocket();
        }

        return base.ConnectMechanism(newParent, startingRotation);;
    }

    public override void DisconnectMechanism()
    {
        base.DisconnectMechanism();
        Debug.Log("Desconectando outputs antiguos");
        foreach(OutputSocket outputSocket in _outputSockets){
            outputSocket.DisconnectSocket();
        }
    }

    // Propagate status to each outputSocket to the network
    protected override void OnUpdateStatus(){
        for(int i = 0; i < _outputSockets.Count; i++){
            _outputSockets[i].PropagateStatus(StatusTransform(i));
        }
    }

    //Function to transfer the input status to the output
    protected abstract MechanismStatus StatusTransform(int outputIndex);

}