using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MiniLevelEditor : EditorWindow
{
    private GridMiniLevelData miniLevelData;
    private Transform puzzleContainer;
    private GridBoardManager gridBoardManager;

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
        gridBoardManager = (GridBoardManager)EditorGUILayout.ObjectField(
            "Board Manager",
            gridBoardManager,
            typeof(GridBoardManager),
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
        if (gridBoardManager != null)
        {
            miniLevelData.boardWidth = gridBoardManager.boardWidth;
            miniLevelData.boardHeight = gridBoardManager.boardHeight;
            miniLevelData.cellSize = gridBoardManager.cellSize;
            miniLevelData.boardOrigin = gridBoardManager.origin;
        }

        GridDragObject[] objects = puzzleContainer.GetComponentsInChildren<GridDragObject>();
        List<GridObjectData> objectList = new List<GridObjectData>();

        foreach (var obj in objects)
        {
            GridObjectData data = new GridObjectData();
            data.objectId = obj.objectId;
            data.startWorldPosition = obj.transform.localPosition;
            data.scale = obj.transform.localScale;
            data.currentLayer = obj.currentLayer;
            data.prefab = PrefabUtility.GetCorrespondingObjectFromSource(obj.gameObject);
            data.states = new GridObjectState[obj.states.Length];
            for (int i = 0; i < obj.states.Length; i++)
            {
                data.states[i] = obj.states[i];
            }
            objectList.Add(data);
        }

        LandingZone[] zones = puzzleContainer.GetComponentsInChildren<LandingZone>();
        if (zones.Length > 0)
        {
            List<LandingZoneData> zoneList = new List<LandingZoneData>();
            foreach (var zone in zones)
            {
                LandingZoneData data = new LandingZoneData();
                data.zoneId = zone.zoneId;
                data.position = zone.transform.localPosition;
                data.prefab = PrefabUtility.GetCorrespondingObjectFromSource(zone.gameObject);
                data.scale = zone.transform.localScale;
                data.spriteVisual = zone.zoneSprite;
                data.colorVisual = zone.zoneColor;
                // objectId[] is a relational config — preserve existing or leave empty for manual setup
                data.objectId = GetExistingZoneObjectIds(zone.zoneId);
                zoneList.Add(data);
            }
            miniLevelData.landingZones = zoneList.ToArray();
            miniLevelData.useLandingZones = true;
        }

        // ===== SAVE =====
        miniLevelData.objects = objectList.ToArray();

        EditorUtility.SetDirty(miniLevelData);
        AssetDatabase.SaveAssets();

        Debug.Log("✅ MiniLevelData updated from scene!");
    }
    private string[] GetExistingZoneObjectIds(string zoneId)
    {
        if (miniLevelData.landingZones == null) return new string[0];
        foreach (var existing in miniLevelData.landingZones)
        {
            if (existing.zoneId == zoneId)
                return existing.objectId ?? new string[0];
        }
        return new string[0];
    }
}
