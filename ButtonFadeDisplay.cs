using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class ButtonFadeDisplay : MonoBehaviour
{
    public float displayTime = 1; //Time that message is displayed without fade
    public float fadeOutTime = 0.5f; //Time taken to fade out
    public GameObject messageButton; //This is a button

    public void Display(string message)
    {
        messageButton.SetActive(true);
        StartCoroutine(DisplayRoutine(message));
    }

    private IEnumerator DisplayRoutine(string message)
    {
        Image image;
        Color OGImageColor, OGTextColor;
        Text text;

        image = messageButton.GetComponent<Image>(); //Background of button
        OGImageColor = image.color; //Original image colour

        text = messageButton.GetComponentInChildren<Text>(); //Button text
        OGTextColor = text.color; //Save original text colour

        text.text = message; //Set button text to message

        //Display without fade
        for (float t = 0; t < displayTime; t += Time.deltaTime)
        {
            yield return new WaitForSeconds(Time.deltaTime);
        }

        //Fade out
        for (float t = 0; t < fadeOutTime; t += Time.deltaTime)
        {
            image.color = Color.Lerp(OGImageColor, Color.clear, Mathf.Min(1, t/fadeOutTime));
            text.color = Color.Lerp(OGTextColor, Color.clear, Mathf.Min(1, t/fadeOutTime));
            yield return new WaitForSeconds(Time.deltaTime);
        }

        //Destroy
        Destroy(messageButton);
    }
}
