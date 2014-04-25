using UnityEngine;
using System.Collections;

public class PrefabOperator : MonoBehaviour {

	public GameObject otherPlayer;
	private Vector3 moveDirection = Vector3.zero;
	private GameObject prefab;

	// Use this for initialization
	void Start () {
		Vector3 startPos = new Vector3(Random.Range(1.0f, 63.0f), 1, Random.Range(1.0f, 63.0f));
		prefab = Instantiate(otherPlayer, startPos, Quaternion.identity) as GameObject;
		prefab.AddComponent<CapsuleCollider> ();
		prefab.AddComponent<TrailRenderer> ();
		//moveDirection = transform.TransformDirection(Vector3.forward);
		//moveDirection = prefab.transform.TransformDirection(Vector3.back);
		//Quaternion.LookRotation(moveDirection);
	}
	
	// Update is called once per frame
	void Update () {
		moveDirection = prefab.transform.TransformDirection(Vector3.back);
		Vector3 movement = moveDirection * 0.1f;
		prefab.transform.position = prefab.transform.position + movement;
		//prefab.transform.rotation = Quaternion.LookRotation(moveDirection);
	}
}
