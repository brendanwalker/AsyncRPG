using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;

public class VisionConeWidget : Widget
{
    private static Vector3 zVector = new Vector3(0.0f, 0.0f, 1.0f);
    private static Color transparentWhite = new Color(1.0f, 1.0f, 1.0f, 0.5f);
    private static Vector2 textureCenter = new Vector2(0.5f, 0.5f);

    public string Name { get; private set; }
    public float Depth { get; private set; }
    public float Range { get; private set; } // Room Coordinate Scale
    public float ConeAngle { get; private set; } // Degrees
    public float ConeFacing // Degrees
    {
        get { 
            return m_facingAngle; 
        }

        set { 
            m_facingAngle = value;

            if (m_gameObject != null)
            {
                m_gameObject.transform.rotation = Quaternion.AngleAxis(value, new Vector3(0.0f, 0.0f, -1.0f));
            }
        }
    }

    protected GameObject m_gameObject;
    protected MeshFilter m_meshFilter;
    protected MeshRenderer m_meshRenderer;
    protected Mesh m_mesh;
    protected Material m_material;
    protected float m_facingAngle;

    public VisionConeWidget(WidgetGroup parentGroup, string name, float range, float coneAngle, float x, float y, float z) :
        base(parentGroup, 0, 0, x, y)
    {
        Name = name;
        Range = range;
        ConeAngle = coneAngle;
        Depth = z;

        CreateMesh();
        UpdateWorldPosition();
    }

    public override void OnDestroy()
    {
        ClearMesh();
        base.OnDestroy();
    }

	private void CreateMesh()
    {
        ClearMesh();

        m_gameObject = new GameObject("VisionCone_" + this.Name, typeof(MeshRenderer), typeof(MeshFilter));
        m_gameObject.transform.position = Vector3.zero;
        m_gameObject.transform.rotation = Quaternion.identity;

        m_meshFilter = m_gameObject.GetComponent<MeshFilter>();
        m_meshRenderer = m_gameObject.GetComponent<MeshRenderer>();

        m_material = Resources.Load<Material>("Gfx/Materials/VisionCone");
        m_meshRenderer.renderer.material = m_material;
        m_mesh = m_meshFilter.mesh;

        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> UVs = new List<Vector2>();
            List<int> indices = new List<int>();
            List<Color> colors = new List<Color>();
            int vertexCount = 0;
            
            float coneHalfRadians= (ConeAngle / 2.0f) * MathConstants.DEGREES_TO_RADIANS;
            float angleStep = coneHalfRadians / 10;

            Point3d originRoomPoint = new Point3d(0.0f, 0.0f, this.Depth);
            Point3d currentRoomPoint = new Point3d();
            Point3d nextRoomPoint = new Point3d();

            Vector3 vertexOffset= ClientGameConstants.ConvertRoomPositionToVertexPosition(originRoomPoint); 

            for (float angle = -coneHalfRadians; angle < coneHalfRadians; angle+= angleStep)
            {
                float nextAngle = angle + angleStep;
                float cosAngle = (float)Math.Cos((float)angle);
                float sinAngle = (float)Math.Sin((float)angle);
                float cosNextAngle = (float)Math.Cos((float)nextAngle);
                float sinNextAngle = (float)Math.Sin((float)nextAngle);

                currentRoomPoint.Set(Range * cosAngle, Range * sinAngle, Depth);
                nextRoomPoint.Set(Range * cosNextAngle, Range * sinNextAngle, Depth);

                Vector3 currentVertex = ClientGameConstants.ConvertRoomPositionToVertexPosition(currentRoomPoint) - vertexOffset;
                Vector3 nextVertex = ClientGameConstants.ConvertRoomPositionToVertexPosition(nextRoomPoint) - vertexOffset;

                // Pie wedges
                {
                    // Create a triangle for each pie slice
                    vertices.Add(Vector3.zero);
                    vertices.Add(currentVertex);
                    vertices.Add(nextVertex);

                    // All normals face the camera
                    normals.Add(zVector);
                    normals.Add(zVector);
                    normals.Add(zVector);

                    // Make the visibility arc partially transparent
                    colors.Add(transparentWhite);
                    colors.Add(transparentWhite);
                    colors.Add(transparentWhite);

                    // Create the UVs for each vertex

                    // NOTE: UVs origin is in the lower left of the texture
                    //  _______(1,1)
                    //  |      |
                    //  |      |
                    //  |______|
                    // (0,0)
                    UVs.Add(textureCenter);
                    UVs.Add(new Vector2(0.5f + 0.5f * cosAngle, 0.5f + 0.5f * sinAngle));
                    UVs.Add(new Vector2(0.5f + 0.5f * cosNextAngle, 0.5f + 0.5f * sinNextAngle));


                    // Create the indices for the triangle that makes up the pie slice
                    indices.Add(vertexCount++);
                    indices.Add(vertexCount++);
                    indices.Add(vertexCount++);
                }

                // Edge highlight
                {
                    // Create a triangle for each pie slice
                    vertices.Add(currentVertex * 0.99f);
                    vertices.Add(currentVertex);
                    vertices.Add(nextVertex);
                    vertices.Add(currentVertex * 0.99f);
                    vertices.Add(nextVertex);
                    vertices.Add(nextVertex * 0.99f);

                    // All normals face the camera
                    normals.Add(zVector);
                    normals.Add(zVector);
                    normals.Add(zVector);
                    normals.Add(zVector);
                    normals.Add(zVector);
                    normals.Add(zVector);

                    // Make the visibility arc partially transparent
                    colors.Add(transparentWhite);
                    colors.Add(transparentWhite);
                    colors.Add(transparentWhite);
                    colors.Add(transparentWhite);
                    colors.Add(transparentWhite);
                    colors.Add(transparentWhite);

                    UVs.Add(textureCenter);
                    UVs.Add(textureCenter);
                    UVs.Add(textureCenter);
                    UVs.Add(textureCenter);
                    UVs.Add(textureCenter);
                    UVs.Add(textureCenter);

                    // Create the indices for the triangle that makes up the pie slice
                    indices.Add(vertexCount++);
                    indices.Add(vertexCount++);
                    indices.Add(vertexCount++);
                    indices.Add(vertexCount++);
                    indices.Add(vertexCount++);
                    indices.Add(vertexCount++);
                }
            }

            m_mesh.vertices = vertices.ToArray();
            m_mesh.uv = UVs.ToArray();
            m_mesh.triangles = indices.ToArray();
            m_mesh.normals = normals.ToArray();
            m_mesh.colors= colors.ToArray();
        }
    }

    private void ClearMesh()
    {
        if (m_gameObject != null)
        {
            GameObject.Destroy(m_gameObject);
        }

        m_gameObject = null;
        m_meshFilter = null;
        m_meshRenderer = null;
        m_material = null;
    }

    public override void UpdateWorldPosition()
    {
        base.UpdateWorldPosition();

        if (m_gameObject != null)
        {
            m_gameObject.transform.position =
                ClientGameConstants.ConvertPixelPositionToVertexPosition(this.WorldX, this.WorldY, this.Depth);
        }
    }

    public void SetVisible(bool visible)
    {
        m_meshRenderer.enabled = visible;
        this.Visible = visible;
    }
}
