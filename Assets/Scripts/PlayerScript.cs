using UnityEngine;

public class PlayerScript : MonoBehaviour
{
	public Vector3 mousePosition; // debug purposes only, to show it in editor

	public Camera Camera;

	void Start()
	{
	}
	
	void Update()
	{
		this.mousePosition = Input.mousePosition;

		RaycastHit hit;
		var ray = this.Camera.ScreenPointToRay(Input.mousePosition);

		const int mask = 1 << 9;

		if (Physics.Raycast(ray, out hit, 1000.0f, mask))
		{
			var go = hit.transform.gameObject;
			var level = go.GetComponent<LevelScript>();

			if (Input.GetMouseButtonDown(0))
			{
				level.Hit(hit);
			}
			else if (Input.GetMouseButtonDown(1))
			{
				level.Tih(hit);
			}
		}
	}
}
