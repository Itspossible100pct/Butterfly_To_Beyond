using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consumer : MonoBehaviour
{
    private Collider _collider;
    
    void Start()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {

        Consumable consumable = other.GetComponent<Consumable>();
        if (consumable != null && !consumable.IsFinished)
        {
            consumable.Consume();
        }
    }
}
