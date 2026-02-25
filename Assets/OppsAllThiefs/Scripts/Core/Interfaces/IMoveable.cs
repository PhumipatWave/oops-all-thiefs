using UnityEngine;

public interface IMoveable
{
    public void Move(Vector2 dir);
    public void MoveRotator(Vector2 dir, Transform transform);
    public void Jump();
}
