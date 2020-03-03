using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeactivateZooming : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
/**********************************************************************************
* AUTHOR: Mitchell O'Sullivan
* LAST UPDATED: 5/2/2020
* PURPOSE: Sets the value of scrollingActive in CameraController.cs when the mouse first
*         enters or exits the GUI element this script is attached to.
***********************************************************************************/
{
    public CameraController cameraScript; //Allow access to variables in camera script

    public void OnPointerEnter(PointerEventData eventData)
    /************************************************************
    * Called when user's mouse first hovers over GUI element
    * For scrollable texts
    *************************************************************/
    {
        cameraScript.scrollingActive = false;
    }
    public void OnPointerExit(PointerEventData eventData)
    /***********************************************************
    * Called when user's mouse stops hovering over GUI element
    * For scrollable texts
    ************************************************************/
    {
        cameraScript.scrollingActive = true;
    }
}
