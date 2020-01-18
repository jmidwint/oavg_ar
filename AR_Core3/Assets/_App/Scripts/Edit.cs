using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Edit : MonoBehaviour
{
    public Text label;
    private bool flip = false;

    // Update Label
    public void updateLabel()
    {
        if (flip)
        {
            label.text = "Edit";
            flip = false;
        }
        else
        {
            label.text = "Flip";
            flip = true;
        }
    }
}
