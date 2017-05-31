using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class CameraController : MonoBehaviour {

    public ZoomButton zoomIn;
    public ZoomButton zoomOut;
    private Transform startPosition;
    private const float ZOOM = 0.2f;
    
    private float topBorder;
    private float rightBorder;
    private float bottomBorder;

    private GameObject mainCamera;
    
    private Vector3 topLeftCorner;

    private void Start()
    {
        startPosition = transform;
        mainCamera = GameObject.Find("Main Camera");
    }

    private void FixedUpdate()
    {
        if (transform.position.y > 5 && zoomIn.isZooming)
        {
            transform.position = new Vector3(startPosition.position.x,
                transform.position.y - ZOOM,
                transform.position.z + ZOOM
                );
        }
        else if (transform.position.z > -5 && transform.position.y < 10f && zoomOut.isZooming)
        {
            transform.position = new Vector3(startPosition.position.x,
                transform.position.y + ZOOM,
                transform.position.z - ZOOM
                );
        }
        startPosition = transform;
    }
    
    public void SetTopLeftMapCorner(Vector3 topLeftCorner)
    {
        this.topLeftCorner = topLeftCorner;
        topBorder = topLeftCorner.z - 9;
    }

    public void SetBottomRightCorner(Vector3 bottomRightCorner)
    {
        rightBorder = bottomRightCorner.x;
        bottomBorder = -6f;
    }
    
    public float GetRightBorder()
    {
        return rightBorder;
    }

    public float GetTopBorder()
    {
        return topBorder;
    }
    
    public Vector3 GetTopLeftCorner()
    {
        return topLeftCorner;
    }
}
