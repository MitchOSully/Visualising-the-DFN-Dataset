using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
/*********************************************************
* AUTHOR: Mitchell O'Sullivan
* HELPER CODE: https://pastebin.com/2RX8fpJ3 by Blackzow
* TAKEN: 3/12/19
* LAST UPDATED: 17/01/2020
*********************************************************/
{
    /*Public global variables****************************/
    public BodyPlotter plotterScript; //Access to body plotter script
    public GameObject pivot; //An actual object in the scene to pivot around
    public float dragSensitivity = 4f; //How far we travel per 'drag'
                                       //Large number ==> Travel quicker
    public float dragSpeed = 10f; //How quick the drag animation is
                                  //Large number ==> More instant (less smooth)
    public float scrollSensitivity = 2f; //Scroll equivalent of dragSensitivity
    public float scrollSpeed = 6f; //Scroll equivalent of dragSpeed
    public float moveSpeed = 10f; //Speed at which we move to a new pivot
    public bool draggingActive = true; //False if user is dragging a GUI element
    public bool scrollingActive = true; //False if user is scrolling on a GUI element

    /*Private global variables***************************/
    private Vector3 localRotation = new Vector3(90, 0, 0);
                                      //Current rotation of camera (used for dragging)
    private float camDistance = 30f; //Distance from pivot (used to adjust scrolling)

    private Vector3 offset; //Vector between camera and pivot at the start
    private Transform cameraTransform;
    private Transform parentTransform; //Camera will follow its parent. Assigned from pivot.transform.

    public void start()
    /**************************************************************
     * Called by the Start() function in MasterScript.cs
     * ************************************************************/
    {
        cameraTransform = this.transform;
        parentTransform  = this.transform.parent;
	}

    public void update()
    /**************************************************************
    * Called by the Update() function in MasterScript.cs
    * Calls updateLocalRotation() and updateCamDistance() if the necessary user-commands are made
    ********************************************************/
    {
		/*Moving parent to new pivot************************/
        if (Vector3.Distance(parentTransform.position, pivot.transform.position) < 1)
        //If parent and pivot are very close
        {
            parentTransform.position = pivot.transform.position; //Update instantly
        }
        else //If parent and pivot are at separate locations
        {
            moveParent(); //Animate the movement
        }

        plotterScript.movingCamera = false; //Initially set to false. Set to true if we go
                                            //into one of the following if-statements

        /*Dragging******************************************/
        if (Input.GetMouseButton(0) && draggingActive)
        //When left mouse button is held down and not using GUI
        {
            plotterScript.movingCamera = true;

            //Only execute when mouse is not in centre (saves computation effort)
            if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
            {
                updateLocalRotation();
            }
        }
        animateDrag();
        
        /*Scrolling*****************************************/
        //Only execute when there's scrolling (saves computation effort)
        if (Input.GetAxis("Mouse ScrollWheel") != 0f && scrollingActive) 
        {
            plotterScript.movingCamera = true;
            
            updateCamDistance();
        }
        //Only execute when camera's potition is not where it should be (saves comp effort)
        if (cameraTransform.localPosition.y != camDistance)
        {
            animateZoom();
        }
	}

    private void moveParent()
    /******************************************************
    * Repositions the parent of camera to the object we're pivoting around.
    * Uses Vector3.Lerp to animate it
    *******************************************************/
    {
        Vector3 curPos, targetPos;
        float distance;

        curPos = parentTransform.position;
        targetPos = pivot.transform.position;
        distance = Time.deltaTime * moveSpeed;

        parentTransform.position = Vector3.Lerp(curPos, targetPos, distance);
    }

    private void updateLocalRotation()
    /*******************************************************
    * Updates localRotation according to how much the mouse moves
    *******************************************************/
    {
        //Update localRotation
        localRotation.x += Input.GetAxis("Mouse X") * dragSensitivity;
        localRotation.y -= Input.GetAxis("Mouse Y") * dragSensitivity;

        //Put limits on y rotation
        localRotation.y = Mathf.Clamp(localRotation.y, -180f, 0f);
    }

    private void animateDrag()
    /********************************************************
    * Rotates the camera around the x-axis and y-axis smoothly
    ********************************************************/
    {
        Quaternion rotation;
        float xRot, yRot, zRot; //Imports for Quaternion creation
        Quaternion startOrientation, endOrientation; 
        float time;                                  //Imports for Quaternion.Lerp

        //Get how much we've rotated by
        xRot = localRotation.x;
        yRot = localRotation.y;
        zRot = 0f; 
        rotation = Quaternion.Euler(yRot, xRot, zRot);

        //Do the animating
        startOrientation = parentTransform.rotation;
        endOrientation   = rotation;
        time             = Time.deltaTime * dragSpeed;
        parentTransform.rotation = Quaternion.Lerp(startOrientation, endOrientation, time);
    }

    private void updateCamDistance()
    /*******************************************************
    * Updates camDistance according to how much we're scrolling
    ********************************************************/
    {
        float scrollAmount;

        scrollAmount = Input.GetAxis("Mouse ScrollWheel")*scrollSensitivity;

        //Scroll slowly when we're close, but fast when we're far away
        scrollAmount *= camDistance * 0.3f;

        //Do the zooming: Update distance from pivot
        camDistance -= scrollAmount;

        //Set limits on zooming
        camDistance = Mathf.Clamp(camDistance, 1.5f, 1500f);
    }

    private void animateZoom()
    /**********************************************************
    * Moves the camera backwards and forwards along y-axis
    ***********************************************************/
    {
        float xPos, yPos, zPos; //Imports for new Vector3
        float start, end, time; //Imports for Math.Lerp (animation)

        start = cameraTransform.localPosition.y;
        end   = camDistance;
        time  = Time.deltaTime * scrollSpeed;

        xPos = 0f;
        yPos = Mathf.Lerp(start, end, time); 
        zPos = 0f;


        //Update camera position
        cameraTransform.localPosition = new Vector3(xPos, yPos, zPos);
    }

    public void changePivot(int selection)
    /*****************************************************************
    * This is called when a selection from the dropdown menu is made.
    * It changes the camera pivot according to the selection.
    *****************************************************************/
    {
        switch(selection)
        {
            case 0:
                pivot = GameObject.Find("Sun");
                break;
            case 1:
                pivot = GameObject.Find("Mercury");
                break;
            case 2:
                pivot = GameObject.Find("Venus");
                break;
            case 3: 
                pivot = GameObject.Find("Earth");
                break;
            case 4:
                pivot = GameObject.Find("Mars");
                break;
            case 5:
                pivot = GameObject.Find("Jupiter");
                break;
            case 6:
                pivot = GameObject.Find("Saturn");
                break;
            case 7:
                pivot = GameObject.Find("Uranus");
                break;
            case 8:
                pivot = GameObject.Find("Neptune");
                break;
        }
    }
}
