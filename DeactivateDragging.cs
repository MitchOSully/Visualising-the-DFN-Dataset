using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeactivateDragging : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
/**********************************************************************************
* AUTHOR: Mitchell O'Sullivan
* LAST UPDATED: 5/2/2020
* PURPOSE: Sets the value of draggingActive in CameraController.cs when we click or 
*         release the GUI element this script is attached to.
***********************************************************************************/
{
    public CameraController cameraScript; //Allow access to variables in camera script

    public void OnPointerDown(PointerEventData eventData)
    /***********************************************************
    * Called when user first clicks on GUI element
    * Mainly for sliders
    ************************************************************/
    {
        cameraScript.draggingActive = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    /***********************************************************
    * Called when user releases hold on GUI element
    * Mainly for sliders
    ************************************************************/
    {
        cameraScript.draggingActive = true;
    }
}
