using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class DistanceHandPhysicsInteractor : DistanceHandGrabInteractor
{
    // additional properties and methods for physics interaction

    [SerializeField] private GameObject _flickFinger;
    
    
    protected override void DoSelectUpdate()
    {
        if (SelectedInteractable == null)
        {
            return;
        }

        // Instead of updating grab movement, apply physics interaction
        ApplyPhysicsInteraction(SelectedInteractable);
    }

    private void ApplyPhysicsInteraction(DistanceHandGrabInteractable selectedInteractable)
    {
        // a collider will be attached to my middle finger with rigidbody
        // when it flicks the interactable Ant (enemy npc), the ant goes flying
        // Calculate force direction (towards the interactable)
        // Apply force
        Debug.Log("Ant goes flying!");
    }
    
    
    
}
