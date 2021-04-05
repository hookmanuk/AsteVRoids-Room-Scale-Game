using MText;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD : MonoBehaviour
{
    public GameObject[] Lives;
    public Modular3DText Score;
    
    public void ReduceLives()
    {
        foreach (var item in Lives)
        {
            if (item.activeSelf)
            {
                item.SetActive(false);
                break;
            }
        }
    }
}
