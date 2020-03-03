using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/*****************************************
 * AUTHOR: Mitchell O'Sullivan
 * PURPOSE: Holds references to the 4 main scripts of the project
 *         and calls their start() and update() functions in the 
 *         desired order.
 * LAST UPDATED: 30/01/2020
 * **************************************/
public class MasterScript : MonoBehaviour
{
    /************************************
     * PUBLIC GLOBAL VARIABLES
     * **********************************/
    public DateHelper helperScript;
    public BodyPlotter plotterScript;
    public ScreenMaster screenScript;
    public CameraController cameraScript;

    void Start()
    /******************************************************
     * Executed before frame 1
     * Calls the public start() functions of other scripts
     *in the desired order.
     * ****************************************************/
    {
        helperScript.start();
        plotterScript.start();
        screenScript.start();
        cameraScript.start();
    }

    void Update()
    /******************************************************
     * Executed every frame
     * Calls the public update() functions of other scripts
     *in the desired order.
     * ****************************************************/
    {
        plotterScript.update();
        screenScript.update();
        cameraScript.update();
    }

    public void quitGame()
    /******************************************************
     * Called when the quit button (bottom-right) is pressed.
     * Quits the game.
     * ****************************************************/
    { 
        Application.Quit();
    }
}
