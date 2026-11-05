using UnityEngine; // Gives this script access to Unity's features.
using UnityEngine.AI; // Lets us use Unity's navigation components.

public class MyEnemyAI : MonoBehaviour
{ // The class starts here.
    // The different behaviors our enemy can perform.
    // Keeping this enum inside MyEnemyAI avoids conflicts
    // with the original project's EnemyState enum.
    private enum AIState
    {
        Patrolling,
        SearchingForWeapon,
        ChasingOpponent,
        Attacking,
        Dead
    }

    // Start with the patrol behavior we've already built.
    // SerializeField lets us see the current state in the Inspector.
    [SerializeField] private AIState currentState = AIState.Patrolling;
    // Stores the enemy's movement component.
    private NavMeshAgent agent;

    // References this enemy's existing health component.
    private Health enemyHealth;

    // Shows a destination field in Unity's Inspector.
    // Vector3 stores a position using X, Y, and Z.
    [SerializeField] private Vector3 destination;

    // Groups these settings under a heading in the Inspector.
    [Header("Patrol Settings")]

    // How far from its current position the enemy can
    // look for its next patrol destination.
    [SerializeField] private float patrolRadius = 100f;
    // Prevents the enemy from choosing a spot too close to itself.
    [SerializeField] private float minimumPatrolDistance = 20f;


    // How many seconds the enemy pauses after reaching a spot.
    [SerializeField] private float waitTime = 2f;

    // Tracks how long this enemy has been waiting.
    // Each spawned enemy has its own timer.
    private float waitTimer = 0f;

    [Header("Weapon Search Settings")]

    // How far away the enemy can notice weapon pickups.
    [SerializeField] private float weaponSearchRadius = 25f;

    // How many seconds to wait between searches.
    // This avoids searching every single frame.
    [SerializeField] private float weaponSearchInterval = 1f;

    // Counts down until the next search.
    // Starting at zero lets the first search happen immediately
    // once we connect the search logic.
    private float weaponSearchTimer = 0f;

    // Remembers the pickup this enemy wants to collect.
    private LootItem targetWeapon;

    // Tracks whether our new AI has collected a weapon.
    // Enemies begin unarmed.
    private bool hasWeapon = false;

    private void Start()
    {
        // Finds this enemy's health component.
        enemyHealth = GetComponent<Health>();
        // Finds the movement component on this enemy.
        agent = GetComponent<NavMeshAgent>();

        // Stop this method if the movement component is missing
        // or the enemy hasn't spawned on a walkable NavMesh.
        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogWarning("Enemy needs a NavMeshAgent on the NavMesh.", gameObject);
            return;
        }

        // Allows the agent to move.
        agent.isStopped = false;

