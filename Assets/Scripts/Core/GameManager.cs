using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public enum GameState { Menu, Playing, Paused, GameOver }
    public static GameManager Instance { get; private set; }
    [Header("Scene references")]
    [SerializeField] PlayerCarController player;
    [SerializeField] RoadManager road;
    [SerializeField] GameUI ui;
    [SerializeField] GameEffects effects;
    [Header("Run settings")]
    [SerializeField] float initialSpeed = 20f, maxSpeed = 36f, acceleration = .08f;
    [SerializeField] float maxFuel = 100f, fuelPerSecond = 2f, fuelPerCan = 25f;
    [SerializeField] int startingLives = 3, coinValue = 50;
    public GameState State { get; private set; }
    public float Speed { get; private set; }
    public float Fuel { get; private set; }
    public float MaxFuel => maxFuel;
    public int Lives { get; private set; }
    public float Distance { get; private set; }
    public float Elapsed { get; private set; }
    public int Score => Mathf.FloorToInt(Distance) + coins * coinValue;
    public int HighScore { get; private set; }
    int coins;
    float damageCooldown;
    InputActionAsset input;
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this; Time.timeScale = 1;
        if (!player || !road || !ui || !effects) { Debug.LogError("GameManager: assign all scene references in the Inspector.", this); enabled = false; return; }
        HighScore = PlayerPrefs.GetInt("NightDrive.HighScore", 0);
        input = ScriptableObject.CreateInstance<InputActionAsset>();
        var map = new InputActionMap("Gameplay");
        var left = map.AddAction("Left", InputActionType.Button); left.AddBinding("<Keyboard>/a"); left.AddBinding("<Keyboard>/leftArrow");
        var right = map.AddAction("Right", InputActionType.Button); right.AddBinding("<Keyboard>/d"); right.AddBinding("<Keyboard>/rightArrow");
        var jump = map.AddAction("Jump", InputActionType.Button, "<Keyboard>/space");
        var pause = map.AddAction("Pause", InputActionType.Button, "<Keyboard>/escape");
        left.performed += _ => { if (State == GameState.Playing) player.ChangeLane(-1); };
        right.performed += _ => { if (State == GameState.Playing) player.ChangeLane(1); };
        jump.performed += _ => { if (State == GameState.Playing) player.Jump(); };
        pause.performed += _ => { if (State == GameState.Playing) Pause(); else if (State == GameState.Paused) Resume(); };
        input.AddActionMap(map); input.Enable();
        road.Initialize(); effects.Initialize(); ReturnToMenu();
    }
    void Update()
    {
        if (State != GameState.Playing) return;
        float dt = Time.deltaTime; Elapsed += dt; damageCooldown = Mathf.Max(0, damageCooldown - dt);
        Speed = Mathf.Min(maxSpeed, initialSpeed + Elapsed * acceleration);
        Distance += Speed * dt; Fuel = Mathf.Max(0, Fuel - fuelPerSecond * dt);
        if (Fuel <= 0) { EndRun(); return; }
        player.Tick(dt, Distance); road.Tick(dt, player, Distance, Elapsed); effects.Tick(dt, player.transform, Speed); ui.Refresh(this);
    }
    public void StartRun()
    {
        Time.timeScale = 1; Distance = Elapsed = damageCooldown = 0; coins = 0; Lives = startingLives; Fuel = maxFuel; Speed = initialSpeed;
        player.ResetRun(); road.ResetRun(); State = GameState.Playing; effects.StartRun(); ui.Show(State); ui.Refresh(this);
    }
    public void Pause() { State = GameState.Paused; Time.timeScale = 0; effects.PauseEngine(); ui.Show(State); }
    public void Resume() { State = GameState.Playing; Time.timeScale = 1; effects.ResumeEngine(); ui.Show(State); }
    public void ReturnToMenu() { Time.timeScale = 1; State = GameState.Menu; effects.StopEngine(); ui.Show(State); }
    void EndRun()
    {
        State = GameState.GameOver; effects.StopEngine();
        if (Score > HighScore) { HighScore = Score; PlayerPrefs.SetInt("NightDrive.HighScore", HighScore); PlayerPrefs.Save(); }
        ui.Refresh(this); ui.Show(State);
    }
    public void Damage(Vector3 position)
    {
        if (State != GameState.Playing || damageCooldown > 0) return;
        Lives--; damageCooldown = 1; player.PlayCrashFeedback(); effects.PlayHit(position); if (Lives <= 0) EndRun();
    }
    public void CollectCoin(Vector3 position) { coins++; effects.PlayCoin(position); }
    public void CollectFuel(Vector3 position) { Fuel = Mathf.Min(maxFuel, Fuel + fuelPerCan); effects.PlayFuel(position); }
    public void Quit() { Application.Quit(); }
    void OnDestroy() { if (Instance == this) { Instance = null; Time.timeScale = 1; if (input) { input.Disable(); Destroy(input); } } }
}
