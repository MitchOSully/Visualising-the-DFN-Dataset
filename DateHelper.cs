using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DateHelper : MonoBehaviour
/************************************************************
* Contains methods concerned with manipulating date and time
* Used by ScreenMaster (extensively) and BodyPlotter (lil bit)
* AUTHOR: Mitchell O'Sullivan
* LAST UPDATED: 24/2/2020
*************************************************************/
{
    /***************************
    * HELPFUL CONSTANTS
    ****************************/
    public double JD2000; //JD of start date for moving bodies (1/1/2000)
    public double JD2020; //JD of finish date for moving bodies (31/12/2020)
    public double JDOrbitStart; //JD of start date for orbit lines (1/1/1800)

    public void start()
    /**********************************************
     * Called by the Start() function in MasterScript.cs
     * ********************************************/
    {
        JD2000 = dateToJD(new int[] { 1, 1, 2000 });
        JD2020 = dateToJD(new int[] {31,12, 2020 });
        JDOrbitStart = JD2000 - 60183; //Neptune takes 60182 days to fully orbit the sun
    }

    public string getDateErrorMessage(string[] dateElements)
    /********************************************************
    * Conducts validation on date. If return string == null, then no
    *error occured. If return string != null, then error occured
    *********************************************************/
    {
        string errorMessage = null;
        int day, month, year;

        try
        {
            year  = Int32.Parse(dateElements[2]);
            month = Int32.Parse(dateElements[1]);
            day   = Int32.Parse(dateElements[0]);

            if (dateElements.Length != 3) //validate number of elements
                errorMessage = "Incorrect Formatting";
            else if (year < 2000 || year > 2020) //validate year
                errorMessage = "Year out of bounds";
            else if (month < 1 || month > 12) //validate month
                errorMessage = "Incorrect Month";
            else if (!daysValid(day, month, year)) //validate days
                errorMessage = "Incorrect Day";
        }
        catch (FormatException) //Not a number
        {
            errorMessage = "Incorrect Formatting";
        }
        catch (IndexOutOfRangeException) //Not enough slashes (/)
        {
            errorMessage = "Incorrect Formatting";
        }


        return errorMessage;
    }

    public double dateToJD(int[] dateElements)
    /*******************************************************************
    * Converts a Gregorian Calendar date, represented by an array of ints, 
    *into a Julian Day (float). 
    * NOTE: dateElements = {<day>, <month>, <year>}
    * Calculation taken from Jean Meeus' "Astronomical Algorithms, 2nd ed." p61
    ********************************************************************/
    {
        double JD; //Julian day (return value)
        int Y = dateElements[2], M = dateElements[1], D = dateElements[0];
        int A, B; //Intermediate values (meaningless)

        if (M <= 2) //If we're in January or February
        {
            Y -= 1;
            M += 12; 
            //We're now working in the 13th or 14th month of the previous year
        }

        A = Y/100;      //Automatically truncated
        B = 2 - A + A/4;//^ ^ 

        JD = (double)(int)(365.25f*((double)Y + 4716f)) + (double)(int)(30.6001f*((double)M + 1f)) + (double)D + (double)B - 1524.5f;

        return JD;
    }

    public int[] JDToDate(double JD)
    /********************************************************************
    * Converts a Julian Day (float) to a date represented by an array of ints.
    * Calculation taken from Jean Meeus' "Astronomical Algorithms, 2nd ed." p63
    *********************************************************************/
    {
        int[] dateElements = new int[3]; //day, month, year
        double F; //Fractional part of JD+0.5
        int   Z; //Integer part of JD+0.5
        int alpha, A, B, C, D, E; //Intermediate values (meaningless)

        /*Abort if we have negative JD number (we won't tho)****************/
        if (JD < 0) //Method doesn't work for negative values
            throw new Exception("JD is negative. Invalid");

        /*Get fractional and integer parts**********************************/
        JD += 0.5f;
        Z = (int)JD;
        F = JD - (double)Z;

        /*Calculate intermediate values*************************************/
        if (Z < 2299161)
            A = Z;
        else
        {
            alpha = (int)(((double)Z - 1867216.25f)/36524.25f);
            A = Z + 1 + alpha - alpha/4; //alpha/4 is automatically truncated
        }
        B = A + 1524;
        C = (int)(((double)B - 122.1f)/365.25f);
        D = (int)(365.25f * (double)C);
        E = (int)(((double)B - (double)D)/30.6001f);

        /*Calculate dateElements*********************************************/
        //Day
        dateElements[0] = (int)((double)B - (double)D - (double)(int)(30.6001f * (double)E) + F);
        //Month
        if (E < 14)
            dateElements[1] = E - 1;
        else //E == 14 || E == 15
            dateElements[1] = E - 13;
        //Year
        if (dateElements[1] > 2)
            dateElements[2] = C - 4716;
        else //month == 1 || month == 2
            dateElements[2] = C - 4715;

        return dateElements;
    }

    public string daysToDate(double daysElapsed)
    /*************************************************************************
    * Converts daysElapsed (days since 01/01/1800) to a date string in the format:
    * "<day>/<month>/<year>
    **************************************************************************/
    {
        string dateString; //Return value
        int[] date; //date[0] == day, date[1] == month, date[2] == year
        double currJD; //current Julian day

        currJD = JD2000 + daysElapsed; //Add daysElapsed to original JD to get current JD
        date = JDToDate(currJD); //Use other function to get date elements

        //Convert to string:
        dateString = date[0].ToString("00") + "/" + date[1].ToString("00") + "/" + date[2];

        return dateString;
    }

    /*-----HELPER FUNCTIONS FOR THE HELPER CLASS-------------------*/

    private bool isALeapYear(int year)
    /******************************************
    * Returns true if year is a leap year, and false if not
    * For a year to be a leap one, it must satisfy 3 conditions:
    * 1. Is a multiple of 4
    * 2. BUT is not a multiple of 100
    * 3. UNLESS is a multiple of 400 
    * https://www.timeanddate.com/date/leapyear.html
    *******************************************/
    {
        bool verdict = false;

        if ((year % 4 == 0 && year % 100 != 0) || year % 400 == 0)
        //   goes into 4     BUT not into 100    UNLESS goes into 400
            verdict = true;

        return verdict;
    }

    private bool daysValid(int day, int month, int year)
    /******************************************
    * Returns true if day is valid, and false if not
    *******************************************/
    {
        int daysInMonth = calcDaysInMonth(month, year); //Call helper function

        return (0 < day && day <= daysInMonth);
    }

    private int calcDaysInMonth(int month, int year)
    /********************************************
    * Determines the days in the month, accounting for leap years
    *********************************************/
    {
        int daysInMonth;

        switch (month)
        {
            case 1: case 3: case 5: case 7: case 8: case 10: case 12:
                daysInMonth = 31;
                break;
            case 4: case 6: case 9: case 11:
                daysInMonth = 30;
                break;
            default://February
                if (isALeapYear(year))
                    daysInMonth = 29;
                else
                    daysInMonth = 28;
                break;
        }

        return daysInMonth;
    }
}
