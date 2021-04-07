using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameController : MonoBehaviour
{
	public GameObject obj;
	//public GameObject sliderController;
	private float lineWidth = 0.025f;


	private float camWidth;
	private float camHeight = 5;

	private float angle;
	private LineRenderer lineRenderer;
	private Vector3 observe;

	void Start()
	{
		lineRenderer = this.gameObject.AddComponent<LineRenderer>();
		lineRenderer.alignment = LineAlignment.TransformZ;
		lineRenderer.startWidth = lineWidth;
		lineRenderer.useWorldSpace = false;
		lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
		lineRenderer.positionCount = 4;
		lineRenderer.startColor = new Color(1, 1, 1, 1);
		lineRenderer.endColor = new Color(1, 1, 1, 1);

        //angle = 2 * Mathf.PI / 3;
        angle = -Mathf.PI / 2;

		camWidth = camHeight * Camera.main.aspect;
	}

	// Update is called once per frame
	void Update()
	{

		observe = obj.GetComponent<ModelController>().observe;
		//angle = sliderController.GetComponent<SliderController>().angle;

		Vector3 camPos = new Vector3(observe.x, observe.y, 0);
		lineRenderer.SetPosition(0, new Vector3(
			- camWidth,
			- camHeight,
			0
		));
		lineRenderer.SetPosition(1, new Vector3(
			2 * camWidth * Mathf.Cos(Mathf.PI - angle) - camWidth,
			- camHeight,
			2 * camWidth * Mathf.Sin(Mathf.PI - angle)
		));
		lineRenderer.SetPosition(2, new Vector3(
			2 * camWidth * Mathf.Cos(Mathf.PI - angle) - camWidth,
			camHeight,
			2 * camWidth * Mathf.Sin(Mathf.PI - angle)
		));
		lineRenderer.SetPosition(3, new Vector3(
			- camWidth,
			camHeight,
			0
		));

	}

}
