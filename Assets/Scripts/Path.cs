using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Path : MonoBehaviour {
	
	public Vector3[] points;

	private List<float> accuracyData;

	void Start() {
		accuracyData = new List<float> ();
	}

	public float getDist(Vector3 pos) {
		float dist = Mathf.Infinity;
		int index = 0;
		for(int i = 0; i < points.Length; i++) {
			Vector3 test = points[i];
			if(Vector3.Distance(pos, test) < dist) {
				dist = Vector3.Distance(pos, test);
				index = i;
			}
		}
		
		float angle = 90;
		
		if(index == 0)
			angle = Vector3.Angle(pos - points[index], points[index + 1] - points[index]);
		else if(index == points.Length - 1)
			angle = Vector3.Angle(pos - points[index], points[index - 1] - points[index]);
		else
			angle = Mathf.Min(Vector3.Angle(pos - points[index], points[index - 1] - points[index]), 
			                  Vector3.Angle(pos - points[index], points[index + 1] - points[index]));
		
		dist = dist * Mathf.Sin(angle * Mathf.Deg2Rad);

		return dist;
	}

	public void addAccuracyPoint(float distance) {
		accuracyData.Add (distance);
	}

	public float getAccuracy() {
		float accuracy = 0;
		foreach (float datum in accuracyData)
			accuracy += datum;
		accuracy /= (float)accuracyData.Count;
		return Mathf.Clamp01(accuracy);
	}

	void OnDrawGizmosSelected() {
		Gizmos.color = Color.white;
		for (int i = 0; i < points.Length - 1; i++) {
			Gizmos.DrawLine(points[i], points[i+1]);
		}
	}
}
