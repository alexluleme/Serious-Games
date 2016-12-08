using UnityEngine;
using System.Collections;

public class TouchInput : MonoBehaviour {

	private Vector3 pos;
	
	void Update() {
		pos = Input.mousePosition;
		pos.z = 1.5f;
		pos = Camera.main.ScreenToWorldPoint(pos);
	}
	
	public Vector3 getPos() {
		return pos;
	}
}
