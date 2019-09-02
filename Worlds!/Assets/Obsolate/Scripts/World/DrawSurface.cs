using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SurfaceDrawTools
{
	static public class DrawSurface
	{
		/// <summary>
		/// <para>Draw a sphere from center to radius</para>
		/// </summary>
		static public void Sphere(SurfaceMap surfaceMap, Vector3Int center, int radius, int material, bool contour)
		{
			if(center.x < 0 || center.y < 0 || center.z < 0) throw new Exception("PointBelowZero");
			if(center.x >= surfaceMap.resolution || center.y >= surfaceMap.resolution || center.z >= surfaceMap.resolution) throw new Exception("PointAboweResolution");

			int radius2 = radius * radius;
			for(int z = center.z - radius; z <= center.z + radius && z < surfaceMap.resolution; z++)
			{
				if(z < 0) z = 0;
				for(int y = center.y - radius; y <= center.y + radius && y < surfaceMap.resolution; y++)
				{
					if(y < 0) y = 0;
					for(int x = center.x - radius; x <= center.x + radius && x < surfaceMap.resolution; x++)
					{
						if(x < 0) x = 0;
						int pointOnSphere = (int)(Mathf.Pow(x - center.x, 2) + Mathf.Pow(y - center.y, 2) + Mathf.Pow(z - center.z, 2));
						if(pointOnSphere <= radius2)
						{
							surfaceMap.SetBiome(x, y, z, 1);
							surfaceMap.SetMaterial(x, y, z, material);

							if(contour && (
								(int)(Mathf.Pow(x - center.x + 1, 2) + Mathf.Pow(y - center.y, 2) + Mathf.Pow(z - center.z, 2)) > radius2 ||
								(int)(Mathf.Pow(x - center.x, 2) + Mathf.Pow(y - center.y + 1, 2) + Mathf.Pow(z - center.z, 2)) > radius2 ||
								(int)(Mathf.Pow(x - center.x, 2) + Mathf.Pow(y - center.y, 2) + Mathf.Pow(z - center.z + 1, 2)) > radius2 ||
								(int)(Mathf.Pow(x - center.x - 1, 2) + Mathf.Pow(y - center.y, 2) + Mathf.Pow(z - center.z, 2)) > radius2 ||
								(int)(Mathf.Pow(x - center.x, 2) + Mathf.Pow(y - center.y - 1, 2) + Mathf.Pow(z - center.z, 2)) > radius2 ||
								(int)(Mathf.Pow(x - center.x, 2) + Mathf.Pow(y - center.y, 2) + Mathf.Pow(z - center.z - 1, 2)) > radius2 ||
								x == 0 || x == surfaceMap.resolution - 1 ||
								y == 0 || y == surfaceMap.resolution - 1 ||
								z == 0 || y == surfaceMap.resolution - 1)
											)
								surfaceMap.SetContour(x, y, z);
						}
						//else surfaceMap.SetMaterial(x, y, z, 0);
					}
				}
			}
		}

		/// <summary>
		/// <para>Draw sphere from radius1 to radius2</para>
		/// </summary>
		static public void Sphere(SurfaceMap surfaceMap, Vector3Int center, int radius1, int radius2, int material, bool contour)
		{
			if(center.x < 0 || center.y < 0 || center.z < 0) throw new Exception("PointBelowZero");
			if(center.x >= surfaceMap.resolution || center.y >= surfaceMap.resolution || center.z >= surfaceMap.resolution) throw new Exception("PointAboweResolution");

			if(radius1 > radius2)
			{
				int tmp = radius1;
				radius1 = radius2;
				radius2 = tmp;
			}

			int radius12 = radius1 * radius1;
			int radius22 = radius2 * radius2;
			for(int z = center.z - radius2; z <= center.z + radius2 && z < surfaceMap.resolution; z++)
			{
				if(z < 0) z = 0;
				for(int y = center.y - radius2; y <= center.y + radius2 && y < surfaceMap.resolution; y++)
				{
					if(y < 0) y = 0;
					for(int x = center.x - radius2; x <= center.x + radius2 && x < surfaceMap.resolution; x++)
					{
						if(x < 0) x = 0;
						int pointOnSphere = (int)(Mathf.Pow(x - center.x, 2) + Mathf.Pow(y - center.y, 2) + Mathf.Pow(z - center.z, 2));
						if(pointOnSphere <= radius22 && pointOnSphere >= radius12)
						{
							surfaceMap.SetBiome(x, y, z, 1);
							surfaceMap.SetMaterial(x, y, z, material);

							if(contour && (
								(int)(Mathf.Pow(x - center.x + 1, 2) + Mathf.Pow(y - center.y, 2) + Mathf.Pow(z - center.z, 2)) > radius22 ||
								(int)(Mathf.Pow(x - center.x, 2) + Mathf.Pow(y - center.y + 1, 2) + Mathf.Pow(z - center.z, 2)) > radius22 ||
								(int)(Mathf.Pow(x - center.x, 2) + Mathf.Pow(y - center.y, 2) + Mathf.Pow(z - center.z + 1, 2)) > radius22 ||
								(int)(Mathf.Pow(x - center.x - 1, 2) + Mathf.Pow(y - center.y, 2) + Mathf.Pow(z - center.z, 2)) > radius22 ||
								(int)(Mathf.Pow(x - center.x, 2) + Mathf.Pow(y - center.y - 1, 2) + Mathf.Pow(z - center.z, 2)) > radius22 ||
								(int)(Mathf.Pow(x - center.x, 2) + Mathf.Pow(y - center.y, 2) + Mathf.Pow(z - center.z - 1, 2)) > radius22 ||
								x == 0 || x == surfaceMap.resolution - 1 ||
								y == 0 || y == surfaceMap.resolution - 1 ||
								z == 0 || y == surfaceMap.resolution - 1)
											)
								surfaceMap.SetContour(x, y, z);
						}
					}
				}
			}
		}

		/// <summary>
		/// <para>Draw a straight line between p1 & p2</para>
		/// <para>Mode: 0 - overlay, 1 - add, 2 - inverse</para>
		/// </summary>
		static public void Line(SurfaceMap surfaceMap, Vector3Int p1, Vector3Int p2, int width, int material, bool contour, int mode)
		{
			if(p1.x < 0 || p1.x >= surfaceMap.resolution || p1.y < 0 || p1.y >= surfaceMap.resolution || p1.z < 0 || p1.z >= surfaceMap.resolution) throw new Exception("p1OutOfBounds");
			if(p2.x < 0 || p2.x >= surfaceMap.resolution || p2.y < 0 || p2.y >= surfaceMap.resolution || p2.z < 0 || p2.z >= surfaceMap.resolution) throw new Exception("p2OutOfBounds");
			if(mode < 0 || mode > 2) throw new Exception("InvalidMode");

			if(Vector3Int.Distance(Vector3Int.zero, p1) > Vector3Int.Distance(Vector3Int.zero, p2))
			{
				Vector3Int tmp = p2;
				p2 = p1;
				p1 = tmp;
			}

			Vector3Int direction = p2 - p1;
			int x, y, z;
			for(float t = 0.0f; t <= 1.0f; t = t + (1.0f / (2 * Vector3.Distance(p1, p2))))
			{
				int offset = 0;
				int widthCopy = width;
				while(widthCopy > 0)
				{
					if(width % 2 == 1)
					{
						x = p1.x + offset + (int)(direction.x * t);
						y = p1.y + offset + (int)(direction.y * t);
						z = p1.z + offset + (int)(direction.z * t);
					}
					else
					{
						x = p1.x - offset + (int)(direction.x * t);
						y = p1.y - offset + (int)(direction.y * t);
						z = p1.z - offset + (int)(direction.z * t);
					}
					offset++;
					widthCopy--;
					surfaceMap.SetMaterial(x, y, z, material);
					if(contour) surfaceMap.SetContour(x, y, z);
				}
			}
		}

		/// <summary>
		/// <para>Draw a cylinder between p1 & p2</para>
		/// <para>Mode: 0 - overlay, 1 - add, 2 - inverse</para>
		/// </summary>
		static public void Cylinder(SurfaceMap surfaceMap, Vector3Int p1, Vector3Int p2, int radius, int material)
		{
			if(p1.x < 0 || p1.x >= surfaceMap.resolution || p1.y < 0 || p1.y >= surfaceMap.resolution || p1.z < 0 || p1.z >= surfaceMap.resolution) throw new Exception("p1OutOfBounds");
			if(p2.x < 0 || p2.x >= surfaceMap.resolution || p2.y < 0 || p2.y >= surfaceMap.resolution || p2.z < 0 || p2.z >= surfaceMap.resolution) throw new Exception("p2OutOfBounds");

			float distance = Vector3Int.Distance(p1, p2);
			Quaternion rotation = Quaternion.FromToRotation(new Vector3Int(1, 0, 0), p2 - p1);
			for(int x = 0; x < distance; x++)
			{
				for(int z = - radius; z <= radius; z++)
				{
					for(int y = - radius; y <= radius; y++)
					{
						if(Mathf.Pow(y, 2) + Mathf.Pow(z, 2) <= radius * radius)
						{
							//dodać mody
							Vector3Int pointOnCylinder = Vector3Int.RoundToInt(rotation * new Vector3(x, y, z));
							surfaceMap.SetMaterial(p1.x + pointOnCylinder.x, p1.y + pointOnCylinder.y, p1.z + pointOnCylinder.z, material);
							surfaceMap.SetMaterial(p1.x + pointOnCylinder.x + 1, p1.y + pointOnCylinder.y + 1, p1.z + pointOnCylinder.z + 1, material);
						}
					}
				}
			}
		}

		static public void ApplyNoiseToSphere(SurfaceMap sphereLayer, Vector3Int center)
		{
			foreach(int contourPoint in sphereLayer.contour3D)
			{
				Vector3Int contourStartPoint = new Vector3Int(	contourPoint % sphereLayer.resolution,
																(contourPoint / sphereLayer.resolution) % sphereLayer.resolution,
																contourPoint / sphereLayer.resolution2);
				Vector3 direction = contourStartPoint - center;
				direction = direction.normalized;

				Vector3 contourEndPointF = (contourStartPoint + direction * NoiseSettings.noiseMethod(contourStartPoint, NoiseSettings.frequency) * NoiseSettings.gain);
				Vector3Int contourEndPoint = new Vector3Int(Mathf.RoundToInt(contourEndPointF.x),
															Mathf.RoundToInt(contourEndPointF.y),
															Mathf.RoundToInt(contourEndPointF.z));

				Cylinder(sphereLayer, contourStartPoint, contourEndPoint, 1,
						sphereLayer.ReadMaterial(contourStartPoint.x, contourStartPoint.y, contourStartPoint.z));
			}
			Vector3Int point = new Vector3Int(	sphereLayer.contour3D[0] % sphereLayer.resolution,
														(sphereLayer.contour3D[0] / sphereLayer.resolution) % sphereLayer.resolution,
														sphereLayer.contour3D[0] / sphereLayer.resolution2);
			Sphere(	sphereLayer, center,
					(int)((point - center).magnitude),
					(int)((point - center).magnitude + 1),
					/*sphereLayer.ReadMaterial(point.x, point.y, point.z)*/1,
					false);


		}

		static public class NoiseSettings
		{
			public delegate float NoiseMethod(Vector3 point, float frequency);
			static public NoiseMethod noiseMethod = Noise.Value1D;
			static public float frequency = 1.0f;
			static public int octaves = 3;
			static public float lacunarity = 2.0f;
			static public float persistance = 0.25f;
			static public float gain = 2.0f;
		}
	}
}

