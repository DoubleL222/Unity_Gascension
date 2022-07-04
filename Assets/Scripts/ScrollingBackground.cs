using UnityEngine;
using System.Collections;

public class ScrollingBackground : MonoBehaviour
{
    [Range(0.01f, 0.1f)]
    public float scrollSpeed;
    private Vector2 offset;

    private void Start()
    {
    }

    void Update()
    {
        //offset = new Vector2(0, Time.time * scrollSpeed);
        gameObject.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0, gameObject.transform.parent.position.y * scrollSpeed);
    }
}