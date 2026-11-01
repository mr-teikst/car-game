using UnityEngine;

public class KenneyVisual : MonoBehaviour
{
    [SerializeField] GameObject model;
    [SerializeField] bool roadTiles;
    [SerializeField] Vector3 modelScale = Vector3.one;
    [SerializeField] Vector3 modelOffset;
    [SerializeField] Vector3 modelRotation;
    [SerializeField] int tileCount = 5;
    [SerializeField] float tileSpacing = 10f;

    void Awake()
    {
        if (!roadTiles) IlluminateFallback();
        if (!model) { Debug.LogWarning("Kenney OBJ model is not assigned; using the scene geometry.", this); return; }
        if (roadTiles) {
            for (int i = 0; i < tileCount; i++) CreateModel(modelOffset + Vector3.forward * ((i + .5f) * tileSpacing));
        } else CreateModel(modelOffset);
    }

    void IlluminateFallback()
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>()) {
            foreach (var material in renderer.sharedMaterials) {
                if (!material || !material.HasProperty("_EmissionColor")) continue;
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", material.color * .35f);
            }
        }
    }

    void CreateModel(Vector3 position)
    {
        GameObject instance;
        try { instance = Instantiate(model, transform); }
        catch (System.InvalidCastException) {
            Debug.LogWarning("Invalid Kenney model reference; keeping fallback geometry.", this);
            return;
        }
        instance.name = "Kenney " + model.name;
        instance.transform.localPosition = position;
        instance.transform.localRotation = Quaternion.Euler(modelRotation);
        instance.transform.localScale = modelScale;
        foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true)) {
            renderer.gameObject.SetActive(true);
            renderer.enabled = true;
        }
    }
}
