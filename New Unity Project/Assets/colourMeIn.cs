using UnityEngine;
using System.Collections;

public class colourMeIn : MonoBehaviour {

	public float detectionRange;
	public bool closeEnough;
	public Transform player;


	// Use this for initialization
	void Start () {
		//Renderer rend = GetComponent<Renderer> ();
		//rend.material.color = Color.red;
	}
	
	// Update is called once per frame
	void Update () {

		//here need to call from a database (lookup) to check if the current shop is the one requested
		bool req = false;

		if (req == true) {
			//Debug.Log (player.position);
			Renderer rend = GetComponent<Renderer> ();
			rend.material.color = Color.red;

			closeEnough = false;

			if (Vector3.Distance (player.position, transform.position) <= detectionRange) {
				closeEnough = true;
			}

			if (closeEnough == true) {
				rend = GetComponent<Renderer> ();
				rend.material.color = Color.green;
				Debug.Log ("You have arrived at " + transform.name);
			}
		}
	}
}
