using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************
* AUTHOR: Mitchell O'Sullivan
* PURPOSE: Acts like an object class.
*         Stores important info and functions to do with a meteoroid
* LAST UPDATED: 10/1/2020
***************************************/
public class MeteoroidInfo : MonoBehaviour
{
    /**********************************
    * Reference to helper function
    ***********************************/
    public DateHelper helper;

    /**********************************
    * CLASS FIELDS
    ***********************************/
    public double[] elements = new double[6]; //Array to store orbital elements
    /*-------J. MEEUS VALUES-------------------*/
    public double M0;  //Mean anomaly at time of detection (deg)
    public double vu0; //True anomaly at time of detection (deg)
    public double A, B, C, a, b, c; //Constants of a meteoroid's orbit
    /*-------STANDISH VALUES-------------------*/
    public double vuLanded; //True anomaly at date of detection
    public double vuRate; //Rate of change of true anomaly (updates every frame)
    
    public float plotScale; //Need plotting scale for calculating worldspace positions
    public double dayDetected; //JD of the date of impact with Earth
    public bool landed; //Tells program if meteoroid has landed on Earth yet
    public Vector3 crashPosition; //Position of meteoroid at time of detection
    public Vector3[] positions; //List of daily positions for entire game
    public Material lineMaterial; //Material to give to solid orbit line
    public int trailLength; //Number of points in the meteoroid's trail
    public string infoText; //All variables that follow are for the information panel:
      public float period; //(days)
      public float burnDuration; //(secs)
      public float mass; //(kg)
      public float massError; //(kg)
      public float numDataPts;
      public float numCameras;
   
    /*-----------J. MEEUS START FUNCTIONS------------------*/
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

    public void setM0(double vu)
    /***********************************
     * Converts true anomaly at time of detection (vu0) (deg) to
     *mean anomaly at time of detection (M0) (deg)
     * *********************************/
    {
        double e, E;

        e = elements[1]; //Eccentricity (unitless)
        vu = toRadians(vu); //(rad) for Math.Sin and Math.Tan
        E = 2 * Math.Atan2(Math.Sqrt(1 - e) * Math.Sin(vu/2), Math.Sqrt(1 + e) * Math.Cos(vu/2)); //Eccentric anomaly (rad)

        M0 = E - e * Math.Sin(E); //Mean anomaly (rad)
        M0 = toDegrees(M0);
    }

    public void calculateCrashPosition(double vu)
    /********************************************
     * Calculates the position of meteoroid at time of detection.
     * Uses very similar method to getPositionAt(), but we can bypass
     *the calculation of vu because we're given it in the CSV file (in degrees)
     * ******************************************/
    {
        double SMA, e, w; //Orbital elements
        double E; //Eccentric anomaly
        double r; //Distance to sun at time of detection
        double x, y, z; //Position coordinates

        /*Extract necessary orbital elements*********************/
        SMA = elements[0]; //(AU)
        e   = elements[1]; //(unitless)
        w   = elements[4]; //(deg)

        /*Compute E and r****************************************/
        vu = toRadians(vu); //(rad) for use in Math.Tan and Math.Sin
        E = 2 * Math.Atan2(Math.Sqrt(1 - e)*Math.Sin(vu/2), Math.Sqrt(1 + e)*Math.Cos(vu/2)); //(rad)
        r = SMA * (1 - e*Math.Cos(E)); //(AU)

        /*Compute x,y,z coordinates******************************/
        w = toRadians(w); //(rad) for use in Math.Sin
        x = r*a*Math.Sin(A + w + vu) * plotScale; //(AU)
        y = r*b*Math.Sin(B + w + vu) * plotScale; //(AU)
        z = r*c*Math.Sin(C + w + vu) * plotScale; //(AU)

        /*Swap y & z to match Unity plane************************/
        double tempZ = z;
        z = y;
        y = tempZ;

        /*Return the position************************************/
        crashPosition = new Vector3((float)x, (float)y, (float)z);
    }
    
