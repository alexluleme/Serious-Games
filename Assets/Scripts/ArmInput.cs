using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

public class ArmInput : MonoBehaviour {
	
	private TcpClient clientSocket;
	private string clientData;
	private Vector3 pos;
	private float x = 0, y = 0, z = 0;
	private float prevX = 0, prevY = 0, prevZ = 0;
	
	public Vector3 offset;
	public Vector3 scale;
	public int vibrate = 0;


	void Start() {
		clientSocket = new TcpClient ();
		clientSocket.Connect ("192.168.1.2", 13131);
		Debug.Log ("Start Barrett Arm Client");
	}
	
	void Update() {
		NetworkStream networkStream = clientSocket.GetStream ();
		byte[] bytesFrom = new byte[100000];
		networkStream.Read (bytesFrom, 0, bytesFrom.Length);
		clientData = System.Text.Encoding.ASCII.GetString (bytesFrom);
		clientData = clientData.Substring (0, clientData.IndexOf ("\n"));





		char[] splitters = {' ', ',', '\n'};
		string[] points = clientData.Split (splitters);
		try {
			x = float.Parse (points [0]);
			y = float.Parse (points [1]);
			z = float.Parse (points [2]);
			Debug.Log("[" +  x + "," +  y + "," +  z + "]");
		} catch(IndexOutOfRangeException i) {
			Debug.Log(i);
		};
		
		/*Pre transform
		 * pos.x = (float) y * 10;
		 * pos.y = (float) z * 10;
		 * pos.z = (float) x * (-10);
		*/
		
		if (Vector3.Distance (new Vector3 (x, y, z), new Vector3 (prevX, prevY, prevZ)) < 0.003f) {
			pos.x = ((float)x + offset.x) * (-scale.x);
			pos.y = ((float)z + offset.y) * scale.y;
			pos.z = ((float)y + offset.z) * (-scale.z);
		} else {
			//Debug.Log (Vector3.Distance(new Vector3(x, y, z), new Vector3(prevX, prevY, prevZ)));
			int a = 0;
		}
		
		prevX = x;
		prevY = y;
		prevZ = z;

		if(vibrate == 1){
			Debug.Log ("Vibration :)");
			vibrate = 0;
		}



		if (Input.GetKey (KeyCode.Escape)) {
			//TODO
				
			networkStream.Close();
		}

		//Debug.Log (pos.ToString());
	}

	public void closeConnection() {

	}
	
	public Vector3 getPos() {
		return pos;
	}






}