using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

/// <summary>
/// This code was adapted from https://github.com/hammmm/unity-obj-loader, check the URL for more information
/// about the author and LICENSE.
/// </summary>
public class OBJ : MonoBehaviour
{

	public string ObjPath;

	/* OBJ file tags */
	private const string O = "o";
	private const string G = "g";
	private const string V = "v";
	private const string VT = "vt";
	private const string VN = "vn";
	private const string F = "f";
	private const string MTL = "mtllib";
	private const string UML = "usemtl";

	/* MTL file tags */
	private const string NML = "newmtl";
	private const string NS = "Ns"; // Shininess
	private const string KA = "Ka"; // Ambient component (not supported)
	private const string KD = "Kd"; // Diffuse component
	private const string KS = "Ks"; // Specular component
	private const string D = "d";   // Transparency (not supported)
	private const string TR = "Tr"; // Same as 'd'
	private const string ILLUM = "illum"; // Illumination model. 1 - diffuse, 2 - specular
	private const string MAP_KA = "map_Ka"; // Ambient texture
	private const string MAP_KD = "map_Kd"; // Diffuse texture
	private const string MAP_KS = "map_Ks"; // Specular texture
	private const string MAP_KE = "map_Ke"; // Emissive texture
	private const string MAP_BUMP = "map_bump"; // Bump map texture
	private const string BUMP = "bump"; // Bump map texture

	private string basepath;
	private string mtllib;
	private GeometryBuffer _buffer;

	void Start()
	{
		this._buffer = new GeometryBuffer();
		this.Load(this.ObjPath);
	}

	public void Load(string filePath)
	{
		this.SetGeometryData(filePath);

		if (this._hasMaterials)
		{
			this.SetMaterialData(Path.Combine(this.basepath, this.mtllib));

			foreach (MaterialData m in _materialData)
			{
				if (m.DiffuseTexPath != null)
				{
					byte[] texbytes = GetTextureLoader(m, Path.Combine(this.basepath, m.DiffuseTexPath));
					Texture2D tex = new Texture2D(1, 1);
					tex.LoadImage(texbytes);
					m.DiffuseTex = tex;
				}
				if (m.BumpTexPath != null)
				{
					byte[] texbytes = GetTextureLoader(m, Path.Combine(this.basepath, m.BumpTexPath));
					Texture2D tex = new Texture2D(1, 1);
					tex.LoadImage(texbytes);
					m.BumpTex = tex;
				}
			}
		}
		this.Build();
	}

	private byte[] GetTextureLoader(MaterialData m, string texpath)
	{
		return File.ReadAllBytes(texpath);
	}

	private void GetFaceIndicesByOneFaceLine(FaceIndices[] faces, string[] p, bool isFaceIndexPlus)
	{
		if (isFaceIndexPlus)
		{
			for (int j = 1; j < p.Length; j++)
			{
				string[] c = p[j].Trim().Split("/".ToCharArray());
				FaceIndices fi = new FaceIndices();
				// vertex
				int vi = ci(c[0]);
				fi.vi = vi - 1;
				// uv
				if (c.Length > 1 && c[1] != "")
				{
					int vu = ci(c[1]);
					fi.vu = vu - 1;
				}
				// normal
				if (c.Length > 2 && c[2] != "")
				{
					int vn = ci(c[2]);
					fi.vn = vn - 1;
				}
				else {
					fi.vn = -1;
				}
				faces[j - 1] = fi;
			}
		}
		else { // for minus index
			int vertexCount = _buffer._vertices.Count;
			int uvCount = _buffer._uvs.Count;
			for (int j = 1; j < p.Length; j++)
			{
				string[] c = p[j].Trim().Split("/".ToCharArray());
				FaceIndices fi = new FaceIndices();
				// vertex
				int vi = ci(c[0]);
				fi.vi = vertexCount + vi;
				// uv
				if (c.Length > 1 && c[1] != "")
				{
					int vu = ci(c[1]);
					fi.vu = uvCount + vu;
				}
				// normal
				if (c.Length > 2 && c[2] != "")
				{
					int vn = ci(c[2]);
					fi.vn = vertexCount + vn;
				}
				else {
					fi.vn = -1;
				}
				faces[j - 1] = fi;
			}
		}
	}

