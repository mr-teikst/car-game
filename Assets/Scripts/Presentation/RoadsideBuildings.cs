using UnityEngine;

public class RoadsideBuildings : MonoBehaviour
{
    [SerializeField] GameObject[] buildingModels;
    [SerializeField] float distanceFromRoad = 10.5f;
    [SerializeField] float buildingScale = 8f;
    [SerializeField] float groundLevel = 0f;
    [SerializeField] Material groundMaterial;
    [SerializeField] float groundWidth = 23f;
    static readonly float[] Positions = { 8f, 25f, 42f };

    void Awake()
    {
        for (int side = -1; side <= 1; side += 2) {
            CreateGround(side);
            for (int i = 0; i < Positions.Length; i++) {
                var model = buildingModels != null && buildingModels.Length > 0
                    ? buildingModels[(i + (side == 1 ? 1 : 0)) % buildingModels.Length] : null;
                var position = new Vector3(side * distanceFromRoad, groundLevel, Positions[i]);
                bool placed = false;
                if (model) {
                    try {
                        var house = Instantiate(model, transform);
                        house.name = "Roadside building " + (side == -1 ? "left " : "right ") + (i + 1);
                        house.transform.localPosition = position;
                        house.transform.localRotation = Quaternion.Euler(0, side == -1 ? 90 : -90, 0);
                        house.transform.localScale = Vector3.one * buildingScale;
                        placed = true;
                    } catch (System.InvalidCastException) {
                        Debug.LogWarning("Invalid imported building reference; using fallback.", this);
                    }
                }
                if (!placed) {
                    var house = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    house.name = "Roadside building fallback";
                    house.transform.SetParent(transform, false);
                    house.transform.localPosition = position + Vector3.up * buildingScale * .45f;
                    house.transform.localScale = new Vector3(buildingScale * .8f, buildingScale * .9f, buildingScale * .8f);
                    Destroy(house.GetComponent<Collider>());
                }
            }
        }
    }
    void CreateGround(int side)
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = side < 0 ? "Ground under left buildings" : "Ground under right buildings";
        ground.transform.SetParent(transform, false);
        ground.transform.localPosition = new Vector3(side * (5f + groundWidth * .5f), groundLevel - .16f, 25f);
        ground.transform.localScale = new Vector3(groundWidth, .3f, 50f);
        if (groundMaterial) ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;
        Destroy(ground.GetComponent<Collider>());
    }

}
