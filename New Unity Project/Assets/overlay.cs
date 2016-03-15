using UnityEngine;
using System.Collections;

public class overlay : MonoBehaviour {

	public Transform me;
	public Texture2D meArrow;
	public Camera minimap;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI() {

		if (minimap.enabled == true) {

			Vector3 objPos = GetComponent<Camera> ().WorldToViewportPoint(me.transform.position);

			float meAngle = me.transform.eulerAngles.y;
			Matrix4x4 guiRotationMatrix = GUI.matrix;
			Vector2 pivotMe;

			pivotMe.x = Screen.width * (minimap.rect.x + (objPos.x * minimap.rect.width));
			pivotMe.y = Screen.height * (1 - (minimap.rect.y + (objPos.y * minimap.rect.height)));

			GUIUtility.RotateAroundPivot (meAngle, pivotMe);

			GUI.DrawTexture (new Rect (Screen.width * (minimap.rect.x + (objPos.x * minimap.rect.width)) - 7.5f,
				Screen.height * (1 - (minimap.rect.y + (objPos.y * minimap.rect.height))) - 7.5f,
				15, 15), meArrow);

			GUI.matrix = guiRotationMatrix;
		}
	}
}
