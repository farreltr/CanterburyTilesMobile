using UnityEngine;
using System.Collections;

public class TapToRestart : MonoBehaviour
{

	void OnMouseDown ()
	{
		Application.LoadLevel (0);
	}
}
