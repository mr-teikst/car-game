using UnityEngine;

public class PlayerCarController : MonoBehaviour
{
    [Header("Driving")]
    [SerializeField] float laneWidth = 3f, laneChangeSpeed = 12f;
    [SerializeField] float jumpSpeed = 15f, jumpGravity = 25f;
    [SerializeField] Transform visual;
    [SerializeField] Transform followCamera;
    [SerializeField] Vector3 cameraOffset = new Vector3(0, 6, -11);
    [Header("Crash feedback")]
    [SerializeField] float shakeDuration = .45f, shakeStrength = .28f;
    [SerializeField] float blinkDuration = .9f, blinkInterval = .12f;

    int lane = 1;
    float height, velocity, jumpElapsed, shakeRemaining, blinkRemaining;
    Renderer[] carRenderers;
    bool[] rendererStates;
    Light[] carLights;
    bool[] lightStates;
    public float JumpHeight => height;

    public void ChangeLane(int direction) { lane = Mathf.Clamp(lane + direction, 0, 2); }
    public void Jump() { if (height <= 0 && velocity <= 0) velocity = jumpSpeed; }
    public void ResetRun()
    {
        RestoreVisuals(); shakeRemaining = blinkRemaining = 0;
        lane = 1; height = velocity = jumpElapsed = 0;
        transform.position = Vector3.zero;
        if (visual) visual.localRotation = Quaternion.identity;
    }
    public void Tick(float dt, float distance)
    {
        if (height > 0 || velocity > 0) {
            jumpElapsed += dt; velocity -= jumpGravity * dt; height += velocity * dt;
            if (visual) visual.localRotation = Quaternion.Euler(0, 0, Mathf.Clamp01(jumpElapsed / (2 * jumpSpeed / jumpGravity)) * 360);
            if (height <= 0) { height = velocity = jumpElapsed = 0; if (visual) visual.localRotation = Quaternion.identity; }
        }
        transform.position = new Vector3(Mathf.MoveTowards(transform.position.x, (lane - 1) * laneWidth, laneChangeSpeed * dt), height, distance);
    }
    public void PlayCrashFeedback()
    {
        shakeRemaining = shakeDuration;
        blinkRemaining = blinkDuration;
        if (!visual) return;
        carRenderers = visual.GetComponentsInChildren<Renderer>(true);
        rendererStates = new bool[carRenderers.Length];
        for (int i = 0; i < carRenderers.Length; i++) rendererStates[i] = carRenderers[i].enabled;
        carLights = visual.GetComponentsInChildren<Light>(true);
        lightStates = new bool[carLights.Length];
        for (int i = 0; i < carLights.Length; i++) lightStates[i] = carLights[i].enabled;
        SetVisualsVisible(false);
    }
    void Update()
    {
        if (shakeRemaining > 0) shakeRemaining = Mathf.Max(0, shakeRemaining - Time.deltaTime);
        if (blinkRemaining <= 0) return;
        blinkRemaining = Mathf.Max(0, blinkRemaining - Time.deltaTime);
        if (blinkRemaining == 0) { RestoreVisuals(); return; }
        int phase = Mathf.FloorToInt((blinkDuration - blinkRemaining) / Mathf.Max(.01f, blinkInterval));
        SetVisualsVisible((phase & 1) == 1);
    }
    void SetVisualsVisible(bool visible)
    {
        if (carRenderers != null) for (int i = 0; i < carRenderers.Length; i++)
            if (carRenderers[i]) carRenderers[i].enabled = visible && rendererStates[i];
        if (carLights != null) for (int i = 0; i < carLights.Length; i++)
            if (carLights[i]) carLights[i].enabled = visible && lightStates[i];
    }
    void RestoreVisuals()
    {
        SetVisualsVisible(true);
        carRenderers = null; rendererStates = null;
        carLights = null; lightStates = null;
    }
    void LateUpdate()
    {
        if (!followCamera) return;
        followCamera.position = transform.position + cameraOffset + Vector3.down * height * .65f;
        followCamera.LookAt(transform.position + new Vector3(0, 1, 7));
        if (shakeRemaining > 0 && shakeDuration > 0) {
            Vector2 shake = Random.insideUnitCircle * (shakeStrength * shakeRemaining / shakeDuration);
            followCamera.position += followCamera.right * shake.x + followCamera.up * shake.y;
        }
    }
    void OnDisable() { RestoreVisuals(); }
}
