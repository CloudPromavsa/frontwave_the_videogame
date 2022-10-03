using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
EnemyControl class set the enemy configuration,
movement, rotation, take damage and send score to the GameManager.
*/
public class EnemyControl : MonoBehaviour
{
    [Header("Game Manager")]
    GameManager m_GameManager; //GameManager class

    [Header("Enemy Waypoints")]
    public float m_maxWayPointDistance; //Determines if the distance between the enemy character and waypoint is close enough to continue to next.
    int wp_index = 0; //Gets from one on one all waypoints. 

    [Header("Enemy configuration")]
    public float m_EnemySpeed = 0.5f; //Enemy translation speed
    public GameObject[] m_EnemyOrientation; //Enemy orientation (to change or flip sprites)
    public bool m_UseVerticalFlip; //If character has only two states (as a tank, forward and up) flip will aply to horizontal and vertical
    public float m_EnemyLife = 100.0f; //Each enemy has different life quantity
    public float m_Enemy_CurrentLife = 100.0f;
    public float m_EnemyDamage = 5.0f;
    public float m_sendScore = 0.5f;
    public float m_sendReward = 0.5f;

    [Header("VISUAL")]
    public HealthBar m_EnemyHealthBar;

    [Header("DEBUG")]
    public bool debug = false;

    private void Awake()
    {
        // Find the Game Manager Game Object to Access the Game Manager Component to communicate with
        // to follow waypoints and scoring.
        m_GameManager = GameObject.Find("GameManager").GetComponent<GameManager>(); 
    }

    // Start is called before the first frame update
    private void Start()
    {
        if (debug) Debug.Log(m_GameManager.m_WaypointsList.Length);
        if (debug) Debug.Log(m_EnemyOrientation.Length);

        m_EnemyHealthBar.MaxHealth(m_EnemyLife); //Health bar at maximum
        m_Enemy_CurrentLife = m_EnemyLife; //Set current bar value at maximum

    } 

