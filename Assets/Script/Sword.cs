using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] private int damage;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.tag == "Enemy")
        {
            other.GetComponent<EnemyHealt>().HurtEnemy(damage);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.tag == "Enemy")
        {
            other.GetComponent<EnemyHealt>().HurtEnemy(damage);
        }
    }
}
