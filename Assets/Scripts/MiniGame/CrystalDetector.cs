using System;
using UnityEngine;

public class CrystalDetector : MonoBehaviour
{
    public static event Action OnPlaced;

    public static bool IsPlaced { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        IsPlaced = false;
        OnPlaced = null;
    }

    private void Start()
    {
        if (IsPlaced)
        {
            return;
        }

        IsPlaced = true;
        OnPlaced?.Invoke();
    }
}