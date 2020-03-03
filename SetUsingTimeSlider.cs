using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SetUsingTimeSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
/**********************************************************************************
* AUTHOR: Mitchell O'Sullivan
* LAST UPDATED: 13/12/19
* PURPOSE: Calls the setUsingTimeSlider(bool) function in the ScreenMaster script, telling
*         it if the slider is being used or not.
***********************************************************************************/
{
    public ScreenMaster screenScript;

    public void OnPointerDown(PointerEventData pointerEventData)
    /***********************************************************
    * Called when user first clicks on slider
    ************************************************************/
    {
        screenScript.setUsingTimeSlider(true);
    }

    public void OnPointerUp(PointerEventData pointerEventData)
    /***********************************************************
    * Called when user releases hold on slider
    ************************************************************/
    {
        screenScript.setUsingTimeSlider(false);
    }

}
