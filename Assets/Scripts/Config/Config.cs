using UnityEngine;

public class Config : MonoBehaviour
{
    public Transform MainPlayer;
    public Rigidbody2D PlayerRigidbody2D;
    public Transform Camera;
    public float CameraMultiple = 0.1f;
    public float followSpeed = 5f;
    public LayerMask GroundLayer;
}