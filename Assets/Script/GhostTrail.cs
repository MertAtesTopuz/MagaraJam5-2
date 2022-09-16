using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostTrail : MonoBehaviour
{
    private Transform player;

    private bool faceRight = true;

    public CharacterController control;

    private SpriteRenderer sr;
    private SpriteRenderer playerSr;

    private Color color;

    public Material ghostMaterial;

    public float activeTime = 0.1f;
    private float timeActivated;
    private float alpha;
    public float alphaSet = 0.8f;
    private float alphaMultiplier = 0.85f;


    private void OnEnable()
    {
        sr = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerSr = player.GetComponent<SpriteRenderer>();

        alpha = alphaSet;
        sr.sprite = playerSr.sprite;
        transform.position = player.position;
        transform.rotation = player.rotation;
        timeActivated = Time.time;
    }

    private void Update()
    {
        alpha *= alphaMultiplier;
        //color = new Color(1f, 1f, 1f, alpha);
        // sr.color = color;
        CheckDirection();

        sr.material = ghostMaterial;

        if (Time.time >= (timeActivated + activeTime))
        {
            AfterImagePool.instance.AddToPool(gameObject);
        }
    }

    private void CheckDirection()
    {
        if (faceRight && control.moveInput < 0)
        {
            Flip();
        }

        else if (!faceRight && control.moveInput > 0)
        {
            Flip();
        }
    }

    void Flip()
    {
        faceRight = !faceRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;

    }
}
