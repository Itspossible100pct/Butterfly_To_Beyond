using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerFlicker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Collision just happened with: " + other.gameObject.name);
    }
}
