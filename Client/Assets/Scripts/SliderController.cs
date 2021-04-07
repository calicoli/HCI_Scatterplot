using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
	public GameObject slider;
	public GameObject cam;

	public Text debugText;

	[HideInInspector]
	public Vector3 acceOther;

	[HideInInspector]
	public float angle;

    //private float defaultAngle = 2 * Mathf.PI / 3;
    private float defaultAngle = Mathf.PI / 2;
	private const float minAngle = Mathf.PI / 2;
	private const float maxAngle = Mathf.PI;
	private const float leftMost = 18.75f;
	private const float rightMost = -18.75f;
	// Start is called before the first frame update
	void Start()
	{
		angle = defaultAngle;
	}

	// Update is called once per frame
	void Update()
	{
        /*
		Vector3 acceThis = Input.acceleration;
		acceOther.y = 0;
		acceThis.y = 0;
		float angleTemp = Vector3.Angle(acceThis, acceOther);

		debugText.text = acceThis + "\n" + acceOther + "\n" + angleTemp;

		angleTemp = Mathf.PI - angleTemp * Mathf.PI / 180;

		angle = Mathf.Lerp(angle, angleTemp, Time.deltaTime * 3);

		float size = cam.GetComponent<Camera>().orthographicSize;
		slider.transform.localScale = new Vector3(size * 0.025f, size * 0.025f, size * 0.025f);

		angle = angle > minAngle ? angle : minAngle;
		angle = angle < maxAngle ? angle : maxAngle;
        */
        angle = defaultAngle;
        /*
        float pos = leftMost - (leftMost - rightMost) * (angle - Mathf.PI / 2) / (Mathf.PI / 2);
		transform.localPosition = new Vector3(pos, transform.localPosition.y, 0);
        */
	}
}
