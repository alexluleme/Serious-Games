using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class Shop : MonoBehaviour {
	
	public UIController uiController;
	public LayerMask mask;
	public GameObject[] items;
	public GameObject[] listItems;
	public GameObject shoppingList;
	public GameObject gameCursor;
	public GameObject shelfSegment;
	public GameObject ceilingSegment;
	public GameObject helperParticle;
	
	private GameManager gameManager;
	private List<GameObject> staticItems;
	private List<GameObject> dynamicItems;
	private Queue<GameObject> dynamicItemQueue;
	private float spawnTimer;
	private float spawnPos = 2;
	private float dynamicSpawnChance;
	private float score;
	private float speed;
	private float listTimer;
	private float playTimer;
	private int itemsPicked;
    private int iteration = 1;
    private int buffer;
	private bool gameActive;
	
	void Start() {

		staticItems = new List<GameObject> ();
		dynamicItems = new List<GameObject> ();
		dynamicItemQueue = new Queue<GameObject> ();
		gameManager = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameManager> ();
		gameManager.gamelevel = 2;
		speed = 1.2f;
		listTimer = 15;
		switch (gameManager.difficulty) {
		case 0:
			speed = 0.6f;
			listTimer = 15;
			break;
		case 2:
			speed = 2;
			listTimer = 5;
			break;
		default:
			break;
		}
		
		//Adjust view distance of items for performance
		float[] distances = new float[32];
		distances [9] = distances [10] = 7;
		distances [11] = 20;
		Camera.main.layerCullDistances = distances;
		
		foreach (GameObject temp in items) {
			if(temp.layer == 9) {
				staticItems.Add(temp);
			}
			if(temp.layer == 10) {
				dynamicItems.Add(temp);
			}
		}

        while(dynamicItemQueue.Count < dynamicItems.Count)
        {
            GameObject temp = dynamicItems[UnityEngine.Random.Range(0, dynamicItems.Count)];
            if (!dynamicItemQueue.Contains(temp))
                dynamicItemQueue.Enqueue(temp);
        }
		
		for (int i = 0; i < 10; i++) {
			spawnAisle();
		}

		if (gameManager.showTutorials)
			StartCoroutine (seq2 ());
		else
			StartCoroutine (seq0());
	}
	
	void Update() {
		if (!gameActive)
			return;

		playTimer += Time.deltaTime;
		
		Vector3 pos = Input.mousePosition;
		pos = new Vector3 (pos.x, pos.y,  1.5f);
		pos = Camera.main.ScreenToWorldPoint (pos);
		
		if (Input.GetMouseButton (0))
			gameCursor.transform.position = Vector3.Lerp (gameCursor.transform.position, pos, Time.deltaTime * 10f);		

		//Constantly spawn new aisles
		spawnTimer += Time.deltaTime * speed;
		if (spawnTimer > 2) {
			spawnTimer = 0;
            if(dynamicItemQueue.Count == 0) {
                if(buffer++ > 10) {
                    buffer = 0;
					speed *= 0.75f;
                    iteration++;
                    foreach (GameObject temp in dynamicItems)
                        dynamicItemQueue.Enqueue(temp);
					StartCoroutine(seq0());
				}
            }
			spawnAisle();
		}
		
		//Collision with objects
		Ray ray = new Ray (Camera.main.transform.position, gameCursor.transform.localPosition);
		Debug.DrawRay (ray.origin, ray.direction * 2, Color.yellow);
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit, 2.5f, mask)) {
			itemsPicked++;
			StartCoroutine(movToCart(hit.transform));
		}
		
		if (dynamicItems.Count == 0)
			StartCoroutine (seq1 ());

        Camera.main.transform.Translate (Vector3.forward * Time.deltaTime * speed, Space.Self);
	}
	
	//Spawn a shelf segment and populate it with items
	private void spawnAisle() {
		GameObject temp = Instantiate (shelfSegment, Vector3.forward * spawnPos, Quaternion.identity) as GameObject;
		temp.name = shelfSegment.name;
		foreach (Collider col in temp.GetComponentsInChildren<Collider>()) {
			float pos = -0.8f;
            GameObject item;
			bool contains = false;
            if (UnityEngine.Random.Range(0, 70) < dynamicSpawnChance && dynamicItemQueue.Count > 0)
            {
				contains = true;
                item = dynamicItemQueue.Dequeue();
                dynamicSpawnChance = 0;
            }
            else
                item = staticItems[UnityEngine.Random.Range(0, staticItems.Count)];
            while (pos < 0.8f) {				
				GameObject instance = Instantiate(item, 
				                                  new Vector3(col.transform.position.x, col.transform.position.y, col.transform.position.z + pos), 
				                                  col.transform.name == "L" ? Quaternion.Euler(new Vector3(0, -90, 0)) : Quaternion.Euler(new Vector3(0, 90, 0))) as GameObject;
				instance.name = item.name;
				if(iteration >= 3 && contains) {
					GameObject helper = Instantiate(helperParticle, instance.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0))) as GameObject;
					helper.transform.parent = instance.transform;
				}
				pos += UnityEngine.Random.Range(0.3f, 0.5f);
			}
		}
		if (spawnPos % 12 == 0)
			Instantiate (ceilingSegment, Vector3.forward * spawnPos, Quaternion.identity);
		dynamicSpawnChance += 1;
		spawnPos += 2;
	}
	
	IEnumerator seq0() {
		gameActive = false;
		foreach (GameObject listItem in listItems)
			listItem.SetActive (false);
		foreach (GameObject temp in dynamicItems) {
			foreach(GameObject listItem in listItems) {
				if(temp.name == listItem.name)
					listItem.SetActive(true);
			}
		}
		while (shoppingList.GetComponent<CanvasGroup>().alpha < 1) {
			shoppingList.GetComponent<CanvasGroup>().alpha += Time.deltaTime;
			yield return null;
		}
		float realTimer = iteration == 1 ? listTimer : listTimer / 2;
		uiController.startCountdown (realTimer);
		yield return new WaitForSeconds(realTimer - 3);
		gameManager.playAudio (1);
		yield return new WaitForSeconds (3);		
		gameActive = true;
		
		while (shoppingList.GetComponent<CanvasGroup>().alpha > 0) {
			shoppingList.GetComponent<CanvasGroup>().alpha -= Time.deltaTime;
			yield return null;
		}
	}
	
	IEnumerator seq1() {
		gameActive = false;
		gameManager.addShopEvent (iteration, playTimer);
		yield return new WaitForSeconds(1);
		gameManager.loadScene (3);
	}

	IEnumerator seq2() {
		shoppingList.GetComponent<CanvasGroup> ().alpha = 0;
		gameActive = true;
		uiController.showGameText ("Move over items to pick them up.", 7);
		yield return new WaitForSeconds (7);
		uiController.showGameText ("You will be given a list of needed items to memorize.", 7);
		yield return new WaitForSeconds (7);
		uiController.showGameText ("Only pick up items from the list.", 7);
		yield return new WaitForSeconds (7);
		gameManager.loadScene (3);
	}

	IEnumerator movToCart(Transform t) {
		//Move to cart
		t.parent = Camera.main.transform;
		t.gameObject.layer = 0;
		Vector3 target = new Vector3(0, -0.5f, 1.8f);
		while (Vector3.Distance(t.localPosition, target) > 0.2f) {
			t.localPosition = Vector3.Lerp (t.localPosition, target, Time.deltaTime * 3);
			yield return null;
		}
		
		//Check if dynamic item
		bool contains = false;
		foreach(GameObject item in dynamicItems) {
			if(item.name == t.name) {
				contains = true;
				float deltaScore = (50 + gameManager.difficulty * 50) * (1f / (float)iteration);
				StartCoroutine(uiController.showFloatingText("+" + deltaScore.ToString("F0"), Color.green));
				score += deltaScore;
				dynamicItems.Remove(item);
				break;
			}
		}
		if(!contains) {
			StartCoroutine(uiController.showFloatingText("-5", Color.red));
			score -= 5;
		}
		gameManager.playAudio(0);
		
		//Drop into cart and then destroy
		t.gameObject.AddComponent<Rigidbody> ();
		t.transform.parent = null;
		yield return new WaitForSeconds (1);
		Destroy (t.gameObject);
	}
}
