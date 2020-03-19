using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    [SerializeField]
    private float _scrollSpeed;

    [SerializeField]
    private float _scrollSpeedBoost;

    private Vector2 _mainOffset;

    private Renderer _spriteRenderer;

    void Start()
    {
        _spriteRenderer = GetComponent<Renderer>();
        if (_spriteRenderer == null)
        {
            Debug.Log("Renderer is NULL.");
        }

        _mainOffset = _spriteRenderer.material.mainTextureOffset;
    }

    public void BackgroundScroll()
    {
        float y = Mathf.Repeat(_scrollSpeed * Time.time, 1);
        Vector2 offset = new Vector2(_mainOffset.x, y);
        _spriteRenderer.material.mainTextureOffset = offset;
    }

    public void IncreaseScrollSpeed()
    {
        _scrollSpeed *= _scrollSpeedBoost;
    }

    public void DecreaseScrollSpeed()
    {
        _scrollSpeed /= _scrollSpeedBoost;
    }
}
