using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModelController : MonoBehaviour
{
	public GameObject sender;
	public GameObject renderCam;
	public GameObject touchProcessor;
    //public GameObject sliderController;
    public GameObject obj;

	private float camWidth;
	private float camHeight;

	[HideInInspector]
	public Vector3 observe;
	private Vector3 defaultObserve = new Vector3(0, 0, -5f);

    private Quaternion factorRotate;
    private bool isConnecting;
	
	void Start() {
		Camera cam = Camera.main;
		camHeight = 2f * cam.orthographicSize;
		camWidth = camHeight * cam.aspect;

		observe = defaultObserve;

        float angle = -Mathf.PI / 2;
        factorRotate = Quaternion.Euler(0f, 180 * angle / Mathf.PI - 180, 0f);
    }

	void Update() {
        transform.position = touchProcessor.GetComponent<TouchProcessor>().pos;
        //transform.rotation = Quaternion.Euler(0, 180 * sliderController.GetComponent<SliderController>().angle / Mathf.PI - 180, 0);
        obj.transform.rotation = factorRotate * touchProcessor.GetComponent<TouchProcessor>().rot;
        //obj.transform.rotation = touchProcessor.GetComponent<TouchProcessor>().rot;
        obj.transform.localScale = touchProcessor.GetComponent<TouchProcessor>().sca;

    }

	void updateFov() {
		renderCam.transform.position = observe;
		Camera cam = renderCam.GetComponent<Camera>();
		float fovHorizontal = Mathf.Atan(-(Mathf.Abs(renderCam.transform.position.x) + camWidth / 2) / renderCam.transform.position.z) * 2;
		fovHorizontal = fovHorizontal * 180 / Mathf.PI;
		fovHorizontal = Camera.HorizontalToVerticalFieldOfView(fovHorizontal, cam.aspect);
		float fovVertical = Mathf.Atan(-(Mathf.Abs(renderCam.transform.position.y) + camHeight / 2) / renderCam.transform.position.z) * 2;
		fovVertical = fovVertical * 180 / Mathf.PI;
		cam.fieldOfView = (fovVertical > fovHorizontal ? fovVertical : fovHorizontal);
	}

	public void resetAll() {
		observe = defaultObserve;
	}

    public void setFov() {
        updateFov();
    }

}
