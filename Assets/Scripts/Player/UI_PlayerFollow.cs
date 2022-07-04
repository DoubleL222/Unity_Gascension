using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PlayerFollow : MonoBehaviour
{

    public GameObject Obj;

    Camera mCamera;
    private RectTransform rt;
    Vector2 pos;
    public float xOffset;
    public float yOffset;

    void Start()
    {
        mCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        rt = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (Obj)
        {
            pos = RectTransformUtility.WorldToScreenPoint(mCamera, Obj.transform.position);
            pos.x = pos.x + xOffset;
            pos.y = pos.y + yOffset;
            rt.position = pos;
        }
        else
        {
            Debug.LogError(this.gameObject.name + ": No Object Attached (TrackObject)");
        }


    }
}
