using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/****************************************
* AUTHOR: Mitchell O'Sullivan
* PURPOSE: Acts like an object class. Stores important info and functions to do with 
*         a planet.
* LAST UPDATED: 10/1/2020
*****************************************/
public class PlanetInfo : MonoBehaviour
{
    /************************************
    * CLASS FIELDS
    *************************************/
    public double[] elements = new double[6]; //Orbital elements of previous frame
    public double M0; //The mean anomaly at J2000 - sort of a 7th orbital element
    public double A, B, C, a, b, c; //Constants for a planet's orbit
    public float plotScale; //Need plotting scale for calculating worldspace positions
    public float autoSize, realSize; //Sizes of planet on different scales
    public Vector3[] positions; //List of daily positions for entire game
    public Material lineMaterial; //Material to give to the solid orbit line
    public int trailLength; //Number of points in the planet's trail
    public string infoText; //All following variables are for information purposes
      public float radius; //(km)
      public float mass; //(kg) 
      public float gravity; //(m/s^2)
      public float period; //(days)

    private void Start()
    {
        autoSize *= 0.4f;
    }

    public void setConstants()
    /******************************************
     * Initialises the constants A, B, C, a, b & c. These are used for 
     *position calculations.
     * Executed before program begins.
     * *****************************************/
    {
        //Temporary values used to calculate A,B,C,a,b,c:
        double F, G, H, P, Q, R;
        double OM, I; //long asc node and inclination

        I  = toRadians(elements[2]); //(rad)
        OM = toRadians(elements[5]); //(rad)

        F = Math.Cos(OM); //(unitless)
        G = Math.Sin(OM); //(unitless)
        H =            0; //(unitless)

        P = -Math.Sin(OM) * Math.Cos(I); //(unitless)
        Q =  Math.Cos(OM) * Math.Cos(I); //(unitless)
        R =                 Math.Sin(I); //(unitless)

        A = Math.Atan2(F, P); //(rad)
        B = Math.Atan2(G, Q); //(rad)
        C = Math.Atan2(H, R); //(rad)

        a = Math.Sqrt(F*F + P*P); //(unitless)
        b = Math.Sqrt(G*G + Q*Q); //(unitless)
        c = Math.Sqrt(H*H + R*R); //(unitless)
    }
    
    /**************************************
    * GETTER / SETTER for orbital elements at 
    *                a specific ctysElapsed value
    ***************************************/    
    public Vector3 getPositionAt(double JDay)
    /********************************************
    * Calculates the position of the planet at the imported JDay based on
    *the orbital elements.
    * Method taken from 'Astronomical Algorithms' 2nd ed. by Jean Meeus (p227)
    *********************************************/
    {
        Vector3 position; //Return value

        double daysSinceJ2000;
        double SMA, e, n, w; //orbital elements
        double M/*current mean anomaly*/, E/*eccentric anomaly*/;
        double vu/*true anomaly*/, r/*current distance to sun*/;
        double x, y, z;

        /*Get orbital elements from array************************/
        SMA = elements[0]; //Semi-major axis (AU)
        e   = elements[1]; //Eccentricity (rad)
        n   = elements[3]; //Mean motion (deg/day)
        w   = elements[4]; //Argument of perifocus (deg)
        //NOTE: We don't need I or OM because we've already utilised them in
        //     the constants A, B, C, a, b & c

        /*Calculate vu and r************************************/
        daysSinceJ2000 = JDay - 2451544.5;
        M = M0 + n*daysSinceJ2000; //(deg)
        while (M > 360)
        { 
            M -= 360;    
        }
        while (M < 0)
        {
            M += 360;
        }

        E = calculateE(e, M); //(deg)
        E = toRadians(E); //(rad)
        vu = 2 * Math.Atan(Math.Sqrt((1 + e) / (1 - e)) * Math.Tan(E/2)); //(rad)
        r = SMA * (1 - e*Math.Cos(E)); //(AU)

        /*Compute x,y,z coordinates******************************/
        w = toRadians(w);
        x = r*a*Math.Sin(A + w + vu) * plotScale; //(AU)
        y = r*b*Math.Sin(B + w + vu) * plotScale; //(AU)
        z = r*c*Math.Sin(C + w + vu) * plotScale; //(AU)

        /*Swap y & z for Unity positioning***********************/
        double tempZ = z;
        z = y;
        y = tempZ;

        /*Return the position************************************/
        position = new Vector3((float)x, (float)y, (float)z);
        return position;
    }

    private double calculateE(double e, double M)
    /****************************************************************
    * Calculates the eccentric anomaly (E) based on a planet's e and M
    * Returns E in degrees
    *****************************************************************/
    {
        double E, deltaM, deltaE;

        /*Kepler's equation: M = E - e*sinE ,
        (where e is in degrees).
        The solution is iterative*/
        E = M + toDegrees(e) * Math.Sin(toRadians(M)); //E0
        deltaE = 1; //Default value to run thru loop atleast once

        //Iterate for precision (10^-6 is sufficient)
        while (Math.Abs(deltaE) > 0.000001)
        {
            deltaM = M + (toDegrees(e) * Math.Sin(toRadians(E)) - E);
            deltaE = deltaM / (1 - e * Math.Cos(toRadians(E)));
            E += deltaE;
        }
        return E;
    }

    public void setInfoText()
    {
        float solarMass = 1.9884e30f; //(kg)           cite: http://asa.hmnao.com/static/files/2018/Astronomical_Constants_2018.pdf
        float G = 1.48818e-34f; //(AU^3 kg^-1 days^-2) cite: ^ ^
        float SMA = (float)elements[0]; //semi-major axis

        //Calculate period from Kepler's 3rd Law
        period = 2 * Mathf.PI / Mathf.Sqrt(G * solarMass) * Mathf.Pow(SMA, 3f/2f);

        infoText = "Radius: " + radius + " km\n";
        infoText += "(" + Mathf.Pow(radius/6371f, 2).ToString("0.000") + " times the size of Earth)\n";
        infoText += "Year length: " + period + " days\n";
        infoText += "       (" + (period/365.2585f).ToString("0.00") + " Earth yrs)\n";
        infoText += "Mass: " + mass + " kg\n";
        infoText += "Gravity: " + gravity + " m/s^2\n";
        infoText += "\n";
        infoText += "Orbital elements:\n";
        infoText += "a = " + (float)elements[0] + " AU\n";
        infoText += "e = " + (float)elements[1] + " (rad)\n";
        infoText += "i = " + (float)elements[2] + " (deg)\n";
        infoText += "mean_long = " + (float)elements[3] + " (deg)\n";
        infoText += "long_Peri = " + (float)elements[4] + " (deg)\n";
        infoText += "long_Asc_Nod = " + (float)elements[5] + " (deg)\n";
    }

    /*-----------------------HELPER FUNCTIONS------------------------------*/

    private double toRadians(double degree)
    /*****************************************************************
    * Converts an angle in degrees to radians
    *****************************************************************/
    {
        return degree * Math.PI/180;
    }
    private double toDegrees(double radian)
    /*****************************************************************
    * Converts an angle in radians to degrees
    ******************************************************************/
    {
        return radian * 180/Math.PI;
    }
}
