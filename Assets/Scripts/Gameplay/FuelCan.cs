using UnityEngine;
public class FuelCan : Interactable { public override void Interact(GameManager game) { game.CollectFuel(transform.position); } }
