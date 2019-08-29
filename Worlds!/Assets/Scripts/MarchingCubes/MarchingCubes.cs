using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProceduralTerrain
{
	namespace MarchingCubes
	{
		public class MarchingCubes
		{
			public int m_x_dimension;
			public int m_y_dimension;
			public int m_z_dimension;
			public float m_scale;
			public ComputeShader m_marchingCubesShader;
			public ComputeShader m_clearVerticesShader;
			public ComputeShader m_calculateNormalsShader;
			public float m_offset = 0.0f;

			float[] m_densityMap;
			int m_maxVertices;

			ComputeBuffer m_cubeEdgeFlags, m_traingleConnectionTable;
			ComputeBuffer m_meshBuffer, m_densityBuffer;

			ComputeBuffer m_debugBuffer;

			public RenderTexture m_normalsTexture;

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
				//if(x_dim % 8 != 0 || y_dim % 8 != 0 || z_dim % 8 != 0) throw new System.ArgumentException("x, y or z must be divisible by 8");
				if(lod < 0 || lod > 3) throw new System.ArgumentException("lewel-of-detail must be in range 0 to 3");
				if(m_marchingCubesShader == null) throw new System.ArgumentException("Missing MarchingCubesShader");
				if(m_clearVerticesShader == null) throw new System.ArgumentException("Missing ClearVerticesShader");
				if(m_calculateNormalsShader == null) throw new System.ArgumentException("Missing CalculateNormalsShader");


				lod = (int)Mathf.Pow(2, lod);
				m_x_dimension = x_dim / lod;
				m_y_dimension = y_dim / lod;
				m_z_dimension = z_dim / lod;
				m_scale = size;
				
				

				//max verts
				m_maxVertices = m_x_dimension * m_y_dimension * m_z_dimension * 5 * 3;

				//MarchingCubes
				//tables
				m_cubeEdgeFlags = new ComputeBuffer(256, sizeof(int));
				m_cubeEdgeFlags.SetData(MarchingCubesTables.CubeEdgeFlags);
				m_traingleConnectionTable = new ComputeBuffer(256 * 16, sizeof(int));
				m_traingleConnectionTable.SetData(MarchingCubesTables.TriangleConnectionTable);

				m_debugBuffer = new ComputeBuffer(8, sizeof(float));

				//output mesh. 
				m_meshBuffer = new ComputeBuffer(m_maxVertices, sizeof(float) * 7);
				m_densityBuffer = new ComputeBuffer(x_dim * y_dim * z_dim, sizeof(float));

				//normals
				m_normalsTexture = new RenderTexture(m_x_dimension, m_y_dimension, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
				m_normalsTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
				m_normalsTexture.volumeDepth = m_z_dimension;
				m_normalsTexture.enableRandomWrite = true;
				m_normalsTexture.useMipMap = false;
				m_normalsTexture.Create();

				//initalize variables in shader
				m_marchingCubesShader.SetInt("_DensityMap_sizex", x_dim);
				m_marchingCubesShader.SetInt("_DensityMap_sizey", y_dim);
				m_marchingCubesShader.SetInt("_DensityMap_sizez", z_dim);
				m_marchingCubesShader.SetInt("_Lod", lod);
				m_marchingCubesShader.SetFloat("_DensityOffset", m_offset);
				m_marchingCubesShader.SetFloat("_Scale", m_scale);
				m_marchingCubesShader.SetBuffer(0, "_CubeEdgeFlags", m_cubeEdgeFlags);
				m_marchingCubesShader.SetBuffer(0, "_TriangleConnectionTable", m_traingleConnectionTable);
				m_marchingCubesShader.SetBuffer(0, "_Debug", m_debugBuffer);

				//ClearVertices
				//Filling with -1 to all the vertices have.w direction -1(which means that it has not been modified)
				m_clearVerticesShader.SetInt("_x", m_x_dimension);
				m_clearVerticesShader.SetInt("_y", m_y_dimension);
				m_clearVerticesShader.SetInt("_z", m_z_dimension);
				m_clearVerticesShader.SetBuffer(0, "_Vertices", m_meshBuffer);

				//CalculateNormals
				m_calculateNormalsShader.SetInt("_x", m_x_dimension);
				m_calculateNormalsShader.SetInt("_y", m_y_dimension);
				m_calculateNormalsShader.SetInt("_z", m_z_dimension);
				m_calculateNormalsShader.SetInt("_Lod", lod);
				m_calculateNormalsShader.SetTexture(0, "_Normals", m_normalsTexture);
			}

			public Mesh ComputeMesh(float[] densityMap)
			{
				m_densityBuffer.SetData(densityMap);
				//Clear vertices
				//m_clearVerticesShader.Dispatch(0, m_x_dimension / 8, m_y_dimension / 8, m_z_dimension / 8);
				m_clearVerticesShader.Dispatch(0, m_x_dimension, m_y_dimension, m_z_dimension);

				//Calculate normals
				m_calculateNormalsShader.SetBuffer(0, "_DensityMap", m_densityBuffer);
				m_calculateNormalsShader.SetTexture(0, "_Normals", m_normalsTexture);
				//m_calculateNormalsShader.Dispatch(0, m_x_dimension / 8, m_y_dimension / 8, m_z_dimension / 8);
				m_calculateNormalsShader.Dispatch(0, m_x_dimension, m_y_dimension, m_z_dimension);

				//Initalize MC
				m_marchingCubesShader.SetFloat("_DensityOffset", m_offset);
				m_marchingCubesShader.SetTexture(0, "_Normals", m_normalsTexture);
				m_marchingCubesShader.SetBuffer(0, "_Vertices", m_meshBuffer);
				m_marchingCubesShader.SetBuffer(0, "_DensityMap", m_densityBuffer);
				//m_marchingCubesShader.Dispatch(0, m_x_dimension / 8, m_y_dimension / 8, m_z_dimension / 8); //start the magic
				m_marchingCubesShader.Dispatch(0, m_x_dimension, m_y_dimension, m_z_dimension);

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
				//mesh.RecalculateNormals();

				float[] receivedDebug = new float[8];
				m_debugBuffer.GetData(receivedDebug);

				return mesh;
			}

			public void Release()
			{
				m_cubeEdgeFlags.Release();
				m_traingleConnectionTable.Release();
				m_meshBuffer.Release();
				m_densityBuffer.Release();
			}

			/*public Vector3[] CalculateNormals(float[] densityMap)
			{
				m_calculateNormalsShader.SetInt("_x", m_x_dimension);
				m_calculateNormalsShader.SetInt("_y", m_y_dimension);
				m_calculateNormalsShader.SetInt("_z", m_z_dimension);
				m_calculateNormalsShader.SetBuffer(0, "_DensityMap", m_densityBuffer);
				m_calculateNormalsShader.SetBuffer(0, "_Normals", normalsBuffer);
				m_calculateNormalsShader.Dispatch(0, m_x_dimension / 2, m_y_dimension / 2, m_z_dimension / 2);
				Vector3[] normals = new Vector3[m_x_dimension * m_y_dimension * m_z_dimension];
				normalsBuffer.GetData(normals);
				return normals;
			}*/
		}
	}
}