using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InteractWithObject : MonoBehaviour
/***************************************************************
* If this script is added to an object, you can do three things:
* 1. Hover over object to see its name
* 2. Click on object to display a window with information about that object
* 3. Double-click on object to make it the center of the camera
****************************************************************/
{
    public GameObject nameDisplayPrefab;//A Canvas that will be made a child of this Object
    public float labelSize = 1; //The local labelSize of the canvas
    public ScreenMaster screenScript; //Reference to screen for displaying information and
                                      //changing pivot. (Assigned at runtime)

    private GameObject nameDisplay;

    /************************************************************
    * Hovering over object displays a label
    *************************************************************/
    void OnMouseEnter()
    {
        float scale;

        if (!EventSystem.current.IsPointerOverGameObject()) //Check we're not over GUI
        {
            nameDisplay = Instantiate(nameDisplayPrefab);
            nameDisplay.GetComponentInChildren<Text>().text = this.transform.name;
            nameDisplay.transform.SetParent(this.transform);
            nameDisplay.transform.localPosition = new Vector3(0, 1, 0);
            nameDisplay.transform.localRotation = Camera.main.gameObject.transform.rotation;
            scale = getDistance() / 700 * labelSize;
            scale /= this.transform.lossyScale.x;
            nameDisplay.transform.localScale = new Vector3(scale, scale, scale);
        }
    }
    void OnMouseExit()
    {
        Destroy(nameDisplay);
    }

    /***********************************************************
    * Single-click changes pivot
    ************************************************************/
    void OnMouseUpAsButton()
    /***********************************************************
    * Change the value of the dropdown menu directly so it displays 
    *the correct pivot
    ************************************************************/
    {
        bool clickingActive = screenScript.clickingActive;

        if (clickingActive) //Check we're not over GUI
        {
            Dropdown dropdown = screenScript.pivotMenu.GetComponent<Dropdown>();

            if (string.Compare(this.transform.name, "Sun") == 0)
            {
                dropdown.value = 0;
            }
            if (string.Compare(this.transform.name, "Mercury") == 0)
            {
                dropdown.value = 1;
            }
            if (string.Compare(this.transform.name, "Venus") == 0)
            {
                dropdown.value = 2;
            }
            if (string.Compare(this.transform.name, "Earth") == 0)
            {
                dropdown.value = 3;
            }
            if (string.Compare(this.transform.name, "Mars") == 0)
            {
                dropdown.value = 4;
            }
            if (string.Compare(this.transform.name, "Jupiter") == 0)
            {
                dropdown.value = 5;
            }
            if (string.Compare(this.transform.name, "Saturn") == 0)
            {
                dropdown.value = 6;
            }
            if (string.Compare(this.transform.name, "Uranus") == 0)
            {
                dropdown.value = 7;
            }
            if (string.Compare(this.transform.name, "Neptune") == 0)
            {
                dropdown.value = 8;
            }
        }
    }

    /*******************************************************************
     * Right-Click shows more information
     *******************************************************************/
    private void OnMouseOver()
    {
        PlanetInfo planetInfo;
        MeteoroidInfo meteorInfo;
        string name, infoText = "", citation = "";
        bool notTheSun = true; //Tells program if we should display info or not
                               //Will only be false for the sun
        bool clickingActive = screenScript.clickingActive;

        if (Input.GetMouseButton(1) && clickingActive)
        //If there's a right click and we're not over a GUI element
        {
        /*Deduce what type of body it is and retrieve appropriate info***/
            name = this.transform.name;
            if (name.StartsWith("DN"))      //All meteoroids start with DN
            {
                meteorInfo = this.GetComponent<MeteoroidInfo>();
                infoText = meteorInfo.infoText;
                citation = "Cite: DFN dataset";
            }
            else if (name.StartsWith("Su")) //Check if it's the sun
            {
                notTheSun = false;
            }
            else                            //Anything else is a planet
            {
                planetInfo = this.GetComponent<PlanetInfo>();
                infoText = planetInfo.infoText;
                citation  = "Cite: https://solarsystem.gov/planet-compare/\n";
                citation += "        https://ssd.jpl.nasa.gov/horizons.cgi";
            }

        /*Display the info***********************************************/
            if (notTheSun)
                screenScript.displayInfoPanel(name, infoText, citation);
        }
    }

    private float getDistance()
    /*******************************************************************
    * Calculates the distance between the camera and this Object
    ********************************************************************/
    {
        float distance;
        Vector3 bodyPos, cameraPos;

        bodyPos = this.transform.position; //Position of body
        cameraPos = Camera.main.gameObject.transform.position; //Position of viewer

        distance = Vector3.Distance(bodyPos, cameraPos); //Distance between them

        return distance;
    }
}
