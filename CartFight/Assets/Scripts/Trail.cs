using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Thanks to answers.unity3d.com/questions/461425/trail-rendering-for-car-skid-marks.html for this!
public class Trail : MonoBehaviour 
{
	//Trail properties.
	private float width;
	private float decayTime;
	private Material mat;
	private int rough; //Roughness timer.
	private int maxRough; //Determines how often to generate segments.
	private bool softSource;

	private Transform parent;

	//Mesh generation stuff.
	private GameObject trail;
	private MeshFilter filter;
	private MeshRenderer renderer;
	private Mesh mesh;

	//Lists for mesh data.
	private LinkedList<Vector2> verts = new LinkedList<Vector2> ();
	private LinkedList<Vector2> uvs = new LinkedList<Vector2> ();
	private LinkedList<int> tris = new LinkedList<int> ();
	private LinkedList<Color> cols = new LinkedList<Color> ();

	//For checking if the trail is still being generated.
	private bool finished = false;
	public bool Finished { get { return this.finished; } }

	//For checking if the trail faded out (ie: run out of blood).
	private bool dead = false;
	public bool Dead
	{
		get { return dead; }
		private set { dead = true; GameObject.Destroy (trail); }
	}

	//Constructor.
	public Trail(Transform parent, Material material, float decayTime, 
		int roughness, bool softSourceEdges, float width = 0.1f)
	{
		this.parent = parent;
		this.mat = material;
		this.decayTime = decayTime;
		this.maxRough = roughness;
		this.softSource = softSourceEdges;
		this.width = width;

		this.trail = new GameObject ("Trail");
		this.filter = trail.AddComponent (typeof(MeshFilter)) as MeshFilter;
		this.renderer = trail.AddComponent (typeof(MeshRenderer)) as MeshRenderer;
		this.mesh = new Mesh ();

		this.renderer.material = mat;
		this.filter.mesh = mesh;
	}

	//Called when the trail should stop emitting.
	public void Finish()
	{
		finished = true;
	}

	//Updates the trail. Must be called manually.
	public void Update()
	{
		if (!finished) 
		{
			if (rough > 0) 
			{
				rough--;
			} 
			else 
			{
				rough = maxRough;

				//Checks in which order to add verts. Keeps a consistent shape.
				bool odd = !(verts.Count % 4 == 0);

				//Add new verts as the current position.
				verts.AddLast (parent.position + (odd ? -1 : 1) * parent.up * width / 2f);
				verts.AddLast (parent.position + (odd ? 1 : -1) * parent.up * width / 2f);

				//Fades out the newest verts if soft source edges is true.
				if (softSource) 
				{
					if (cols.Count >= 4) 
					{
						cols.Last.Value = Color.white;
						cols.Last.Previous.Value = Color.white;
					}
					cols.AddLast (Color.clear);
					cols.AddLast (Color.clear);
				}
				else //Fade the first vert, but not the others.
				{
					if (cols.Count >= 2) 
					{
						cols.AddLast (Color.white);
						cols.AddLast (Color.white);
					}
					else
					{
						cols.AddLast (Color.clear);
						cols.AddLast (Color.clear);
					}
				}


				if (!odd) //Set up UV mapping. 
				{
					uvs.AddLast (new Vector2 (1, 0));
					uvs.AddLast (new Vector2 (0, 0));
				} 
				else 
				{
					uvs.AddLast (new Vector2 (0, 1));
					uvs.AddLast (new Vector2 (1, 1));
				}


				//Don't draw the trail unless there's enough verts.
				if (verts.Count < 4)
					return;

				//Add new triangles to the mesh.
				int c = verts.Count;
				tris.AddLast (c - 4);
				tris.AddLast (c - 3);
				tris.AddLast (c - 2);
				tris.AddLast (c - 4);
				tris.AddLast (c - 2);
				tris.AddLast (c - 1);

				//Copy lists to arrays, ready to rebuild the mesh.
				Vector2[] v = new Vector2[c];
				Vector2[] uv = new Vector2[c];
				int[] t = new int[tris.Count];
				verts.CopyTo (v, 0);
				uvs.CopyTo (uv, 0);
				tris.CopyTo (t, 0);

				//Build the mesh.
				Vector3[] vertsTemp = new Vector3[v.Length];
				for (int j = 0; j < vertsTemp.Length; j++)
				{
					vertsTemp [j] = (Vector3)v [j];
				}
				mesh.vertices = vertsTemp;
				mesh.triangles = t;
				mesh.uv = uv;
			}
		}

		//The next section updates the colors in the mesh.
		int i = cols.Count;

		//If no verts, don't update.
		if (i == 0)
			return;

		//Check if the trail has faded out or not.
		bool alive = false;

		//Loop over the colors but allow editing.
		LinkedListNode<Color> d = cols.First;
		do 
		{
			if (d.Value.a > 0)
			{
				Color t = d.Value;
				alive = true;

				//Decrease the alpha.
				t.a -= Mathf.Min (Time.deltaTime / decayTime, t.a);
				d.Value = t;
			}
			d = d.Next;
		} while (d != null);

		//Remove trail if not emitting and faded out.
		if (!alive && finished) 
		{
			Dead = true;
		}
		else 
		{
			//Don't set color if number of verts is mismatched.
			if (i != mesh.vertices.Length)
				return;

			//Copy colors to an array and build the mesh colors.
			Color[] cs = new Color[i];
			cols.CopyTo (cs, 0);
			mesh.colors = cs;
		}
	}
}