    // Update is called once per frame
    private void Update()
    {
        //STATEMENT FOR ENEMY TRANSLATE AND ROTATION USING WAYPOINTS
        if (wp_index < m_GameManager.m_WaypointsList.Length)
        {
            if (debug) Debug.Log(wp_index);
            
            //newDirection is a vector that stores the vector substraction of the next waypoint and the current position.
            //The enemy then will be translated to the resulted coodinate.
            //The next waypoint will be acceded when the wp_index get updated. 
            //wp_index update will be calculated when the enemy reach successfuly the current waypoint.
            //A waypoint is considered as "reached" when the enemy is very close to it. 
            Vector3 newDirection = m_GameManager.m_WaypointsList[wp_index].transform.position - transform.position;

            //This if statements controls the enemy orientation states. 
            //Operation Dot from Vector3, calculates the math dot operation for a normalized Vector that
            //returns 0 if it has no orientation coincidence or 1 if it is a full orientation coincidence.
            //newDirection is compared with the vector3 right, left, up and down. If it returns at least
            //an 0.2 of coincidence, it is considered as one of the mentioned orientations.
            //If newDirection has a Vector3.right coincidence, this current respawned enemy will change
            //its orientation between its childen activation/deactivation or by flip its sprite in the
            //sprite renderer component.
            //sprite will flip horizontally if its moving to the right or left.
            if (Vector3.Dot(newDirection, Vector3.right) > 0.2 || Vector3.Dot(newDirection, Vector3.left) > 0.2)
            {
                if (debug) Debug.Log("RIGHT");
                m_EnemyOrientation[0].SetActive(true); // Horizontal enemy gameobject/animation active
                m_EnemyOrientation[1].SetActive(false); // Vertical enemy gameobject/animation deactive

                // If m_EnemyOrientation has more than two children GameObject, mean that has more than two
                // animation states configurated
                if (m_EnemyOrientation.Length > 2) m_EnemyOrientation[2].SetActive(false);

                // If the Horizontal GameObject is active, the sprite will be flipped when is right or left movement.
                if(Vector3.Dot(newDirection, Vector3.right) > 0.2)
                     GetComponentInChildren<SpriteRenderer>().flipX = false;
                if (Vector3.Dot(newDirection, Vector3.left) > 0.2)
                    GetComponentInChildren<SpriteRenderer>().flipX = true;
            }

            //sprite will flip vertically if its moving up or down.
            if (Vector3.Dot(newDirection, Vector3.up) > 0.2)
            {
                if (debug) Debug.Log("UP");
                m_EnemyOrientation[0].SetActive(false); // Horizontal sprite/animation active
                m_EnemyOrientation[1].SetActive(true); // Vertical sprite/animation active

                // If the Vertical GameObject is active, the sprite will be flipped when is up or down movement.
                // Flip will only occur if m_UseVerticalFlip is true.
                if (m_UseVerticalFlip) GetComponentInChildren<SpriteRenderer>().flipY = false;

                else
                {
                    m_EnemyOrientation[1].SetActive(true);

                    // If m_EnemyOrientation has more than two children GameObject, mean that has more than two
                    // animation states configurated
                    if (m_EnemyOrientation.Length > 2) m_EnemyOrientation[2].SetActive(false);
                } 
            }

            //sprite will flip vertically if its moving up or down.
            if (Vector3.Dot(newDirection, Vector3.down) > 0.2)
            {
                if (debug) Debug.Log("DOWN");
                m_EnemyOrientation[0].SetActive(false); // Horizontal sprite/animation active
                m_EnemyOrientation[1].SetActive(true); // Vertical sprite/animation active

                // If the Vertical GameObject is active, the sprite will be flipped when is up or down movement.
                // Flip will only occur if m_UseVerticalFlip is true.

                if (m_UseVerticalFlip) GetComponentInChildren<SpriteRenderer>().flipY = true;
                else
                {
                    m_EnemyOrientation[1].SetActive(false);

                    // If m_EnemyOrientation has more than two children GameObject, mean that has more than two
                    // animation states configurated
                    if (m_EnemyOrientation.Length > 2) m_EnemyOrientation[2].SetActive(true);
                } 
            }

            //The Enemy gameobject will be translated to the newDirection at m_EnemySpeed by time deltatime
            //to convert frames to seconds.
            transform.Translate(newDirection.normalized * m_EnemySpeed * Time.deltaTime, Space.World); 

            //The distance of the waypoint and the enemy is used to access to the next waypoint. 
            //dist is a float received by the Vector3.Distance that will be compared to the max distance we want
            //to access the next way point. m_maxWayPointDistance can be adjusted in inspector. Suggested: 0.05
            float dist = Vector3.Distance(transform.position, m_GameManager.m_WaypointsList[wp_index].transform.position);
            if (dist <= m_maxWayPointDistance) 
            {
                //Debug.Log("Distance to other: " + dist);
                wp_index++;
            }  
        }
        if (m_EnemyLife <= 0) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {  
        //Delete Enemy at the end of the track in the last endpoint
        if (other.gameObject.name == "DeleteEnemyWaypoint")
        {
            m_GameManager.m_MainTowerCurrentLife -= m_EnemyDamage;
            m_GameManager.EnemyHitTower();
            m_GameManager.m_EnemySpawnedList.Remove(gameObject.transform);
            Destroy(this.gameObject);
        }

        if(other.gameObject.tag == "Bullet")
        {
            m_Enemy_CurrentLife -= other.gameObject.GetComponent<Bullet>().m_Damage;
            m_EnemyHealthBar.HealtH(m_Enemy_CurrentLife);
            m_GameManager.UpdateScoreAndReward(m_sendScore, m_sendReward);
            Destroy(other.gameObject);

            if(m_Enemy_CurrentLife <= 0)
            { 
                m_GameManager.UpdateScoreAndReward(m_sendScore*2.0f, m_sendReward*2.0f);
                m_GameManager.m_EnemySpawnedList.Remove(gameObject.transform);
                Destroy(gameObject);
            }
        }
    } 
}
