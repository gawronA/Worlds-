using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeographicCoordinate
{
    private float m_lambda;
    private float m_phi;
    public float Radius { get; set; }

    public float LambdaRad
    {
        get
        {
            return m_lambda;
        }
        set
        {
            int period_count = 0;
            if(value > Mathf.PI) period_count = (int)((value + Mathf.PI) / (2 * Mathf.PI));
            else if(value <= -Mathf.PI) period_count = (int)((value - Mathf.PI) / (2 * Mathf.PI));
            m_lambda = value - period_count * 2 * Mathf.PI;
        }
    }

    public float PhiRad
    {
        get
        {
            return m_phi;
        }
        set
        {
            int period_count = 0;
            if(value > Mathf.PI) period_count = (int)((value + Mathf.PI) / (2 * Mathf.PI));
            else if(value <= -Mathf.PI) period_count = (int)((value - Mathf.PI) / (2 * Mathf.PI));
            m_phi = value - period_count * 2 * Mathf.PI;
        }
    }

    public float LambdaDeg
    {
        get
        {
            return m_lambda * Mathf.Rad2Deg;
        }
        set
        {
            LambdaRad = value * Mathf.Deg2Rad;
        }
    }

    public float PhiDeg
    {
        get
        {
            return m_phi * Mathf.Rad2Deg;
        }
        set
        {
            PhiRad = value * Mathf.Deg2Rad;
        }
    }

    

}