    /*-------------STANDISH START FUNCTIONS-------------------*/
    public void setVu0()
    {
        double vuAtDetection, vu;

        vuAtDetection = elements[3]; //vu value on the date of detection

        vu = vuAtDetection; //Start at detection time
        for (double day = dayDetected; day >= helper.JD2000; day--) 
        //Loop backwards to 1800
        {
            setVuRate(vu);
            vu = vu - 1*vuRate; //Decrement vu value by one day
            
            //Normalise for accuracy
            while (vu < 0) //If it gets too small
                vu += 360;
            while (vu > 360) //If it gets too big
                vu -= 360;
        }

        vu0 = vu; //Set vu0
        elements[3] = vu0; //Also set vu in elements array to initial value since this
                          //function is only called at the Start
    }
    public double setVuRate(double vu)
    /**************************************************
     * Calculates the current rate of change of true anomaly (vu) in degrees.
     * Uses equations from 'Orbital Mechanics for Engineering Students'
     * Equations used are also in the following two books:
     * (1) Astronomical Algorithms (2nd ed.), by Jean Meeus
     * (2) Astrophysics with a PC
     **************************************************/
    {
        double mu, p; //gravitational parameter and orbital parameter
        double solarMass = 1.9884e30f; //(kg)           cite: http://asa.hmnao.com/static/files/2018/Astronomical_Constants_2018.pdf
        double G = 1.48818e-34f; //(AU^3 kg^-1 days^-2) cite: ^ ^
        double a = elements[0], e = elements[1]; //Semi-major axis (AU) and Eccentricity (radians)
        
        mu = G * (solarMass + mass);
        p = a * (1 - Math.Pow(e, 2));
        vuRate = Math.Sqrt(mu / Math.Pow(p, 3)) * Math.Pow(1 + e * Math.Cos(toRadians(vu)), 2);
        vuRate = toDegrees(vuRate);
        return vuRate;
    }
    public Vector3 standishCrashPosition(double vu)
    {
        Vector3 position; //value to return
        double a, e, I, argPeri, longAsc; //Orbital elements
        double E; //Eccentric anomaly
        double xDash, yDash; //Coordinates in orbital plane (x', y')
        double x, y, z; //Coordinates in 3D space (x, y, z)

        /*Retrieve orbital elements ****************************/
        a       = elements[0]; //(AU)  Semi-major axis
        e       = elements[1]; //(rad) Eccentricity
        I       = elements[2]; //(deg) Inclination
        argPeri = elements[4]; //(deg) Argument of perihelion
        longAsc = elements[5]; //(deg) Longitude of ascending node

        /*Convert to radians************************************/
        I       = toRadians(I);
        vu      = toRadians(vu);
        argPeri = toRadians(argPeri);
        longAsc = toRadians(longAsc);

        /*Compute eccentric anomaly (E) in radians*********************/
        E = 2 * Math.Atan(Math.Sqrt((1 - e)/(1 + e)) * Math.Tan(vu/2));
        //E = Mathf.Acos((e + Mathf.Cos(vu)) / (1 + e*Mathf.Cos(vu)));

        /*Compute planet's x-y coordinates in orbital plane (x', y')***/
        xDash = a * (Math.Cos(E) - e);
        yDash = a * Math.Sqrt(1 - e * e) * Math.Sin(E);

        /*Compute planet's x-y-z coordinates in 3D space****************/
        x = (Math.Cos(argPeri) * Math.Cos(longAsc) - Math.Sin(argPeri) * Math.Sin(longAsc) * Math.Cos(I)) * xDash + (-Math.Sin(argPeri) * Math.Cos(longAsc) - Math.Cos(argPeri) * Math.Sin(longAsc) * Math.Cos(I)) * yDash;
        y = (Math.Cos(argPeri) * Math.Sin(longAsc) + Math.Sin(argPeri) * Math.Cos(longAsc) * Math.Cos(I)) * xDash + (-Math.Sin(argPeri) * Math.Sin(longAsc) + Math.Cos(argPeri) * Math.Cos(longAsc) * Math.Cos(I)) * yDash;
        z = (Math.Sin(argPeri) * Math.Sin(I)) * xDash + (Math.Cos(argPeri) * Math.Sin(I)) * yDash; 

        /*Scale*********************************************************/
        x *= plotScale;
        y *= plotScale;
        z *= plotScale;
        /*Change to x-z plane for Unity*********************************/
        double zTemp = z; //Temporarily store z component
        z = y;
        y = zTemp;

        position = new Vector3((float)x, (float)y, (float)z);
        return position;
    }

