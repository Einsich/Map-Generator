using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelMover : MonoBehaviour
{
    [Header("Перемещает в эту точку при вызове Show()")]
    [SerializeField] public Vector2 OnPosition;
    [Header("Перемещает в эту точку при вызове Hide()")]
    [SerializeField] public Vector2 OffPosition;
    private Vector2 Target;
    private RectTransform Rect;
    private bool Move = false, On = false;
    private Coroutine Coroutine;
    public bool isOn => On;
    void Start()
    {
        Rect = GetComponent<RectTransform>();
        Rect.anchoredPosition = OffPosition;
    }

    public void Show(bool lerped = true)
    {
        Move = lerped;
        On = true;
        Target = OnPosition;
        gameObject.SetActive(true);
        CoroutineAnalyze();
    }
    public void Hide(bool lerped = true)
    {
        Move = lerped;
        On = false;
        Target = OffPosition;
        CoroutineAnalyze();
    }
    private void CoroutineAnalyze()
    {
        if (Move)
        {
            if (Coroutine == null)
                Coroutine = StartCoroutine(Translate());
        } else
        {
            if(Coroutine != null)
            {
                StopCoroutine(Coroutine);
                Coroutine = null;
            }
            Rect.anchoredPosition = Target;
        }
        
    }
    private IEnumerator Translate()
    {
        while(true)
        {
            Rect.anchoredPosition = Vector2.Lerp(Rect.anchoredPosition, Target, 0.1f);
            if(Vector2.Distance(Rect.anchoredPosition,Target) < 0.1)
            {
                Rect.anchoredPosition = Target;
                Move = false;
                if (!On)
                    gameObject.SetActive(false);
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        Coroutine = null;
    }
}
