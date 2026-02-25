using UnityEngine;

public interface IMoveable
{
    public void Move(Vector2 dir);
    public void MoveRotator();
    public void Jump();
}
