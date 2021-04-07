﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public GameObject renderCam;
	private Camera thisCam;

	private const float renderWidth = 5f;

	private float fov;
	private float eyeHeight;
	private float camWidth;
	private float camHeight;

    [HideInInspector]
    public float curCameraOrthSize;
    [HideInInspector]
    public float curCameraHeight;
    [HideInInspector]
    public float curCameraWidth;

    // Start is called before the first frame update
    void Start() {
		thisCam = GetComponent<Camera>();
		Camera cam = Camera.main;
		camHeight = 2f * cam.orthographicSize;
		camWidth = camHeight * cam.aspect;
        //Debug.Log("START:: CameraHeight: " + camHeight + "; CameraWidth: " + camWidth);
    }

    // Update is called once per frame
    void Update() {
		eyeHeight = renderCam.transform.position.z;
		fov = Mathf.PI * Camera.VerticalToHorizontalFieldOfView(renderCam.GetComponent<Camera>().fieldOfView, renderCam.GetComponent<Camera>().aspect) / 360f;
		thisCam.orthographicSize = - camHeight / (eyeHeight * Mathf.Tan(fov) / (renderWidth / 2)) / 2f;
		transform.position = new Vector3(-renderCam.transform.position.x * (renderWidth / 2) / Mathf.Tan(fov) / eyeHeight, renderCam.transform.position.y * (renderWidth / 2) / Mathf.Tan(fov) / eyeHeight, transform.position.z);

		renderCam.GetComponent<Camera>().nearClipPlane = - renderCam.transform.position.z;
        
        //Debug.Log("Update:: CameraHeight: " + thisCam.orthographicSize*2.0f + "; CameraWidth: " + thisCam.orthographicSize * 2.0f * thisCam.aspect);
        curCameraOrthSize = thisCam.orthographicSize;
        curCameraHeight = thisCam.orthographicSize * 2.0f;
        curCameraWidth = curCameraHeight * thisCam.aspect;
    }
}
