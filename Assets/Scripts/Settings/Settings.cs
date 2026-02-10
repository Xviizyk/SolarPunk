using UnityEngine;

public enum Graphycs
{
    Very_slow,
    Slow,
    Normal,
    Fast,
    Very_fast
}

public enum DeviceType
{
    Mobile,
    PC,
    Console
}

public class Settings : MonoBehaviour
{
    [Header("Graphycs Settings")]
    public Graphycs Graphycs;

    [Header("Device info")]
    public DeviceType Type;

    [Header("Movement settings")]
    public bool IsSprintTogglable;
}