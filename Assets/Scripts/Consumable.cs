using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class Consumable : MonoBehaviour
{
    [SerializeField] private GameObject[] portions;
    [SerializeField] private int index = 0;

    public bool IsFinished => index == portions.Length; 

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _eatingSounds;
    
    
    // Start is called before the first frame update
    void Start()
    {
        _audioSource.playOnAwake = false;
        
        // Activate all portions at the start
        foreach (GameObject portion in portions)
        {
            portion.SetActive(true);
        }

        //SetVisuals();
    }

    private void OnValidate()
    {
        SetVisuals();
    }

    [ContextMenu("Consume")]

    public void Consume()
    {
        if (!IsFinished)
        {
            
            SetVisuals();
            index++;
            _audioSource.PlayOneShot(_eatingSounds[Random.Range(0, _eatingSounds.Length)], .3f);
        }
    }

    private void SetVisuals()
    {
        //for (int i = 0; i < portions.Length; i++)
        //{
        //    portions[i].SetActive(i == index);
        //}
        
        if (index < portions.Length)
        {
            portions[index].SetActive(false);
        }
        
    }
}