	private void SetGeometryData(string filePath)
	{
		this.basepath = Path.GetDirectoryName(filePath);

		Stream fileStream = new FileStream(filePath, FileMode.Open);
		StreamReader streamReader = new StreamReader(fileStream);

		Regex regexWhitespaces = new Regex(@"\s+");
		bool isFirstInGroup = true;
		bool isFaceIndexPlus = true;
		int i = 0;
		for (string l = streamReader.ReadLine(); l != null; l = streamReader.ReadLine(), i++)
		{
			l = l.Trim();
			if (l.IndexOf("#") != -1)
				continue;

			string[] p = regexWhitespaces.Split(l);
			switch (p[0])
			{
				case O:
					this._buffer.PushObject(p[1].Trim());
					isFirstInGroup = true;
					break;
				case G:
					string groupName = null;
					if (p.Length >= 2)
						groupName = p[1].Trim();

					isFirstInGroup = true;
					this._buffer.PushGroup(groupName);
					break;
				case V:
					this._buffer.PushVertex(new Vector3(cf(p[1]) * -1, cf(p[2]), cf(p[3])));
					break;
				case VT:
					this._buffer.PushUV(new Vector2(cf(p[1]), cf(p[2])));
					break;
				case VN:
					this._buffer.PushNormal(new Vector3(cf(p[1]), cf(p[2]), cf(p[3])));
					break;
				case F:
					FaceIndices[] faces = new FaceIndices[p.Length - 1];
					if (isFirstInGroup)
					{
						isFirstInGroup = false;
						string[] c = p[1].Trim().Split("/".ToCharArray());
						isFaceIndexPlus = (ci(c[0]) >= 0);
					}
					this.GetFaceIndicesByOneFaceLine(faces, p, isFaceIndexPlus);
					if (p.Length == 4)
					{
						this._buffer.PushFace(faces[2]);
						this._buffer.PushFace(faces[1]);
						this._buffer.PushFace(faces[0]);
					}
					else if (p.Length == 5)
					{
						this._buffer.PushFace(faces[2]);
						this._buffer.PushFace(faces[1]);
						this._buffer.PushFace(faces[3]);
						this._buffer.PushFace(faces[3]);
						this._buffer.PushFace(faces[1]);
						this._buffer.PushFace(faces[0]);
					}
					else
					{
						Debug.LogWarning("face vertex count :" + (p.Length - 1) + " larger than 4:");
					}
					break;
				case MTL:
					mtllib = l.Substring(p[0].Length + 1).Trim();
					break;
				case UML:
					_buffer.PushMaterialName(p[1].Trim());
					break;
			}
		}
	}

	private float cf(string v)
	{
		try
		{
			return float.Parse(v);
		}
		catch (Exception e)
		{
			Debug.Log(e);
			return 0;
		}
	}

	private int ci(string v)
	{
		try
		{
			return int.Parse(v);
		}
		catch (Exception e)
		{
			print(e);
			return 0;
		}
	}

	private bool _hasMaterials
	{
		get
		{
			return mtllib != null;
		}
	}

	/* ############## MATERIALS */
	private List<MaterialData> _materialData;

	private class MaterialData
	{
		public string Name;
		public Color Ambient;
		public Color Diffuse;
		public Color Specular;
		public float Shininess;
		public float Alpha;
		public int IllumType;
		public string DiffuseTexPath;
		public string BumpTexPath;
		public Texture2D DiffuseTex;
		public Texture2D BumpTex;
	}

	private void SetMaterialData(string filePath)
	{

		Stream fileStream = new FileStream(filePath, FileMode.Open);
		StreamReader streamReader = new StreamReader(fileStream);

		this._materialData = new List<MaterialData>();
		MaterialData current = new MaterialData();
		Regex regexWhitespaces = new Regex(@"\s+");

		int i = 0;
		for (string l = streamReader.ReadLine(); l != null; l = streamReader.ReadLine(), i++)
		{
			l = l.Trim();
			if (l.IndexOf("#") != -1)
				l = l.Substring(0, l.IndexOf("#"));
			string[] p = regexWhitespaces.Split(l);
			if (p[0].Trim() == "")
				continue;

			switch (p[0])
			{
				case NML:
					current = new MaterialData();
					current.Name = p[1].Trim();
					this._materialData.Add(current);
					break;
				case KA:
					current.Ambient = gc(p);
					break;
				case KD:
					current.Diffuse = gc(p);
					break;
				case KS:
					current.Specular = gc(p);
					break;
				case NS:
					current.Shininess = cf(p[1]) / 1000;
					break;
				case D:
				case TR:
					current.Alpha = cf(p[1]);
					break;
				case MAP_KD:
					current.DiffuseTexPath = p[p.Length - 1].Trim();
					break;
				case MAP_BUMP:
				case BUMP:
					this.BumpParameter(current, p);
					break;
				case ILLUM:
					current.IllumType = ci(p[1]);
					break;
				default:
					Debug.Log("this line was not processed :" + l);
					break;
			}
		}
	}

	private Material GetMaterial(MaterialData md)
	{
		Material m;

		if (md.IllumType == 2)
		{
			string shaderName = (md.BumpTex != null) ? "Bumped Specular" : "Specular";
			m = new Material(Shader.Find(shaderName));
			m.SetColor("_SpecColor", md.Specular);
			m.SetFloat("_Shininess", md.Shininess);
		}
		else {
			string shaderName = (md.BumpTex != null) ? "Bumped Diffuse" : "Diffuse";
			m = new Material(Shader.Find(shaderName));
		}

		if (md.DiffuseTex != null)
		{
			m.SetTexture("_MainTex", md.DiffuseTex);
		}
		else {
			m.SetColor("_Color", md.Diffuse);
		}
		if (md.BumpTex != null) m.SetTexture("_BumpMap", md.BumpTex);

		m.name = md.Name;

		return m;
	}

