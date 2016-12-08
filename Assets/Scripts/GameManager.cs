using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Parse;
using TETCSharpClient;
using TETCSharpClient.Data;
using Assets.Scripts;
using System;
using System.Text;
using System.IO;


public class GameManager : MonoBehaviour, IGazeListener {

    public GameObject eye;
	public GameObject vibrate;
	public bool showTutorials;
	public int gamelevel = 0;
	public bool cooked;
	public int difficulty;

    private GazeDataValidator gazeUtils;
	private AudioSource[] audios;
	private float fadeTimer;
	private int next = 9;
	private ParseObject session;


	private double eyesDistance;
	private double baseDist;
	private double depthMod;

	public string leftSize;
	public string rightSize;
	
	public double stress;
	
	private StreamWriter file;
	private DateTime time;


	void Start() {
		DontDestroyOnLoad (this.gameObject);
		difficulty = 1;
		gamelevel = 0;
        gazeUtils = new GazeDataValidator(50);
        GazeManager.Instance.AddGazeListener(this);

        audios = GetComponents<AudioSource> ();
		session = new ParseObject ("Session");
		session.SaveAsync ();

		file = new StreamWriter(@"stress.txt");
	}


	public void OnGazeUpdate(GazeData gazeData)
	{
		//Add frame to GazeData cache handler
		gazeUtils.Update(gazeData);
		leftSize = gazeData.LeftEye.PupilSize.ToString ();
		rightSize = gazeData.RightEye.PupilSize.ToString ();
	}


	void Update() {
        //Eye position
        Point2D gazeCoords = gazeUtils.GetLastValidSmoothedGazeCoordinates();

        if (gazeCoords != null)
        {
            Point2D gp = UnityGazeUtils.getGazeCoordsToUnityWindowCoords(gazeCoords);

            Vector3 screenPoint = new Vector3((float)gp.X, (float)gp.Y, Camera.main.nearClipPlane + .1f);

            Vector3 planeCoord = Camera.main.ScreenToWorldPoint(screenPoint);
            eye.transform.position = planeCoord;
        }




		eyesDistance = gazeUtils.GetLastValidUserDistance();
		depthMod = 2 * eyesDistance;

		double distance = (1/eyesDistance)*100;

		Debug.Log ("Distance = " + distance);

		double rr = Convert.ToDouble(rightSize);
		double ll = Convert.ToDouble (leftSize);
		double ten = 10000;

		double pupilSize = (rr + ll)/2;
		
		stress = distance * pupilSize;
		
		//Debug.Log("Eye stress = " + stress);
		
		//time = DateTime.Now;
		//file.WriteLine (Application.loadedLevel);
		file.WriteLine(stress);


		//Debug.Log ("Show tutorial = " + showTutorials);

		/*
		if(showTutorials == false){
			//Debug.Log("Application = "+ Application.loadedLevel);
			Debug.Log("Game level = "+ gamelevel);

		}
		*/

        if (next != 9) {
			fadeTimer += Time.deltaTime * 2;
			GetComponentInChildren<CanvasGroup>().alpha = fadeTimer;
			if(fadeTimer > 1) {
				Application.LoadLevel(next);
				next = 9;
			}
		} else if (fadeTimer > 0) {
			fadeTimer -= Time.deltaTime;
			GetComponentInChildren<CanvasGroup>().alpha = fadeTimer;
		}


		if (Input.GetKeyDown (KeyCode.Alpha1))
			//loadScene (3);
			file.WriteLine(ten);
		if (Input.GetKeyDown (KeyCode.Alpha2))
			//loadScene (3);
			file.WriteLine(ten);
		if (Input.GetKeyDown (KeyCode.Alpha3))
			//loadScene (3);
			file.WriteLine(ten);
		if (Input.GetKeyDown (KeyCode.Alpha4))
			//loadScene (3);
			file.WriteLine(ten);
		

	}

	public void addPaintEvent(int mode, List<Paint.PaintDatum> data) {
		foreach (Paint.PaintDatum datum in data) {
			ParseObject paintDatum = new ParseObject("Paint");
			paintDatum["session"] = session;
			paintDatum["mode"] = mode;
			paintDatum["eyeDelay"] = datum.eyeDelay;
			paintDatum["moveDelay"] = datum.moveDelay;
			paintDatum["paintDelay"] = datum.paintDelay;
			paintDatum.SaveAsync();
		}
	}

	public void addShopEvent(int iteration, float timeTaken) {
		ParseObject shopDatum = new ParseObject ("Shop");
		shopDatum ["session"] = session;
		shopDatum ["iteration"] = iteration;
		shopDatum ["timeTaken"] = timeTaken;
		shopDatum.SaveAsync ();
	}

	public void addCookEvent(List<Cook.TemporalPosition> trajectories) {
		foreach (Cook.TemporalPosition traj in trajectories) {
			ParseObject cookDatum = new ParseObject("Cook");
			cookDatum["session"] = session;
			cookDatum["item"] = traj.item;
			cookDatum["time"] = traj.time;
			cookDatum["x"] = traj.pos.x;
			cookDatum["y"] = traj.pos.y;
			cookDatum["z"] = traj.pos.z;
			cookDatum.SaveAsync();
		}
	}

	/*
	public void loadScene(int scene) {
		if (scene == 0)
			GetComponentInChildren<ArmInput> ().closeConnection ();
		next = scene;
	}
	*/

	public void loadScene(int scene) {
		next = scene;
	}

	public void playAudio(int i) {
		audios [i].Play ();
	}

	public void Vibrate(){

	}

	void OnGUI()
	{
		int padding = 10;
		int btnWidth = 160;
		int btnHeight = 40;
		int y = padding;

		/*
		if (GUI.Button(new Rect(padding, y, btnWidth, btnHeight), "Press to Exit"))
		{
			Application.Quit();
		}
		
		y += padding + btnHeight;
		
		if (GUI.Button(new Rect(padding, y, btnWidth, btnHeight), "Press to Re-calibrate"))
		{
			Application.LoadLevel(0);
		}
		*/
		int stressint = Convert.ToInt32(stress*0.05);
		/*
		if (GUI.Button(new Rect(200, 200, 100, stressint), "eye stress"))
		{
			//Application.LoadLevel(0);
		}
		*/
		
	}

    
}
