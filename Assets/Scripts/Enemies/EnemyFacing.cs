using UnityEngine;

public class EnemyFacing : MonoBehaviour
{
    public void FaceLeft() => transform.rotation = Quaternion.Euler(0f, 180f, 0f);
    public void FaceRight() => transform.rotation = Quaternion.Euler(0f, 0f, 0f);
}
