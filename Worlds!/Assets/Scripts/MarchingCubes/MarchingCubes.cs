using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProceduralTerrain
{
	namespace MarchingCubes
	{
		public class MarchingCubes
		{

			public ComputeShader m_marchingCubesShader;
			public ComputeShader m_clearVerticesShader;
			public ComputeShader m_calculateNormalsShader;
            public Material m_drawMesh;
			public float m_offset = 0.0f;
			public bool m_recalculateNormals = false;

			int m_x_dim;
			int m_y_dim;
			int m_z_dim;

			float m_scale;
			int m_maxVertices;

			int m_lod;
			int m_x_lod_dim;
			int m_y_lod_dim;
			int m_z_lod_dim;
			
			ComputeBuffer m_cubeEdgeFlags, m_traingleConnectionTable;
			ComputeBuffer m_meshBuffer;
			ComputeBuffer m_densityBuffer, m_ChunkXdensityBuffer, m_ChunkYdensityBuffer, m_ChunkZdensityBuffer,
											m_ChunkXYdensityBuffer, m_ChunkYZdensityBuffer, m_ChunkXZdensityBuffer,
											m_ChunkXYZdensityBuffer;

			RenderTexture m_normalsTexture;

			#pragma warning disable 0649
			struct Vertex
			{
				
				public Vector4 position;
				public Vector3 normal;
				
			};
			#pragma warning restore 0649

			public void Initalize(int x_dim, int y_dim, int z_dim, float size, int lod)
			{
				//each dimension must be divisable by 8, densitymap and MC shader must be present
				if(x_dim % 8 != 0 || y_dim % 8 != 0 || z_dim % 8 != 0) throw new System.ArgumentException("x, y or z must be divisible by 8");
				if(lod < 0 || lod > 3) throw new System.ArgumentException("lewel-of-detail must be in range 0 to 3");
				if(m_marchingCubesShader == null) throw new System.ArgumentException("Missing MarchingCubesShader");
				if(m_clearVerticesShader == null) throw new System.ArgumentException("Missing ClearVerticesShader");
				if(m_calculateNormalsShader == null) throw new System.ArgumentException("Missing CalculateNormalsShader");

                //CameraPostRender.AddEvent(Camera.main, DrawMesh);

				m_x_dim = x_dim;
				m_y_dim = y_dim;
				m_z_dim = z_dim;

				m_scale = size;

				m_lod = (int)Mathf.Pow(2, lod);
				m_x_lod_dim = x_dim / m_lod;
				m_y_lod_dim = y_dim / m_lod;
				m_z_lod_dim = z_dim / m_lod;
				
				//max verts
				m_maxVertices = m_x_lod_dim * m_y_lod_dim * m_z_lod_dim * 5 * 3;

				//MarchingCubes
				//tables
				m_cubeEdgeFlags = new ComputeBuffer(256, sizeof(int));
				m_cubeEdgeFlags.SetData(MarchingCubesTables.CubeEdgeFlags);
				m_traingleConnectionTable = new ComputeBuffer(256 * 16, sizeof(int));
				m_traingleConnectionTable.SetData(MarchingCubesTables.TriangleConnectionTable);

				//output mesh. 
				m_meshBuffer = new ComputeBuffer(m_maxVertices, sizeof(float) * 7);
				m_densityBuffer = new ComputeBuffer(m_x_dim * m_y_dim * m_z_dim, sizeof(float));
				m_ChunkXdensityBuffer = new ComputeBuffer(2 * m_y_dim * m_z_dim, sizeof(float));
				m_ChunkYdensityBuffer = new ComputeBuffer(2 * m_x_dim * m_z_dim, sizeof(float));
				m_ChunkZdensityBuffer = new ComputeBuffer(2 * m_x_dim * m_y_dim, sizeof(float));
				m_ChunkXYdensityBuffer = new ComputeBuffer(3 * m_z_dim, sizeof(float));
				m_ChunkYZdensityBuffer = new ComputeBuffer(3 * m_x_dim, sizeof(float));
				m_ChunkXZdensityBuffer = new ComputeBuffer(3 * m_y_dim, sizeof(float));
				m_ChunkXYZdensityBuffer = new ComputeBuffer(4, sizeof(float));

				//normals
				m_normalsTexture = new RenderTexture(m_x_dim + 1, m_y_dim + 1, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
				{
					dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
					volumeDepth = m_z_dim + 1,
					enableRandomWrite = true,
					useMipMap = false
				};
				m_normalsTexture.Create();

				//initalize variables in MC, ClearVertices, initalize normals shader
				InitMC();
				InitMesh();
				InitNormals();
			}

			public Mesh ComputeMesh(float[] densityMap, float[] borderMapx, float[] borderMapy, float[] borderMapz, float[] borderMapxy, float[] borderMapyz, float[] borderMapxz, float[] borderMapxyz)
			{
				m_densityBuffer.SetData(densityMap);
				m_ChunkXdensityBuffer.SetData(borderMapx);
				m_ChunkYdensityBuffer.SetData(borderMapy);
				m_ChunkZdensityBuffer.SetData(borderMapz);
				m_ChunkXYdensityBuffer.SetData(borderMapxy);
				m_ChunkYZdensityBuffer.SetData(borderMapyz);
				m_ChunkXZdensityBuffer.SetData(borderMapxz);
				m_ChunkXYZdensityBuffer.SetData(borderMapxyz);

				//Clear vertices
				m_clearVerticesShader.SetBuffer(0, "_Vertices", m_meshBuffer);
				m_clearVerticesShader.Dispatch(0, m_x_lod_dim / (8 / m_lod), m_y_lod_dim / (8 / m_lod), m_z_lod_dim / (8 / m_lod));

				//Calculate normals
				m_calculateNormalsShader.SetBuffer(0, "_DensityMap", m_densityBuffer);
				m_calculateNormalsShader.SetBuffer(0, "_BorderMapx", m_ChunkXdensityBuffer);
				m_calculateNormalsShader.SetBuffer(0, "_BorderMapy", m_ChunkYdensityBuffer);
				m_calculateNormalsShader.SetBuffer(0, "_BorderMapz", m_ChunkZdensityBuffer);
				m_calculateNormalsShader.SetBuffer(0, "_BorderMapxy", m_ChunkXYdensityBuffer);
				m_calculateNormalsShader.SetBuffer(0, "_BorderMapyz", m_ChunkYZdensityBuffer);
				m_calculateNormalsShader.SetBuffer(0, "_BorderMapxz", m_ChunkXZdensityBuffer);
				m_calculateNormalsShader.SetBuffer(0, "_BorderMapxyz", m_ChunkXYZdensityBuffer);
				m_calculateNormalsShader.SetTexture(0, "_Normals", m_normalsTexture);
				m_calculateNormalsShader.Dispatch(0, m_x_dim / 8, m_y_dim / 8, m_z_dim / 8);

				//Initalize MC
				m_marchingCubesShader.SetFloat("_DensityOffset", m_offset);
				m_marchingCubesShader.SetTexture(0, "_Normals", m_normalsTexture);
				m_marchingCubesShader.SetBuffer(0, "_Vertices", m_meshBuffer);
				m_marchingCubesShader.SetBuffer(0, "_DensityMap", m_densityBuffer);
				m_marchingCubesShader.SetBuffer(0, "_BorderMapx", m_ChunkXdensityBuffer);
				m_marchingCubesShader.SetBuffer(0, "_BorderMapy", m_ChunkYdensityBuffer);
				m_marchingCubesShader.SetBuffer(0, "_BorderMapz", m_ChunkZdensityBuffer);
				m_marchingCubesShader.SetBuffer(0, "_BorderMapxy", m_ChunkXYdensityBuffer);
				m_marchingCubesShader.SetBuffer(0, "_BorderMapyz", m_ChunkYZdensityBuffer);
				m_marchingCubesShader.SetBuffer(0, "_BorderMapxz", m_ChunkXZdensityBuffer);
				m_marchingCubesShader.SetFloat("_BorderMapxyz", borderMapxyz[0]);
				m_marchingCubesShader.Dispatch(0, m_x_lod_dim / (8 / m_lod), m_y_lod_dim / (8 / m_lod), m_z_lod_dim / (8 / m_lod)); //start the magic

                //receive the verts
                Vertex[] receivedData = new Vertex[m_maxVertices];
				List<Vector3> vertices = new List<Vector3>();
				List<Vector3> normals = new List<Vector3>();
				List<int> triangles = new List<int>();
				Mesh mesh = new Mesh();

				m_meshBuffer.GetData(receivedData);
				for(int i = 0, idx = 0; i < receivedData.Length; i++)
				{
					if(receivedData[i].position.w != -1.0f)
					{
						vertices.Add(new Vector3(receivedData[i].position.x, receivedData[i].position.y, receivedData[i].position.z));
						normals.Add(new Vector3(receivedData[i].normal.x, receivedData[i].normal.y, receivedData[i].normal.z));
						triangles.Add(idx++);
					}
				}

				mesh.SetVertices(vertices);
				mesh.SetNormals(normals);
				mesh.SetTriangles(triangles, 0);

                return mesh;
                //return new Mesh();
			}

			private void InitMC()
			{
				m_marchingCubesShader.SetInt("_DensityMap_sizex", m_x_dim);
				m_marchingCubesShader.SetInt("_DensityMap_sizey", m_y_dim);
				m_marchingCubesShader.SetInt("_DensityMap_sizez", m_z_dim);
				m_marchingCubesShader.SetInt("_Lod", m_lod);
				m_marchingCubesShader.SetFloat("_DensityOffset", m_offset);
				m_marchingCubesShader.SetFloat("_Scale", m_scale);
				m_marchingCubesShader.SetBool("_RecalculateNormals", m_recalculateNormals);
				m_marchingCubesShader.SetBuffer(0, "_CubeEdgeFlags", m_cubeEdgeFlags);
				m_marchingCubesShader.SetBuffer(0, "_TriangleConnectionTable", m_traingleConnectionTable);
			}

			private void InitMesh()
			{
				//Filling with -1 to all the vertices have.w direction -1(which means that it has not been modified)
				m_clearVerticesShader.SetInt("_x", m_x_lod_dim);
				m_clearVerticesShader.SetInt("_y", m_y_lod_dim);
				m_clearVerticesShader.SetInt("_z", m_z_lod_dim);
				m_clearVerticesShader.SetBuffer(0, "_Vertices", m_meshBuffer);
			}

			private void InitNormals()
			{
				m_calculateNormalsShader.SetInt("_x", m_x_dim);
				m_calculateNormalsShader.SetInt("_y", m_y_dim);
				m_calculateNormalsShader.SetInt("_z", m_z_dim);
				m_calculateNormalsShader.SetInt("_Lod", m_lod);
				m_calculateNormalsShader.SetTexture(0, "_Normals", m_normalsTexture);
			}

            /*public void DrawMesh(Camera camera)
            {
                m_drawMesh.SetBuffer("_MeshBuffer", m_meshBuffer);
                m_drawMesh.SetPass(0);
                Graphics.DrawProceduralNow(MeshTopology.Triangles, m_maxVertices);
            }*/

			public void Release()
			{
				m_cubeEdgeFlags.Release();
				m_traingleConnectionTable.Release();
				m_meshBuffer.Release();
				m_densityBuffer.Release();
				m_ChunkXdensityBuffer.Release();
				m_ChunkYdensityBuffer.Release();
				m_ChunkZdensityBuffer.Release();
				m_ChunkXYdensityBuffer.Release();
				m_ChunkYZdensityBuffer.Release();
				m_ChunkXZdensityBuffer.Release();
				m_ChunkXYZdensityBuffer.Release();
			}
		}
	}
}