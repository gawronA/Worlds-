using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Voxel
{
	public bool state;
	public Vector3 position, xEdgePosition, yEdgePosition, zEdgePosition;
	public Quaternion rotation;
	private float size;
	public Voxel() { }
	public Voxel(Vector3 position, Quaternion rotation, float size)
	{
		this.rotation = rotation;
		this.position = rotation * position * size;
		this.size = size;
		xEdgePosition = this.position;
		xEdgePosition.x += size * 0.5f;
		xEdgePosition = rotation * xEdgePosition;
		yEdgePosition = this.position;
		yEdgePosition.y += size * 0.5f;
		yEdgePosition = rotation * yEdgePosition;
		zEdgePosition = this.position;
		zEdgePosition.z += size * 0.5f;
		zEdgePosition = rotation * zEdgePosition;
	}

	public void SetCrossings(float interpX, float interpY, float interpZ)
	{
		xEdgePosition = this.position;
		xEdgePosition.x += size * interpX;
		xEdgePosition = rotation * xEdgePosition;
		yEdgePosition = this.position;
		yEdgePosition.y += size * interpY;
		yEdgePosition = rotation * yEdgePosition;
		zEdgePosition = this.position;
		zEdgePosition.z += size * interpZ;
		zEdgePosition = rotation * zEdgePosition;
	}
	public void BecomeXDummyOf(Voxel voxel, float offset)
	{
		state = voxel.state;
		position = voxel.position;
		xEdgePosition = voxel.xEdgePosition;
		yEdgePosition = voxel.yEdgePosition;
		zEdgePosition = voxel.zEdgePosition;
		position.x += offset;
		xEdgePosition.x += offset;
		yEdgePosition.x += offset;
		zEdgePosition.x += offset;
	}

	public void BecomeYDummyOf(Voxel voxel, float offset)
	{
		state = voxel.state;
		position = voxel.position;
		xEdgePosition = voxel.xEdgePosition;
		yEdgePosition = voxel.yEdgePosition;
		zEdgePosition = voxel.zEdgePosition;
		position.y += offset;
		xEdgePosition.y += offset;
		yEdgePosition.y += offset;
		zEdgePosition.y += offset;
	}

	public void BecomeZDummyOf(Voxel voxel, float offset)
	{
		state = voxel.state;
		position = voxel.position;
		xEdgePosition = voxel.xEdgePosition;
		yEdgePosition = voxel.yEdgePosition;
		zEdgePosition = voxel.zEdgePosition;
		position.z += offset;
		xEdgePosition.z += offset;
		yEdgePosition.z += offset;
		zEdgePosition.z += offset;
	}

	public void BecomeXYDummyOf(Voxel voxel, float offset)
	{
		state = voxel.state;
		position = voxel.position;
		xEdgePosition = voxel.xEdgePosition;
		yEdgePosition = voxel.yEdgePosition;
		zEdgePosition = voxel.zEdgePosition;
		position.x += offset;
		xEdgePosition.x += offset;
		yEdgePosition.x += offset;
		zEdgePosition.x += offset;
		position.y += offset;
		xEdgePosition.y += offset;
		yEdgePosition.y += offset;
		zEdgePosition.y += offset;
	}

	public void BecomeXZDummyOf(Voxel voxel, float offset)
	{
		state = voxel.state;
		position = voxel.position;
		xEdgePosition = voxel.xEdgePosition;
		yEdgePosition = voxel.yEdgePosition;
		zEdgePosition = voxel.zEdgePosition;
		position.x += offset;
		xEdgePosition.x += offset;
		yEdgePosition.x += offset;
		zEdgePosition.x += offset;
		position.z += offset;
		xEdgePosition.z += offset;
		yEdgePosition.z += offset;
		zEdgePosition.z += offset;
	}

	public void BecomeYZDummyOf(Voxel voxel, float offset)
	{
		state = voxel.state;
		position = voxel.position;
		xEdgePosition = voxel.xEdgePosition;
		yEdgePosition = voxel.yEdgePosition;
		zEdgePosition = voxel.zEdgePosition;
		position.y += offset;
		xEdgePosition.y += offset;
		yEdgePosition.y += offset;
		zEdgePosition.y += offset;
		position.z += offset;
		xEdgePosition.z += offset;
		yEdgePosition.z += offset;
		zEdgePosition.z += offset;
	}

	public void BecomeXYZDummyOf(Voxel voxel, float offset)
	{
		state = voxel.state;
		position = voxel.position;
		xEdgePosition = voxel.xEdgePosition;
		yEdgePosition = voxel.yEdgePosition;
		zEdgePosition = voxel.zEdgePosition;
		position.x += offset;
		xEdgePosition.x += offset;
		yEdgePosition.x += offset;
		zEdgePosition.x += offset;
		position.y += offset;
		xEdgePosition.y += offset;
		yEdgePosition.y += offset;
		zEdgePosition.y += offset;
		position.z += offset;
		xEdgePosition.z += offset;
		yEdgePosition.z += offset;
		zEdgePosition.z += offset;
	}
}