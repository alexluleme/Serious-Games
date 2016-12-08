using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Paint : MonoBehaviour {

	public UIController uiController;
	public LayerMask mask;
	public Material[] colors;
	public GameObject fenceSegment;
	public GameObject gameCursor;
	public GameObject brush;
	public GameObject paints;
	public Image activator;
	public Text stroopText;

	private GameManager gameManager;
	private List<PaintDatum> paintData;
	private Queue<GameObject> fenceSegmentQueue;
	private GameObject currentSegment;
	private string target;
	private float activatorTimer;
	private float activeTimer;
	private float moveTimer;
	private float eyeTimer;
	private bool gameActive;
	private bool stroopActive;
	private bool sawText;

	void Start() {

		fenceSegmentQueue = new Queue<GameObject> ();
		gameManager = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameManager>();
		gameManager.gamelevel = 1;
		paintData = new List<PaintDatum> ();
		brush.SetActive (true);
		paints.SetActive (true);
		gameCursor.GetComponentInChildren<Animator> ().SetTrigger ("GrabBrush");

		for(int i = 0; i < 10; i++) {
			GameObject temp = Instantiate(fenceSegment, new Vector3(2 * i, 0, 3), Quaternion.identity) as GameObject;
			temp.name = fenceSegment.name;
			fenceSegmentQueue.Enqueue(temp);
		}
		currentSegment = fenceSegmentQueue.Dequeue ();

		if (gameManager.showTutorials)
			StartCoroutine (seq2 ());
		gameActive = true;
	}

	void Update() {
		//Adjust hand rotation
		if (gameCursor.transform.localPosition.y < 0.1f && gameCursor.transform.localPosition.y > -0.3f)
			gameCursor.transform.rotation = Quaternion.Euler (new Vector3 ((gameCursor.transform.localPosition.y) * -180 - 5, 0, 0));
		gameManager.gamelevel = 1;
		//Adjust camera position
		Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, new Vector3 (currentSegment.transform.position.x, 1.2f, 0), Time.deltaTime * 5);

		if (!gameActive)
			return;

		if (stroopActive) {
			activator.fillAmount = 1;

			//If stroop active, increase active timer
			activeTimer += Time.deltaTime;

			//If stroop active and outside activator, increase move timer
			if (!RectTransformUtility.RectangleContainsScreenPoint (activator.rectTransform, Camera.main.WorldToScreenPoint (gameCursor.transform.position), Camera.main))
				moveTimer += Time.deltaTime;

            //Check if saw text
			if(RectTransformUtility.RectangleContainsScreenPoint(stroopText.rectTransform, Camera.main.WorldToScreenPoint(gameManager.eye.transform.position), Camera.main))
            {
                sawText = true;
            }

            //If haven't seen text, increase eye timer
            if (!sawText)
				eyeTimer += Time.deltaTime;

			//Check collision with paint
			RaycastHit[] hits = Physics.SphereCastAll(gameCursor.transform.position, 0.3f, Vector3.forward, 0.3f, mask);
			if(hits.Length > 0) {
				paintData.Add(new PaintDatum(eyeTimer, moveTimer, activeTimer));
				Material temp = null;
				foreach(Material color in colors) {
					if(string.Compare(hits[0].transform.gameObject.name, color.name, System.StringComparison.OrdinalIgnoreCase) == 0)
						temp = color;
				}
				if(string.Compare(temp.name, target, System.StringComparison.OrdinalIgnoreCase) == 0) {
					StartCoroutine(uiController.showFloatingText(string.Format("{0}/{1}/{2}", eyeTimer.ToString("F2"), activeTimer.ToString("F2"), moveTimer.ToString("F2")), Color.green));
					gameManager.playAudio(2);
				}
				else {
					StartCoroutine(uiController.showFloatingText(string.Format("{0}/{1}/{2}", eyeTimer.ToString("F2"), activeTimer.ToString("F2"), moveTimer.ToString("F2")), Color.red));
					gameManager.playAudio(3);
				}
				currentSegment.GetComponentInChildren<MeshRenderer>().material = temp;
				if(fenceSegmentQueue.Count > 0)
					currentSegment = fenceSegmentQueue.Dequeue();
				else {
					if(gameManager.cooked)
						StartCoroutine(seq1());
					else
						StartCoroutine(seq0());
				}
				stroopActive = false;
			}
		} else {
			stroopText.text = "";
			activeTimer = 0;
			moveTimer = 0;
			eyeTimer = 0;
			sawText = false;

			//If inside activator, increment toward stroop activation
			if (RectTransformUtility.RectangleContainsScreenPoint (activator.rectTransform, Camera.main.WorldToScreenPoint (gameCursor.transform.position), Camera.main)) {
				activatorTimer += Time.deltaTime;
				activator.fillAmount = activatorTimer / 1f;

				//If over activator for over a second,
				if(activatorTimer > 1f) {
					stroopActive = true;

					//Set text to random color
					stroopText.rectTransform.anchoredPosition = new Vector2(Random.Range(-1, 1) * 250, -50);
					stroopText.text = target = gameManager.showTutorials ? colors[0].name.ToUpper() : colors[Random.Range(0, colors.Length)].name.ToUpper();

					//If cooked, match color instead of text
					if(gameManager.cooked) {
						Material temp = gameManager.showTutorials ? colors[1] : colors[Random.Range(0, colors.Length)];
						stroopText.color = temp.color;
						target = temp.name;
						gameManager.gamelevel = 1;
					}
				}
			} else
				activatorTimer = activator.fillAmount = 0;
		}
	}

	IEnumerator seq0() {
		gameActive = false;
		uiController.showGameText ("That's enough now. Time to go to the supermarket.", 5);
		gameManager.addPaintEvent (0, paintData);
		yield return new WaitForSeconds(6);
		gameManager.loadScene (2);
	}

	IEnumerator seq1() {
		gameActive = false;
		gameManager.addPaintEvent (1, paintData);
		yield return new WaitForSeconds (2);
		gameManager.loadScene (0);
	}

	IEnumerator seq2() {
		uiController.showGameText ("First, move to the circle in the middle.", 7);
		while (!stroopActive) {
			gameCursor.GetComponent<GameCursor> ().drawHelperTo (new Vector3(0, 1.5f, 2));
			yield return null;
		}
		if (gameManager.cooked)
			uiController.showGameText ("Then this time, match the color of the text.", 7);
		else
			uiController.showGameText ("Then, touch the paint color matching the text displayed.", 7);
		while (stroopActive) {
			Vector3 tar = gameManager.cooked ? new Vector3(-0.5f, 0.5f, 1.5f) : new Vector3(-1, 0.5f, 1.5f);
			gameCursor.GetComponent<GameCursor>().drawHelperTo(tar);
			yield return null;
		}
		gameManager.loadScene (gameManager.cooked ? 0 : 2);
	}

	public class PaintDatum {
		public float eyeDelay;
		public float moveDelay;
		public float paintDelay;

		public PaintDatum(float eyeDelay, float moveDelay, float paintDelay) {
			this.eyeDelay = eyeDelay;
			this.moveDelay = moveDelay;
			this.paintDelay = paintDelay;
		}
	}
}
