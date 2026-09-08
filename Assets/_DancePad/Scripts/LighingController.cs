using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LighingController : MonoBehaviour
{
    [SerializeField] private RelayController _controller;
    [SerializeField] private float _offDelay = 0.1f;

    private readonly Dictionary<int, Coroutine> _offCoroutines = new Dictionary<int, Coroutine>();

    private void OnEnable()
    {
        GameEvents.OnNoteHitTimeReached += OnPadDirectionPressed;
    }

    private void OnDisable()
    {
        GameEvents.OnNoteHitTimeReached -= OnPadDirectionPressed;

        foreach (var coroutine in _offCoroutines.Values)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        _offCoroutines.Clear();

        // Выключаем все реле, чтобы не оставить их включенными
        for (int i = 1; i <= 8; i++)
        {
            _controller.SetRelay(i, false);
        }
    }

    private void OnPadDirectionPressed(DancePadDirection direction)
    {
        int relayIndex = GetRelayIndex(direction);
        if (relayIndex == -1)
            return;

        // Включаем реле
        _controller.SetRelay(relayIndex, true);

        // Если для этого реле уже была запущена корутина, отменяем её
        if (_offCoroutines.TryGetValue(relayIndex, out var existingCoroutine))
        {
            if (existingCoroutine != null)
                StopCoroutine(existingCoroutine);
        }

        // Запускаем новую корутину выключения
        Coroutine newCoroutine = StartCoroutine(DelayedOff(relayIndex));
        _offCoroutines[relayIndex] = newCoroutine;
    }

    private IEnumerator DelayedOff(int relayIndex)
    {
        yield return new WaitForSeconds(_offDelay);
        _controller.SetRelay(relayIndex, false);
        _offCoroutines.Remove(relayIndex);
    }

    private int GetRelayIndex(DancePadDirection direction)
    {
        switch (direction)
        {
            case DancePadDirection.Up: return 1;
            case DancePadDirection.Down: return 2;
            case DancePadDirection.Left: return 3;
            case DancePadDirection.Right: return 4;
            case DancePadDirection.UpLeft: return 5;
            case DancePadDirection.UpRight: return 6;
            case DancePadDirection.DownLeft: return 7;
            case DancePadDirection.DownRight: return 8;
            default: return -1;
        }
    }

    //private void OnPadDirectionPressedDebug(DancePadDirection direction)
    //{
    //    switch (direction)
    //    {
    //        case DancePadDirection.Up: Debug.Log("1"); break; //w
    //        case DancePadDirection.Down: Debug.Log("2"); break; //s
    //        case DancePadDirection.Left: Debug.Log("3"); break; //a
    //        case DancePadDirection.Right: Debug.Log("4"); break; //d
    //        case DancePadDirection.UpLeft: Debug.Log("5"); break; //q
    //        case DancePadDirection.UpRight: Debug.Log("6"); break; //e
    //        case DancePadDirection.DownLeft: Debug.Log("7"); break; //z
    //        case DancePadDirection.DownRight: Debug.Log("8"); break; //c
    //        default: break;
    //    }
    //}
}
