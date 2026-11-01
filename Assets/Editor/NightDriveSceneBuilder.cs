using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class NightDriveSceneBuilder
{
    const string Root = "Assets/NightDriveGenerated";
    [MenuItem("Night Drive/Create or rebuild game scene")]
    public static void Build()
    {
        if (!AssetDatabase.IsValidFolder(Root)) AssetDatabase.CreateFolder("Assets", "NightDriveGenerated");
        if (!AssetDatabase.IsValidFolder(Root+"/Materials")) AssetDatabase.CreateFolder(Root,"Materials");
        if (!AssetDatabase.IsValidFolder(Root+"/Prefabs")) AssetDatabase.CreateFolder(Root,"Prefabs");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/StarryNight.mat"); RenderSettings.ambientLight = new Color(.1f,.12f,.22f); RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(.015f,.02f,.06f); RenderSettings.fogDensity = .006f;
        var asphalt=Material("Asphalt",new Color(.045f,.055f,.09f)); var stripe=Material("LaneLight",new Color(.6f,.8f,1),true);
        var edge=Material("RoadEdge",new Color(.8f,.05f,.4f),true); var red=Material("RoadsterRed",new Color(.9f,.92f,.95f));
        var dark=Material("GlassAndTyres",new Color(.025f,.035f,.055f)); var white=Material("Headlights",new Color(.8f,.9f,1),true);
        var tail=Material("TailLight",new Color(1,.025f,.035f),true); var gold=Material("Coin",new Color(1,.75f,.04f),true);
        var green=Material("Fuel",new Color(.1f,1,.35f),true); var traffic=Material("Traffic",new Color(.75f,.12f,.18f));
        var road=BuildRoad(asphalt,stripe,edge); var stationary=BuildTraffic<StationaryObstacle>("StationaryTraffic",traffic,dark,tail);
        var moving=BuildTraffic<MovingObstacle>("MovingTraffic",traffic,dark,tail); var coin=BuildPickup<Coin>("Coin",gold,true); var fuel=BuildPickup<FuelCan>("FuelCan",green,false);
        var camera = new GameObject("Main Camera"); camera.tag="MainCamera"; var cam=camera.AddComponent<Camera>(); cam.clearFlags=CameraClearFlags.Skybox; cam.backgroundColor=new Color(.012f,.018f,.06f); cam.fieldOfView=65;
        camera.AddComponent<AudioListener>(); camera.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing=true;
        var moon=new GameObject("Moonlight").AddComponent<Light>(); moon.type=LightType.Directional; moon.color=new Color(.35f,.46f,1); moon.intensity=.4f; moon.transform.rotation=Quaternion.Euler(50,-30,0);
        var volume=new GameObject("Bloom Volume").AddComponent<Volume>(); volume.isGlobal=true;
        var profile=ScriptableObject.CreateInstance<VolumeProfile>(); var bloom=profile.Add<Bloom>(true); bloom.intensity.Override(1.1f); bloom.threshold.Override(.8f);
        string profilePath=Root+"/NightVolume.asset"; AssetDatabase.DeleteAsset(profilePath); AssetDatabase.CreateAsset(profile,profilePath); AssetDatabase.AddObjectToAsset(bloom,profile); volume.sharedProfile=profile;
        var player=new GameObject("Player Car"); var controller=player.AddComponent<PlayerCarController>(); var visual=new GameObject("Roadster Visual"); visual.transform.SetParent(player.transform,false);
        BuildRoadster(visual.transform,red,dark,white,tail);
        AddKenney(visual, "Assets/kenney_car-kit/Models/OBJ format/sedan-sports.obj", false, new Vector3(1.45f,1,1.45f), Vector3.zero);
        var glow=new GameObject("Neon Underglow").AddComponent<Light>(); glow.transform.SetParent(visual.transform,false); glow.transform.localPosition=new Vector3(0,.3f,0); glow.type=LightType.Point; glow.range=7; glow.color=Color.cyan;
        var exhaust=Particle("Exhaust",visual.transform,new Color(.5f,.55f,.65f,.5f),true,18,.8f,2); exhaust.transform.localPosition=new Vector3(0,.3f,-2); exhaust.transform.localRotation=Quaternion.Euler(0,180,0);
        Ref(controller,"visual",visual.transform); Ref(controller,"followCamera",camera.transform);
        var roadManager=new GameObject("Road and Object Pools").AddComponent<RoadManager>();
        Ref(roadManager,"roadPrefab",road); Ref(roadManager,"stationaryPrefab",stationary); Ref(roadManager,"movingPrefab",moving); Ref(roadManager,"coinPrefab",coin); Ref(roadManager,"fuelPrefab",fuel);
        var fx=new GameObject("Audio and Effects").AddComponent<GameEffects>(); var engine=fx.gameObject.AddComponent<AudioSource>(); var music=fx.gameObject.AddComponent<AudioSource>(); var sounds=fx.gameObject.AddComponent<AudioSource>();
        var sparks=Particle("Interaction Sparks",fx.transform,new Color(1,.5f,.1f),false,0,.7f,5); var fireworks=Particle("Fireworks",fx.transform,new Color(1,.68f,.12f),false,0,2,10);
        fireworks.GetComponent<ParticleSystemRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/FireworkParticles.mat");
        var fireworkSettings = fireworks.main; fireworkSettings.startSize=.75f; fireworkSettings.startLifetime=1.8f; fireworkSettings.startSpeed=8f;
        Ref(fx,"underglow",glow); Ref(fx,"engine",engine); Ref(fx,"music",music); Ref(fx,"effects",sounds); Ref(fx,"sparks",sparks); Ref(fx,"fireworks",fireworks);
        var manager=new GameObject("Game Manager").AddComponent<GameManager>(); var ui=BuildUI(manager);
        Ref(manager,"player",controller); Ref(manager,"road",roadManager); Ref(manager,"ui",ui); Ref(manager,"effects",fx);
        var es=new GameObject("EventSystem"); es.AddComponent<EventSystem>(); es.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        var settings=new EditorBuildSettingsScene("Assets/Scenes/NightDrive.unity",true); EditorSceneManager.SaveScene(scene,settings.path); EditorBuildSettings.scenes=new[]{settings};
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); Debug.Log("Night Drive scene and Inspector references created: "+settings.path);
    }
    static void Ref(Object target,string field,Object value) { var so=new SerializedObject(target); so.FindProperty(field).objectReferenceValue=value; so.ApplyModifiedPropertiesWithoutUndo(); }
    static Material Material(string name,Color color,bool emission=false)
    {
        string path=Root+"/Materials/"+name+".mat"; var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!mat) { mat=new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat,path); }
        mat.color=color; if(emission) { mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor",color*3); } EditorUtility.SetDirty(mat); return mat;
    }
    static GameObject Shape(string name,PrimitiveType type,Transform parent,Vector3 pos,Vector3 scale,Material mat)
    {
        var go=GameObject.CreatePrimitive(type); go.name=name; go.transform.SetParent(parent,false); go.transform.localPosition=pos; go.transform.localScale=scale; go.GetComponent<Renderer>().sharedMaterial=mat; Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
    }
    static GameObject SavePrefab(GameObject obj,string name) { var prefab=PrefabUtility.SaveAsPrefabAsset(obj,Root+"/Prefabs/"+name+".prefab"); Object.DestroyImmediate(obj); return prefab; }
    static GameObject BuildRoad(Material asphalt,Material stripe,Material edge)
    {
        var go=new GameObject("Road Segment"); Shape("Road",PrimitiveType.Cube,go.transform,new Vector3(0,-.15f,25),new Vector3(10,.3f,50),asphalt);
        for(int x=-1;x<=1;x+=2) { Shape("Edge",PrimitiveType.Cube,go.transform,new Vector3(x*4.95f,.02f,25),new Vector3(.12f,.04f,50),edge); for(int z=3;z<50;z+=8) Shape("Lane Marking",PrimitiveType.Cube,go.transform,new Vector3(x*1.5f,.02f,z),new Vector3(.08f,.04f,4),stripe); }
        AddStreetlights(go.transform, stripe);
        AddKenney(go, "Assets/kenney_city-kit-roads/Models/OBJ format/road-straight.obj", true, new Vector3(10,1,10), new Vector3(0,.025f,0));
        var roadVisual = new SerializedObject(go.GetComponent<KenneyVisual>());
        roadVisual.FindProperty("modelRotation").vector3Value = new Vector3(0,90,0);
        roadVisual.ApplyModifiedPropertiesWithoutUndo();
        var buildings = go.AddComponent<RoadsideBuildings>();
        var buildingData = new SerializedObject(buildings);
        var buildingRefs = buildingData.FindProperty("buildingModels"); buildingRefs.arraySize = 3;
        string commercial = "Assets/kenney_city-kit-commercial_2.1/Models/OBJ format/";
        buildingRefs.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(commercial + "building-a.obj");
        buildingRefs.GetArrayElementAtIndex(1).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(commercial + "building-c.obj");
        buildingRefs.GetArrayElementAtIndex(2).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(commercial + "building-g.obj");
        buildingData.FindProperty("groundMaterial").objectReferenceValue = Material("RoadsideGround", new Color(.12f,.15f,.17f));
        buildingData.ApplyModifiedPropertiesWithoutUndo();
        return SavePrefab(go,"RoadSegment");
    }
    static void AddKenney(GameObject target, string path, bool roadTiles, Vector3 scale, Vector3 offset)
    {
        var component = target.AddComponent<KenneyVisual>();
        var data = new SerializedObject(component);
        data.FindProperty("model").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        data.FindProperty("roadTiles").boolValue = roadTiles;
        data.FindProperty("modelScale").vector3Value = scale;
        data.FindProperty("modelOffset").vector3Value = offset;
        data.ApplyModifiedPropertiesWithoutUndo();
    }
    static void AddStreetlights(Transform road, Material lampMaterial)
    {
        for (int i = 0; i < 2; i++) {
            float side = i == 0 ? -1f : 1f;
            var lamp = new GameObject("Streetlight " + (i + 1)); lamp.transform.SetParent(road, false);
            lamp.transform.localPosition = new Vector3(side * 5.25f, 0, 12.5f + i * 25f);
            Shape("Pole", PrimitiveType.Cylinder, lamp.transform, new Vector3(0,3,0), new Vector3(.16f,3,.16f), lampMaterial);
            Shape("Arm", PrimitiveType.Cube, lamp.transform, new Vector3(-side*.75f,6,0), new Vector3(1.5f,.12f,.12f), lampMaterial);
            Shape("Lamp", PrimitiveType.Cube, lamp.transform, new Vector3(-side*1.4f,5.9f,0), new Vector3(.55f,.16f,.65f), lampMaterial);
            var light = new GameObject("Road Light").AddComponent<Light>(); light.transform.SetParent(lamp.transform, false);
            light.transform.localPosition = new Vector3(-side*1.35f,5.7f,0); light.type = LightType.Point;
            light.color = new Color(1f,.78f,.52f); light.range = 24f; light.intensity = 15f; light.shadows = LightShadows.None;
        }
    }
    [MenuItem("Night Drive/Add streetlights to road prefab")]
    public static void UpdateStreetlights()
    {
        const string path = Root + "/Prefabs/RoadSegment.prefab";
        var road = PrefabUtility.LoadPrefabContents(path);
        try {
            if (!road.transform.Find("Streetlight 1") && !road.transform.Find("Streetlight 1 Pole")) {
                AddStreetlights(road.transform, AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/LaneLight.mat"));
                PrefabUtility.SaveAsPrefabAsset(road, path);
            }
        } finally { PrefabUtility.UnloadPrefabContents(road); }
        AssetDatabase.SaveAssets();
        Debug.Log("Night Drive streetlights updated.");
    }
    static T BuildTraffic<T>(string name,Material paint,Material glass,Material lights) where T:Interactable
    {
        var go=new GameObject(name); go.AddComponent<T>(); Shape("Body",PrimitiveType.Cube,go.transform,new Vector3(0,.65f,0),new Vector3(1.8f,.65f,3.5f),paint);
        Shape("Cabin",PrimitiveType.Cube,go.transform,new Vector3(0,1.1f,0),new Vector3(1.4f,.5f,1.7f),glass); Shape("Tail Lights",PrimitiveType.Cube,go.transform,new Vector3(0,.7f,-1.77f),new Vector3(1.5f,.12f,.05f),lights);
        AddKenney(go, "Assets/kenney_car-kit/Models/OBJ format/sedan.obj", false, new Vector3(1.2f,1,1.35f), Vector3.zero);
        return SavePrefab(go,name).GetComponent<T>();
    }
    static T BuildPickup<T>(string name,Material mat,bool coin) where T:Interactable
    {
        var go=new GameObject(name); go.AddComponent<T>(); var mesh=Shape("Visual",coin?PrimitiveType.Cylinder:PrimitiveType.Cube,go.transform,new Vector3(0,1,0),coin?new Vector3(.8f,.12f,.8f):new Vector3(.7f,1,.45f),mat); if(coin) mesh.transform.localRotation=Quaternion.Euler(90,0,0);
        return SavePrefab(go,name).GetComponent<T>();
    }
    static void BuildRoadster(Transform car,Material paint,Material dark,Material white,Material tail)
    {
        Shape("Lower Body",PrimitiveType.Cube,car,new Vector3(0,.36f,-.1f),new Vector3(1.72f,.32f,3.75f),paint);
        var hood=Shape("Hood",PrimitiveType.Cube,car,new Vector3(0,.53f,1.05f),new Vector3(1.64f,.16f,1.7f),paint); hood.transform.localRotation=Quaternion.Euler(5,0,0);
        Shape("Rear Deck",PrimitiveType.Cube,car,new Vector3(0,.55f,-1.35f),new Vector3(1.68f,.18f,1.05f),paint);
        var canopy=Shape("Glass Canopy",PrimitiveType.Cube,car,new Vector3(0,.83f,-.25f),new Vector3(1.3f,.45f,1.7f),dark); canopy.transform.localRotation=Quaternion.Euler(-5,0,0);
        Shape("Tail Light",PrimitiveType.Cube,car,new Vector3(0,.52f,-1.99f),new Vector3(1.45f,.07f,.06f),tail);
        for(int x=-1;x<=1;x+=2) {
            Shape("Headlight",PrimitiveType.Cube,car,new Vector3(x*.57f,.5f,1.83f),new Vector3(.46f,.07f,.08f),white);
            var light=new GameObject("Headlight Beam").AddComponent<Light>(); light.transform.SetParent(car,false); light.transform.localPosition=new Vector3(x*.65f,.5f,1.9f); light.type=LightType.Spot; light.range=32; light.spotAngle=55; light.intensity=8;
            for(int z=-1;z<=1;z+=2) { var wheel=Shape("Wheel",PrimitiveType.Cylinder,car,new Vector3(x*.88f,.35f,z*1.25f),new Vector3(.68f,.16f,.68f),dark); wheel.transform.localRotation=Quaternion.Euler(0,0,90); }
        }
    }
    static ParticleSystem Particle(string name,Transform parent,Color color,bool loop,float rate,float life,float speed)
    {
        var go=new GameObject(name); go.transform.SetParent(parent,false); var ps=go.AddComponent<ParticleSystem>(); var main=ps.main; main.loop=loop; main.playOnAwake=loop; main.startColor=color; main.startLifetime=life; main.startSpeed=speed; main.startSize=.22f; main.simulationSpace=ParticleSystemSimulationSpace.World;
        var emission=ps.emission; emission.rateOverTime=rate; var shape=ps.shape; shape.shapeType=ParticleSystemShapeType.Sphere; shape.radius=.3f; return ps;
    }
    static GameUI BuildUI(GameManager manager)
    {
        var canvas=new GameObject("UI",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)); canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1920,1080);
        var ui=canvas.AddComponent<GameUI>(); var hud=Panel("HUD",canvas.transform,Color.clear); var stats=Label("Stats",hud.transform,"",30,new Vector2(30,-25),new Vector2(800,160),false);
        var back=UIRect("Fuel Bar",hud.transform,new Vector2(30,-160),new Vector2(320,20),false).gameObject.AddComponent<Image>(); back.color=new Color(.1f,.14f,.22f,.9f);
        var fill=UIRect("Fuel Fill",back.transform,Vector2.zero,new Vector2(320,20),false).gameObject.AddComponent<Image>(); fill.color=new Color(.05f,.9f,.7f); fill.type=Image.Type.Filled; fill.fillMethod=Image.FillMethod.Horizontal;
        var overlay=Panel("Overlay",canvas.transform,new Color(.01f,.02f,.06f,.86f)); var menu=Panel("Main Menu",overlay.transform,Color.clear); var pause=Panel("Pause Menu",overlay.transform,Color.clear); var over=Panel("Game Over",overlay.transform,Color.clear);
        Label("Title",menu.transform,"NIGHT DRIVE",86,new Vector2(0,170),new Vector2(1100,120)); Label("Subtitle",menu.transform,"THREE LANES / ONE ENDLESS NIGHT",25,new Vector2(0,75),new Vector2(1100,50));
        Button("START ENGINE",menu.transform,new Vector2(0,-30),manager.StartRun); Button("EXIT",menu.transform,new Vector2(0,-110),manager.Quit);
        Label("Controls",menu.transform,"A / D or arrows: steer   |   Space: jump   |   Esc: pause",22,new Vector2(0,-205),new Vector2(1100,50));
        Label("Title",pause.transform,"PAUSED",70,new Vector2(0,150),new Vector2(800,100)); Button("RESUME",pause.transform,new Vector2(0,35),manager.Resume); Button("RESTART",pause.transform,new Vector2(0,-45),manager.StartRun); Button("MAIN MENU",pause.transform,new Vector2(0,-125),manager.ReturnToMenu);
        Label("Title",over.transform,"RUN COMPLETE",70,new Vector2(0,170),new Vector2(900,100)); var score=Label("Score",over.transform,"",30,new Vector2(0,75),new Vector2(800,80)); Button("DRIVE AGAIN",over.transform,new Vector2(0,-35),manager.StartRun); Button("MAIN MENU",over.transform,new Vector2(0,-115),manager.ReturnToMenu);
        Ref(ui,"hud",hud); Ref(ui,"overlay",overlay); Ref(ui,"menu",menu); Ref(ui,"pause",pause); Ref(ui,"gameOver",over); Ref(ui,"stats",stats); Ref(ui,"finalScore",score); Ref(ui,"fuelFill",fill);
        return ui;
    }
    static RectTransform UIRect(string name,Transform parent,Vector2 pos,Vector2 size,bool center=true)
    {
        var go=new GameObject(name,typeof(RectTransform)); var rect=go.GetComponent<RectTransform>(); rect.SetParent(parent,false); rect.anchorMin=rect.anchorMax=center?new Vector2(.5f,.5f):new Vector2(0,1); rect.pivot=center?new Vector2(.5f,.5f):new Vector2(0,1); rect.anchoredPosition=pos; rect.sizeDelta=size; return rect;
    }
    static GameObject Panel(string name,Transform parent,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Image)); var rect=go.GetComponent<RectTransform>(); rect.SetParent(parent,false); rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one; rect.offsetMin=rect.offsetMax=Vector2.zero; go.GetComponent<Image>().color=color; return go;
    }
    static Text Label(string name,Transform parent,string value,int size,Vector2 pos,Vector2 bounds,bool center=true)
    {
        var text=UIRect(name,parent,pos,bounds,center).gameObject.AddComponent<Text>(); text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.text=value; text.fontSize=size; text.alignment=center?TextAnchor.MiddleCenter:TextAnchor.UpperLeft; text.color=Color.white; text.raycastTarget=false; return text;
    }
    static void Button(string label,Transform parent,Vector2 pos,UnityEngine.Events.UnityAction action)
    {
        var go=UIRect(label,parent,pos,new Vector2(330,60)).gameObject; var image=go.AddComponent<Image>(); image.color=new Color(.06f,.42f,.5f,.95f);
        var button=go.AddComponent<Button>(); button.targetGraphic=image; UnityEventTools.AddPersistentListener(button.onClick,action); Label("Label",go.transform,label,24,Vector2.zero,new Vector2(330,60));
    }
}
