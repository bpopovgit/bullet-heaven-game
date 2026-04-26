using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f; // Movement speed of the enemy

    private Transform player; // Reference to the player's transform
    private StatusReceiver _status;

    private void Awake()
    {
        _status = GetComponent<StatusReceiver>();
    }

    private void Start()
    {
        if (_status == null)
            _status = GetComponent<StatusReceiver>();

        // Find the player GameObject by tag
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure the player has the 'Player' tag assigned.");
        }
    }

    private void Update()
    {
        if (player != null)
        {
            MoveTowardsPlayer();
        }
    }

    private void MoveTowardsPlayer()
    {
        // Calculate the direction to the player
        Vector2 direction = (player.position - transform.position).normalized;
        float speedMultiplier = _status != null ? _status.SpeedMultiplier : 1f;

        // Move the enemy toward the player
        transform.position = Vector2.MoveTowards(transform.position, player.position, speed * speedMultiplier * Time.deltaTime);
    }
}
