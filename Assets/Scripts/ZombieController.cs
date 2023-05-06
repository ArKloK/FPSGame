using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    [Header("Zombie Things")]
    public NavMeshAgent zombieAgent;
    public Transform target;
    public LayerMask playerLayer;
    private float zombieSpeed = 2;
    public Animator animController;

    [Header("Zombie Attack variables")]
    private bool previouslyAttack;
    public float timeBtwAttack;
    public Camera attackingRaycastArea;

    [Header("Zombie Health and Damage")]
    public float zombieHealth = 30f;
    public int giveDamage = 20;

    [Header("Zombie moods")]
    public bool playerInAttackingRadius;
    public float attackingRadius;

    private void Awake()
    {
        animController = GetComponent<Animator>();
        zombieAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        playerInAttackingRadius = Physics.CheckSphere(transform.position, attackingRadius, playerLayer);

        if (zombieAgent.speed > 0)
            PursuePlayer();
        if (playerInAttackingRadius) AttackPlayer();
    }

    public void ObjectHitDamage(float amount)
    {
        StartCoroutine(ReciveDamage());
        zombieHealth -= amount;
        if (zombieHealth <= 0) { Die(); }
    }

    private void PursuePlayer()
    {
        zombieAgent.speed = zombieSpeed;
        zombieAgent.SetDestination(target.position);
        transform.LookAt(target.position);
        animController.SetFloat("ZombieSpeed", zombieAgent.speed);
    }
    void Die()
    {
        Destroy(gameObject);
    }
    private void AttackPlayer()
    {
        if (!previouslyAttack)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(attackingRaycastArea.transform.position, attackingRaycastArea.transform.forward, out hitInfo, attackingRadius))
            {
                playerInAttackingRadius = true;
                Debug.Log("Attacking " + hitInfo.transform.name);

                PlayerController playerController = hitInfo.transform.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.DoDamage(giveDamage);
                }
            }
            else
            {
                playerInAttackingRadius = false;
            }
            animController.SetBool("Attacking", playerInAttackingRadius);

            previouslyAttack = true;
            Invoke(nameof(ActiveAttacking), timeBtwAttack);
        }
    }

    private void ActiveAttacking()
    {
        previouslyAttack = false;
        animController.SetBool("Attacking", playerInAttackingRadius);
    }

    IEnumerator ReciveDamage()
    {
        float speed = zombieAgent.speed;
        zombieAgent.speed = 0;
        animController.SetFloat("ZombieSpeed", zombieAgent.speed);
        animController.SetBool("isHitted", true);
        yield return new WaitForSeconds(1);
        zombieAgent.speed = speed;
        animController.SetFloat("ZombieSpeed", zombieAgent.speed);
        animController.SetBool("isHitted", false);
    }

}
