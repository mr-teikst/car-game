using UnityEngine;
public class Coin : Interactable { public override void Interact(GameManager game) { game.CollectCoin(transform.position); } }
