using UnityEngine;
public class StationaryObstacle : Interactable { public override void Interact(GameManager game) { game.Damage(transform.position); } }
