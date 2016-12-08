using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cook : MonoBehaviour {

	public LayerMask mask;
	public UIController uiController;
	public Color accurate;
	public Color inaccurate;
	public GameManager gameManager;
	public GameObject gameCursor;

	private List<TemporalPosition> data;
	private LineRenderer pathRenderer;
	private GameObject heldObject;
	private Vector3 prev;
	private float heldTimer;
	private int vertexCount;
	private int itemCount;
	private bool gameActive;

	void Start() {

		pathRenderer = GetComponent<LineRenderer> ();
		gameManager = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameManager> ();
		gameManager.gamelevel = 3;
		data = new List<TemporalPosition> ();

		pathRenderer.SetVertexCount (0);
		prev = gameCursor.transform.position;
		uiController.showGameText ("Grab each item and follow the 3D path", 7);
		if (gameManager.showTutorials)
			StartCoroutine (seq1 ());
		gameActive = true;
	}

	void Update() {
		if (!gameActive)
			return;

		Vector3 pos = gameCursor.transform.position;

		//Clamp cursor position in view
		gameCursor.transform.position = new Vector3 (pos.x, Mathf.Clamp (pos.y, 0.95f, 1.8f), Mathf.Clamp(pos.z, 0, 1.8f));

		//Detect collisions
		RaycastHit[] hits = Physics.SphereCastAll (pos, 0.05f, Vector3.down, 0.05f, mask);
		if (hits.Length > 0) {
			if(hits[0].transform.gameObject.layer == 10 && heldObject == null) {
				hits[0].collider.enabled = false;
				heldTimer = 0;
				heldObject = hits[0].transform.gameObject;
				heldObject.transform.parent = gameCursor.transform;
				heldObject.transform.localPosition = new Vector3(0, -0.05f, 0);
				gameCursor.GetComponentInChildren<Animator>().SetTrigger("GrabItem");
			}
			if(hits[0].transform.gameObject.layer == 9 && heldObject != null) {
				float avgAccuracy = heldObject.GetComponent<Path>().getAccuracy();
				StartCoroutine(uiController.showFloatingText("+" +(avgAccuracy * 100).ToString("F0"), Color.Lerp(inaccurate, accurate, avgAccuracy)));
				Destroy(heldObject);
				gameManager.playAudio(4);
				gameCursor.GetComponentInChildren<Animator>().SetTrigger("ReleaseItem");
				vertexCount = 0;
				pathRenderer.SetVertexCount(vertexCount);
				if(++itemCount == 5)
					StartCoroutine(seq0());
			}
		}

		//Get accuracy
		if (heldObject != null && Vector3.Distance (pos, prev) > 0.1f) {
			prev = pos;
			heldTimer += Time.deltaTime;
			data.Add (new TemporalPosition(heldTimer, pos, itemCount));
			float dist = heldObject.GetComponent<Path> ().getDist (gameCursor.transform.position);
			float accuracy = (0.3f - dist) / 0.3f;
			Color col = Color.Lerp (inaccurate, accurate, accuracy);
			pathRenderer.material.color = col;
			pathRenderer.SetColors (col, col);
			heldObject.GetComponent<Path> ().addAccuracyPoint (accuracy);
		}
	}

	void FixedUpdate() {
		//Draw path
		if (heldObject != null) {
			if(vertexCount < heldObject.GetComponent<Path> ().points.Length) {
				pathRenderer.SetVertexCount(++vertexCount);
				pathRenderer.SetPosition(vertexCount - 1, heldObject.GetComponent<Path>().points[vertexCount - 1]);
			}
		}
	}

	IEnumerator seq0() {
		gameActive = false;
		gameManager.cooked = true;
		gameManager.addCookEvent (data);
		uiController.showGameText ("Great! Back to painting the fence.", 4);
		yield return new WaitForSeconds(5);
		gameManager.loadScene (1);
	}

	IEnumerator seq1() {
		while (heldObject == null) {
			gameCursor.GetComponent<GameCursor>().drawHelperTo(new Vector3(0, 0.9f, 1.5f));
			yield return null;
		}
		gameCursor.GetComponent<GameCursor> ().clearHelper ();
		while (itemCount < 1)
			yield return null;
		gameManager.cooked = true;
		gameManager.loadScene (1);
	}

	public class TemporalPosition {
		public float time;
		public Vector3 pos;
		public int item;

		public TemporalPosition(float time, Vector3 pos, int item) {
			this.time = time;
			this.pos = pos;
			this.item = item;
		}
	}
}
