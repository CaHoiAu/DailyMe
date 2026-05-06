using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MiniLevelEditor : EditorWindow
{
    private GridMiniLevelData miniLevelData;
    private Transform puzzleContainer;

    [MenuItem("Tools/Mini Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<MiniLevelEditor>("Mini Level Editor");
    }
    private void OnGUI()
    {
        GUILayout.Label("Mini Level Capture Tool", EditorStyles.boldLabel);
        miniLevelData = (GridMiniLevelData)EditorGUILayout.ObjectField(
            "Mini Level Data", 
            miniLevelData, 
            typeof(GridMiniLevelData), 
            false);
        puzzleContainer = (Transform)EditorGUILayout.ObjectField(
            "Puzzle Container", 
            puzzleContainer, 
            typeof(Transform), 
            true);
        GUILayout.Space(10);
        if (GUILayout.Button("Capture Mini Level"))
        {
            if (miniLevelData == null || puzzleContainer == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign both Mini Level Data and Puzzle Container.", "OK");
                return;
            }
            CaptureData();
        }
    }
    private void CaptureData()
    {
        // ===== SLOT =====
        //Slot[] slots = puzzleContainer.GetComponentsInChildren<Slot>();
        //List<SlotData> slotList = new List<SlotData>();

        //foreach (var slot in slots)
        //{
        //    SlotData data = new SlotData();
        //    data.slotId = slot.slotId;
        //    data.position = slot.transform.localPosition;
        //    data.prefab = PrefabUtility.GetCorrespondingObjectFromSource(slot.gameObject);
        //    data.maxLayer = slot.maxLayer;

        //    slotList.Add(data);
        //}

        // ===== OBJECT =====
        GridDragObject[] objects = puzzleContainer.GetComponentsInChildren<GridDragObject>();
        List<GridObjectData> objectList = new List<GridObjectData>();

        foreach (var obj in objects)
        {
            GridObjectData data = new GridObjectData();
            data.objectId = obj.objectId;
            data.startWorldPosition = obj.transform.localPosition;
            data.prefab = PrefabUtility.GetCorrespondingObjectFromSource(obj.gameObject);
            data.states = new GridObjectState[obj.states.Length];
            for (int i = 0; i < obj.states.Length; i++)
            {
                data.states[i] = obj.states[i];
            }
            objectList.Add(data);
        }

        // ===== SAVE =====
        miniLevelData.objects = objectList.ToArray();

        EditorUtility.SetDirty(miniLevelData);
        AssetDatabase.SaveAssets();

        Debug.Log("✅ MiniLevelData updated from scene!");
    }
}
