using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeographicCoordinate
{
    private float m_theta;
    private float m_phi;
    public float Radius { get; set; }

    /// <summary>
    /// A point in Cartesian System
    /// </summary>
    public Vector3 Point
    {
        get
        {
            return new Vector3( Radius * Mathf.Sin(Phi) * Mathf.Cos(Theta),
                                Radius * Mathf.Cos(Phi),
                                Radius * Mathf.Sin(Phi) * Mathf.Sin(Theta));
        }
        set
        {
            Radius = Mathf.Sqrt(Mathf.Pow(value.x, 2) + Mathf.Pow(value.y, 2) + Mathf.Pow(value.z, 2));
            if(value.x == 0f) Theta = value.z > 0f ? Mathf.PI / 2f : Mathf.PI* 3f / 2f;
            else Theta = Wrap2PI(Mathf.Atan2(value.z, value.x));
            Phi = Radius == 0f ? 0f : WrapPI(Mathf.Acos(value.y / Radius));
        }
    }

    /// <summary>
    /// Azimuth (longitude) in radians
    /// </summary>
    public float Theta
    {
        get
        {
            return m_theta;
        }
        set
        {
            m_theta = Wrap2PI(value);
        }
    }

    /// <summary>
    /// Inclination (latitude) in radians
    /// </summary>
    public float Phi
    {
        get
        {
            return m_phi;
        }
        set
        {
            float abs_value = Mathf.Abs(value);
            int period_count = Mathf.FloorToInt(abs_value / Mathf.PI);
            m_phi = period_count % 2 == 1 ? (period_count + 1) * Mathf.PI - abs_value : abs_value - period_count * Mathf.PI;

            Theta += Mathf.FloorToInt(value / Mathf.PI) * Mathf.PI;
            //m_phi = WrapPI(value);
        }
    }

    /// <summary>
    /// Longitude in degrees
    /// </summary>
    public float ThetaDeg
    {
        get
        {
            return m_theta * Mathf.Rad2Deg;
        }
        set
        {
            Theta = value * Mathf.Deg2Rad;
        }
    }

    /// <summary>
    /// Latitude in degrees
    /// </summary>
    public float PhiDeg
    {
        get
        {
            return m_phi * Mathf.Rad2Deg;
        }
        set
        {
            Phi = value * Mathf.Deg2Rad;
        }
    }

    /// <summary>
    /// Ensures that alpha stays within [0, 2PI) range
    /// </summary>
    /// <param name="alpha"></param>
    /// <returns></returns>
    public static float Wrap2PI(float alpha)
    {
        int period_count = Mathf.FloorToInt(alpha / (2 * Mathf.PI));
        return alpha - period_count * 2 * Mathf.PI;
    }

    /// <summary>
    /// Ensures that alpha stays within [0, PI] range
    /// </summary>
    /// <param name="alpha"></param>
    /// <returns></returns>
    public static float WrapPI(float alpha)
    {
        float period_count = alpha / Mathf.PI;
        period_count = period_count > 1f ? Mathf.Floor(period_count) : 0f;
        return alpha - period_count * Mathf.PI;
    }
}
