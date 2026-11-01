using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] GameObject hud, overlay, menu, pause, gameOver;
    [SerializeField] Text stats, finalScore;
    [SerializeField] Image fuelFill;
    public void Show(GameManager.GameState state)
    {
        hud.SetActive(state == GameManager.GameState.Playing || state == GameManager.GameState.Paused);
        overlay.SetActive(state != GameManager.GameState.Playing);
        menu.SetActive(state == GameManager.GameState.Menu); pause.SetActive(state == GameManager.GameState.Paused); gameOver.SetActive(state == GameManager.GameState.GameOver);
    }
    public void Refresh(GameManager game)
    {
        stats.text = $"SPEED  {game.Speed*3.6f:0} KM/H     LIVES  {game.Lives}\nSCORE  {game.Score:N0}     BEST  {Mathf.Max(game.HighScore,game.Score):N0}\nFUEL  {game.Fuel:0}%";
        fuelFill.fillAmount = game.Fuel / game.MaxFuel;
        finalScore.text = $"SCORE  {game.Score:N0}     BEST  {game.HighScore:N0}";
    }
}
