using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public virtual void Spawn(Vector3 position) { transform.position = position; gameObject.SetActive(true); }
    public virtual void Tick(float deltaTime) { }
    public abstract void Interact(GameManager game);
}
