using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkVerticesCoordinates
{
    public GeographicCoordinate[] m_points;
    
    ChunkVerticesCoordinates(int n)
    {
        m_points = new GeographicCoordinate[n];
    }
}
