using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechanismEditor : MonoBehaviour
{

    [SerializeField]
    private InputMechanism _selectedMechanism;

    [SerializeField]
    private float selectedRotation;

    public float scrollMultiplier = 1.5f;

    //---------- GUI ------------

    private static string[] _partsNames;

    public List<GameObject> _partsList;
    private int _selectedPart;
    
    void OnGUI(){
        GUILayout.BeginArea(new Rect(4f,4f,150f,500f));
        if(IsSomethingSelected()){
            if(GUILayout.Button("Delete selected part")) DeleteSelected();

        }else{

            GUILayout.Label("Add");
            GUILayout.BeginScrollView(new Vector2(100,200));
            foreach (GameObject part in _partsList){
                if(GUILayout.Button(part.name)){
                    GameObject newPart = Instantiate(part);
                    SelectMechanism(newPart.transform);
                };
            }
            GUILayout.EndScrollView();
        }

        GUILayout.EndArea();
    }

    private void InitPartsListNames(){
        _partsNames = new string[_partsList.Count];
        for(int i = 0 ; i < _partsList.Count; i++){
            _partsNames[i] = _partsList[i].name;
        }
    }

    void Awake(){
        InitPartsListNames();
    }

    void Update(){
        RaycastHit hitInfo;

        //Right click
        if(Input.GetMouseButtonDown(1)){

            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo)){
                SelectMechanism(hitInfo.transform);
                return;
            }
        }
        
        if(Input.GetMouseButtonDown(0)){

            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo)){
                ConnectMechanism(hitInfo.transform);
                return;
            }
        }

        if(_selectedMechanism != null){
            
            selectedRotation += Input.mouseScrollDelta.y * scrollMultiplier;

            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo)){
                if (hitInfo.transform.TryGetComponent<OutputSocket>(out OutputSocket hoveredOutputSocket) && !hoveredOutputSocket.IsConnected()){
                    
                    _selectedMechanism.transform.SetPositionAndRotation(hoveredOutputSocket.transform.position, hoveredOutputSocket.transform.rotation);
                    _selectedMechanism.transform.Rotate(_selectedMechanism.transform.up, selectedRotation, Space.World);

                }else{

                    _selectedMechanism.transform.position = hitInfo.point;
                    _selectedMechanism.transform.eulerAngles = new Vector3(0, selectedRotation, 0);
                }
            }
        }else{
            selectedRotation = 0.0f;
        }
        

    }

    private bool IsSomethingSelected(){
        return _selectedMechanism != null;
    }

    private void SelectMechanism(Transform mechanism){
        if(IsSomethingSelected()){
            DeselectMechanism();
            return;
        }
        if (mechanism.TryGetComponent<InputMechanism>(out _selectedMechanism)){
            Debug.Log("Selected: " + mechanism.name);
            SetAllLayersTo(_selectedMechanism.transform, 2);
            //_selectedMechanism.gameObject.layer = 2;

            if (_selectedMechanism.IsConnected()){
                DisconnectMechanism();
            }
        }
    }
    private void SetAllLayersTo(Transform parent, int layer){
        parent.gameObject.layer = layer;
        foreach (Transform child in parent){
            SetAllLayersTo(child,layer);
        }
    }

    private void DeselectMechanism(){
        SetAllLayersTo(_selectedMechanism.transform, 0);
        _selectedMechanism = null;
    }

    private void DeleteSelected(){
        Destroy(_selectedMechanism.gameObject);
        _selectedMechanism = null;
    }

    private void ConnectMechanism(Transform mechanism){
        OutputSocket selectedOutputSocket;

        if( _selectedMechanism == null){
            Debug.Log("No mechanism selected.");
            return;
        }

        Debug.Log(mechanism.transform.name);

        if (mechanism.TryGetComponent<OutputSocket>(out selectedOutputSocket)){
            //Test if the selected socket belongs to the selected mechanism.
            InputMechanism socketMechanism;
            if (selectedOutputSocket.transform.parent.TryGetComponent<InputMechanism>(out socketMechanism)){
                if(socketMechanism.Equals(_selectedMechanism)){
                    Debug.Log("Can't connect to itself!");
                    return;
                }
            }

            if (_selectedMechanism.IsConnected()){
                DisconnectMechanism();
            }
        
            if(_selectedMechanism.ConnectMechanism(selectedOutputSocket,selectedRotation)){
                Debug.Log("Mechanism " + _selectedMechanism.transform.name + "connected to" + selectedOutputSocket.transform.parent.name);
                DeselectMechanism();
            }else{
                Debug.Log("Can't connect " + _selectedMechanism.transform.name + " to" + selectedOutputSocket.transform.parent.name);
            }

            
        }
    }

    private void DisconnectMechanism(){

        Debug.Log("Desconectando");
        _selectedMechanism.DisconnectMechanism();
        
    }

}
