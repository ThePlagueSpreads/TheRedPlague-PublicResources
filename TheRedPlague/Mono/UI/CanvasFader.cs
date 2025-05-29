using UnityEngine;

namespace TheRedPlague.Mono.UI;

public class CanvasFader : MonoBehaviour
{
    public float defaultValue;
    public float speed = 1f;
    public CanvasGroup group;

    private float _target;
    private bool _wasSet;

    private void Start()
    {
        if (_wasSet)
            return;
        
        _target = defaultValue;

        if (Mathf.Approximately(group.alpha, _target))
        {
            enabled = false;
            if (Mathf.Approximately(group.alpha, 0))
                gameObject.SetActive(false);
        }
    }

    public void SetAlpha(float alpha, bool instant)
    {
        _wasSet = true;
        
        _target = alpha;
        
        if (instant)
        {
            group.alpha = alpha;
        }

        enabled = !instant;
        gameObject.SetActive(true);
    }
    
    private void Update()
    {
        group.alpha = Mathf.MoveTowards(group.alpha, _target, speed * Time.deltaTime);
        
        if (Mathf.Approximately(group.alpha, _target))
        {
            enabled = false;
            if (Mathf.Approximately(_target, 0))
            {
                gameObject.SetActive(false);
            }
        }
    }
}