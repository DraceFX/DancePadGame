using UnityEngine;

public class StartEffects : MonoBehaviour
{
    [SerializeField] private ParticleSystem particle;

    private void OnEnable()
    {
        GameEvents.OnStartPlay += OnPlayStart;
        GameEvents.OnMusicFinished += OnStopEffects;
    }

    private void OnDisable()
    {
        GameEvents.OnStartPlay -= OnPlayStart;
        GameEvents.OnMusicFinished -= OnStopEffects;
    }

    private void OnPlayStart()
    {
        particle.Play();
    }

    private void OnStopEffects()
    {
        particle.Stop();
    }
}
