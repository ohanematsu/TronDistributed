using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollisionDetector : MonoBehaviour {

	private GameStateManager gameStateManager;

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

		// Send DELETE Message
		Dictionary<string, object> message = new Dictionary<string, object>();
		message["type"] = MessageDispatcher.DELETE_USER;
		message["userID"] = gameStateManager.GetUserID();
		gameStateManager.GetNetworkManager().writeSocket(message);
		Debug.Log("DELETE USER message sent out");

		// Close Socket
		gameStateManager.GetNetworkManager().closeSocket();
		Debug.Log("Socket closed");

		Application.LoadLevel(2);
	}

	public void SetGameStateManager(GameStateManager globalGameStateManager) {
		gameStateManager = globalGameStateManager;
	}
}
