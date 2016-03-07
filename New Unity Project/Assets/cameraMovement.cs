using UnityEngine;
using System.Collections;

public class cameraMovement : MonoBehaviour {

	public GameObject player;
	private Vector3 offset;
	public float RotateSpeed = 30f;

	// Use this for initialization
	void Start () {
		offset = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		/*if (Input.GetKey (KeyCode.LeftArrow))
			transform.Rotate (-Vector3.up * RotateSpeed * Time.deltaTime);
		else if (Input.GetKey (KeyCode.RightArrow))
			transform.Rotate (Vector3.up * RotateSpeed * Time.deltaTime);*/
	}

	void LateUpdate() {
		transform.position = player.transform.position + offset;
	}
}
