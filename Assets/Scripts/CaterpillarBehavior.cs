using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Oculus.Interaction.Samples;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class CaterpillarBehaviour : MonoBehaviour
{
    public static CaterpillarBehaviour Instance;

    public Transform headTransform;
    
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip hungrySound;
    public AudioClip eatSound;
    public AudioClip cuteSound;
    public AudioClip scaredSound;
    public AudioClip reliefSound;
    public AudioClip happySound;
    public Transform[] waypoints;
    
    public float moveSpeed = 1.0f; // Speed of movement, adjustable in the inspector
    public ParticleSystem celebrationParticles; // Particle system for the celebration moment

    public GameObject antPrefab; // Reference to the Ant prefab
    public int totalAnts = 3; // Total number of ants to spawn
    private List<GameObject> spawnedAnts = new List<GameObject>();  // List to keep track of spawned ants
    private float antSpawnDelay = 3.0f;  // Delay between spawning each ant
    public bool isFed = false;
    
    private NavMeshAgent agent;
    private int currentWaypoint = 0;
    private bool enemiesDefeated = false;
    
    // Serialize fields for the specific leaf fragments to check
    [SerializeField] private GameObject criticalFragmentLeaf1;
    [SerializeField] private GameObject criticalFragmentLeaf2;

    public Transform foodItem; // Assign in the inspector
    public Transform player; // Assign in the inspector
    
    // Define an enum to hold the state identifiers
    public enum State
    {
        Idle,
        Walking,
        Hungry,
        Eating,
        Afraid,
        Happy,
        Celebrating
    }
    // Current state of the Caterpillar
    private State currentState;
    private bool isIdleCoroutineStarted = false;
    private Coroutine cuteSoundCoroutine = null;
    
    // Initialization
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = 0.1f;

        // Set a reasonable speed for the caterpillar
        agent.speed = moveSpeed;  // Ensure this is set to a value that makes sense for your game

        TransitionToState(State.Idle);
        
    }

    
    // IEnumerator PlayInitialHungrySound()
    // {
    //     yield return new WaitForSeconds(1);
    //     if (!audioSource.isPlaying) {
    //         audioSource.PlayOneShot(hungrySound);
    //         Debug.Log("Start: Played initial hungry sound after 1 second.");
    //     }
    // }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case State.Idle:
                IdleBehavior();
                break;
            case State.Walking:
                WalkingBehavior();
                break;
            case State.Hungry:
                HungryBehavior();
                break;
            case State.Eating:
                EatingBehavior();
                break;
            case State.Afraid:
                AfraidBehavior();
                break;
            case State.Happy:
                HappyBehavior();
                break;
            case State.Celebrating:
                CelebratingBehavior();
                break;
        }
    }

    void TransitionToState(State newState)
    {
        
        // Stop the current playing coroutine if we are leaving the Idle state
        if (currentState == State.Idle && newState != State.Idle)
        {
            if (cuteSoundCoroutine != null)
            {
                StopCoroutine(cuteSoundCoroutine);
                cuteSoundCoroutine = null;
            }
        }
        Debug.Log($"Transitioning from {currentState} to {newState}");
        currentState = newState;
        Debug.Log("Transitioning to: " + newState);
    }


    IEnumerator PlayCuteSoundRandomly()
    {
        while (currentState == State.Idle)
        {
            yield return new WaitForSeconds(Random.Range(5f, 10f));
            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(cuteSound);
                Debug.Log("IdleBehavior: Played a cute sound after a random interval.");
            }
        }
    }


    void LookAround()
    {
        // Ensure the headTransform is assigned
        if (headTransform != null)
        {
            // Calculate the rotation angle using a sine wave for smooth oscillation
            float rotationAngle = Mathf.Sin(Time.time * 0.5f) * 30f; // 0.5f to control the speed of rotation
            headTransform.localRotation = Quaternion.Euler(0, rotationAngle, 0);
            Debug.Log("IdleBehavior: Head looking around with limited Y-axis rotation.");
        }
        else
        {
            Debug.LogError("Head Transform is not assigned in the Inspector.");
        }
    }


    void WalkingBehavior()
    {
        if (currentWaypoint >= waypoints.Length)
        {
            Debug.LogError("Walking: Current waypoint index is out of range.");
            return;
        }
        Debug.Log($"Remaining distance to waypoint {currentWaypoint}: {agent.remainingDistance}");
        if (!agent.pathPending)
        {
            
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                Debug.Log($"Walking: Reached waypoint {currentWaypoint}.");

                switch (currentWaypoint)
                {
                    case 1:
                        StartCoroutine(HandleWaypoint1Actions());
                        break;
                    case 2:
                        StartCoroutine(HandleWaypoint2Actions());  // Ensure this is triggered
                        break;
                    default:
                        MoveToNextWaypoint();
                        break;
                }
            }
        }
    }


    
    IEnumerator HandleWaypoint1Actions()
    {
        // Start Idle before eating
        TransitionToState(State.Idle);
        yield return new WaitForSeconds(2); // Idle for 2 seconds

        // Transition to Hungry to simulate hunger behavior
        TransitionToState(State.Hungry);
        yield return new WaitUntil(() => isFed); // Wait until the caterpillar is fed

        // Ensure Catp eats and completes its eating behavior
        TransitionToState(State.Eating);
        yield return new WaitWhile(() => currentState == State.Eating); // Wait until eating is done

        // Idle for a bit after eating
        TransitionToState(State.Idle);
        yield return new WaitForSeconds(2); // Idle for 2 seconds

        // Now move to the next waypoint
        MoveToNextWaypoint();
    }


    void MoveToNextWaypoint()
    {
        currentWaypoint++;
        if (currentWaypoint < waypoints.Length)
        {
            Debug.Log($"Walking: Moving to waypoint {currentWaypoint}.");
            StartCoroutine(LookAtDirection(waypoints[currentWaypoint].position));
            agent.SetDestination(waypoints[currentWaypoint].position);
            TransitionToState(State.Walking);
        }
        else
        {
            Debug.Log("Walking: All waypoints visited, transitioning to Idle.");
            TransitionToState(State.Idle);
        }
    }

    
    
    private float nextCuteSoundTime = 0f;
    private float soundCooldown = 10f;  // Cooldown in seconds before playing the next cute sound

    void IdleBehavior()
    {
        if (!isIdleCoroutineStarted)
        {
            // Play the cute sound just once when first entering Idle state, with cooldown
            if (Time.time >= nextCuteSoundTime)
            {
                audioSource.PlayOneShot(cuteSound);
                Debug.Log("IdleBehavior: Played a cute sound once entering Idle state.");
                nextCuteSoundTime = Time.time + soundCooldown;
            }
            isIdleCoroutineStarted = true;
        }

        LookAround();

        if (currentWaypoint < waypoints.Length)
        {
            isIdleCoroutineStarted = false;  // Reset for the next time we enter Idle state
            TransitionToState(State.Walking);
        }
    }





    void HungryBehavior()
    {
        Debug.Log("Hungry: Starting Hungry Behavior.");
        StartCoroutine(HungrySequence());
    }

    IEnumerator HungrySequence()
    {
        Debug.Log("HungrySequence: Started");

        yield return LookAtTarget(foodItem.position);
        yield return new WaitForSeconds(2);

        if (!audioSource.isPlaying && Time.time >= nextCuteSoundTime)
        {
            audioSource.PlayOneShot(hungrySound);
            Debug.Log("HungrySequence: Played hunger sound.");
            nextCuteSoundTime = Time.time + soundCooldown;
        }

        yield return new WaitForSeconds(1);
        yield return LookAtTarget(player.position);

        if (!audioSource.isPlaying && Time.time >= nextCuteSoundTime)
        {
            audioSource.PlayOneShot(cuteSound);
            Debug.Log("HungrySequence: Played cute sound.");
            nextCuteSoundTime = Time.time + soundCooldown;
        }

        yield return new WaitForSeconds(2);

        if (!audioSource.isPlaying && Time.time >= nextCuteSoundTime)
        {
            audioSource.PlayOneShot(cuteSound);
            Debug.Log("HungrySequence: Played another cute sound.");
            nextCuteSoundTime = Time.time + soundCooldown;
        }

        while (!isFed)
        {
            yield return LookAtTarget(player.position);
            yield return new WaitForSeconds(0.1f);
        }

        TransitionToState(State.Eating);
    }


    
    IEnumerator LookAtTarget(Vector3 target)
    {
        Vector3 direction = (target - headTransform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        float time = 0;
        while (time < 1)
        {
            headTransform.rotation = Quaternion.Slerp(headTransform.rotation, lookRotation, time);
            time += Time.deltaTime * 2; // Adjust this value to change the speed of the rotation
            yield return null;
        }
    }

    // Define how many times the caterpillar should scale up in total
private const int totalBites = 4;
private int currentBiteCount = 0;
private const float scalePerBite = 1.03f; // This represents a 1% scale increase per bite

public void EatingBehavior()
{
    if (currentBiteCount >= totalBites)
    {
        // If all bites are done, finalize eating
        Debug.Log("Eating: All bites are done. Finalizing eating.");
        FinalizeEating();
        return;
    }

    // Play eating sound and animation only if not already playing
    if (!audioSource.isPlaying)
    {
        audioSource.PlayOneShot(eatSound);
        Debug.Log("Eating: Played eating sound for bite " + (currentBiteCount + 1));
    }

    animator.SetTrigger("Eating");  // Play eating animation
    transform.localScale *= scalePerBite;  // Scale up the caterpillar
    
    Debug.Log("Eating: Caterpillar scaled for bite " + (currentBiteCount + 1));

    // Increment the bite count
    currentBiteCount++;

    // Check if it's time to transition away from eating
    if (currentBiteCount >= totalBites)
    {
        // If this was the last bite, proceed with finalizing the eating process
        FinalizeEating();
    }
}

void FinalizeEating()
{
    // Reset bite count for the next time
    currentBiteCount = 0;

    // Check critical conditions to decide next state
    if ((currentWaypoint == 1 && !criticalFragmentLeaf1.activeSelf) ||
        (currentWaypoint == 3 && !criticalFragmentLeaf2.activeSelf))
    {
        // Transition to Idle and then move to next waypoint after a delay
        Debug.Log("Eating: Finished eating, transitioning to Idle.");
        TransitionToState(State.Idle);
        StartCoroutine(DelayedMoveToNextWaypoint(2));  // Wait 2 seconds in Idle before moving
    }
    else
    {
        // If not all conditions for waypoint logic are met, go back to Hungry or Idle
        Debug.Log("Eating: Not all conditions met, transitioning to Hungry.");
        TransitionToState(State.Hungry);
    }
}

IEnumerator DelayedMoveToNextWaypoint(float delay)
{
    yield return new WaitForSeconds(delay);
    MoveToNextWaypoint();
}


IEnumerator HandleWaypoint2Actions()
{
    Debug.Log("HandleWaypoint2Actions: Entered handling for Waypoint 2");

    TransitionToState(State.Idle);
    yield return new WaitForSeconds(2);  // Idle for 2 seconds

    Debug.Log("HandleWaypoint2Actions: Starting to spawn ants...");
    StartCoroutine(SpawnAnts());  // This line should trigger the spawning

    TransitionToState(State.Afraid);  // Catp should be afraid when ants appear

    // Ensure we wait until all ants are defeated
    yield return new WaitUntil(() => spawnedAnts.TrueForAll(ant => ant == null));

    TransitionToState(State.Happy);  // Transition to happy after defeating all ants
    yield return new WaitForSeconds(2);

    TransitionToState(State.Idle);  // Be idle for 2 seconds before moving on
    yield return new WaitForSeconds(2);

    MoveToNextWaypoint();  // Move to the next waypoint
}





void AfraidBehavior()
{
    Debug.Log("Afraid: Playing scared sounds and animations.");
    // Implement Afraid behavior
    audioSource.PlayOneShot(scaredSound, 1);
    //transform.DOShakePosition(5f, 3f, 5, 2f, false, true, ShakeRandomnessMode.Full);

    // Check if all ants are defeated
    if (spawnedAnts.TrueForAll(ant => ant == null))
    {
        Debug.Log("Afraid: All enemies defeated, transitioning to Happy.");
        TransitionToState(State.Happy);
    }
    else
    {
        // Optionally, keep checking periodically or set up an event/listener to trigger this check
        StartCoroutine(CheckEnemiesDefeated());
    }
}

IEnumerator CheckEnemiesDefeated()
{
    // Wait for a short period before checking again to give time for conditions to change
    yield return new WaitForSeconds(0.5f);

    // Re-check if all ants are defeated
    if (spawnedAnts.TrueForAll(ant => ant == null))
    {
        Debug.Log("Afraid: All enemies defeated on re-check, transitioning to Happy.");
        TransitionToState(State.Happy);
    }
    else
    {
        // Continue to check if still in Afraid state
        if (currentState == State.Afraid)
        {
            StartCoroutine(CheckEnemiesDefeated());
        }
    }
}

    void HappyBehavior()
    {
        Debug.Log("Happy: Playing happy sounds and animations.");
        // Implement Happy behavior
        audioSource.PlayOneShot(happySound, 1);
        transform.DOJump(transform.position, 0.5f, 1, .2f);

        // Transition to next state based on conditions
        TransitionToState(State.Idle);
    }

    void CelebratingBehavior()
    {
        Debug.Log("Celebrating: Playing celebration animations and sounds.");
        // Implement Celebrating behavior
        audioSource.PlayOneShot(happySound, 1);
        transform.DOJump(transform.position, 0.5f, 1, .2f);
    }
   

    IEnumerator LookAtDirection(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        float time = 0;

        while (time < 1f)
        {
            // Smoothly interpolate the rotation over one second
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, time);
            time += Time.deltaTime;
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered: " + other.gameObject.name + tag);
        if (other.CompareTag("AntSpawnTrigger") )
        {
            StartCoroutine(SpawnAnts());
            if (currentState == State.Idle && currentWaypoint == 2 && !enemiesDefeated)
            {
                currentState = State.Afraid;
            }
            else if (enemiesDefeated)
            {
                return;
            }
            
        }
    }

    IEnumerator SpawnAnts()
    {
        Debug.Log("Spawning Ants Now");
        for (int i = 0; i < totalAnts; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * 5;
            randomDirection.y = 0;
            Vector3 spawnPosition = transform.position + randomDirection;

            // Ensure the spawn position is not too close to the Catp
            while (Vector3.Distance(spawnPosition, transform.position) < 0.25f)
            {
                randomDirection = Random.insideUnitSphere * 5;
                randomDirection.y = 0;
                spawnPosition = transform.position + randomDirection;
            }

            GameObject ant = Instantiate(antPrefab, spawnPosition, Quaternion.identity);
            spawnedAnts.Add(ant);

            // Set the target for the ant to follow
            AntEnemyBehavior antBehavior = ant.GetComponent<AntEnemyBehavior>();
            if (antBehavior != null)
            {
                antBehavior.target = this.transform;
            }
            else
            {
                Debug.LogError("AntBehavior component is not found on the antPrefab. Make sure it's attached.");
            }

            
            
            
            // Wait before spawning the next ant
            yield return new WaitForSeconds(antSpawnDelay);
        }
    }



    
    
    
    
    
    /* void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component missing from this game object.");
            agent = gameObject.AddComponent<NavMeshAgent>();
        }

        agent.speed = moveSpeed;
        agent.autoBraking = false;

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("Caterpillar is not on the NavMesh at the start.");
            if (!agent.Warp(waypoints[0].position))
            {
                Debug.LogError("Failed to warp the agent to the starting waypoint.");
            }
        }

        currentWaypoint = 1;
        GoToNextState();
    }

    void GoToNextState()
    {
        Debug.Log($"Transitioning from {currentState} to next state at waypoint {currentWaypoint}");
        switch (currentState)
        {
            case State.Idle:
                MoveToNextWaypoint();
                break;
            case State.Hungry:
                // Ensure the caterpillar only moves if it is fed and the critical consumable object is deactivated
                if (isFed && ((currentWaypoint == 1 && !criticalFragmentLeaf1.activeSelf) ||
                              (currentWaypoint == 3 && !criticalFragmentLeaf2.activeSelf)))
                {
                    MoveToNextWaypoint();
                    isFed = false;
                }
                break;
            case State.Eating:
                //MoveToNextWaypoint();
                break;
            case State.Afraid:
                if (currentWaypoint == 2 && !enemiesDefeated) {
                    StartCoroutine(PerformAfraidBehavior());
                }
                break;
            case State.Happy:
                if (currentWaypoint == 3)
                {
                    currentState = State.Celebration;
                    GoToNextState();
                }
                else
                {
                    MoveToNextWaypoint();
                }
                break;
            case State.Celebration:
                StartCoroutine(StartCelebration());
                break;
        }
        Debug.Log($"New state is {currentState}");
    }
    
    void MoveToNextWaypoint()
    {
        if (currentWaypoint < waypoints.Length - 1)
        {
            //currentWaypoint++;
            
            if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
            {
                Debug.LogError("Agent is not active or not on NavMesh when trying to move.");
                return;
            }
            
            agent.SetDestination(waypoints[currentWaypoint].position);
            Debug.Log($"Setting destination to waypoint {currentWaypoint} at position {waypoints[currentWaypoint].position}");

            // Prepare the caterpillar for the next state based on the waypoint
            if (currentWaypoint == 1 || currentWaypoint == 3)
            {
                StartCoroutine(BehaveHungry());
            }
            else if (currentWaypoint == 2)
            {
                // At waypoint 2, check if ants should spawn
                if (!isAntSpawned) {
                    SpawnAnts(); // Spawn ants if they have not been spawned
                    isAntSpawned = true; // Prevents re-spawning ants
                }
                currentState = State.Afraid; // Set state to Afraid
                GoToNextState(); // Proceed to process the next state
            }
        }
        else
        {
            currentState = State.Celebration;
            GoToNextState();
        }
    }

    IEnumerator BehaveHungry()
    {
        yield return new WaitForSeconds(0.1f);
        if ((currentWaypoint == 1 || currentWaypoint == 3)) {
            audioSource.PlayOneShot(hungrySound);
            //animator.SetTrigger("isHungry");  // instead of animation LookAt rotation
            yield return new WaitForSeconds(2);
            currentState = State.Hungry;
        }
    }

    public void Feed()
    {
        StartCoroutine(HandleFeeding());
    }
    
    IEnumerator PerformAfraidBehavior()
    {
        
        
        // Check if the caterpillar is at the right waypoint to start the "Afraid" behavior
        if (currentWaypoint == 2 && !isAntSpawned) 
        {
            
            // Spawn ants here and ensure it's not repeated
            SpawnAnts();
            isAntSpawned = true;
            
            yield return new WaitForSeconds(5);
            
            audioSource.PlayOneShot(scaredSound);
            animator.SetTrigger("Afraid");

            yield return new WaitForSeconds(2);
            currentState = State.Happy;
            GoToNextState();
        }
    }

    IEnumerator HandleFeeding()
    {
        if (currentState == State.Hungry && (currentWaypoint == 1 || currentWaypoint == 3))
        {
            Debug.Log("Feeding at waypoint: " + currentWaypoint);
            isFed = true;
            audioSource.PlayOneShot(eatSound);
            animator.SetTrigger("Eating");
            transform.localScale *= 1.05f;
            //currentState = State.Eating;
            Debug.Log("State changed to Eating after feeding");
            
            yield return new WaitForSeconds(2); // Wait for the eating animation to finish
            
            
            // Check if it's the first feeding at Waypoint 1
            if (currentWaypoint == 1 && !criticalFragmentLeaf1.activeSelf)
            {
                currentState = State.Idle; // Make sure the state is reset to idle before moving
                currentWaypoint = 2;  // Move to the next waypoint logically
                Debug.Log("Waypoint change to 2. !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                MoveToNextWaypoint(); // Move physically to the next waypoint
                
                
            }
            else if (currentWaypoint == 3 && !criticalFragmentLeaf2.activeSelf)
            {
                Debug.Log("Transition to celebration after feeding at waypoint 3.");
                yield return new WaitForSeconds(2);
                currentState = State.Celebration;
                GoToNextState();
            }
            else
            {
                Debug.Log("Unexpected feeding situation at waypoint " + currentWaypoint);
                currentState = State.Eating;
                MoveToNextWaypoint(); // Make sure to continue moving correctly
            }
            
            
        }
    }



    IEnumerator ShowHappyAndRelief()
    {
        audioSource.PlayOneShot(happySound);
        yield return new WaitForSeconds(1);
        audioSource.PlayOneShot(reliefSound);
        yield return new WaitForSeconds(1);
        enemiesDefeated = false;
        currentState = State.Happy;
        GoToNextState();
    }

    public void SpawnAnts()
    {
        Debug.Log("Spawning ants now.");
        for (int i = 0; i < totalAnts; i++)
        {
            GameObject ant = Instantiate(antPrefab, waypoints[currentWaypoint].position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)), Quaternion.identity);
        
            // Make sure the ant is active. If the prefab was inactive, we need to activate it here.
            ant.SetActive(true);

            // Now ensure that the ant's behavior script is also properly activated
            AntEnemyBehavior antBehavior = ant.GetComponent<AntEnemyBehavior>();
            if (antBehavior != null)
            {
                antBehavior.ActivateAnt(transform);
            }
            else
            {
                Debug.LogError("Ant prefab does not have AntEnemyBehavior attached!");
            }
        }
    }

    // void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("AntSpawnTrigger") && currentWaypoint == 2)
    //     {
    //         Debug.Log("AntSpawnTrigger activated at waypoint " + currentWaypoint);
    //         StartCoroutine(SpawnAnts());
    //     }
    // }

    public void EnemyDefeated()
    {
        defeatedEnemiesCount++;
        Debug.Log($"Enemy defeated. Total defeated: {defeatedEnemiesCount}. Required: {totalAnts}");
        if (defeatedEnemiesCount >= totalAnts)
        {
            enemiesDefeated = true;
            if (currentState == State.Afraid)
            {
                StartCoroutine(ShowHappyAndRelief()); // Show happy and relief after all enemies are defeated
            }

            StartCoroutine(MoveToWaypoint3());

        }
    }

    IEnumerator StartCelebration()
    {
        animator.SetTrigger("Celebrate");
        celebrationParticles.Play();
        audioSource.PlayOneShot(happySound);
            yield return new WaitForSeconds(happySound.length);
        
    }

    IEnumerator MoveToWaypoint3()
    {
        yield return new WaitForSeconds(5);
        currentWaypoint = 3;
        MoveToNextWaypoint();
    }*/
}
