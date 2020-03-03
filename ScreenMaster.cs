using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenMaster : MonoBehaviour
/******************************************
 * AUTHOR: Mitchell O'Sullivan
 * LAST UPDATED: 10/2/2020
 ******************************************/
{
    /**************************************
    * PUBLIC GLOBAL VARIABLES
    ***************************************/
    //Access to other scripts
    public BodyPlotter plotterScript; //For checking daysElapsed and other things
    public DateHelper helper;         //For displaying dates
    public CameraController cameraScript; //For changing pivots
    //Boolean used by InteractWithObject script
    public bool clickingActive; //We deactivate clicking if mouse is over GUI element
    //References to all GUI elements:
    public GameObject topLeftPanel;
      public GameObject pivotMenu;
    public GameObject topRightPanel;
      public GameObject dateInput;
      public GameObject speedSlider;
      public GameObject errorMessagePrefab;
    public GameObject topCentrePanel;
      public GameObject pauseButton;
      public GameObject playButton;
      public GameObject timeSlider;
    public GameObject BottomRightPanel;
      public GameObject hideButton;
      public GameObject showButton;
      public GameObject tutorialButton;
      public GameObject tutorialPanel;
      public GameObject settingsButton;
      public GameObject settingsPanel;
        public GameObject meteorLinesPanel;
        public GameObject meteorLinesGonePanel;
        public GameObject planetScaleAutoToggle; //Planet scale options
        public GameObject planetScaleRealToggle;
        public GameObject planetScaleSlider;
        public GameObject sunScaleAutoToggle; //Sun scale options
        public GameObject sunScaleRealToggle;
        public GameObject sunScaleSlider;
        public GameObject meteorScaleAutoToggle; //Meteoroid scale options
        public GameObject meteorScaleSlider; 
    public GameObject infoPanel;
      public GameObject infoPanelTitle;
      public GameObject infoPanelText;
      public GameObject infoPanelCitation;
      public GameObject infoPanelScrollbar;

    /**************************************
    * PRIVATE GLOBAL VARIABLES
    ***************************************/
    private bool usingTimeSlider = false; //Tells us if timeSlider is being used

    public void start()
    /**************************************************************
     * Called by the Start() function in MasterScript.cs
     * ************************************************************/
    {
        //Deactivate necessary GUI elements
        playButton.SetActive(false);
        showButton.SetActive(false);
        tutorialPanel.SetActive(false);
        if (!plotterScript.showMeteorLines) //If meteor lines deactivated
        {
            meteorLinesPanel.SetActive(false);
            meteorLinesGonePanel.SetActive(true);
        }

        //Make date input field and year slider non-interractable
        dateInput.GetComponent<InputField>().interactable = false;
        timeSlider.GetComponent<Slider>().interactable = false;
    }

    public void update()
    /**************************************************************
    * Called by the Update() function in MasterScript.cs
    **************************************************************/
    {
        double daysElapsed = plotterScript.daysElapsed;
        bool paused = plotterScript.paused;
        InputField date;

        if (!paused) //If game is running, update time every frame
        {
            date = dateInput.GetComponent<InputField>();
            date.text = helper.daysToDate(daysElapsed); //Update date text

            timeSlider.GetComponent<Slider>().value = (float)(helper.JD2000 + daysElapsed);
                                           //Update time slider on pause screen
        }
        else if (usingTimeSlider) //If game is paused but we're changing timeSlider value, 
                                  //update time every frame
        {
            date = dateInput.GetComponent<InputField>();
            date.text = helper.daysToDate(daysElapsed); //Update date text
        }
        //else if game is paused and we're not using slider, do nothing 
    }

    /*******************************
    * Pause / Play
    ********************************/
    public void pauseGame()
    {
        pauseButton.SetActive(false);
        playButton.SetActive(true);

        dateInput.GetComponent<InputField>().interactable = true;
        timeSlider.GetComponent<Slider>().interactable = true;
    }
    public void resumeGame()
    {
        pauseButton.SetActive(true);
        playButton.SetActive(false);

        dateInput.GetComponent<InputField>().interactable = false;
        timeSlider.GetComponent<Slider>().interactable = false;
    }

    /**********************************
    * Hide / Show controls
    ***********************************/
    public void hideControls()
    {
        showButton.SetActive(true);
        hideButton.SetActive(false);

        topLeftPanel.SetActive(false);
        topRightPanel.SetActive(false);
        topCentrePanel.SetActive(false);
        tutorialPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }
    public void showControls()
    {
        showButton.SetActive(false);
        hideButton.SetActive(true);

        topLeftPanel.SetActive(true);
        topRightPanel.SetActive(true);
        topCentrePanel.SetActive(true);
    }

    /***********************************
    * Display/Close the following panels
    * 1. Tutorial panel
    * 2. Settings panel
    ************************************/
    public void controlTutorialPanel()
    {
        bool active = tutorialPanel.activeSelf;
        tutorialPanel.SetActive(!active); //Whatever it was before, make it the opposite
        settingsPanel.SetActive(false); //Make sure settings panel is deactivated
    }
    public void controlSettingsPanel()
    {
        bool active = settingsPanel.activeSelf;
        settingsPanel.SetActive(!active); //Whatever it was before, make it the opposite
        tutorialPanel.SetActive(false); //Make sure tutorial penel is deactivated
    }
    
    /***********************************
    * Display/Close the information panel
    ************************************/
    public void displayInfoPanel(string title, string info, string citation)
    //Called by clicking a body in the scene
    {
        infoPanelTitle.GetComponent<Text>().text = title;
        infoPanelText.GetComponent<Text>().text = info;
        infoPanelCitation.GetComponent<Text>().text = citation;
        infoPanel.SetActive(true);
    }
    public void closeInfoPanel() 
    //Called by the close button on the info panel
    {
        infoPanelScrollbar.GetComponent<Scrollbar>().value = 1;
        infoPanel.SetActive(false);
        cameraScript.scrollingActive = true; //Reactivate scrolling
        clickingActive = true; //Reactivate worldspace clicking
    }

    public void changeSpeed(float inSliderValue)
    /******************************************************************
    * Called when the value of the speed slider is changed.
    * Updates the daysPerFrame value and the value displayed on the slider
    *******************************************************************/
    {
        Text speedText;
		double newDaysPerFrame;

		//Scale according to equation (x/5)^5
		//newDaysPerFrame = Math.Pow((double)inSliderValue, 5) / 5;
		newDaysPerFrame = inSliderValue;

        //Update speed 
        plotterScript.daysPerFrame = newDaysPerFrame;

        //Update display text on slider
        speedText = speedSlider.GetComponentInChildren<Text>();
        speedText.text = "SPEED: " + newDaysPerFrame.ToString("0.00");
    }

    public void setUsingTimeSlider(bool inUsingTimeSlider)
    /*********************************************
    * Called by a script in timeSlider. Tells us if slider is being used. 
    **********************************************/
    {
        usingTimeSlider = inUsingTimeSlider;
    }

    public void changeDateLive(string newDate)
    /*********************************************
    * Called as soon as the user makes a change to the date input box,
    *even if they haven't pressed 'enter'. 
    * If input is valid, it changes the value of the year slider.
    **********************************************/
    {
        string[] dateElements;
        int day, month, year;
        double newJD;
        
        if (plotterScript.paused && !usingTimeSlider)
        //Only do this on pause screen and when not using year slider
        {
            dateElements = newDate.Split('/');
            if (helper.getDateErrorMessage(dateElements) == null) //No error message means it's valid
            {
                //Parse string elements to ints
                day   = Int32.Parse(dateElements[0]);
                month = Int32.Parse(dateElements[1]);
                year  = Int32.Parse(dateElements[2]);

                //Change text colour to green - valid input
                dateInput.GetComponentInChildren<Text>().color = new Color(0.5f,1f,0.5f);

                //Use helper to calculate new JD
                newJD = helper.dateToJD(new int[] {day, month, year});

                //Update year slider to make the change in JD
                timeSlider.GetComponent<Slider>().value = (float)newJD;
            }
            else //We got an error message, therefore it's invalid
            {
                //Change text colour to red - invalid input
                dateInput.GetComponentInChildren<Text>().color = new Color(1f,0.5f,0.5f);
            }
        }
        //Else if game is playing, date is updating every frame automatically, so leave it be
    }

    public void changeDateEnd(string newDate)
    /*********************************************
    * Called when the user presses 'enter' in the date input box.
    * Doesn't change daysElapsed because that's done while user is typing
    * Displays an error message if input is invalid
    **********************************************/
    {
        string[] dateElements;
        string errorMessage;

        dateInput.GetComponentInChildren<Text>().color = Color.white;
                                          //Set colour back to white

        dateElements = newDate.Split('/');
        errorMessage = helper.getDateErrorMessage(dateElements);

        if (errorMessage != null) //If we got an error message, then input is invalid
            displayErrorMessage(errorMessage); 
    }

    public void displayErrorMessage(string message)
    /*********************************************
    * Displays "Incorrect formatting" message for date/time input
    **********************************************/
    {
        GameObject messageButton;

        messageButton = Instantiate(errorMessagePrefab);
        messageButton.transform.SetParent(topRightPanel.transform); //Set parent
        messageButton.transform.localPosition = new Vector3(0,-62,0);
                                   //Must reapply local position after parent change
        messageButton.transform.localScale = new Vector3(1,1,1);
                                   //Also reapply scale because that stuffs up too

        messageButton.GetComponent<ButtonFadeDisplay>().Display(message);
    }

    /*--------------SETTINGS PANEL-------------------------------------------*/
    public void planetScaleAUTO(bool isOn)
    {
        if (isOn)
        { 
            //Use the slider to change the scale
            planetScaleSlider.GetComponent<Slider>().value = 1; 
            //Turn AUTO toggle back on because slider would've turned it off
            planetScaleAutoToggle.GetComponent<Toggle>().isOn = true;
        }
    }
    public void planetScaleREAL(bool isOn)
    {
        if (isOn)
        { 
            //Change slider to 0 because the actual value is pretty close to 0
            //The actual scales are given in BodyPlotter.cs
            planetScaleSlider.GetComponent<Slider>().value = 0;
            planetScaleSlider.GetComponentInChildren<Text>().text = "Real Scale"; 
            //Turn REAL toggle back on because slider would've turned it off
            planetScaleRealToggle.GetComponent<Toggle>().isOn = true;
        }
    }
    public void planetScaleCUSTOM(float inScale) //Called on value change of slider
    {
        //Turn off two toggles
        planetScaleAutoToggle.GetComponent<Toggle>().isOn = false;
        planetScaleRealToggle.GetComponent<Toggle>().isOn = false;
        //Update text on slider
        planetScaleSlider.GetComponentInChildren<Text>().text = inScale.ToString("0.0");
    }

    /**********************************************
    * Called by buttons under 'Sun Scale'
    * The job of altering the sun's size is left to BodyPlotter.cs
    ***********************************************/
    public void sunScaleAUTO(bool isOn)
    {
        if (isOn)
        { 
            //Use the slider to change the scale
            sunScaleSlider.GetComponent<Slider>().value = 1; 
            //Turn AUTO toggle back on because slider would've turned it off
            sunScaleAutoToggle.GetComponent<Toggle>().isOn = true;
        }
    }
    public void sunScaleREAL(bool isOn)
    {
        if (isOn)
        { 
            //Change slider to 0 because the actual value is pretty close to 0
            //The actual scales are given in BodyPlotter.cs
            sunScaleSlider.GetComponent<Slider>().value = 0;
            sunScaleSlider.GetComponentInChildren<Text>().text = "Real Scale"; 
            //Turn REAL toggle back on because slider would've turned it off
            sunScaleRealToggle.GetComponent<Toggle>().isOn = true;
        }

    }
    public void sunScaleCUSTOM(float inScale) //Called on value change
    {
        //Turn off two toggles
        sunScaleAutoToggle.GetComponent<Toggle>().isOn = false;
        sunScaleRealToggle.GetComponent<Toggle>().isOn = false;
        //Update text of slider
        sunScaleSlider.GetComponentInChildren<Text>().text = inScale.ToString("0.0");
    }

    /**********************************************
    * Called by buttons under 'Meteoroid Scale'
    * The job of altering the sun's size is left to BodyPlotter.cs
    ***********************************************/
    public void meteorScaleAUTO(bool isOn)
    {
        if (isOn)
        { 
            //Use the slider to change the scale
            meteorScaleSlider.GetComponent<Slider>().value = 0.3f; 
            //Turn AUTO toggle back on because slider would've turned it off
            meteorScaleAutoToggle.GetComponent<Toggle>().isOn = true;
        }
        else if (meteorScaleSlider.GetComponent<Slider>().value == 0.3f)
        {
            //If they try to turn it off, turn it back on. That cheeky bugger.
            meteorScaleAutoToggle.GetComponent<Toggle>().isOn = true;
        }
    }
    public void meteorScaleCUSTOM(float inScale) //Called on value change
    {
        //Turn off two toggles
        meteorScaleAutoToggle.GetComponent<Toggle>().isOn = false;
        //Update text of slider
        meteorScaleSlider.GetComponentInChildren<Text>().text = inScale.ToString("0.00");
    }
}
