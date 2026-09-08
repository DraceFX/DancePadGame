using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UISpriteAnimation : MonoBehaviour
{
    [SerializeField] private Image image;

    [SerializeField] private Sprite[] spriteArray;
    [SerializeField] private float speed = 0.02f;
    [SerializeField] private bool isNeedReverse = false;
    [SerializeField] private bool isPingPong = false;

    private int indexSprite;
    private int direction;
    private Coroutine coroutineAnim;
    private bool isPlaying;

    private void Start()
    {
        PlayUIAnimation();
    }

    public void PlayUIAnimation()
    {
        if (coroutineAnim != null)
        {
            StopCoroutine(coroutineAnim);
            coroutineAnim = null;
        }

        if (spriteArray == null || spriteArray.Length == 0)
        {
            Debug.LogWarning("Sprite array is empty!");
            return;
        }

        // Начальные параметры
        direction = isNeedReverse ? -1 : 1;
        indexSprite = isNeedReverse ? spriteArray.Length - 1 : 0;

        isPlaying = true;
        coroutineAnim = StartCoroutine(AnimationLoop());
    }

    public void StopUIAnimation()
    {
        isPlaying = true;
        if (coroutineAnim != null)
        {
            StopCoroutine(coroutineAnim);
            coroutineAnim = null;
        }
    }

    private IEnumerator AnimationLoop()
    {
        while (isPlaying)
        {
            image.sprite = spriteArray[indexSprite];
            yield return new WaitForSeconds(speed);

            indexSprite += direction;
            if (indexSprite >= spriteArray.Length)
            {
                if (isPingPong)
                {
                    direction = -1;
                    indexSprite = spriteArray.Length - 2;
                }
                else
                {
                    indexSprite = 0;
                }
            }
            else if (indexSprite < 0)
            {
                if (isPingPong)
                {
                    direction = 1;
                    indexSprite = 1;
                }
                else
                {
                    indexSprite = spriteArray.Length - 1;
                }
            }
        }
    }
}