        // Choose a patrol spot instead of using the fixed destination.
        ChoosePatrolDestination();
    }

    private void Update()
    {
        // Skip movement checks if the agent isn't ready.
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
            return;

        // Death takes priority over patrolling.
        if (enemyHealth != null && enemyHealth.isDead)
        {
            currentState = AIState.Dead;
            agent.isStopped = true;
            agent.ResetPath();
            return;
        }

        // Handle traveling toward a weapon.
        if (currentState == AIState.SearchingForWeapon)
        {
            // If the pickup disappeared, return to patrolling.
            // Another character might have collected it.
            if (targetWeapon == null)
            {
                currentState = AIState.Patrolling;
                waitTimer = 0f;

                // Respect stops requested by other game systems.
                if (!agent.isStopped)
                    ChoosePatrolDestination();
            }

            // Don't run the patrol logic while pursuing a weapon.
            return;
        }

        // Other states don't have their behavior implemented yet.
        if (currentState != AIState.Patrolling)
            return;

        // Don't patrol while other game systems have stopped us.
        if (agent.isStopped)
        {
            return;
        }

        // Look for a weapon while continuing our patrol.
        SearchForWeapon();

        // Wait while Unity is calculating the path.
        if (agent.pathPending)
            return;

        // If we're still traveling, reset the waiting timer.
        if (agent.hasPath &&
            agent.remainingDistance > Mathf.Max(agent.stoppingDistance, 0.2f))
        {
            waitTimer = 0f;
            return;
        }

        // Count how long we've been waiting.
        waitTimer += Time.deltaTime;

        // After waiting, try choosing another patrol destination.
        if (waitTimer >= waitTime)
        {
            waitTimer = 0f;
            ChoosePatrolDestination();
        }
    }

    private void ChoosePatrolDestination()
    {
        // Only try to move if the agent is enabled and on the NavMesh.
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
            return;

        // Try up to 10 random locations.
        for (int attempt = 0; attempt < 10; attempt++)
        {
            // Pick a random offset across the ground.
            Vector2 offset = UnityEngine.Random.insideUnitCircle * patrolRadius;

            // Add that offset to this enemy's current position.
            Vector3 randomPoint = transform.position
                + new Vector3(offset.x, 0f, offset.y);

            // Find a NavMesh position within 2 units of that point.
            if (!NavMesh.SamplePosition(
                randomPoint, out NavMeshHit hit, 2f, agent.areaMask))
                continue;
            // Measure the ground distance to the actual NavMesh position.
            Vector3 offsetToSpot = hit.position - transform.position;

            // Ignore height differences when measuring distance.
            offsetToSpot.y = 0f;

            float distanceToSpot = offsetToSpot.magnitude;

            // Reject spots that are too close or beyond our patrol radius.
            // "continue" tells the loop to try another random spot.
            if (distanceToSpot < minimumPatrolDistance || distanceToSpot > patrolRadius)
            {
                continue;
            }

            // Check whether the enemy can reach that position.
            NavMeshPath path = new NavMeshPath();

            if (!agent.CalculatePath(hit.position, path)
                || path.status != NavMeshPathStatus.PathComplete)
                continue;

            // Follow the path we found.
            agent.isStopped = false;
            agent.SetPath(path);

            // We found a destination, so stop trying other locations.
            return;
        }

        Debug.Log("Couldn't find a patrol spot this time.", gameObject);
    }

    // Checks whether a pickup is a weapon.
    private bool IsWeapon(LootItem item)
    {
        // Ignore missing or already collected pickups.
        if (item == null)
            return false;

        // Ignore pickups without item settings.
        if (item.itemStat == null)
            return false;

        // True means weapon; false means another item type.
        return item.itemStat.itemType == ItemType.Weapon;
    }

    private LootItem FindNearestWeapon()
    {
        // We need a working agent to check paths.
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
            return null;

        // Find nearby colliders, including pickup triggers.
        // ~0 means search every physics layer.
        Collider[] nearbyObjects = Physics.OverlapSphere(
            transform.position,
            weaponSearchRadius,
            ~0,
            QueryTriggerInteraction.Collide);

        LootItem nearestWeapon = null;
        float nearestDistance = Mathf.Infinity;

        // Reuse this object to check each candidate's path.
        NavMeshPath path = new NavMeshPath();

        foreach (Collider nearbyObject in nearbyObjects)
        {
            // Look for a LootItem on the collider or its parent.
            LootItem item = nearbyObject.GetComponentInParent<LootItem>();

            // Skip anything that isn't a weapon.
            if (!IsWeapon(item))
                continue;

            // Measure straight-line distance to the pickup.
            float distance = Vector3.Distance(
                transform.position, item.transform.position);

            // Skip pickups outside our range or farther than our best match.
            if (distance > weaponSearchRadius || distance >= nearestDistance)
                continue;

            // Only accept a weapon if Unity finds a complete path.
            if (!agent.CalculatePath(item.transform.position, path)
                || path.status != NavMeshPathStatus.PathComplete)
                continue;

            // Remember this as the closest suitable weapon so far.
            nearestWeapon = item;
            nearestDistance = distance;
        }

        // Returns null if no suitable weapon was found.
        return nearestWeapon;
    }

    private void SearchForWeapon()
    {
        // No need to search if we're armed
        // or already have a weapon pickup selected.
        if (hasWeapon || targetWeapon != null)
            return;

        // Count down the time until our next search.
        weaponSearchTimer -= Time.deltaTime;

        if (weaponSearchTimer > 0f)
            return;

        // Reset the timer so we don't search every frame.
        weaponSearchTimer = weaponSearchInterval;

        // Find and remember a suitable weapon pickup.
        targetWeapon = FindNearestWeapon();

        if (targetWeapon != null)
        {
            // Switch from patrolling to collecting this weapon.
            currentState = AIState.SearchingForWeapon;

            // Clear the patrol waiting timer.
            waitTimer = 0f;

            // Replace the patrol destination with the weapon's position.
            agent.SetDestination(targetWeapon.transform.position);

            Debug.Log(
                gameObject.name + " is heading toward: "
                + targetWeapon.itemStat.itemName,
                gameObject);
        }
    }

    // The pickup script will call this after we collect a weapon.
    public void OnWeaponCollected()
    {
        // Remember that this enemy is now armed.
        hasWeapon = true;

        // Stop tracking the pickup we just collected.
        targetWeapon = null;

        // Patrol until we add opponent detection.
        currentState = AIState.Patrolling;
        waitTimer = 0f;

        // Clear the old path and allow our patrol timer to run.
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = false;
        }

        Debug.Log(gameObject.name + " collected a weapon.", gameObject);
    }
}