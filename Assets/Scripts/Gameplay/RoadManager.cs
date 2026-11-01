using System.Collections.Generic;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    [Header("Prefab references")]
    [SerializeField] GameObject roadPrefab;
    [SerializeField] StationaryObstacle stationaryPrefab;
    [SerializeField] MovingObstacle movingPrefab;
    [SerializeField] Coin coinPrefab;
    [SerializeField] FuelCan fuelPrefab;
    [Header("Pool sizes and spacing")]
    [SerializeField] int roadCount = 6, stationaryCount = 32, movingCount = 32, coinCount = 48, fuelCount = 16;
    [SerializeField] float segmentLength = 50f, laneWidth = 3f;
    readonly List<GameObject> segments = new List<GameObject>();
    readonly List<Interactable> items = new List<Interactable>();
    float nextRoad, nextRow;
    public void Initialize()
    {
        if (!roadPrefab || !stationaryPrefab || !movingPrefab || !coinPrefab || !fuelPrefab) { Debug.LogError("RoadManager: assign all pool prefabs.", this); enabled = false; return; }
        for (int i = 0; i < roadCount; i++) segments.Add(Instantiate(roadPrefab, transform));
        Prewarm(stationaryPrefab, stationaryCount); Prewarm(movingPrefab, movingCount); Prewarm(coinPrefab, coinCount); Prewarm(fuelPrefab, fuelCount);
    }
    void Prewarm<T>(T prefab, int count) where T : Interactable
    {
        for (int i=0; i<count; i++) { var item = Instantiate(prefab, transform); item.gameObject.SetActive(false); items.Add(item); }
    }
    public void ResetRun()
    {
        nextRoad = 0; nextRow = 65;
        foreach (var item in items) item.gameObject.SetActive(false);
        foreach (var segment in segments) { segment.transform.position = new Vector3(0,0,nextRoad); nextRoad += segmentLength; }
    }
    void Spawn<T>(int lane, float z) where T : Interactable
    {
        foreach (var item in items) if (item is T && !item.gameObject.activeSelf) { item.Spawn(new Vector3((lane-1)*laneWidth, 0, z)); return; }
    }
    void PopulateRow(float z, float elapsed)
    {
        int safe = Random.Range(0,3), obstacle = (safe + Random.Range(1,3)) % 3;
        if (Random.value < .5f) Spawn<MovingObstacle>(obstacle,z); else Spawn<StationaryObstacle>(obstacle,z);
        if (Random.value < Mathf.Clamp01(elapsed/150f)*.65f) Spawn<StationaryObstacle>(3-safe-obstacle,z);
        if (Random.value < .35f) Spawn<FuelCan>(safe,z); else for (int i=0;i<3;i++) Spawn<Coin>(safe,z+i*3);
    }
    public void Tick(float dt, PlayerCarController player, float distance, float elapsed)
    {
        foreach (var segment in segments) if (segment.transform.position.z+segmentLength < distance-25) { segment.transform.position = new Vector3(0,0,nextRoad); nextRoad += segmentLength; }
        while (nextRow < distance+180) { PopulateRow(nextRow, elapsed); nextRow += Mathf.Lerp(38,23,Mathf.Clamp01(elapsed/150)); }
        foreach (var item in items) if (item.gameObject.activeSelf) {
            item.Tick(dt); var delta = item.transform.position - player.transform.position;
            if (delta.z < -12) item.gameObject.SetActive(false);
            else { bool pickup = item is Coin || item is FuelCan;
                if (player.JumpHeight < (pickup ? 1.2f : 1.45f) && Mathf.Abs(delta.x)<1.3f && Mathf.Abs(delta.z)<(pickup ? 1.8f : 3f)) { item.Interact(GameManager.Instance); item.gameObject.SetActive(false); }
            }
        }
    }
}
