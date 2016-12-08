using UnityEngine;
using System.Collections;

public class GameCursor : MonoBehaviour {

	public float dampening;
	public LineRenderer helper;
	public int input; //0 - mouse; 1 - arm; 2 - touch;

	private MouseInput mouseInput;
	private ArmInput armInput;
	private TouchInput touchInput;

	void Start () {
		mouseInput = GetComponent<MouseInput> ();
		armInput = GameObject.Find ("ArmInput").GetComponent<ArmInput> ();
		touchInput = GetComponent<TouchInput> ();
	}
	
	void Update () {
		Vector3 pos = transform.position;

		switch (input) {
		case 0:
			pos = mouseInput.getPos();
			pos = new Vector3(pos.x - transform.parent.position.x, pos.y - 1.5f, pos.z);
			break;
		case 1:
			pos = armInput.getPos();
			break;
		case 2:
			if(Input.GetMouseButton(0))
				pos = touchInput.getPos();
			break;
		default:
			break;
		}

		transform.localPosition = Vector3.Lerp (transform.localPosition, pos, Time.deltaTime * 100f / dampening);
	}

	public void drawHelperTo(Vector3 pos) {
		helper.SetVertexCount (2);
		helper.SetPosition (0, transform.position);
		helper.SetPosition (1, pos);
	}

	public void clearHelper() {
		helper.SetVertexCount (0);
	}
}
