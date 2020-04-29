using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeTextColor : MonoBehaviour
{
    public Text[] texts;
    public void ChangeColor(int position)
    {
        for (int i = 0; i < 3; i++)
            if (i == position)
                texts[i].color = Color.white;
            else
                texts[i].color = Color.black;
    }
}
