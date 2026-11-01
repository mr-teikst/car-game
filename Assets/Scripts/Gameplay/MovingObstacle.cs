using UnityEngine;
public class MovingObstacle : Interactable
{
    [SerializeField] float speed = 8f;
    public override void Tick(float deltaTime) { transform.position += Vector3.forward * (speed * deltaTime); }
    public override void Interact(GameManager game) { game.Damage(transform.position); }
}
