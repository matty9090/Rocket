using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerBar : MonoBehaviour
{
    [SerializeField]
    private Gradient Colour = null;

    void Update()
    {
        GetComponent<Image>().color = Colour.Evaluate(transform.localScale.y);
    }
}
