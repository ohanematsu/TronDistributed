using UnityEngine;
using System.Collections;

public class CollisionDetector : MonoBehaviour {

	void OnControllerColliderHit(ControllerColliderHit hit) {
		// Debug.DrawRay(hit.point, hit.normal);
		if (hit.gameObject.tag == "Terrain") {
			return ;
		}
		Debug.Log(hit.gameObject.tag);
		Debug.Log("Collide Something！");

		// Initiate the connection. If failed, show something and quit
		NetworkManager networkManager = gameObject.GetComponent<NetworkManager>();
		if (networkManager == null) {
			Debug.Log("Cannot find NetworkManager");
		}
		Debug.Log("Get network manager success!");
		if (!networkManager.GetSocketState ()) {
			networkManager.closeSocket();
		}

		Application.LoadLevel(2);
	}
}
