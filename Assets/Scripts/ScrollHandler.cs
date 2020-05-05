using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollHandler : MonoBehaviour
{
    public void MoreInfo(GameObject panel)
    {
        bool isActive = panel.activeSelf;
        panel.SetActive(!isActive);
    }
}
