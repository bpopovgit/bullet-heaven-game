using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMover : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 2f;

    private Vector2 moveDir;
    private float lifeTimer;

    public void SetDirection(Vector2 dir)
    {
        moveDir = dir.normalized;
        lifeTimer = 0f;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        transform.position += (Vector3)(moveDir * speed * Time.deltaTime);

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTime)
            gameObject.SetActive(false);
    }
}