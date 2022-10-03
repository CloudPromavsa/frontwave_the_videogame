using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
GunTurretControl class set the turret configuration,
enemy detection, distances calculations & defense.
*/
public class GunTurretControl : MonoBehaviour
{ 

    [Header("Turret Weapon Configuration")]
    public Transform[] m_TurretRespawnPoint; //Bullet start point
    public Rigidbody2D m_TurretBullet; //GameObject to trigger as bullet
    public Transform m_weapon; //Turret weapon (gameobject that rotate looking for enemies).
    public float m_TurretBulletNextRespawn; //Time to respawn next bullet
    float currentTime = 0.0f; //currentTime from 0 sec to m_TurretBulletNextRespawn.
    private bool m_RespOBJ = true; //controls one bullet respawn at time for each respawn point.
    private bool startFire = false;

    [Header("Turret Enemy Detection")]
    public List<Transform> m_EnemyFound; //List of enemies on turret radar. Add/Remove when enter/exit its radar. 
    public float m_torretRotationSpeed; //Weapon rotation speed on the enemy direction
    public float m_RotationOffset = 180.0f; //this offset helps to align in 180 degrees the turret facing the enemy
    Transform threat; //threat is a variable to store the nearest enemy founded from the m_EnemyFound stack list. 

    [Header("DEBUG")]
    public bool debug = false;

    private void Update()
    {
        //currentTime increase in seconds
        //If the currentTime is less than next respawn, a new bullet is instanced
        //in the weapon position.
        //If currentTime reaches the m_TurretBulletNextRespawn, currentTime 
        //restart from 0.
        currentTime += Time.deltaTime; 
        if (currentTime <= m_TurretBulletNextRespawn && startFire)
        {
            if (m_RespOBJ)
            {
                if(debug) Debug.Log(currentTime); 
                //foreach search for all weapon respawn points to instantiate a bullet from each one
                foreach (Transform tresp in m_TurretRespawnPoint)
                {
                    //instantiate the bullet at the weapon respawn point
                    Rigidbody2D turretRespGO = Instantiate(m_TurretBullet, tresp.transform.position, tresp.transform.rotation);
                    //start a couroutine to yield next shoot
                    StartCoroutine("TurrentRespawnerYield");
                }
            }
        }
        else currentTime = 0.0f;
    }

    //Couroutine to yield next shoot
    IEnumerator TurrentRespawnerYield()
    {
        m_RespOBJ = false;
        yield return new WaitForSeconds(m_TurretBulletNextRespawn);
        m_RespOBJ = true;
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (threat != null)
        {
            threat = checkEnemiesDistances(m_EnemyFound); //Check list for the nearest enemy and save it on the threat variable

            //Gets the distance between the target and turret. Then gets the angle to apply rotation plus custom offset.
            Vector3 dir = threat.position - m_weapon.transform.position;
            float rotationZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            m_weapon.transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotationZ + m_RotationOffset);

            startFire = true;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        //If a new enemy enters the turret trigger, the enemy is added to the turret enemy list on m_EnemyFound
        if (other.gameObject.tag == "Enemy")
        {
            if (debug) Debug.Log("Enemy is Entered");
            m_EnemyFound.Add(other.gameObject.transform); //Enemy added.
            threat = checkEnemiesDistances(m_EnemyFound); //Check list for the nearest enemy and save it on the threat variable 
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        //If a new enemy exit the turret trigger, the enemy is removed from the turret enemy list on m_EnemyFound
        if (other.gameObject.tag == "Enemy")
        {
            if (debug) Debug.Log("Enemy is Exit");
            m_EnemyFound.Remove(other.gameObject.transform); //Enemy removed.
            threat = checkEnemiesDistances(m_EnemyFound); //Check list for the nearest enemy and save it on the threat variable 
            startFire = false;
        }
    }

    //this function receives the enemies list and returns a transform with the nearest enemy founded
    Transform checkEnemiesDistances(List<Transform> enemiesSet)
    {
        if (enemiesSet.Count > 0) //Check if the enemies list has one or more elements
        {
            Transform nearEnemy; //nearEnemy to store the nearest enemy founded

            nearEnemy = enemiesSet[0].transform; //always start from the first enemy on the list

            foreach (Transform x in enemiesSet) //Traverse the enemies list 
            {
                //Get the distance from the turret and the next enemy on the list
                float enemyDistance = Vector3.Distance(transform.position, x.transform.position);  

                //Compare the nearest enemy stored on the nearEnemy variable with the distance between
                //the turret and the next enemy on the list
                if (enemyDistance <= Vector3.Distance(transform.position, nearEnemy.transform.position))
                {
                    //If a near enemy is founded, replace the nearest enemy on nearEnemy
                    nearEnemy = x;
                }
                if (debug) Debug.Log("Near Enemy: " + nearEnemy.name);
            }
            return nearEnemy; //return the nearest enemy from the list.
        }
        else
            return null; //if list hasn't elements, return null
    }

}
