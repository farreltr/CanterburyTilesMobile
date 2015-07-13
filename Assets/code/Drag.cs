using UnityEngine;
using System.Collections;

public class Drag : MonoBehaviour
{

	private Vector3 screenPoint;
	private Vector3 offset;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	void OnMouseDown ()
	{
		screenPoint = Camera.main.WorldToScreenPoint (gameObject.transform.position);
		offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
	}

	void OnMouseDrag ()
	{
		Vector3 currentScreenPoint = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
		Vector3 currentPosition = Camera.main.ScreenToWorldPoint (currentScreenPoint) + offset;
		/*float diffx = Mathf.Abs (currentPosition.x - transform.position.x);
		float diffy = Mathf.Abs (currentPosition.y - transform.position.y);
		Vector3 newPos = transform.position;
		if (diffx > diffy) {
			newPos.x = currentPosition.x;
		} else {
			newPos.y = currentPosition.y;
		}*/

		transform.position = currentPosition;
	}
}
