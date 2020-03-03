using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPlotter : MonoBehaviour
/**************************************************************
* AUTHOR: Mitchell O'Sullivan
* LAST UPDATED: 21/1/2020
***************************************************************/
{
    /*Public global variables***********************************/
    public DateHelper helper; //Allows access to helper functions in DateHelper script
    public ScreenMaster screenScript; //Must be given to planets to display info
    public GameObject sun; //Reference to sun object (for scaling)
    public GameObject mercury, venus, earth, mars, jupiter, saturn, uranus, neptune; 
                              //planet prefabs
    public GameObject meteoroidPrefab; //meteoroid prefab
    public string meteorFile;
    public float plotScale;
    public bool showMeteorLines = true;
    
    public double daysPerFrame = 1; //Days to elapse per frame
    public double daysElapsed = 0; //Days elapsed since 01/01/1800
    public bool paused = false;
    public bool movingCamera = false; //Tells us if camera is being moved by user
                                      //(Updated by CameraController script)

    /*Private global variables**********************************/
    private GameObject[] planets, meteoroids;
    private double totalDays;  //Number of days the program runs for
    private List<Dictionary<string, object>>[] elLists; //Stores read-in planet elements

    public void start()
    /**************************************************************
     * Called by the Start() function in MasterScript.cs
     * ************************************************************/
    {
        /*Calculate number of days the program covers**************/
        totalDays  = helper.JD2020 - helper.JD2000 + 1;//+1 for initial day

        /*Make objects*********************************************/
        makePlanets();
        makeMeteoroids();
    }

    public void update()
    /**************************************************************
    * Called by the Update() function in MasterScript.cs
    **************************************************************/
    {
        /*Update daysElapsed**************************************/
        if (!paused)
            daysElapsed += daysPerFrame;
        
        if (daysElapsed >= totalDays)
        //If we reach the end of our time period, start again
            daysElapsed = 1;

        /*Update our objects**************************************/
        updatePlanets();
        updateMeteoroids();
    }

    private void makePlanets()
    /***********************************************************
    * Instantiates all planets in their initial positions along with their orbital lines
    ***********************************************************/
    {
        Vector3 position;
        PlanetInfo planetInfo;
        string planetName;
        float scale;

        /*Store planet objects in array*************************/
        planets = new GameObject[8];
        planets[0] = mercury;
        planets[1] = venus;
        planets[2] = earth;
        planets[3] = mars;
        planets[4] = jupiter;
        planets[5] = saturn;
        planets[6] = uranus;
        planets[7] = neptune;

        /*Read in elements***************************/
        elLists = new List<Dictionary<string, object>>[8];
        elLists[0] = CSVReader.Read("mercuryEl");
        elLists[1] = CSVReader.Read("venusEl");
        elLists[2] = CSVReader.Read("earthEl");
        elLists[3] = CSVReader.Read("marsEl");
        elLists[4] = CSVReader.Read("jupiterEl");
        elLists[5] = CSVReader.Read("saturnEl");
        elLists[6] = CSVReader.Read("uranusEl");
        elLists[7] = CSVReader.Read("neptuneEl");

        /*Give values to the planets****************/
        for (int ii = 0; ii < planets.Length; ii++)
        {
            planetInfo = planets[ii].GetComponent<PlanetInfo>();

        /*Give it orbital elements******************/
            //Semi-major axis (AU):
            planetInfo.elements[0] = Convert.ToDouble(elLists[ii][0]["                      A"]);
            //Eccentricity (unitless):
            planetInfo.elements[1] = Convert.ToDouble(elLists[ii][0]["                     EC"]);
            //Inclination (deg):
            planetInfo.elements[2] = Convert.ToDouble(elLists[ii][0]["                     IN"]);
            //Mean motion (deg/day):
            planetInfo.elements[3] = Convert.ToDouble(elLists[ii][0]["                      N"]);
            //Argument of perifocus (deg):
            planetInfo.elements[4] = Convert.ToDouble(elLists[ii][0]["                      W"]);
            //Longitude of ascending node (deg):
            planetInfo.elements[5] = Convert.ToDouble(elLists[ii][0]["                     OM"]);
            //Mean anomaly at J2000:
            planetInfo.M0          = Convert.ToDouble(elLists[ii][0]["                     MA"]);

        /*Set the orbital constants A,B,C,a,b,c******/
            planetInfo.setConstants();

        /*Give it an initial position and a name*****/
            planetName = planets[ii].transform.name; //Save name so we don't get '(clone)'
            planetInfo.plotScale = plotScale; //Give it plotScale for calculating positions
            position = planetInfo.getPositionAt(helper.JD2000);//Calculate initial position
            planets[ii] = Instantiate(planets[ii], position, Quaternion.identity); 
                                                                  //Put planet in the scene
            planets[ii].transform.name = planetName; //Now give planet the name we saved

        /*Resize to 0.4 times its OG size************/
            scale = planets[ii].transform.localScale.x;
            planets[ii].transform.localScale = new Vector3(0.4f*scale, 0.4f*scale, 0.4f*scale);

        /*Give it an orbit line**********************/
            createLine(planets[ii], "planet"); //This also initialises planetInfo.positions[]

        /*Set the text to be displayed when planet is right-clicked****/
            planetInfo.setInfoText();
            //Also need a reference to screen for displaying the info:
            planets[ii].GetComponent<InteractWithObject>().screenScript = screenScript; 
        }
    }
   
    private void makeMeteoroids()
    /*************************************************************************
    * Loops through the the lines in 'meteorFile' and creates a meteoroid if the
    *orbital elements are valid for this program.
    **************************************************************************/
    {
        List<Dictionary<string, object>> meteoroidList;
        MeteoroidInfo meteorInfo;
        double[] elements; //Orbital elements of an meteoroid
        int numBadMeteors = 0; //Number of meteoroids we don't want to plot
        int newIdx; //The current index in the array, accounting for the fact that bad meteors are not added
        double vu0; //True anomaly at time of detection for a single meteoroid
        GameObject[] tempMeteoroidList; //Temporary array used when resizing meteoroid list
        int newLength; //Length of meteoroid list without all the bad meteoroids

        /*Use helper function to read file. Then prepare meteoroids array***/
        meteoroidList = CSVReader.Read(meteorFile);
        meteoroids = new GameObject[meteoroidList.Count];

        /*Loop through all meteoroids and add them to our list if they're good*******/
        for (int ii = 0; ii < meteoroids.Length; ii++)
        {
            /*Orbital elements*******************************************************/
            elements = new double[6];
            //Semi-major axis (a) (AU):
            elements[0] = Convert.ToDouble(meteoroidList[ii]["semi_major_axis"]);
            //Eccentricity (e) (unitless):
            elements[1] = Convert.ToDouble(meteoroidList[ii]["eccentricity"]);
            //Inclination (i) (deg):
            elements[2] = Convert.ToDouble(meteoroidList[ii]["inclination"]);
            //Mean motion (n) (deg/day):
            elements[3] = 0.9856076686 / (elements[0] * Math.Sqrt(elements[0]));
            //Argument of perifocus (w) (deg):
            elements[4] = Convert.ToDouble(meteoroidList[ii]["arg_p"]);
            //Longitude of ascending node (OM) (deg):
            elements[5] = Convert.ToDouble(meteoroidList[ii]["long_nodes"]);

        /*Don't add to list of meteors if one of the following criteria is true:
        * (1) Semi-major axis less than 0
        * (2) Eccentricity greater than 1
        *****************************************************************************/
            if (elements[0] <= 0 || elements[1] > 1) //Bad meteoroid
            {
                numBadMeteors++; //Incrememnt number of bad meteors

            }
            else //Good meteoroid
            {
                newIdx = ii - numBadMeteors; //Index we're up to in 'meteoroids' array

                meteoroids[newIdx] = Instantiate(meteoroidPrefab);//Put in list
                meteorInfo = meteoroids[newIdx].GetComponent<MeteoroidInfo>();
                meteorInfo.elements = elements; //Give elements to meteoroid GameObject
                
            /*Read the rest of the data from the file*********************************/
                //Name:
                meteoroids[newIdx].transform.name = meteoroidList[ii]["codename"].ToString();
                //Initial true anomaly (deg) - need for M0 calculation:
                vu0 = Convert.ToDouble(meteoroidList[ii]["true_anomaly"]);
                //Date detected (Julian Day):
                meteorInfo.dayDetected = Convert.ToDouble(meteoroidList[ii]["JD"]);
                //Values for info panel:
                meteorInfo.burnDuration = Convert.ToSingle(meteoroidList[ii]["duration"]);
                meteorInfo.numDataPts   = Convert.ToSingle(meteoroidList[ii]["number_datapoints"]);
                meteorInfo.mass         = Convert.ToSingle(meteoroidList[ii]["EKS_m0"]);
                meteorInfo.massError    = Convert.ToSingle(meteoroidList[ii]["EKS_m0_err"]);
                meteorInfo.numCameras   = Convert.ToSingle(meteoroidList[ii]["num_cams_astrometry"]);

            /*And now we can work with what we've got********************************/
            /*Set initial mean anomaly (M0) and constants A, B, C, a, b, c***********/
                meteorInfo.setM0(vu0);
                meteorInfo.setConstants();

            /*Set text to display when right-clicked on the meteoroid****************/
                meteorInfo.setInfoText();

            /*Set position of meteoroid at date of detection*************************/
                meteorInfo.plotScale = plotScale;//Need plotScale for calculating positions
                meteorInfo.calculateCrashPosition(vu0);

            /*Instantiate meteoroid in its initial position*************************/
                meteoroids[newIdx].transform.position = meteorInfo.getPositionAt(helper.JD2000);
                
            /*Create solid orbit line***********************************************/
                if (showMeteorLines)
                    //Give it an orbit line
                    createLine(meteoroids[newIdx], "meteoroid");
                else
                    //Deactivate trail since we don't need it
                    meteoroids[newIdx].GetComponent<LineRenderer>().enabled = false;
                
            /*Give it a reference to screen for information display*****************/
                meteoroids[newIdx].GetComponent<InteractWithObject>().screenScript = screenScript;
            }
        }

        /*Resize the global meteoroid list so there's no empty spots*****************/
        newLength = meteoroids.Length - numBadMeteors; //Size of array without bad meteors
        tempMeteoroidList = new GameObject[newLength]; //Temporarily store values here
        Array.Copy(meteoroids, 0, tempMeteoroidList, 0, newLength);
        meteoroids = new GameObject[newLength]; //Resize
        tempMeteoroidList.CopyTo(meteoroids, 0); //Put everything back
    }

    private void updatePlanets()
    /*********************************************************
     * Puts planets and their trails in next position.
     * Also resizes the planets' lines and halos.
     * *******************************************************/
    {
        PlanetInfo planetInfo;

        for (int ii = 0; ii < planets.Length; ii++)
        {
            planetInfo = planets[ii].GetComponent<PlanetInfo>();

            //Put each planet in next position if playing
            if (!paused)
            {
                planets[ii].transform.position = planetInfo.getPositionAt(daysElapsed + helper.JD2000);
                updateTrail(planets[ii], "planet"); //Move trail to next pos also
            }
            //Resize each planet's line and halo according to user's distance from it
            resize(planets[ii], "planet");
        }
    }

    private void updateMeteoroids()
    /****************************************************
     * Puts meteoroids and their trails in next position.
     * Also resizes lines and halos.
     * Also checks if the current date is past the 'crash date'
     *of the meteoroid
     * **************************************************/
    {
        MeteoroidInfo meteorInfo;

        for (int ii = 0; ii < meteoroids.Length; ii++)
        {
            meteorInfo = meteoroids[ii].GetComponent<MeteoroidInfo>();

            //Check if we're past the landing date before updating position
            checkCollision(meteoroids[ii]);

            //Put each meteoroid in next position if playing and if not landed yet
            if (!paused && !meteorInfo.landed)
            {
                meteoroids[ii].transform.position = meteorInfo.getPositionAt(helper.JD2000 + daysElapsed);
                if (showMeteorLines)
                    updateTrail(meteoroids[ii], "meteoroid"); //Move trail to next pos also
            }
            //Resize each meteoroid's line and halo according to user's distance from it
            resize(meteoroids[ii], "meteoroid");
        }
    }

    private void createLine(GameObject body, string type)
    /********************************************************************
    * Creates an empty object with a LineRenderer component and fills it with
    *the positions array in the body object (every position it will every occupy).
    * The 'type' variable tells us if it's a planet or meteoroid, and we get the
    *appropriate component and values
    ********************************************************************/
    {
        PlanetInfo planetInfo;    //Reference to body's values if it's a planet
        MeteoroidInfo meteorInfo; //^                        ^ if it's an meteoroid
        GameObject orbitLineObj; //Referenct to object with a line renderer component
        LineRenderer orbitLine; //The line renderer component
        Vector3[] linePositions; //The array holding all positions for the line renderer
        double orbitStartDay; //The JD at which we will start this body's solid orbit line
        int startIdx; //The idx in linePositions where the day is J2000 (our start time)
        int posArrLength; //Length of array of positions for J2000 to 2020

        /*Give body a child GameObject with a line renderer component**********/
        //(NOTE: we cannot add a line renderer component straight to 'body' because 
        //      it already has one: the fade-away trail
        orbitLineObj = new GameObject(body.transform.name + " OrbitLine");
        orbitLineObj.transform.SetParent(body.transform);
        orbitLineObj.AddComponent<LineRenderer>();
        orbitLine = orbitLineObj.GetComponent<LineRenderer>();

        if (string.Compare(type, "planet") == 0) //If we got a planet
        {
            planetInfo = body.GetComponent<PlanetInfo>();

        /*Prepare line-renderer component**************************************/
            orbitLine.material = planetInfo.lineMaterial; //Give it a colour
            orbitLine.positionCount = (int)planetInfo.period;
            linePositions = new Vector3[(int)planetInfo.period];

        /*Loop through one planetary period to get all positions***************/
            orbitStartDay = helper.JD2020 - planetInfo.period; //Orbit finishes at 2020
            for (int days = 0; days < linePositions.Length; days++)
            {
                linePositions[days] = planetInfo.getPositionAt(orbitStartDay + days);
            }

        /*Give orbitLine the positions****************************************/
            orbitLine.SetPositions(linePositions);
            orbitLine.loop = true; //Also close the loop

        /*Give planetInfo the positions from J2000 to 2020********************/
        //(This is done so the fade-away trail can take positions from planetInfo)
            posArrLength = (int)totalDays;
            planetInfo.positions = new Vector3[posArrLength];
            if (orbitStartDay < helper.JD2000)
            // If we've already covered 2000 to 2020 in the previous loop, we just need
            //to copy those positions into this array
            // (This occurs for Saturn, Uranus and Neptune)
            {
                startIdx = linePositions.Length - posArrLength;
                Array.Copy(linePositions, startIdx, planetInfo.positions, 0, posArrLength);
            }
            else //If we haven't already got those positions, we must copy what we've got
                 //from previous loop, then calculate the rest ourselves
                 // (This occurs for all inner planets and Jupiter)
            {
                //Index in planetInfo.positions at which copying begins:
                startIdx = posArrLength - linePositions.Length; 
                //Copy contents of linePositions to the back-end of planetInfo.positions:
                linePositions.CopyTo(planetInfo.positions, startIdx);
                //Calculate the rest:
                int days = 0;
                while (planetInfo.positions[days].sqrMagnitude == 0)
                //Keep looping till we reach the section we've already filled
                //NOTE: Unfilled spots have magnitude 0
                {
                    planetInfo.positions[days] = planetInfo.getPositionAt(helper.JD2000 + days);
                    days++;
                }
            }
        }
        else //If we got a meteoroid
        {
            meteorInfo = body.GetComponent<MeteoroidInfo>();

        /*Prepare line-renderer component**************************************/
            orbitLine.material = meteorInfo.lineMaterial; //Give it a colour
            orbitLine.positionCount = (int)meteorInfo.period;
            linePositions = new Vector3[(int)meteorInfo.period]; 

        /*Loop through one period (a day at a time) to get all positions*******/
            orbitStartDay = helper.JD2020 - meteorInfo.period; //Orbit finishes at 2020
            for (int day = 0; day < linePositions.Length; day++)
            {
                linePositions[day] = meteorInfo.getPositionAt(orbitStartDay + day);
            }

        /*Give orbitLine the positions*********************************/
            orbitLine.SetPositions(linePositions);
            orbitLine.loop = true; //Also close the loop

        /*Give meteorInfo the positions from J2000 to 2020*************/
        //(This is done so the fade-away trail can take positions from planetInfo)
            posArrLength = (int)totalDays;
            meteorInfo.positions = new Vector3[posArrLength];
            if (orbitStartDay < helper.JD2000)
            // If we've already covered 2000 to 2020 in the previous loop, we just need
            //to copy those positions into this array
            {
                startIdx = linePositions.Length - posArrLength;
                Array.Copy(linePositions, startIdx, meteorInfo.positions, 0, posArrLength);
            }
            else //If we haven't already got those positions, we must copy what we've got
                 //from previous loop, then calculate the rest ourselves
            {
                //Index in planetInfo.positions at which copying begins:
                startIdx = posArrLength - linePositions.Length; 
                //Copy contents of linePositions to the back-end of planetInfo.positions:
                linePositions.CopyTo(meteorInfo.positions, startIdx);
                //Calculate the rest:
                int days = 0;
                while (meteorInfo.positions[days].sqrMagnitude == 0)
                //Keep looping till we reach the section we've already filled
                //NOTE: Unfilled spots have magnitude 0
                {
                    meteorInfo.positions[days] = meteorInfo.getPositionAt(helper.JD2000 + days);
                    days++;
                }
            }
        }

        /*Disable the line so we cannot see it*************************/
        //(There is an option to activate it in the 'Settings' panel)
        orbitLine.enabled = false;
    }

    private void updateTrail(GameObject body, string type)
    /*****************************************************
    * Gives newest positions to the trail of the body
    ******************************************************/
    {
        PlanetInfo planetInfo;
        MeteoroidInfo meteorInfo;
        LineRenderer trail; //Reference to line renderer component
        Vector3[] trailPositions; //Points in the trail line
        Vector3[] positionList; //Complete list of daily positions for body throughout game
        int trailLength;

        /*Retrieve values depending on whether we have a planet or meteoroid***/
        if (string.Compare(type, "planet") == 0) //If we got a planet
        {
            planetInfo = body.GetComponent<PlanetInfo>();
            trailLength = planetInfo.trailLength;
            positionList = planetInfo.positions;
        }
        else //If we got a meteoroid
        {
            meteorInfo = body.GetComponent<MeteoroidInfo>();
            trailLength = meteorInfo.trailLength;
            positionList = meteorInfo.positions;
        }

        /*Prepare position vector for trail***********************/
        if (daysElapsed < trailLength) //At the start
            trailPositions = new Vector3[(int)daysElapsed +1]; //Extra spot for initial pos
        else //Any time other than the start
            trailPositions = new Vector3[trailLength];

        /*Loop backwards for 'trailPositions.Length' number of days***/
        for (int ii = 0; ii < trailPositions.Length; ii++)
        {
            trailPositions[ii] = positionList[(int)daysElapsed - ii];
        }

        /*Give positions to the trail*****************************/
        trail = body.GetComponent<LineRenderer>();
        trail.positionCount = trailPositions.Length;
        trail.SetPositions(trailPositions);
    }

    private void checkCollision(GameObject meteoroid)
    /************************************************
    * Checks if the imported meteoroid has collided with Earth yet
    *************************************************/
    {
        MeteoroidInfo meteorInfo = meteoroid.GetComponent<MeteoroidInfo>();
        double currentDay;
        Light halo = meteoroid.GetComponent<Light>(); //The halo of the meteoroid

        currentDay = helper.JD2000 + daysElapsed;

        if (currentDay >= meteorInfo.dayDetected) //Meteoroid has collided
        {
            if (!meteorInfo.landed) //If it hasn't landed already
            {
                //Remove the trail
                if (showMeteorLines)
                    meteoroid.GetComponent<LineRenderer>().enabled = false;
            }
            meteorInfo.landed = true;
            halo.color = Color.red;
            meteoroid.transform.position = meteorInfo.crashPosition;
        }
        else //Meteoroid has not collided 
        {
            if (meteorInfo.landed) //If it had landed previously
            {
                //Reactivate the trail
                if (showMeteorLines)
                    meteoroid.GetComponent<LineRenderer>().enabled = true;
            }
            meteorInfo.landed = false;
            halo.color = Color.white;
        }
    }

    private void resize(GameObject body, string type)
    /*******************************************************************
    * Alters the size of the halos, sphere colliders and orbit lines of 
    *a celestial body according to how far away the camera is.
    ********************************************************************/
    {
        float bodySize, haloSize, lineSize;
        Vector3 cameraPos; //Position of viewer

        cameraPos = Camera.main.gameObject.transform.position;

        bodySize = body.transform.localScale.x; //Scale of object (x = y = z)
        lineSize = Vector3.Magnitude(cameraPos) / 500;
        haloSize = bodySize + getDistanceY(body) / 200;

        //Resize halo
        body.GetComponent<Light>().range = haloSize;
        //Resize the trail
        body.GetComponent<LineRenderer>().widthMultiplier = lineSize;

        if (string.Compare(type, "meteoroid") == 0)
        {
            //Resize sphere collider for meteoroids
            body.GetComponent<SphereCollider>().radius = haloSize;
            //Resize orbit line
            if (showMeteorLines)
                body.GetComponentsInChildren<LineRenderer>(true)[1].widthMultiplier = lineSize;
        }
        else //planet
        {
            //Resize the orbit line
            body.GetComponentsInChildren<LineRenderer>(true)[1].widthMultiplier = lineSize;
        }
    }

    private float getDistanceY(GameObject body)
    /*****************************************************************
    * Calculates the y component of the distance between camera and body
    ******************************************************************/
    {
        float yDist;
        Vector3 bodyPos, cameraPos;

        bodyPos = body.transform.position; //Position of body
        cameraPos = Camera.main.gameObject.transform.position; //Position of viewer

        yDist = Mathf.Abs((bodyPos - cameraPos).y);
        return yDist;
    }

    public void skipToDay(float newJD)
    /**************************************************************************
    * Executed when the year slider is changed, but only on the pause screen
    * Moves all planets and meteoroids to their proper positions at the new JD
    ***************************************************************************/
    {
        PlanetInfo planetInfo;
        MeteoroidInfo meteorInfo;

        /*Set new daysElapsed value********************************************/
        daysElapsed = newJD - helper.JD2000;

        /*Loop through planets*************************************************/
        for (int ii = 0; ii < planets.Length; ii++)
        {
            planetInfo = planets[ii].GetComponent<PlanetInfo>();

            //Update position
            planets[ii].transform.position = planetInfo.getPositionAt(newJD);
            //Update trail
            updateTrail(planets[ii], "planet");
        }

        /*Loop through meteoroids***********************************************/
        for (int ii = 0; ii < meteoroids.Length; ii++)
        {
            meteorInfo = meteoroids[ii].GetComponent<MeteoroidInfo>();

            checkCollision(meteoroids[ii]); //Check if new JD is past the collision
            if (!meteorInfo.landed) //Only change position if meteoroid hasn't landed yet
            {
                //Update position
                meteoroids[ii].transform.position = meteorInfo.getPositionAt(newJD);
                //Update trail
                if (showMeteorLines)
                    updateTrail(meteoroids[ii], "meteoroid");
            }
        }
    }

    /*-----------------------GUI METHODS------------------------------------
    * The following methods are called when a GUI element is used
    ------------------------------------------------------------------------*/
    public void pauseGame()
    /************************************
    * Called when pause button is pressed
    * Saves the current speed and stops time
    *************************************/
    {
        paused = true;
    }
    public void resumeGame()
    /*************************************
    * Called when play button is pressed
    * Reassigns the saved speed value to daysPerFrame
    **************************************/
    {
        paused = false;
    }

    public void changeYear(float newJD)
    /***************************************
    * Called by year slider at top of screen whenever its value changes
    * Since its value constantly changes with time, we must only skip to
    *the newJD if we're on the pause screen.
    * This makes the game run faster
    ****************************************/
    {
        if (paused)
        {
            skipToDay(newJD);
        }
    }

    /****************************************
    * Called when a toggle changes value under 'planets' in settings panel
    * Deactivates/Reactivates all planets
    *****************************************/
    public void planetsON(bool isOn)
    {
        if (isOn) //Toggle is turned on
            for (int ii = 0; ii < planets.Length; ii++)
                planets[ii].SetActive(true);
    }
    public void planetsOFF(bool isOn)
    {
        if (isOn) //Toggle is turned on
            for (int ii = 0; ii < planets.Length; ii++)
                planets[ii].SetActive(false);
    }

    /****************************************
    * Called when a toggle changes value under 'Meteoroids' in settings panel
    * Deactivates/Reactivates all meteoroids
    *****************************************/
    public void meteoroidsON(bool isOn)
    {
        if (isOn) //Toggle is turned on
            for (int ii = 0; ii < meteoroids.Length; ii++)
                meteoroids[ii].SetActive(true);
    }
    public void meteoroidsOFF(bool isOn)
    {
        if (isOn) //Toggle is turned off
            for (int ii = 0; ii < meteoroids.Length; ii++)
                meteoroids[ii].SetActive(false);
    }

    /**************************************
    * Called when a button under 'Planet Lines' in settings panel is pressed
    * Deactivates/Reactivates all orbit lines of planets or replaces the full 
    *orbit line with one that fades off behind it.
    **************************************/
    public void planetLinesFADE(bool isOn)
    {
        if (isOn) //Toggle is turned on
        {
            for (int ii = 0; ii < planets.Length; ii++)
            {
                //Activate trail
                planets[ii].GetComponent<LineRenderer>().enabled = true;
            }
        }
        else //Toggle is turned off
        { 
            for (int ii = 0; ii < planets.Length; ii++)
            {
                //Deactivate trail
                planets[ii].GetComponent<LineRenderer>().enabled = false;
            }
        }
    }
    public void planetLinesSOLID(bool isOn)
    {
        if (isOn) //Toggle turned on
        { 
            for (int ii = 0; ii < planets.Length; ii++)
            {
                //Activate full orbit line
                planets[ii].GetComponentsInChildren<LineRenderer>(true)[1].enabled = true;
            }
        }
        else //Toggle turned off
        { 
            for (int ii = 0; ii < planets.Length; ii++)
            {
                //Deactivate full orbit line
                planets[ii].GetComponentsInChildren<LineRenderer>(true)[1].enabled = false;
            }
        }
    }
    public void planetLinesOFF(bool isOn)
    {
        if (isOn) //Toggle turned on
        { 
            for (int ii = 0; ii < planets.Length; ii++)
            {
                //Deactivate trail
                planets[ii].GetComponent<LineRenderer>().enabled = false;
                //Deactivate full orbit line 
                planets[ii].GetComponentsInChildren<LineRenderer>(true)[1].enabled = false;
            }
        }

        //NOTE: Do nothing if toggle is turned off. The toggle that was turned on will
        //     activate the appropriate LineRenderer component
    }

    /**************************************
    * Called when a toggle under 'Meteoroid Lines' in settings panel is pressed
    * Deactivates/Reactivates the trail of all meteoroids
    **************************************/
    public void meteoroidLinesFADE(bool isOn)
    {
        if (isOn) //Toggle is turned on
        {
            for (int ii = 0; ii < meteoroids.Length; ii++)
            {
                //Activate trail
                meteoroids[ii].GetComponent<LineRenderer>().enabled = true;
            }
        }
        else //Toggle is turned off
        { 
            for (int ii = 0; ii < meteoroids.Length; ii++)
            {
                //Deactivate trail
                meteoroids[ii].GetComponent<LineRenderer>().enabled = false;
            }
        }
    }
    public void meteoroidLinesSOLID(bool isOn)
    {
        if (isOn) //Toggle turned on
        { 
            for (int ii = 0; ii < meteoroids.Length; ii++)
            {
                //Activate full orbit line
                meteoroids[ii].GetComponentsInChildren<LineRenderer>(true)[1].enabled = true;
            }
        }
        else //Toggle turned off
        { 
            for (int ii = 0; ii < meteoroids.Length; ii++)
            {
                //Deactivate full orbit line
                meteoroids[ii].GetComponentsInChildren<LineRenderer>(true)[1].enabled = false;
            }
        }
    }
    public void meteoroidLinesOFF(bool isOn)
    {
        if (isOn) //Toggle turned on
        { 
            for (int ii = 0; ii < meteoroids.Length; ii++)
            {
                //Deactivate trail
                meteoroids[ii].GetComponent<LineRenderer>().enabled = false;
                //Deactivate full orbit line 
                meteoroids[ii].GetComponentsInChildren<LineRenderer>(true)[1].enabled = false;
            }
        }

        //NOTE: Do nothing if toggle is turned off. The toggle that was turned on will
        //     activate the appropriate LineRenderer component
    }

    /****************************************
    * Called when a button under 'Planet Scale' in settings panel is pressed
    * Changes the scale of all planets to their predestined auto/real sizes
    ****************************************/
    public void planetScaleAUTO(bool isOn)
    {
        PlanetInfo planetInfo;
        float scale; //new scale of specific planet

        if (isOn)
        { 
            for (int ii = 0; ii < planets.Length; ii++)
            {
                planetInfo = planets[ii].GetComponent<PlanetInfo>();
                scale = planetInfo.autoSize;
                planets[ii].transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }
    public void planetScaleREAL(bool isOn)
    {
        PlanetInfo planetInfo;
        float scale; //new scale of specific planet

        if (isOn)
        { 
            for (int ii = 0; ii < planets.Length; ii++)
            {
                planetInfo = planets[ii].GetComponent<PlanetInfo>();
                scale = planetInfo.realSize;
                planets[ii].transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }
    public void planetScaleCUSTOM(float inScale)
    {
        PlanetInfo planetInfo;
        float scale; //new scale of specific planet

        for (int ii = 0; ii < planets.Length; ii++)
        {
            planetInfo = planets[ii].GetComponent<PlanetInfo>();
            scale = inScale * planetInfo.autoSize; //Multiply imported scale by auto scale
            planets[ii].transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    /****************************************
    * Called when a button under 'Sun Scale' in settings panel is pressed
    * Changes the scale of the sun to predestined sizes
    ****************************************/
    public void sunScaleAUTO(bool isOn)
    {
        if (isOn)
            sun.transform.localScale = new Vector3(1, 1, 1);
    }
    public void sunScaleREAL(bool isOn)
    {
        if (isOn)
            sun.transform.localScale = new Vector3(0.0465f, 0.0465f, 0.0465f);
    }
    public void sunScaleCUSTOM(float inScale)
    {
        sun.transform.localScale = new Vector3(1*inScale, 1*inScale, 1*inScale);
    }

    /****************************************
    * Called when a button under 'Meteoroid Scale' in settings panel is pressed
    * Changes the scale of all meteoroids to predestined sizes
    ****************************************/
    public void meteorScaleAUTO(bool isOn)
    {
        if (isOn)
        { 
            for (int ii = 0; ii < meteoroids.Length; ii++)
            {
                meteoroids[ii].transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                meteoroids[ii].GetComponent<Light>().range = 0.3f;
            }
        }
    }
    public void meteorScaleCUSTOM(float inScale)
    {
        for (int ii = 0; ii < meteoroids.Length; ii++)
        {
            meteoroids[ii].transform.localScale = new Vector3(inScale, inScale, inScale);
            meteoroids[ii].GetComponent<Light>().range = inScale;
        }
    }
}
