using UnityEngine;

public class GameSettings : MonoBehaviour
{
    [SerializeField] private int targetFPS = 60;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFPS;

        QualitySettings.antiAliasing = 0;
    }
}
