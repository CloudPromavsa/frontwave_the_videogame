using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
Bullet class apply velocity to the bullet game object rigidbody
to the direction indicated, when respawned from the turrets.
*/
public class Bullet : MonoBehaviour
{
    [Header("Bullet Configuration")]
    //Turret velocity
    public float m_TurretBulletVelocity;
    //Turret rigidbody to apply velocity
    private Rigidbody2D rb;
    //Bullet Damage
    public float m_Damage = 0.5f;
    public float m_Time2Destroy;
    float currentTime;

    [Header("DEBUG")]
    public bool debug = false; 


    // Start is called before the first frame update
    private void Awake()
    {
        //Access bullet rigidbody
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        //Apply velocity to the rigidbody in the opposite direction of right (left)
        //with the custom bullet velocity
        rb.velocity = -transform.right * m_TurretBulletVelocity;

        currentTime += Time.deltaTime;
        if (currentTime >= m_Time2Destroy)
        {
            Destroy(gameObject);
        } 
    }
}
