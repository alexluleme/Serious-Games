using UnityEngine;
using System.Collections;

public class MouseInput : MonoBehaviour {

	private float zPos = 1.4f;
	private Vector3 pos;
	
	void Update() {
		//Draw ray from camera to mouse x and y positions
		Ray ray = Camera.main.ScreenPointToRay (new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

		//Update z-position based on W/S-key and scroll wheel input
		zPos += Input.GetAxis ("Vertical");
		zPos += Input.GetAxis ("Mouse ScrollWheel");

		//Set pos to point at zPos on ray
		pos = new Vector3 (ray.GetPoint (zPos).x, ray.GetPoint (zPos).y, zPos);
	}

	public Vector3 getPos() {
		return pos;
	}
}