    /*-----------------------J. MEEUS UPDATE FUNCTION--------------------*/
    public Vector3 getPositionAt(double JDay)
    /************************************************************
     * Calculates the position of the meteoroid at the imported Julian Day.
     * Uses the orbital elements and the constants A, B, C, a, b & c.
     * **********************************************************/
    {
        Vector3 position; //return value
        double daysBeforeCrash; //Days between 1/1/2000 and JDay
        double SMA, e, n, w; //orbital elements required for this calculation
        double M/*current mean anomaly*/, E/*eccentric anomaly*/;
        double vu/*true anomaly*/, r/*current distance to sun*/;
        double x, y, z; //Coordinates of position

        /*Get orbital elements from array************************/
        SMA = elements[0]; //Semi-major axis (AU)
        e   = elements[1]; //Eccentricity (rad)
        n   = elements[3]; //Mean motion (deg/day)
        w   = elements[4]; //Argument of perifocus (deg)
        //NOTE: We don't need I or OM because we've already utilised them in
        //     the constants A, B, C, a, b & c

        /*Calculate vu and r************************************/
        daysBeforeCrash = dayDetected - JDay;
        //NOTE: If JDay was larger than dayDetected then this function would never
        //     have been called in the first place, so we need not worry bout it
        M = M0 - n*daysBeforeCrash; //(deg)
        while (M > 360)
        {
            M -= 360;
        }
        while (M < 0)
        {
            M += 360;
        }

        E = calculateE(e, M); //(deg)
        E = toRadians(E); //(rad) for use in Math.Tan
        vu = 2 * Math.Atan(Math.Sqrt((1 + e) / (1 - e)) * Math.Tan(E/2)); //(rad)
        r = SMA * (1 - e*Math.Cos(E)); //(AU)

        /*Compute x,y,z coordinates******************************/
        w = toRadians(w); //(rad) for use in Math.Sin
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
    * Calculates the eccentric anomaly (E) based on a meteoroid's e and M
    * Imports e in *radian mode* and M in degrees. Returns E in degrees
    *****************************************************************/
    {
        double E, deltaM, deltaE;

        /*Kepler's equation: M = E - e*sinE, (where e is in degrees).
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

    /*-------------------------STANDISH UPDATE FUNCTIONS----------------------------*/
    public Vector3 getNextPosition(double deltaDays)
    {
        Vector3 position; //Return value

        /*Update the true anomaly (vu)**********************************************/
        setVuRate(elements[3]); //Set new vuRate
        elements[3] += vuRate*deltaDays;

        /*Normalise for accuracy****************************************************/
        while (elements[3] > 360)
        {
            elements[3] -= 360;
        }
        while (elements[3] < 0)
        {
            elements[3] += 360;
        }

        /*Calculate position and return it******************************************/
        position = calculatePosition();
        return position;
    }
    public Vector3 calculatePosition()
    /***********************************************************
    * Calculates the position of a planet in 3D space based on elements
    * ORIGINAL AUTHOR: robbykraft (on 21/11/13)
    * TAKEN FROM: https://gist.github.com/robbykraft/7578514
    * DATE TAKEN: 6/12/19
    * LAST UPDATED BY MITCH: 17/01/2020
    ************************************************************/
    {
        Vector3 position; //value to return
        float a, e, I, vu, argPeri, longAsc; //Orbital elements
        float E; //Eccentric anomaly
        float xDash, yDash; //Coordinates in orbital plane (x', y')
        float x, y, z; //Coordinates in 3D space (x, y, z)

        /*Retrieve orbital elements ****************************/
        a       = (float)elements[0]; //(AU)  Semi-major axis
        e       = (float)elements[1]; //(rad) Eccentricity
        I       = (float)elements[2]; //(deg) Inclination
        vu      = (float)elements[3]; //(deg) True Anomaly
        argPeri = (float)elements[4]; //(deg) Argument of perihelion
        longAsc = (float)elements[5]; //(deg) Longitude of ascending node
        
        /*Convert to radians************************************/
        I       = (float)toRadians(I);
        vu      = (float)toRadians(vu);
        argPeri = (float)toRadians(argPeri);
        longAsc = (float)toRadians(longAsc);

        /*Compute eccentric anomaly (E) in radians*********************/
        E = 2 * Mathf.Atan(Mathf.Sqrt((1 - e)/(1 + e)) * Mathf.Tan(vu/2));
        //E = Mathf.Acos((e + Mathf.Cos(vu)) / (1 + e*Mathf.Cos(vu)));

        /*Compute planet's x-y coordinates in orbital plane (x', y')***/
        xDash = a * (Mathf.Cos(E) - e);
        yDash = a * Mathf.Sqrt(1 - e * e) * Mathf.Sin(E);

        /*Compute planet's x-y-z coordinates in 3D space****************/
        x = (Mathf.Cos(argPeri) * Mathf.Cos(longAsc) - Mathf.Sin(argPeri) * Mathf.Sin(longAsc) * Mathf.Cos(I)) * xDash + (-Mathf.Sin(argPeri) * Mathf.Cos(longAsc) - Mathf.Cos(argPeri) * Mathf.Sin(longAsc) * Mathf.Cos(I)) * yDash;
        y = (Mathf.Cos(argPeri) * Mathf.Sin(longAsc) + Mathf.Sin(argPeri) * Mathf.Cos(longAsc) * Mathf.Cos(I)) * xDash + (-Mathf.Sin(argPeri) * Mathf.Sin(longAsc) + Mathf.Cos(argPeri) * Mathf.Cos(longAsc) * Mathf.Cos(I)) * yDash;
        z = (Mathf.Sin(argPeri) * Mathf.Sin(I)) * xDash + (Mathf.Cos(argPeri) * Mathf.Sin(I)) * yDash;

        /*Scale*********************************************************/
        x *= plotScale;
        y *= plotScale;
        z *= plotScale;
        /*Change to x-z plane for Unity*********************************/
        float zTemp = z; //Temporarily store z component
        z = y;
        y = zTemp;

        position = new Vector3(x, y, z);
        return position;
    }

    public string setInfoText()
    {
        float solarMass = 1.9884e30f; //(kg)           cite: http://asa.hmnao.com/static/files/2018/Astronomical_Constants_2018.pdf
        float G = 1.48818e-34f; //(AU^3 kg^-1 days^-2) cite: ^ ^
        int[] dateDetectedElements;
        int d, m, y; //day, month and year of detection

        period = 2 * Mathf.PI / Mathf.Sqrt(G * solarMass) * Mathf.Pow((float)elements[0], 3f/2f);

        dateDetectedElements = helper.JDToDate(dayDetected);
        d = dateDetectedElements[0];
        m = dateDetectedElements[1];
        y = dateDetectedElements[2];

        infoText = "Period: " + period + " days \n";
        infoText += "       (" + period/365.2585f + " Earth yrs)\n";
        infoText += "Date of detection: " + d + "/" + m + "/" + y + "\n";
        infoText += "       (" + dayDetected + " JD)\n";
        infoText += "Estimated mass: \n";
        infoText += mass.ToString("0.0000000") + " +/- " + massError.ToString("0.0000000") + " kg\n";
        infoText += "\n";
        infoText += "No. of cameras recording: " + numCameras + "\n";
        infoText += "Burn duration: " + burnDuration + "s\n";
        infoText += "No. of data points: " + numDataPts + "\n";
        infoText += "\n";
        infoText += "Orbital elements: \n";
        infoText += "a = " + (float)elements[0] + " AU\n";
        infoText += "e = " + (float)elements[1] + " (rad)\n";
        infoText += "i = " + (float)elements[2] + " (deg)\n";
        infoText += "n = " + (float)elements[3] + " (deg/day)\n";
        infoText += "w = " + (float)elements[4] + " (deg)\n";
        infoText += "long_asc_nod = " + (float)elements[5] + " (deg)\n";
        infoText += "M0 = " + (float)M0 + " (deg)\n";

        return infoText;
    }

    public void setInfoText(string inInfoText)
    {
        infoText = inInfoText;
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
