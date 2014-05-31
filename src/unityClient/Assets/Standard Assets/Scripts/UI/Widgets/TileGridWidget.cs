using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TileGridWidget : Widget
{
    public string Name { get; private set; }
    public float Depth { get; private set; }
    public uint TileRows { get; private set; }
    public uint TileColomns { get; private set; }
    public float TilePixelWidth { get; private set; }
    public float TilePixelHeight { get; private set; }
    public string TileSet { get; private set; }

    protected GameObject m_gameObject;
    protected MeshFilter m_meshFilter;
    protected MeshRenderer m_meshRenderer;
    protected Mesh m_mesh;
    protected Material m_material;

    public TileGridWidget(WidgetGroup parentGroup, string name, float width, float height, float depth, float x, float y) :
        base(parentGroup, width, height, x, y)
    {
        Name = name;
        TileRows = 0;
        TileColomns = 0;
        TilePixelWidth = 0;
        TilePixelHeight = 0;
        TileSet = "";
        Depth = depth;
    }

    public override void OnDestroy()
    {
        ClearMap();
        base.OnDestroy();
    }

	public void LoadMap(
        int[] tileIndices, 
        uint tileRows, 
        uint tileColomns, 
		string tileSetMaterialName, 
        float tilePixelWidth, 
        float tilePixelHeight)
    {
        ClearMap();

        TileRows = tileRows;
        TileColomns = tileColomns;
        TilePixelWidth = tilePixelWidth;
        TilePixelHeight = tilePixelHeight;
        TileSet = tileSetMaterialName;

        m_gameObject = 
            new GameObject(
                "TileGrid_" + this.Name + "_" + tileSetMaterialName, 
                typeof(MeshRenderer), 
                typeof(MeshFilter));
        m_gameObject.transform.position = Vector3.zero;
        m_gameObject.transform.rotation = Quaternion.identity;

        m_meshFilter = m_gameObject.GetComponent<MeshFilter>();
        m_meshRenderer = m_gameObject.GetComponent<MeshRenderer>();

        m_material = Resources.Load<Material>("Gfx/TileSets/" + tileSetMaterialName+"_material");
        m_meshRenderer.renderer.material = m_material;
        m_mesh = m_meshFilter.mesh;

        // Generate a regular grid of vertices
        // Top to Bottom, Left to Right
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> UVs = new List<Vector2>();
            List<int> indices = new List<int>();
            uint tileListIndex = 0;

            for (int rowIndex = 0; rowIndex < tileRows; ++rowIndex)
            {
                for (int colIndex = 0; colIndex < tileColomns; ++colIndex)
                {
                    int tileIndex = tileIndices[tileListIndex];

                    if (tileListIndex >= 0)
                    {
                        int startVertexIndex = vertices.Count;

                        // Create 4 vertices in a square, starting from the upper left, going clockwise
                        vertices.Add(
                            ClientGameConstants.ConvertPixelPositionToVertexPosition(
                                this.WorldX + (float)(TilePixelWidth * colIndex),
                                this.WorldY + (float)(TilePixelHeight * rowIndex),
                                this.Depth));
                        vertices.Add(
                            ClientGameConstants.ConvertPixelPositionToVertexPosition(
                                this.WorldX + (float)(TilePixelWidth * (colIndex+1)),
                                this.WorldY + (float)(TilePixelHeight * rowIndex),
                                this.Depth));
                        vertices.Add(
                            ClientGameConstants.ConvertPixelPositionToVertexPosition(
                                this.WorldX + (float)(TilePixelWidth * (colIndex + 1)),
                                this.WorldY + (float)(TilePixelHeight * (rowIndex + 1)),
                                this.Depth));
                        vertices.Add(
                            ClientGameConstants.ConvertPixelPositionToVertexPosition(
                                this.WorldX + (float)(TilePixelWidth * colIndex),
                                this.WorldY + (float)(TilePixelHeight * (rowIndex + 1)),
                                this.Depth));

                        // Create the UVs for each vertex
                        {
                            float tileTexelWidth= 0;
                            float tileTexelHeight= 0;
                            Vector2 tileTexelOrigin = 
                                ComputeTexelInfoForTileIndex(
                                    tileIndex, out tileTexelWidth, out tileTexelHeight);

                            // NOTE: UVs origin is in the lower left of the texture
                            //  _______(1,1)
                            //  |      |
                            //  |      |
                            //  |______|
                            // (0,0)
                            UVs.Add(tileTexelOrigin);
                            UVs.Add(new Vector2(tileTexelOrigin.x + tileTexelWidth, tileTexelOrigin.y));
                            UVs.Add(new Vector2(tileTexelOrigin.x + tileTexelWidth, tileTexelOrigin.y - tileTexelHeight));
                            UVs.Add(new Vector2(tileTexelOrigin.x, tileTexelOrigin.y - tileTexelHeight));
                        }


                        // Create the indices for the two triangles that make up the square
                        indices.Add(startVertexIndex+0);
                        indices.Add(startVertexIndex+1);
                        indices.Add(startVertexIndex+2);
                        indices.Add(startVertexIndex+0);
                        indices.Add(startVertexIndex+2);
                        indices.Add(startVertexIndex+3);
                    }

                    tileListIndex++;
                }
            }

            m_mesh.vertices = vertices.ToArray();
            m_mesh.uv = UVs.ToArray();
            m_mesh.triangles = indices.ToArray();
        }
    }

    public void ClearMap()
    {
        TileRows = 0;
        TileColomns = 0;
        TilePixelWidth = 0;
        TilePixelHeight = 0;
        TileSet = "";

        if (m_gameObject != null)
        {
            GameObject.Destroy(m_gameObject);
        }

        m_gameObject = null;
        m_meshFilter = null;
        m_meshRenderer = null;
        m_material = null;
    }

    private Vector2 ComputeTexelInfoForTileIndex(
        int tileIndex, 
        out float tileTexelWidth, 
        out float tileTexelHeight)
    {
        int tilesPerRow = m_material.mainTexture.width / (int)Math.Floor(this.TilePixelWidth);
        float textureRowIndex = (float)(tileIndex / tilesPerRow);
        float textureColIndex = (float)(tileIndex % tilesPerRow);

        float textureWidth = (float)m_material.mainTexture.width;
        float textureHeight = (float)m_material.mainTexture.height;

        tileTexelWidth = this.TilePixelWidth / textureWidth;
        tileTexelHeight = this.TilePixelHeight / textureHeight;

        // NOTE: UVs origin is in the lower left of the texture
        //  _______(1,1)
        //  |      |
        //  |      |
        //  |______|
        // (0,0)
        return new Vector2(
            (TilePixelWidth*textureColIndex) / textureWidth,
            (textureHeight - (TilePixelHeight*textureRowIndex - 1.0f)) / textureHeight);
    }
}