	private class BumpParamDef
	{
		public string optionName;
		public string valueType;
		public int valueNumMin;
		public int valueNumMax;
		public BumpParamDef(string name, string type, int numMin, int numMax)
		{
			this.optionName = name;
			this.valueType = type;
			this.valueNumMin = numMin;
			this.valueNumMax = numMax;
		}
	}

	private void BumpParameter(MaterialData m, string[] p)
	{
		Regex regexNumber = new Regex(@"^[-+]?[0-9]*\.?[0-9]+$");

		var bumpParams = new Dictionary<String, BumpParamDef>();
		bumpParams.Add("bm", new BumpParamDef("bm", "string", 1, 1));
		bumpParams.Add("clamp", new BumpParamDef("clamp", "string", 1, 1));
		bumpParams.Add("blendu", new BumpParamDef("blendu", "string", 1, 1));
		bumpParams.Add("blendv", new BumpParamDef("blendv", "string", 1, 1));
		bumpParams.Add("imfchan", new BumpParamDef("imfchan", "string", 1, 1));
		bumpParams.Add("mm", new BumpParamDef("mm", "string", 1, 1));
		bumpParams.Add("o", new BumpParamDef("o", "number", 1, 3));
		bumpParams.Add("s", new BumpParamDef("s", "number", 1, 3));
		bumpParams.Add("t", new BumpParamDef("t", "number", 1, 3));
		bumpParams.Add("texres", new BumpParamDef("texres", "string", 1, 1));
		int pos = 1;
		string filename = null;
		while (pos < p.Length)
		{
			if (!p[pos].StartsWith("-"))
			{
				filename = p[pos];
				pos++;
				continue;
			}
			// option processing
			string optionName = p[pos].Substring(1);
			pos++;
			if (!bumpParams.ContainsKey(optionName))
			{
				continue;
			}
			BumpParamDef def = bumpParams[optionName];
			ArrayList args = new ArrayList();
			int i = 0;
			bool isOptionNotEnough = false;
			for (; i < def.valueNumMin; i++, pos++)
			{
				if (pos >= p.Length)
				{
					isOptionNotEnough = true;
					break;
				}
				if (def.valueType == "number")
				{
					Match match = regexNumber.Match(p[pos]);
					if (!match.Success)
					{
						isOptionNotEnough = true;
						break;
					}
				}
				args.Add(p[pos]);
			}
			if (isOptionNotEnough)
			{
				Debug.Log("bump variable value not enough for option:" + optionName + " of material:" + m.Name);
				continue;
			}
			for (; i < def.valueNumMax && pos < p.Length; i++, pos++)
			{
				if (def.valueType == "number")
				{
					Match match = regexNumber.Match(p[pos]);
					if (!match.Success)
					{
						break;
					}
				}
				args.Add(p[pos]);
			}
			// TODO: some processing of options
			Debug.Log("found option: " + optionName + " of material: " + m.Name + " args: " + String.Concat(args.ToArray()));
		}
		if (filename != null)
		{
			m.BumpTexPath = filename;
		}
	}

	private Color gc(string[] p)
	{
		return new Color(cf(p[1]), cf(p[2]), cf(p[3]));
	}

	private void Build()
	{
		Dictionary<string, Material> materials = new Dictionary<string, Material>();

		if (_hasMaterials)
		{
			foreach (MaterialData md in _materialData)
			{
				if (materials.ContainsKey(md.Name))
				{
					Debug.LogWarning("duplicate material found: " + md.Name + ". ignored repeated occurences");
					continue;
				}
				materials.Add(md.Name, GetMaterial(md));
			}
		}
		else
			materials.Add("default", new Material(Shader.Find("VertexLit")));

		GameObject[] ms = new GameObject[_buffer.NumObjects];

		if (_buffer.NumObjects == 1)
		{
			gameObject.AddComponent(typeof(MeshFilter));
			gameObject.AddComponent(typeof(MeshRenderer));
			ms[0] = gameObject;
		}
		else if (_buffer.NumObjects > 1)
		{
			for (int i = 0; i < _buffer.NumObjects; i++)
			{
				GameObject go = new GameObject();
				// go.transform.localScale = new Vector3(-1, 1, 1);
				go.transform.parent = gameObject.transform;
				go.AddComponent(typeof(MeshFilter));
				go.AddComponent(typeof(MeshRenderer));
				ms[i] = go;
			}
		}
		_buffer.PopulateMeshes(ms, materials);

		MeshFilter[] meshes = this.GetComponentsInChildren<MeshFilter>();
		Vector3 sum = Vector3.zero;
		foreach (MeshFilter mf in meshes)
		{
			sum += -(new Vector3(mf.mesh.bounds.center.x * this.transform.localScale.x, mf.mesh.bounds.center.y * this.transform.localScale.y, mf.mesh.bounds.center.z * this.transform.localScale.z));
		}
		this.transform.localPosition = sum/meshes.Length;
	}
}
