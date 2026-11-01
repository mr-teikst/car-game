using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class KenneyReferenceRepair
{
    const string Commercial = "Assets/kenney_city-kit-commercial_2.1/Models/OBJ format/";
    const string Cars = "Assets/kenney_car-kit/Models/OBJ format/";
    const string Roads = "Assets/kenney_city-kit-roads/Models/OBJ format/";
    static bool pending;

    static KenneyReferenceRepair()
    {
        EditorApplication.delayCall += TryRepair;
    }

    [MenuItem("Night Drive/Repair imported model references")]
    public static void TryRepair()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) {
            if (!pending) { pending = true; EditorApplication.playModeStateChanged += OnPlayModeChanged; }
            return;
        }
        Repair();
    }
    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode) return;
        pending = false; EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.delayCall += Repair;
    }
    static GameObject Model(string path)
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (!model) Debug.LogError("Cannot load Kenney model: " + path);
        return model;
    }
    static bool Set(SerializedObject data, string field, GameObject model)
    {
        var property = data.FindProperty(field);
        if (property == null || property.objectReferenceValue == model) return false;
        property.objectReferenceValue = model; data.ApplyModifiedPropertiesWithoutUndo(); return true;
    }
    static bool RepairVisual(KenneyVisual visual, string path)
    {
        return visual && Set(new SerializedObject(visual), "model", Model(path));
    }
    static void RepairPrefab(string path, string modelPath, bool buildings)
    {
        var root = PrefabUtility.LoadPrefabContents(path);
        if (!root) return;
        bool changed = false;
        try {
            var visual = root.GetComponent<KenneyVisual>();
            if (visual) changed |= RepairVisual(visual, modelPath);
            if (buildings) {
                var roadside = root.GetComponent<RoadsideBuildings>();
                if (roadside) {
                    var data = new SerializedObject(roadside);
                    var models = data.FindProperty("buildingModels");
                    string[] names = { "building-a.obj", "building-c.obj", "building-g.obj" };
                    models.arraySize = names.Length;
                    for (int i = 0; i < names.Length; i++) {
                        var model = Model(Commercial + names[i]);
                        var element = models.GetArrayElementAtIndex(i);
                        if (element.objectReferenceValue != model) { element.objectReferenceValue = model; changed = true; }
                    }
                    data.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            if (changed) PrefabUtility.SaveAsPrefabAsset(root, path);
        } finally { PrefabUtility.UnloadPrefabContents(root); }
    }
    public static void Repair()
    {
        const string prefabRoot = "Assets/NightDriveGenerated/Prefabs/";
        RepairPrefab(prefabRoot + "RoadSegment.prefab", Roads + "road-straight.obj", true);
        RepairPrefab(prefabRoot + "StationaryTraffic.prefab", Cars + "sedan.obj", false);
        RepairPrefab(prefabRoot + "MovingTraffic.prefab", Cars + "sedan.obj", false);

        const string scenePath = "Assets/Scenes/NightDrive.unity";
        Scene scene = SceneManager.GetSceneByPath(scenePath);
        bool opened = !scene.IsValid() || !scene.isLoaded;
        if (opened) scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        bool changed = false;
        foreach (var root in scene.GetRootGameObjects())
            foreach (var visual in root.GetComponentsInChildren<KenneyVisual>(true))
                if (visual.gameObject.name == "Roadster Visual") changed |= RepairVisual(visual, Cars + "sedan-sports.obj");
        if (changed) EditorSceneManager.SaveScene(scene);
        if (opened) EditorSceneManager.CloseScene(scene, true);
        AssetDatabase.SaveAssets();
        Debug.Log("Night Drive imported model references repaired.");
    }
}
