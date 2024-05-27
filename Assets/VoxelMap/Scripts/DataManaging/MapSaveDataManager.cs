using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class MapSaveDataManager
{
    public static void SaveJsonData(IEnumerable<ISaveableMap> a_Saveables, string saveFileName)
    {
        MapSaveData sd = new MapSaveData();
        foreach (var saveable in a_Saveables)
        {
            saveable.PopulateSaveData(sd);
        }

        if (FileManager.WriteToFile(saveFileName + ".dat", sd.ToJson()))
        {
            Debug.Log("Save successful");
        }
    }
    
    public static void LoadJsonData(IEnumerable<ISaveableMap> a_Saveables, string saveFileName)
    {
        if (FileManager.LoadFromFile(saveFileName + ".dat", out var json))
        {
            MapSaveData sd = new MapSaveData();
            sd.LoadFromJson(json);

            foreach (var saveable in a_Saveables)
            {
                saveable.LoadFromSaveData(sd);
            }
            
            Debug.Log("Load complete");
        }
    }
}
