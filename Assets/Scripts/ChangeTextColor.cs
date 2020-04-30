using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeTextColor : MonoBehaviour
{
    public Button[] buttons;
    public Text[] texts;
    public void ChangeColor(int position)
    {
        var whiteColors = buttons[0].colors; whiteColors.normalColor = Color.white;
        var redColors = buttons[0].colors; redColors.normalColor = Color.red;

        for (int i = 0; i < 3; i++)
            if (i == position)
            {
                texts[i].color = Color.white;
                buttons[i].colors = redColors;
            }
            else
            {
                texts[i].color = Color.black;
                buttons[i].colors = whiteColors;
            }
    }
}
