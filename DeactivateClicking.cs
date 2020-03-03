using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeactivateClicking : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
/**********************************************************************************
* AUTHOR: Mitchell O'Sullivan
* LAST UPDATED: 24/01/2020
* HOW IT WORKS: Sets the value of the clickingActive variable in the screen script.
*               This value is, in turn, accessed by all clickable objects in the scene,
*              which use it to decide whether or not to execute the OnClick command.
* PURPOSE: This script will cause its owner to disable clicks of any object behind it.
***********************************************************************************/
{
    public ScreenMaster screenScript; //Allow access to the clickingActive variable

    public void OnPointerEnter(PointerEventData pointerEventData)
    /************************************************************
    * Called when user's mouse first hovers over GUI element
    *************************************************************/
    {
        screenScript.clickingActive = false;
    }
    public void OnPointerExit(PointerEventData pointerEventData)
    /***********************************************************
    * Called when user's mouse stops hovering over GUI element
    ************************************************************/
    {
        screenScript.clickingActive = true;
    }
}
