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

    // The rifle model already attached to this enemy.
    [SerializeField] private GameObject weaponModel;

    [Header("Opponent Detection")]

    // How far the enemy can search for another character.
    [SerializeField] private float detectionRadius = 30f;

    // How often we check for nearby opponents, in seconds.
    [SerializeField] private float opponentSearchInterval = 0.5f;

    // Counts down until the next search.
    private float opponentSearchTimer = 0f;

    // Remembers the character we want to chase.
    // Using Health lets us check whether that character is alive.
    private Health targetOpponent;

    // How often we update the route to a moving opponent.
    [SerializeField] private float chaseUpdateInterval = 0.5f;
    private float chaseTimer = 0f;

    // References the existing animation component.
    private EnemyAnimation enemyAnimation;

    [Header("Combat Settings")]

    // How close we want to get before stopping to attack.
    [SerializeField] private float attackRange = 10f;

    // The existing bullet prefab that we will spawn when firing.
    [SerializeField] private GameObject bulletPrefab;

    // The point at the rifle's muzzle where bullets appear.
    [SerializeField] private Transform shootPoint;

    // Time between shots. 0.5 means two shots per second.
    [SerializeField] private float fireInterval = 0.5f;

    // The time when the next shot is allowed.
    private float fireTimer = 0f;

    // Remembers whether we have started the firing animation.
    private bool isFiringAnimation = false;

    // The muzzle-flash object already attached to the rifle.
    [SerializeField] private GameObject muzzleFlash;

    private void Start()
    {
        // Keep the flash hidden until we fire.
        HideMuzzleFlash();

        // Enemies begin without a visible weapon.
        if (weaponModel != null)
        {
            weaponModel.SetActive(false);
        }

        // Finds this enemy's health component.
        enemyHealth = GetComponent<Health>();

        // Finds this enemy's animation component so we can change its animations.
        enemyAnimation = GetComponent<EnemyAnimation>();

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
        // Handle death before checking whether movement is available.
        if (enemyHealth != null && enemyHealth.isDead)
        {
            currentState = AIState.Dead;

            // Clear our targets and firing flag.
            targetOpponent = null;
            targetWeapon = null;
            isFiringAnimation = false;

            // Stop any remaining muzzle flash.
            CancelInvoke(nameof(HideMuzzleFlash));
            HideMuzzleFlash();

            // Stop moving if the navigation component is still available.
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            // Don't run any more AI behavior after death.
            return;
        }

        // Skip movement checks if the agent isn't ready.
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
            return;

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


        // Both combat states use the same range and visibility checks.
        if (currentState == AIState.ChasingOpponent ||
            currentState == AIState.Attacking)
        {
            // Respect stops requested by other game systems.
            if (agent.isStopped)
                return;

            ChaseOpponent();
            return;
        }

        // Only patrolling enemies continue to the patrol code below.
        if (currentState != AIState.Patrolling)
            return;

        // Don't patrol while other game systems have stopped us.
        if (agent.isStopped)
        {
            return;
        }

        // Unarmed enemies look for a weapon while patrolling.
        SearchForWeapon();

        // Stop patrol logic if we just started pursuing a weapon.
        if (currentState != AIState.Patrolling)
            return;

        // Armed enemies look for nearby opponents.
        SearchForOpponent();

        // Stop patrol logic if we just started chasing.
        if (currentState != AIState.Patrolling)
            return;

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

        // Switch to idle once when an armed enemy begins waiting.
        if (waitTimer == 0f && hasWeapon &&
            enemyAnimation != null && enemyAnimation.animator != null)
        {
            enemyAnimation.animator.CrossFade(
                enemyAnimation.IDLE_PARAM, 0.2f);
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

            // Play rifle-running when an armed enemy starts a patrol route.
            if (hasWeapon && enemyAnimation != null &&
                enemyAnimation.animator != null)
            {
                enemyAnimation.animator.CrossFade(
                    enemyAnimation.RIFLE_RUN_PARAM, 0.2f);
            }

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

        // Show the rifle after collecting a weapon.
        if (weaponModel != null)
        {
            weaponModel.SetActive(true);
        }

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

    // Finds the closest living character within our detection radius.
    private Health FindNearestOpponent()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(
            transform.position,
            detectionRadius,
            ~0,
            QueryTriggerInteraction.Ignore);

        Health nearestOpponent = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider nearbyObject in nearbyObjects)
        {
            // Find the character's health component.
            Health opponent = nearbyObject.GetComponentInParent<Health>();

            // Ignore objects without health, ourselves, and dead characters.
            if (opponent == null || opponent == enemyHealth || opponent.isDead)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                opponent.transform.position);

            // Keep the closest opponent inside our detection range.
            if (distance <= detectionRadius && distance < nearestDistance)
            {
                nearestOpponent = opponent;
                nearestDistance = distance;
            }
        }

        return nearestOpponent;
    }

    // Periodically looks for an opponent when we have a weapon.
    private void SearchForOpponent()
    {
        if (!hasWeapon)
            return;

        opponentSearchTimer -= Time.deltaTime;

        if (opponentSearchTimer > 0f)
            return;

        opponentSearchTimer = opponentSearchInterval;

        Health foundOpponent = FindNearestOpponent();

        // Only print a message when we find a different target.
        if (foundOpponent != null && foundOpponent != targetOpponent)
        {
            Debug.Log(
                gameObject.name + " spotted: " + foundOpponent.gameObject.name,
                gameObject);
        }

        // Also clears our target if no living opponent is nearby.
        targetOpponent = foundOpponent;

        // Switch to chasing and play the rifle-running animation.
        if (targetOpponent != null)
        {
            currentState = AIState.ChasingOpponent;
            // Reset the firing flag when starting a new chase.
            isFiringAnimation = false;
            chaseTimer = 0f;
            waitTimer = 0f;

            if (enemyAnimation != null && enemyAnimation.animator != null)
            {
                enemyAnimation.animator.CrossFade(
                    enemyAnimation.RIFLE_RUN_PARAM, 0.2f);
            }
        }
    }

    private void ChaseOpponent()
    {
        // Give up if the opponent disappears, dies, or leaves our range.
        if (targetOpponent == null || targetOpponent.isDead ||
            Vector3.Distance(transform.position,
                targetOpponent.transform.position) > detectionRadius)
        {
            // Stop combat and resume patrolling.
            ReturnToPatrol();
            return;
        }

        // Measure how far away our opponent is.
        float distanceToOpponent = Vector3.Distance(
            transform.position, targetOpponent.transform.position);

        // Only stop to fire when we're close enough and the shot is clear.
        if (distanceToOpponent <= attackRange && HasClearShot())
        {
            // Show that we are close enough and have a clear shot.
            currentState = AIState.Attacking;



            // Switch to the firing animation once when we enter attack range.
            if (!isFiringAnimation)
            {
                isFiringAnimation = true;
                agent.ResetPath();

                if (enemyAnimation != null && enemyAnimation.animator != null)
                {
                    enemyAnimation.animator.CrossFade(
                        enemyAnimation.FIRING_PARAM, 0.2f);
                }
            }

            // Prepare to update our route immediately if they move away.
            chaseTimer = 0f;


            // Wait until we can fire again.
            ShootAtOpponent();
            return;
        }

        // Resume chasing when we're too far away or the shot is blocked.
        currentState = AIState.ChasingOpponent;


        // The opponent moved away, so start running again.
        if (isFiringAnimation || !agent.hasPath)
        {
            // We are no longer playing the firing animation.
            isFiringAnimation = false;

            if (enemyAnimation != null && enemyAnimation.animator != null)
            {
                enemyAnimation.animator.CrossFade(
                    enemyAnimation.RIFLE_RUN_PARAM, 0.2f);
            }
        }

        // Update the route periodically instead of every frame.
        chaseTimer -= Time.deltaTime;

        if (chaseTimer > 0f)
            return;

        chaseTimer = chaseUpdateInterval;

        NavMeshPath path = new NavMeshPath();

        // Only follow a route that reaches the opponent.
        if (agent.CalculatePath(targetOpponent.transform.position, path)
            && path.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetPath(path);
        }
        else
        {
            // Stop combat and resume patrolling.
            ReturnToPatrol();
        }
    }

    private void ShootAtOpponent()
    {
        // We need a living target and both shooting references.
        if (targetOpponent == null || targetOpponent.isDead ||
            bulletPrefab == null || shootPoint == null)
            return;

        // Aim at the character's collider center instead of their feet.
        Collider targetCollider = targetOpponent.GetComponent<Collider>();

        Vector3 aimPoint = targetCollider != null
            ? targetCollider.bounds.center
            : targetOpponent.transform.position + Vector3.up * 2f;

        // Turn the enemy toward the opponent without tilting its body.
        Vector3 facingDirection =
            targetOpponent.transform.position - transform.position;
        facingDirection.y = 0f;

        if (facingDirection.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(facingDirection);

        // Wait until we can fire again.
        if (Time.time < fireTimer)
            return;
        // Check again after turning, because the gun's position may change.
        if (!HasClearShot())
            return;

        Vector3 shotDirection = aimPoint - shootPoint.position;

        if (shotDirection.sqrMagnitude < 0.001f)
            return;

        // fireTimer now stores the time when the next shot is allowed.
        fireTimer = Time.time + Mathf.Max(fireInterval, 0.05f);

        // Spawn the existing bullet, pointing toward the opponent.
        GameObject bullet = Instantiate(
            bulletPrefab,
            shootPoint.position,
            Quaternion.LookRotation(shotDirection));

        // Briefly show the flash for this shot.
        if (muzzleFlash != null)
        {
            // Restart the effect, even if the previous flash is still active.
            CancelInvoke(nameof(HideMuzzleFlash));
            muzzleFlash.SetActive(false);
            muzzleFlash.SetActive(true);

            // Hide it again after 0.05 seconds.
            Invoke(nameof(HideMuzzleFlash), 0.05f);
        }

        // Prevent the bullet from hitting the enemy that fired it.
        foreach (Collider bulletCollider in bullet.GetComponentsInChildren<Collider>())
        {
            foreach (Collider ownCollider in GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(bulletCollider, ownCollider);
            }
        }
    }

    // Turns off the flash after a shot.
    private void HideMuzzleFlash()
    {
        if (muzzleFlash != null)
            muzzleFlash.SetActive(false);
    }

    // Checks whether the path from the gun to the opponent is clear.
    private bool HasClearShot()
    {
        if (targetOpponent == null || shootPoint == null)
            return false;

        // Aim at the same position used by our shooting method.
        Collider targetCollider = targetOpponent.GetComponent<Collider>();

        Vector3 aimPoint = targetCollider != null
            ? targetCollider.bounds.center
            : targetOpponent.transform.position + Vector3.up * 2f;

        Vector3 direction = aimPoint - shootPoint.position;
        float distance = direction.magnitude;

        if (distance < 0.001f)
            return false;

        // Check everything along the shot, ignoring trigger volumes.
        RaycastHit[] hits = Physics.RaycastAll(
            shootPoint.position,
            direction.normalized,
            distance,
            ~0,
            QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            // Ignore our own body and attached equipment.
            if (hit.transform.IsChildOf(transform))
                continue;

            // Hitting the intended opponent is allowed.
            if (hit.collider.GetComponentInParent<Health>() == targetOpponent)
                continue;

            // Anything else along the shot blocks it.
            return false;
        }

        return true;
    }

    // Stop combat and return to patrolling.
    private void ReturnToPatrol()
    {
        targetOpponent = null;
        currentState = AIState.Patrolling;
        isFiringAnimation = false;
        waitTimer = 0f;

        // Immediately stop any remaining muzzle flash.
        CancelInvoke(nameof(HideMuzzleFlash));
        HideMuzzleFlash();

        agent.ResetPath();
        ChoosePatrolDestination();

        if (enemyAnimation != null && enemyAnimation.animator != null)
        {
            // Run if we found a patrol route; otherwise stand ready.
            enemyAnimation.animator.CrossFade(
                agent.hasPath
                    ? enemyAnimation.RIFLE_RUN_PARAM
                    : enemyAnimation.IDLE_PARAM,
                0.2f);
        }
    }

    // Clean up the firing effect when this AI or enemy is disabled.
    private void OnDisable()
    {
        // Cancel the scheduled flash callback and hide the effect now.
        CancelInvoke(nameof(HideMuzzleFlash));
        HideMuzzleFlash();

        // Reset the flag so the firing animation can restart if needed.
        isFiringAnimation = false;
    }

    // Stop this enemy when the match ends.
    public void StopForGameOver()
    {
        targetOpponent = null;
        targetWeapon = null;

        // Stop movement if navigation is still available.
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        // Don't replace a dead enemy's animation with idle.
        if (enemyHealth != null && !enemyHealth.isDead &&
            enemyAnimation != null && enemyAnimation.animator != null)
        {
            enemyAnimation.animator.CrossFade(
                enemyAnimation.IDLE_PARAM, 0.2f);
        }

        // Disabling this AI stops Update and runs our OnDisable cleanup.
        enabled = false;
    }
}