using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProceduralTerrain
{
	namespace MarchingCubes
	{
        public struct BorderDensities
        {
            public float[] borderMapx;
            public float[] borderMapy;
            public float[] borderMapz;
            public float[] borderMapxy;
            public float[] borderMapyz;
            public float[] borderMapxz;
            public float[] borderMapxyz;
        }

        public class MarchingCubes
		{

			public ComputeShader m_MCRenderShader;
            public ComputeShader m_MCColliderShader;
			public ComputeShader m_clearVerticesShader;
			public ComputeShader m_calculateNormalsShader;
            public Material m_meshMaterial;
			public float m_offset = 0.0f;
			public bool m_recalculateNormals = false;

            public Transform m_chunkTransform;

            MaterialPropertyBlock m_propertyBlock;
            public Bounds m_bounds;

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

            

			public void InitalizeRenderMesh(int x_dim, int y_dim, int z_dim, float size)
			{
				//each dimension must be divisable by 8, densitymap and MC shader must be present
				if(x_dim % 8 != 0 || y_dim % 8 != 0 || z_dim % 8 != 0) throw new System.ArgumentException("x, y or z must be divisible by 8");
				if(m_MCRenderShader == null) throw new System.ArgumentException("Missing MCRenderShader");
				if(m_clearVerticesShader == null) throw new System.ArgumentException("Missing ClearVerticesShader");
				if(m_calculateNormalsShader == null) throw new System.ArgumentException("Missing CalculateNormalsShader");

				m_x_dim = x_dim;
				m_y_dim = y_dim;
				m_z_dim = z_dim;

				m_scale = size;

                m_propertyBlock = new MaterialPropertyBlock();
                m_propertyBlock.SetMatrix("_ObjectToWorld", m_chunkTransform.localToWorldMatrix);
                m_bounds = new Bounds((m_chunkTransform.position + new Vector3(m_x_dim / 2, m_y_dim / 2, m_z_dim / 2)) * m_scale, new Vector3(m_x_dim, m_y_dim, m_z_dim) * m_scale);

				//MarchingCubes
				//tables
				m_cubeEdgeFlags = new ComputeBuffer(256, sizeof(int));
				m_cubeEdgeFlags.SetData(MarchingCubesTables.CubeEdgeFlags);
				m_traingleConnectionTable = new ComputeBuffer(256 * 16, sizeof(int));
				m_traingleConnectionTable.SetData(MarchingCubesTables.TriangleConnectionTable);

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
				InitRenderMC();
				InitRenderMesh();
				InitNormals();
			}

            public void SetLOD(int lod)
            {
                if(lod < 0 || lod > 3) throw new System.ArgumentException("level-of-detail must be in range 0 to 3");

                m_lod = (int)Mathf.Pow(2, lod);
                m_x_lod_dim = m_x_dim / m_lod;
                m_y_lod_dim = m_y_dim / m_lod;
                m_z_lod_dim = m_z_dim / m_lod;

                //max verts
                m_maxVertices = m_x_lod_dim * m_y_lod_dim * m_z_lod_dim * 5 * 3;
               /* m_dummyMesh = new Mesh()
                {
                    vertices = new Vector3[m_maxVertices],
                    bounds = new Bounds(m_chunkPosition + new Vector3(m_x_dim / 2, m_y_dim / 2, m_z_dim / 2), new Vector3(m_x_dim / 2, m_y_dim / 2, m_z_dim / 2))
                };*/
                

                //output mesh. 
                if(m_meshBuffer != null) m_meshBuffer.Release();
                m_meshBuffer = new ComputeBuffer(m_maxVertices, sizeof(float) * 7);

                m_MCRenderShader.SetInt("_Lod", m_lod);
                m_calculateNormalsShader.SetInt("_Lod", m_lod);

                InitRenderMesh();
            }

            public void ComputeRenderMesh(float[] densityMap, BorderDensities maps, Vector3 chunkPosition)
            {
                m_densityBuffer.SetData(densityMap);
                m_ChunkXdensityBuffer.SetData(maps.borderMapx);
                m_ChunkYdensityBuffer.SetData(maps.borderMapy);
                m_ChunkZdensityBuffer.SetData(maps.borderMapz);
                m_ChunkXYdensityBuffer.SetData(maps.borderMapxy);
                m_ChunkYZdensityBuffer.SetData(maps.borderMapyz);
                m_ChunkXZdensityBuffer.SetData(maps.borderMapxz);
                m_ChunkXYZdensityBuffer.SetData(maps.borderMapxyz);

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
                //m_MCRenderShader.SetFloats("_ChunkPosition", new float[] { chunkPosition.x, chunkPosition.y, chunkPosition.z });
                m_MCRenderShader.SetFloat("_DensityOffset", m_offset);
                m_MCRenderShader.SetTexture(0, "_Normals", m_normalsTexture);
                m_MCRenderShader.SetBuffer(0, "_Vertices", m_meshBuffer);
                m_MCRenderShader.SetBuffer(0, "_DensityMap", m_densityBuffer);
                m_MCRenderShader.SetBuffer(0, "_BorderMapx", m_ChunkXdensityBuffer);
                m_MCRenderShader.SetBuffer(0, "_BorderMapy", m_ChunkYdensityBuffer);
                m_MCRenderShader.SetBuffer(0, "_BorderMapz", m_ChunkZdensityBuffer);
                m_MCRenderShader.SetBuffer(0, "_BorderMapxy", m_ChunkXYdensityBuffer);
                m_MCRenderShader.SetBuffer(0, "_BorderMapyz", m_ChunkYZdensityBuffer);
                m_MCRenderShader.SetBuffer(0, "_BorderMapxz", m_ChunkXZdensityBuffer);
                m_MCRenderShader.SetFloat("_BorderMapxyz", maps.borderMapxyz[0]);
                m_MCRenderShader.Dispatch(0, m_x_lod_dim / (8 / m_lod), m_y_lod_dim / (8 / m_lod), m_z_lod_dim / (8 / m_lod)); //start the magic

            }

            private void InitRenderMC()
            {
                m_MCRenderShader.SetInt("_DensityMap_sizex", m_x_dim);
                m_MCRenderShader.SetInt("_DensityMap_sizey", m_y_dim);
                m_MCRenderShader.SetInt("_DensityMap_sizez", m_z_dim);
                m_MCRenderShader.SetFloat("_DensityOffset", m_offset);
                m_MCRenderShader.SetFloat("_Scale", m_scale);
                m_MCRenderShader.SetBool("_RecalculateNormals", m_recalculateNormals);
                m_MCRenderShader.SetBuffer(0, "_CubeEdgeFlags", m_cubeEdgeFlags);
                m_MCRenderShader.SetBuffer(0, "_TriangleConnectionTable", m_traingleConnectionTable);
            }

            private void InitRenderMesh()
            {
                //Filling with -1 to all the vertices have.w direction -1(which means that it has not been modified)
                m_clearVerticesShader.SetInt("_x", m_x_lod_dim);
                m_clearVerticesShader.SetInt("_y", m_y_lod_dim);
                m_clearVerticesShader.SetInt("_z", m_z_lod_dim);
                //m_clearVerticesShader.SetBuffer(0, "_Vertices", m_meshBuffer);
            }

            private void InitNormals()
            {
                m_calculateNormalsShader.SetInt("_x", m_x_dim);
                m_calculateNormalsShader.SetInt("_y", m_y_dim);
                m_calculateNormalsShader.SetInt("_z", m_z_dim);
            }

            public void DrawMesh()
            {
                m_propertyBlock.SetBuffer("_MeshBuffer", m_meshBuffer);
                Graphics.DrawProcedural(m_meshMaterial, m_bounds, MeshTopology.Triangles, m_maxVertices, 1, null, m_propertyBlock, UnityEngine.Rendering.ShadowCastingMode.TwoSided);
            }



            public void InitalizeColliderCompute(int x_dim, int y_dim, int z_dim, float size)
            {
                //each dimension must be divisable by 8, densitymap and MC shader must be present
                if(x_dim % 8 != 0 || y_dim % 8 != 0 || z_dim % 8 != 0) throw new System.ArgumentException("x, y or z must be divisible by 8");
                if(m_MCColliderShader == null) throw new System.ArgumentException("Missing MCColliderShader");
                if(m_clearVerticesShader == null) throw new System.ArgumentException("Missing ClearVerticesShader");

                m_x_dim = x_dim;
                m_y_dim = y_dim;
                m_z_dim = z_dim;

                m_scale = size;

                //max verts
                m_maxVertices = m_x_dim * m_y_dim * m_z_dim * 5 * 3;

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

                //initalize variables in MC, ClearVertices, initalize normals shader
                InitColliderMC();
                InitColliderMesh();
            }

            public Mesh ComputeColliderMesh(float[] densityMap, BorderDensities maps)
			{
				m_densityBuffer.SetData(densityMap);
				m_ChunkXdensityBuffer.SetData(maps.borderMapx);
				m_ChunkYdensityBuffer.SetData(maps.borderMapy);
				m_ChunkZdensityBuffer.SetData(maps.borderMapz);
				m_ChunkXYdensityBuffer.SetData(maps.borderMapxy);
				m_ChunkYZdensityBuffer.SetData(maps.borderMapyz);
				m_ChunkXZdensityBuffer.SetData(maps.borderMapxz);
				m_ChunkXYZdensityBuffer.SetData(maps.borderMapxyz);

				//Clear vertices
				m_clearVerticesShader.SetBuffer(0, "_Vertices", m_meshBuffer);
				m_clearVerticesShader.Dispatch(0, m_x_dim / 8, m_y_dim / 8, m_z_dim / 8);
                
				//Initalize MC
				m_MCColliderShader.SetFloat("_DensityOffset", m_offset);
				m_MCColliderShader.SetBuffer(0, "_Vertices", m_meshBuffer);
				m_MCColliderShader.SetBuffer(0, "_DensityMap", m_densityBuffer);
				m_MCColliderShader.SetBuffer(0, "_BorderMapx", m_ChunkXdensityBuffer);
				m_MCColliderShader.SetBuffer(0, "_BorderMapy", m_ChunkYdensityBuffer);
				m_MCColliderShader.SetBuffer(0, "_BorderMapz", m_ChunkZdensityBuffer);
				m_MCColliderShader.SetBuffer(0, "_BorderMapxy", m_ChunkXYdensityBuffer);
				m_MCColliderShader.SetBuffer(0, "_BorderMapyz", m_ChunkYZdensityBuffer);
				m_MCColliderShader.SetBuffer(0, "_BorderMapxz", m_ChunkXZdensityBuffer);
				m_MCColliderShader.SetFloat("_BorderMapxyz", maps.borderMapxyz[0]);
				m_MCColliderShader.Dispatch(0, m_x_dim / 8, m_y_dim / 8, m_z_dim / 8); //start the magic

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
						triangles.Add(idx++);
					}
				}

				mesh.SetVertices(vertices);
				mesh.SetTriangles(triangles, 0);

                return mesh;
                //return new Mesh();
			}

            private void InitColliderMC()
            {
                m_MCColliderShader.SetInt("_DensityMap_sizex", m_x_dim);
                m_MCColliderShader.SetInt("_DensityMap_sizey", m_y_dim);
                m_MCColliderShader.SetInt("_DensityMap_sizez", m_z_dim);
                m_MCColliderShader.SetFloat("_DensityOffset", m_offset);
                m_MCColliderShader.SetFloat("_Scale", m_scale);
                m_MCColliderShader.SetBuffer(0, "_CubeEdgeFlags", m_cubeEdgeFlags);
                m_MCColliderShader.SetBuffer(0, "_TriangleConnectionTable", m_traingleConnectionTable);
            }

            private void InitColliderMesh()
            {
                //Filling with -1 to all the vertices have.w direction -1(which means that it has not been modified)
                m_clearVerticesShader.SetInt("_x", m_x_dim);
                m_clearVerticesShader.SetInt("_y", m_y_dim);
                m_clearVerticesShader.SetInt("_z", m_z_dim);
            }

            

			public void Release()
			{
				if(m_cubeEdgeFlags != null) m_cubeEdgeFlags.Release();
                if(m_traingleConnectionTable != null) m_traingleConnectionTable.Release();
                if(m_meshBuffer != null) m_meshBuffer.Release();
                if(m_densityBuffer != null) m_densityBuffer.Release();
                if(m_ChunkXdensityBuffer != null) m_ChunkXdensityBuffer.Release();
                if(m_ChunkYdensityBuffer != null) m_ChunkYdensityBuffer.Release();
                if(m_ChunkZdensityBuffer != null) m_ChunkZdensityBuffer.Release();
                if(m_ChunkXYdensityBuffer != null) m_ChunkXYdensityBuffer.Release();
                if(m_ChunkYZdensityBuffer != null) m_ChunkYZdensityBuffer.Release();
                if(m_ChunkXZdensityBuffer != null) m_ChunkXZdensityBuffer.Release();
                if(m_ChunkXYZdensityBuffer != null) m_ChunkXYZdensityBuffer.Release();
			}
		}
	}
}