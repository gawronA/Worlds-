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
			public float m_size;
			public ComputeShader m_marchingCubesShader;
			public ComputeShader m_clearBufferShader;
			public ComputeShader m_fillDensityBufferShader;
			public float m_offset = 0.0f;

			float[] m_densityMap;
			int m_maxVertices;
			float[] m_defaultVerticesValues;

			ComputeBuffer m_cubeEdgeFlags, m_traingleConnectionTable;
			ComputeBuffer m_meshBuffer, m_densityBuffer;

			struct Vertex
			{
				public Vector4 position;
				public Vector3 normal;
			};

			public void Initalize(int x_dim, int y_dim, int z_dim, float size)
			{
				m_x_dimension = x_dim;
				m_y_dimension = y_dim;
				m_z_dimension = z_dim;
				m_size = size;

				//each dimension must be divisable by 8, densitymap and MC shader must be present
				//if(m_x_dimension % 8 != 0 || m_y_dimension % 8 != 0 || m_z_dimension % 8 != 0) throw new System.ArgumentException("x, y or z must be divisible by 8");
				if(m_marchingCubesShader == null) throw new System.ArgumentException("Missing MarchingCubesShader");

				//max verts
				m_maxVertices = (m_x_dimension - 1) * (m_y_dimension - 1) * (m_z_dimension - 1) * 5 * 3; //3 vertices per 5 tris

				//MarchingCubes
				//tables
				m_cubeEdgeFlags = new ComputeBuffer(256, sizeof(int));
				m_cubeEdgeFlags.SetData(MarchingCubesTables.CubeEdgeFlags);
				m_traingleConnectionTable = new ComputeBuffer(256 * 16, sizeof(int));
				m_traingleConnectionTable.SetData(MarchingCubesTables.TriangleConnectionTable);

				//output mesh. 
				m_meshBuffer = new ComputeBuffer(m_maxVertices, sizeof(float) * 7);
				m_densityBuffer = new ComputeBuffer(m_x_dimension * m_y_dimension * m_z_dimension, sizeof(float));

				//initalize variables in shader
				m_marchingCubesShader.SetInt("_DensityMap_sizex", m_x_dimension);
				m_marchingCubesShader.SetInt("_DensityMap_sizey", m_y_dimension);
				m_marchingCubesShader.SetInt("_DensityMap_sizez", m_z_dimension);
				m_marchingCubesShader.SetFloat("_DensityOffset", m_offset);
				m_marchingCubesShader.SetFloat("_Size", m_size);
				m_marchingCubesShader.SetBuffer(0, "_CubeEdgeFlags", m_cubeEdgeFlags);
				m_marchingCubesShader.SetBuffer(0, "_TriangleConnectionTable", m_traingleConnectionTable);

				//ClearVertices
				//Filling with -1 to all the vertices have.w direction -1(which means that it has not been modified)
				m_clearBufferShader.SetInt("_x", m_x_dimension);
				m_clearBufferShader.SetInt("_y", m_y_dimension);
				m_clearBufferShader.SetInt("_z", m_z_dimension);
				m_clearBufferShader.SetBuffer(0, "_Vertices", m_meshBuffer);

				//Fill DensityBuffer
				//m_fillDensityBufferShader.SetBuffer(0, "_DensityMapBuffer", m_densityBuffer);
			}

			public void ComputeMesh(float[] densityMap)
			{
				//Do zrobienia! 1. Napisanie shadera który ustawia w buforze dane.
				//if(densityMap.Length == 0) throw new System.ArgumentException("Missing density map");
				//m_densityBuffer.SetData(densityMap);
				//m_meshBuffer.SetData(m_defaultVerticesValues);
				
				//Clear vertices
				m_clearBufferShader.Dispatch(0, m_x_dimension / 2, m_y_dimension / 2, m_z_dimension / 2);

				//Fill densityBuffer
				//m_fillDensityBufferShader.SetFloats("_DensityMap", densityMap);
				//m_fillDensityBufferShader.Dispatch(0, m_x_dimension / 2, m_y_dimension / 2, m_z_dimension / 2);
				m_densityBuffer.SetData(densityMap);

				//float[] DM = new float[m_x_dimension * m_y_dimension * m_z_dimension];
				//m_densityBuffer.GetData(DM);

				/*Vertex[] receivedData = new Vertex[m_maxVertices];
				List<Vector3> vertices = new List<Vector3>();
				List<int> triangles = new List<int>();
				Mesh mesh = new Mesh();
				m_meshBuffer.GetData(receivedData);
				for(int i = 0; i < receivedData.Length && receivedData[i].position.w != -1.0f; i++)
				{
					vertices.Add(new Vector3(receivedData[i].position.x, receivedData[i].position.y, receivedData[i].position.z));
					triangles.Add(i);
				}*/

				//m_marchingCubesShader.SetFloat("_DensityOffset", m_offset);
				//m_marchingCubesShader.SetBuffer(0, "_Vertices", m_meshBuffer);
				//m_marchingCubesShader.SetBuffer(0, "_DensityMap", m_densityBuffer);
				//m_marchingCubesShader.Dispatch(0, m_x_dimension / 8, m_y_dimension / 8, m_z_dimension / 8); //start the magic
				
				//receive the verts
				/*Vertex[] receivedData = new Vertex[m_maxVertices];
				List<Vector3> vertices = new List<Vector3>();
				List<int> triangles = new List<int>();
				Mesh mesh = new Mesh();

				m_meshBuffer.GetData(receivedData);
				for(int i = 0; i < receivedData.Length && receivedData[i].position.w != -1.0f; i++)
				{
					vertices.Add(new Vector3(receivedData[i].position.x, receivedData[i].position.y, receivedData[i].position.z));
					triangles.Add(i);
				}

				mesh.SetVertices(vertices);
				mesh.SetTriangles(triangles, 0);
				mesh.RecalculateNormals();
				*/
				/*m_traingleConnectionTable.Release();
				m_cubeEdgeFlags.Release();
				m_densityBuffer.Release();
				m_meshBuffer.Release();
				//return mesh;*/
			}

			public void Release()
			{
				m_cubeEdgeFlags.Release();
				m_traingleConnectionTable.Release();
				m_meshBuffer.Release();
				m_densityBuffer.Release();
			}
		}
	}
}