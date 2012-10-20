using UnityEngine;
using System.Collections;

public class ControlPoint : MonoBehaviour {
	
	MeshFilter meshFilter;
	MeshCollider meshCollider;
	Mesh mesh;
	Vector3[] vertices;
	Vector2[] uv;
	int[] triangles;
	private bool isGrab;
	
	Vector3 start_position;
	
	// Use this for initialization
	void Start () {
		meshFilter = GetComponent<MeshFilter>();
		meshCollider = GetComponent<MeshCollider>();
		
		mesh = new Mesh();
		vertices = new Vector3[4];
		triangles = new int[6];
		uv = new Vector2[4];
		
		isGrab = false;
		vertices[0] = new Vector3(0.0f, 0.0f, 0.0f);
		vertices[1] = new Vector3(0.5f, 0.0f, 0.0f);
		vertices[2] = new Vector3(0.5f, 0.5f, 0.0f);
		vertices[3] = new Vector3(0.0f, 0.5f, 0.0f);
		
		uv[0] = new Vector2(0.0f, 0.0f);
		uv[1] = new Vector2(1.0f, 0.0f);
		uv[2] = new Vector2(1.0f, 1.0f);
		uv[3] = new Vector2(0.0f, 1.0f);
		
		triangles[0] = 0;
		triangles[1] = 2;
		triangles[2] = 1;
		
		triangles[3] = 0;
		triangles[4] = 3;
		triangles[5] = 2;
		
		
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uv;
		
		mesh.RecalculateNormals();
		meshFilter.mesh = mesh;
		meshCollider.sharedMesh = mesh;
		
		start_position = transform.position;
	}
	
	void OnGUI(){
		
	}
	// Update is called once per frame
	void Update () {
		
		GrabPoint();
		MovePoint();
	}
	
	void GrabPoint(){
		if(Input.GetMouseButtonDown(0)){			
			Vector3 pos = GetMouseWorldPoint();
			Ray ray = new Ray(Camera.mainCamera.transform.position, pos);
			RaycastHit hitInfo = new RaycastHit();
			bool hit = meshCollider.Raycast(ray,  out hitInfo, Vector3.Distance(Camera.mainCamera.transform.position, pos));
			if(hit){
				isGrab = true;
				Debug.Log("hit");
			}
		}
		
		if(Input.GetMouseButtonUp(0)){
			isGrab = false;
		}
	}
	
	void MovePoint(){
		if(isGrab){
			if(Input.GetMouseButton(0)){
				Vector3 pos = GetMouseWorldPoint();
				transform.position = pos - new Vector3(0.25f, 0.25f, 0.0f);
			}
		}
	}
	
	Vector3 GetMouseWorldPoint(){
		Vector3 pos = Input.mousePosition;
		pos.z = 9.9f;
		pos = Camera.mainCamera.ScreenToWorldPoint(pos);
		return pos;
	}
	
	public void InitializePosition(){
		transform.position = start_position;
	}
}
