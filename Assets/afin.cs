using UnityEngine;
using System.Collections;

public class afin : MonoBehaviour {
	
	private MeshFilter meshFilter;
	
	GameObject[] controlPoints;
	GameObject[] start_controlPoints;
	float[,] W;
	float[,] A;
	
	float[,] A00, A01, A10, A11;
	Vector3[] D;
	
	Vector3[] start_vertices;
	
	ModeControl mode;
	// Use this for initialization
	void Start () {
		meshFilter = GetComponent<MeshFilter>();
		controlPoints = GameObject.FindGameObjectsWithTag("ControlPoint");
		start_controlPoints = GameObject.FindGameObjectsWithTag("ControlPoint");
		mode = GameObject.Find("ModeControl").GetComponent<ModeControl>();
		
		W = new float[meshFilter.mesh.vertexCount,controlPoints.Length];
		A = new float[meshFilter.mesh.vertexCount,controlPoints.Length];
		
		A00 = new float[meshFilter.mesh.vertexCount,controlPoints.Length];
		A01 = new float[meshFilter.mesh.vertexCount,controlPoints.Length];
		A10 = new float[meshFilter.mesh.vertexCount,controlPoints.Length];
		A11 = new float[meshFilter.mesh.vertexCount,controlPoints.Length];
		
		
		Vector3[] vertices = meshFilter.mesh.vertices;
		start_vertices = vertices;
		
		D = new Vector3[meshFilter.mesh.vertexCount];
		
		for(int i = 0 ; i < vertices.Length ; i++){
			vertices[i] = (Quaternion.AngleAxis(180.0f, Vector3.up) * Quaternion.AngleAxis(90.0f, Vector3.right)) * vertices[i];
		}
		meshFilter.mesh.vertices = vertices;
		meshFilter.mesh.RecalculateNormals();
		
		precompSimilarityMLS();
		precompAffinMLS();
	}
	
