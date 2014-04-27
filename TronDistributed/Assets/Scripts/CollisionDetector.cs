using UnityEngine;
using System.Collections;

public class CollisionDetector : MonoBehaviour {

	void OnControllerColliderHit(ControllerColliderHit hit) {
		// Debug.DrawRay(hit.point, hit.normal);
		if (hit.gameObject.tag == "Terrain") {
			return ;
		}
		Debug.Log(hit.gameObject.tag);
		string debugInfo = "Collide Something! ";
		debugInfo += "Position: " + hit.gameObject.transform.position.x + ", " + hit.gameObject.transform.position.y +
						", " + hit.gameObject.transform.position.z;
		BoxCollider collider = hit.gameObject.GetComponent<BoxCollider>();
		if (collider != null) {
			debugInfo += "Size: " + collider.size.x + ", " + collider.size.y + ", " + collider.size.z;
		}
		Debug.Log(debugInfo);

		// Initiate the connection. If failed, show something and quit
		NetworkManager networkManager = gameObject.GetComponent<NetworkManager>();
		if (networkManager == null) {
			Debug.Log("Cannot find NetworkManager");
		}
		Debug.Log("Get network manager success!");
		if (!networkManager.GetSocketState()) {
			networkManager.closeSocket();
		}

		Application.LoadLevel(2);
	}
}
