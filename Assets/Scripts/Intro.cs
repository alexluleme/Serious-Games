using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Intro : MonoBehaviour {

	public GameObject gameManagerPrefab;
	public GameObject gameCursor;
	public Image tutorial;
	public Image play;

	private GameManager gameManager;
	private float tutorialTimer;
	private float playTimer;

	void Awake () {
		if (GameObject.FindGameObjectWithTag ("GameController") == null)
			gameManager = Instantiate (gameManagerPrefab).GetComponent<GameManager> ();
		else
			gameManager = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameManager> ();
	}

    void Update() {
        if (RectTransformUtility.RectangleContainsScreenPoint(tutorial.rectTransform, Camera.main.WorldToScreenPoint(gameCursor.transform.position), Camera.main)) {
			gameManager.showTutorials = true;
			if(tutorialTimer < 1f) {
				tutorial.fillAmount = tutorialTimer / 1f;
				tutorialTimer += Time.deltaTime;
			} else {
				gameManager.showTutorials = true;
				gameManager.loadScene(1);
			}
		} else {
			tutorial.fillAmount = 1;
			tutorialTimer = 0;
		}

		if (RectTransformUtility.RectangleContainsScreenPoint(play.rectTransform, Camera.main.WorldToScreenPoint(gameCursor.transform.position), Camera.main)) {
			//gameManager.GetComponentInParent<ArmInput>().Vibration();
			gameManager.showTutorials = false;
			if(playTimer < 1f) {
				play.fillAmount = playTimer / 1f;
				playTimer += Time.deltaTime;
			} else {
				gameManager.showTutorials = false;
				gameManager.cooked = false;
				gameManager.loadScene(1);
			}
		} else {
			play.fillAmount = 1;
			playTimer = 0;
		}
	}
}
