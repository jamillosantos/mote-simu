using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This code was adapted from https://github.com/hammmm/unity-obj-loader, check the URL for more information
/// about the author and LICENSE.
/// </summary>
public class GeometryBuffer
{

	private List<ObjectData> _objects;
	public List<Vector3> _vertices;
	public List<Vector2> _uvs;
	public List<Vector3> _normals;

	public Vector3 Max;
	public Vector3 Min;

	public int unnamedGroupIndex = 1; // naming index for unnamed group. like "Unnamed-1"

	private ObjectData _current;

	private class ObjectData
	{
		public string Name;
		public List<GroupData> Groups;
		public List<FaceIndices> AllFaces;
		public int NormalCount;

		public ObjectData()
		{
			this.Groups = new List<GroupData>();
			this.AllFaces = new List<FaceIndices>();
			this.NormalCount = 0;
		}
	}

	private GroupData _curgr;

	private class GroupData
	{
		public string Name;
		public string MaterialName;
		public List<FaceIndices> Faces;

		public GroupData()
		{
			Faces = new List<FaceIndices>();
		}

		public bool isEmpty
		{
			get
			{
				return (Faces.Count == 0);
			}
		}
	}

	public GeometryBuffer()
	{
		this._objects = new List<ObjectData>();
		ObjectData d = new ObjectData()
		{
			Name = "default"
		};

		this._objects.Add(d);
		this._current = d;

		GroupData g = new GroupData();
		g.Name = "default";
		d.Groups.Add(g);
		_curgr = g;

		this._vertices = new List<Vector3>();
		this._uvs = new List<Vector2>();
		this._normals = new List<Vector3>();
	}

	public void PushObject(string name)
	{
		if (this.IsEmpty)
			this._objects.Remove(_current);

		ObjectData n = new ObjectData();
		n.Name = name;
		this._objects.Add(n);

		GroupData g = new GroupData();
		g.Name = "default";
		n.Groups.Add(g);

		this._curgr = g;
		this._current = n;
	}

	public void PushGroup(string name)
	{
		if (this._curgr.isEmpty)
			this._current.Groups.Remove(_curgr);
		GroupData g = new GroupData();
		if (name == null)
		{
			name = "Unnamed-" + (this.unnamedGroupIndex++);
		}
		g.Name = name;
		this._current.Groups.Add(g);
		this._curgr = g;
	}

	public void PushMaterialName(string name)
	{
		//Debug.Log("Pushing new material " + name + " with curgr.empty=" + curgr.isEmpty);
		if (!this._curgr.isEmpty)
			this.PushGroup(name);
		if (this._curgr.Name == "default")
			this._curgr.Name = name;
		this._curgr.MaterialName = name;
	}

	public void PushVertex(Vector3 v)
	{
		this._vertices.Add(v);
	}

	public void PushUV(Vector2 v)
	{
		this._uvs.Add(v);
	}

	public void PushNormal(Vector3 v)
	{
		this._normals.Add(v);
	}

	public void PushFace(FaceIndices f)
	{
		this._curgr.Faces.Add(f);
		this._current.AllFaces.Add(f);
		if (f.vn >= 0)
			this._current.NormalCount++;
	}

	public void Trace()
	{
		Debug.Log("OBJ has " + this._objects.Count + " object(s)");
		Debug.Log("OBJ has " + this._vertices.Count + " vertice(s)");
		Debug.Log("OBJ has " + this._uvs.Count + " uv(s)");
		Debug.Log("OBJ has " + this._normals.Count + " normal(s)");
		foreach (ObjectData od in this._objects)
		{
			Debug.Log(od.Name + " has " + od.Groups.Count + " group(s)");
			foreach (GroupData gd in od.Groups)
			{
				Debug.Log(od.Name + "/" + gd.Name + " has " + gd.Faces.Count + " faces(s)");
			}
		}

	}

	public int NumObjects
	{
		get
		{
			return this._objects.Count;
		}
	}
	public bool IsEmpty
	{
		get
		{
			return this._vertices.Count == 0;
		}
	}

	public bool HasUVs
	{
		get
		{
			return this._uvs.Count > 0;
		}
	}

	public bool HasNormals
	{
		get
		{
			return this._normals.Count > 0;
		}
	}

	public static int MAX_VERTICES_LIMIT_FOR_A_MESH = 64999;

	public void PopulateMeshes(GameObject[] gs, Dictionary<string, Material> mats)
	{
		if (gs.Length != NumObjects)
		{
			Debug.LogWarning("The OBJ might be corrupted.");
			return; // Should not happen unless obj file is corrupt...
		}

		for (int i = 0; i < gs.Length; i++)
		{
			ObjectData od = _objects[i];
			bool objectHasNormals = (this.HasNormals && od.NormalCount > 0);

			if (od.Name != "default")
				gs[i].name = od.Name;

			Vector3[] tvertices = new Vector3[od.AllFaces.Count];
			Vector2[] tuvs = new Vector2[od.AllFaces.Count];
			Vector3[] tnormals = new Vector3[od.AllFaces.Count];

			int k = 0;
			foreach (FaceIndices fi in od.AllFaces)
			{
				if (k >= MAX_VERTICES_LIMIT_FOR_A_MESH)
				{
					Debug.LogWarning("maximum vertex number for a mesh exceeded for object:" + gs[i].name);
					break;
				}
				tvertices[k] = _vertices[fi.vi];
				if (this.HasUVs)
					tuvs[k] = this._uvs[fi.vu];
				if (this.HasNormals && fi.vn >= 0)
					tnormals[k] = this._normals[fi.vn];
				k++;
			}

			Mesh m = (gs[i].GetComponent(typeof(MeshFilter)) as MeshFilter).mesh;
			m.vertices = tvertices;
			if (this.HasUVs)
				m.uv = tuvs;
			if (objectHasNormals)
				m.normals = tnormals;

			if (od.Groups.Count == 1)
			{
				GroupData gd = od.Groups[0];
				string matName = (gd.MaterialName != null) ? gd.MaterialName : "default"; // MAYBE: "default" may not enough.
				if (mats.ContainsKey(matName))
					gs[i].GetComponent<Renderer>().material = mats[matName];
				else
					Debug.LogWarning("PopulateMeshes mat:" + matName + " not found.");

				int[] triangles = new int[gd.Faces.Count];
				for (int j = 0; j < triangles.Length; j++)
					triangles[j] = j;

				m.triangles = triangles;
			}
			else
			{
				int gl = od.Groups.Count;
				Material[] materials = new Material[gl];
				m.subMeshCount = gl;
				int c = 0;

				for (int j = 0; j < gl; j++)
				{
					string matName = (od.Groups[j].MaterialName != null) ? od.Groups[j].MaterialName : "default"; // MAYBE: "default" may not enough.
					if (mats.ContainsKey(matName))
						materials[j] = mats[matName];
					else
						Debug.LogWarning("PopulateMeshes mat:" + matName + " not found.");

					int[] triangles = new int[od.Groups[j].Faces.Count];
					int l = od.Groups[j].Faces.Count + c;
					int s = 0;
					for (; c < l; c++, s++)
						triangles[s] = c;
					m.SetTriangles(triangles, j);
				}

				gs[i].GetComponent<Renderer>().materials = materials;
			}
			if (!objectHasNormals)
				m.RecalculateNormals();
		}
	}
}