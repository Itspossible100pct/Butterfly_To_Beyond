using System.Collections;
using UnityEngine;

public class CaterpillarBehaviour : MonoBehaviour
{
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip hungrySound;
    public AudioClip eatSound;
    public AudioClip cuteSound;
    public AudioClip scaredSound;
    public AudioClip reliefSound;
    public AudioClip happySound;
    public Transform[] waypoints;
    private int currentWaypoint = 0;

    private enum State { Idle, Hungry, Eating, Afraid, Happy }
    private State currentState = State.Idle;

    void Start()
    {
        GoToNextState();
    }

    void GoToNextState()
    {
        switch (currentState)
        {
            case State.Idle:
                StartCoroutine(BehaveHungry());
                break;
            case State.Hungry:
                // Hungry state is triggered by player proximity or view
                break;
            case State.Eating:
                StartCoroutine(GrowAndMove());
                break;
            case State.Afraid:
                StartCoroutine(ShowFear());
                break;
            case State.Happy:
                StartCoroutine(WaitAndReset());
                break;
        }
    }

    IEnumerator BehaveHungry()
    {
        audioSource.PlayOneShot(hungrySound);
        animator.SetTrigger("isHungry");
        yield return new WaitForSeconds(2);
        currentState = State.Eating; // Change to eating once the player feeds
        GoToNextState();
    }

    public void Feed()
    {
        if (currentState == State.Hungry)
        {
            audioSource.PlayOneShot(eatSound);
            animator.SetTrigger("eat");
            transform.localScale *= 1.1f; // Grow slightly when fed
            currentState = State.Happy;
            GoToNextState();
        }
    }

    IEnumerator GrowAndMove()
    {
        audioSource.PlayOneShot(cuteSound);
        animator.SetTrigger("wiggle");
        yield return new WaitForSeconds(2);
        MoveToNextWaypoint();
        currentState = State.Afraid;
        GoToNextState();
    }

    void MoveToNextWaypoint()
    {
        transform.position = waypoints[currentWaypoint++ % waypoints.Length].position;
    }

    IEnumerator ShowFear()
    {
        audioSource.PlayOneShot(scaredSound);
        animator.SetTrigger("scared");
        yield return new WaitForSeconds(2);
        // Assume player action defeats ants
        currentState = State.Happy;
        GoToNextState();
    }

    IEnumerator WaitAndReset()
    {
        audioSource.PlayOneShot(reliefSound);
        yield return new WaitForSeconds(2);
        currentState = State.Idle;
        GoToNextState();
    }
}