	// Update is called once per frame
	void Update () {
		
		switch(mode.mode){
		case ModeControl.Mode.AFFIN:
			AffinMLS();
			break;
		case ModeControl.Mode.SIMILARY:
			SimilarityMLS();
			break;
		case ModeControl.Mode.RIGID:
			RigidMLS();
			break;
		}

		if(Input.GetKeyDown(KeyCode.A)){
			Initialize();
		}
		
		if(Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow)){
			mode.ChangeMode();
			if(!Input.GetKey(KeyCode.LeftShift)){
				Initialize();
			}
		}
	}
	
	void Initialize(){
		meshFilter.mesh.vertices = start_vertices;
		meshFilter.mesh.RecalculateNormals();
		controlPoints = start_controlPoints;
		
		for(int i = 0 ; i < controlPoints.Length ; i++){
			controlPoints[i].GetComponent<ControlPoint>().InitializePosition();
		}
		
		switch(mode.mode){
		case ModeControl.Mode.AFFIN:
			precompAffinMLS();
			break;
		case ModeControl.Mode.SIMILARY:
		case ModeControl.Mode.RIGID:
			precompSimilarityMLS();
			break;
		}
	}
	
	void precompAffinMLS(){
		for(int i = 0 ; i < meshFilter.mesh.vertexCount ; i++){
	
			for(int j = 0 ; j < controlPoints.Length ; j++){
				Vector3 cpos = controlPoints[j].transform.position;
				cpos.z = 0.0f;
				W[i,j] = 1.0f / (0.01f + (cpos - meshFilter.mesh.vertices[i]).magnitude);
			}

			Vector3 r = Vector3.zero;
			float wsum = 0.0f;
			for(int j = 0 ; j < controlPoints.Length ; j++){
				Vector3 cpos = controlPoints[j].transform.position;
				cpos.z = 0.0f;
				r += cpos * W[i,j];
				wsum += W[i,j];
			}
			Vector3 pa = r / wsum;
			
			
			float m00 = 0.0f , m01 = 0.0f, m10 = 0.0f, m11 = 0.0f;
			for(int j = 0 ; j < controlPoints.Length ; j++){
				Vector3 cpos = controlPoints[j].transform.position;
				cpos.z = 0.0f;
				
				Vector3 ph = cpos - pa;
				m00 += (ph.x * W[i,j] * ph.x);
				m01 += (ph.x * W[i,j] * ph.y);
				m10 += (ph.y * W[i,j] * ph.x);
				m11 += (ph.y * W[i,j] * ph.y);
			}
			

			float det = m00 * m11 - m01 * m10;
			
			Vector3 lm = new Vector3(m11 / det, -m10 / det, 0.0f);
			Vector3 rm = new Vector3(-m01 / det, m00 / det, 0.0f);
			
			Vector3 t = meshFilter.mesh.vertices[i] - pa;
			Vector3 d = new Vector3(Vector3.Dot(t, lm), Vector3.Dot(t, rm), 0.0f);
			
			for(int j = 0 ; j < controlPoints.Length ; j++){
				Vector3 cpos = controlPoints[j].transform.position;
				cpos.z = 0.0f;
				A[i, j] = Vector3.Dot(d, cpos) * W[i,j];
			}
		}
	}
	
	void AffinMLS(){
		Vector3[] vertices = meshFilter.mesh.vertices;
		for(int i = 0 ; i < meshFilter.mesh.vertexCount ; i++){
			Vector3 r = Vector3.zero;
			float wsum = 0.0f;
			for(int j = 0 ; j < controlPoints.Length ; j++){
				Vector3 cpos = controlPoints[j].transform.position;
				cpos.z = 0.0f;
				r += cpos * W[i,j];
				wsum += W[i,j];
			}
			Vector3 qa = r / wsum;

			vertices[i] = qa;

			for(int j = 0 ; j < controlPoints.Length ; j++){
				Vector3 cpos = controlPoints[j].transform.position;
				cpos.z = 0.0f;
				vertices[i] += ((cpos - qa) * A[i,j]);
			}	
		}
		meshFilter.mesh.vertices = vertices;
	}
	
	void precompSimilarityMLS(){
		for(int i = 0 ; i < meshFilter.mesh.vertexCount ; i++){
	
			for(int j = 0 ; j < controlPoints.Length ; j++){
				Vector3 cpos = controlPoints[j].transform.position;
				cpos.z = 0.0f;
				W[i,j] = 1.0f / (0.01f + (cpos - meshFilter.mesh.vertices[i]).magnitude);
			}

			Vector3 r = Vector3.zero;
			float wsum = 0.0f;
			for(int j = 0 ; j < controlPoints.Length ; j++){
				Vector3 cpos = controlPoints[j].transform.position;
				cpos.z = 0.0f;
				r += cpos * W[i,j];
				wsum += W[i,j];
			}
			Vector3 pa = r / wsum;
			
			
			float mu = 0.0f;
			Vector3 ph = Vector3.zero;
			for(int j = 0 ; j < controlPoints.Length ; j++){
				Vector3 cpos = controlPoints[j].transform.position;
				cpos.z = 0.0f;
				
				ph = cpos - pa;
				mu += Vector3.Dot(ph, ph) * W[i,j];
			}
			
			D[i] = meshFilter.mesh.vertices[i] - pa;
			
			for(int j = 0 ; j < controlPoints.Length ; j++){
				Vector3 cpos = controlPoints[j].transform.position;
				cpos.z = 0.0f;
				Vector3 ph_ortho = new Vector3(-cpos.y, cpos.x , 0.0f);
				Vector3 d_ortho = new Vector3(-D[i].y, D[i].x, 0.0f);
				
				A00[i,j] = W[i,j] / mu * Vector3.Dot(cpos, D[i]);
				A01[i,j] = W[i,j] / mu * Vector3.Dot(cpos, d_ortho * -1.0f);
				A10[i,j] = W[i,j] / mu * Vector3.Dot(ph_ortho, D[i]) * -1.0f;
				A11[i,j] = W[i,j] / mu * Vector3.Dot(ph_ortho, d_ortho);
			}
		}
	}
		
	void SimilarityMLS(){
		Vector3[] vertices = meshFilter.mesh.vertices;
		for(int i = 0 ; i < meshFilter.mesh.vertexCount ; i++){
			Vector3 r = Vector3.zero;
			float wsum = 0.0f;
			for(int j = 0 ; j < controlPoints.Length ; j++){
				Vector3 cpos = controlPoints[j].transform.position;
				cpos.z = 0.0f;
				r += cpos * W[i,j];
				wsum += W[i,j];
			}
			Vector3 qa = r / wsum;

			vertices[i] = qa;

			for(int j = 0 ; j < controlPoints.Length ; j++){
				Vector3 cpos = controlPoints[j].transform.position;
				cpos.z = 0.0f;
				Vector3 qh = cpos - qa;
				vertices[i].x += Vector3.Dot(qh,new Vector3(A00[i,j], A10[i,j], 0.0f));
				vertices[i].y += Vector3.Dot(qh,new Vector3(A01[i,j], A11[i,j], 0.0f));
			}	
		}
		meshFilter.mesh.vertices = vertices;
	}
	
	void RigidMLS(){
		Vector3[] vertices = meshFilter.mesh.vertices;
		for(int i = 0 ; i < meshFilter.mesh.vertexCount ; i++){
			Vector3 r = Vector3.zero;
			float wsum = 0.0f;
			for(int j = 0 ; j < controlPoints.Length ; j++){
				Vector3 cpos = controlPoints[j].transform.position;
				cpos.z = 0.0f;
				r += cpos * W[i,j];
				wsum += W[i,j];
			}
			Vector3 qa = r / wsum;

			vertices[i] = qa;
			
			Vector3 f = Vector3.zero;
			for(int j = 0 ; j < controlPoints.Length ; j++){
				Vector3 cpos = controlPoints[j].transform.position;
				cpos.z = 0.0f;
				Vector3 qh = cpos - qa;
				f.x += Vector3.Dot(qh,new Vector3(A00[i,j], A10[i,j], 0.0f));
				f.y += Vector3.Dot(qh,new Vector3(A01[i,j], A11[i,j], 0.0f));
			}
			
			vertices[i] += f * D[i].magnitude / (f.magnitude + 0.01f);
		}
		meshFilter.mesh.vertices = vertices;
	}
}
