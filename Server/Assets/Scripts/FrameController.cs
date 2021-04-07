using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// server in scatterplot
public class FrameController : MonoBehaviour
{
	public GameObject touchProcessor;
	//public GameObject faceTracker;
	public Text debugText;
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
        angle = Mathf.PI / 2;

		camWidth = camHeight * Camera.main.aspect;
	}

	// Update is called once per frame
	void Update()
	{

        //observe = faceTracker.GetComponent<FaceTracker>().currentObserve;
        //angle = touchProcessor.GetComponent<TouchProcessor>().angle;
        angle = Mathf.PI / 2;

        debugText.text = "" + 180 * angle / Mathf.PI +" " + camWidth + " " + camHeight;

		//Vector3 camPos = new Vector3(observe.x, observe.y, 0);
		lineRenderer.SetPosition(0, new Vector3(
			camWidth,
			- camHeight,
			0
		));
		lineRenderer.SetPosition(1, new Vector3(
			camWidth - 2 * camWidth * Mathf.Cos(Mathf.PI - angle),
			- camHeight,
			2 * camWidth * Mathf.Sin(angle)
		));
		lineRenderer.SetPosition(2, new Vector3(
			camWidth - 2 * camWidth * Mathf.Cos(Mathf.PI - angle),
			camHeight,
			2 * camWidth * Mathf.Sin(angle)
		));
		lineRenderer.SetPosition(3, new Vector3(
			camWidth,
			camHeight,
			0
		));

	}

}
