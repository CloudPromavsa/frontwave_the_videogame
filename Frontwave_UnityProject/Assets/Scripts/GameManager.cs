using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/*
 GameManager class is the main component to control all game flow.
It respawn enemies waves, load waypoints, place turrets, set score
and level features.
 */
public class GameManager : MonoBehaviour
{
    [Header("Turret Assets")]
    public GameObject[] m_TurretGO; //Turret types
    public int m_turretSelection;
    public List<Transform> m_PlacedTurrets; //Manage the placed turrets for upgrades
    public float m_MainTowerLife = 100.0f; //Main Tower Life to defense
    public float m_MainTowerCurrentLife = 100.0f; //Current Main Tower Life

    [Header("Way Points Manager")]
    public Transform m_WayPointsParent; //Waypoints parent
    public Transform[] m_WaypointsList; //Load waypoints childs from the Waypoint parent

    [Header("Enemy Assets")]
    public GameObject[] m_EnemyGO; //Enemy types
    public Transform m_EnemyRespawn; //Respawn position

    [Header("Enemy Waves")]
    public int m_WavesQuantity; //Custom Waves quantity
    int m_currentWave = 0; //start from wave 1
    public float m_minWaveTime; //minimum wave yield time
    public float m_maxWaveTime; //maximum wave yield time
    bool m_newWave = true; //bool to control when newWave can be triggered. 
    public List<Transform> m_EnemySpawnedList; //Store new enemy to m_EnemySpawnedList
    public int level;

    [Header("REWARDS")]
    public float m_Score; //Store generated score
    public float m_Reward;//Store generated reward
    public float m_spent; //Store the turrets spent
    public float[] m_Turret_Reward; //Each turret has a cost and rewards are the currency. Rewards are earned by hits or kills.  

    [Header("VISUAL")]
    public HealthBar m_TowerHealthBar; //Health bar for Tower
    public TMPro.TextMeshProUGUI m_ScoreText; //Store score
    public TMPro.TextMeshProUGUI m_RewardText; //Reward points unblocks different turrets
    public TMPro.TextMeshProUGUI m_SpentText; //Reward points unblocks different turrets
    public Button m_TurretButton01; // Button to choose torret 1.
    public Button m_TurretButton02; // Button to choose torret 2.
    public Button m_TurretButton03; // Button to choose torret 3.
    public Button m_TurretButton04; // Button to choose torret 4.
    public TMPro.TextMeshProUGUI m_LevelUp; 
    public TMPro.TextMeshProUGUI m_waveElements;
    public TMPro.TextMeshProUGUI m_currentWave02;

    [Header("DEBUG")]
    public bool debug = false;

    private void Awake()
    {
        // Get all waypoints from the waypoints parent.
        // Waypoints are the trace, route or track that the enemnies follow
        // from the respawn moment to the end.
        // waypoints are custom placed in the scene.
        // Count and assign waypoints to the waypoints list
        int i = 0;
        int x = m_WayPointsParent.transform.childCount;
        m_WaypointsList = new Transform[x];

        foreach (Transform wp in m_WayPointsParent)
        {
            m_WaypointsList[i] = wp;
            i++;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        level = 0;
        m_TowerHealthBar.MaxHealth(m_MainTowerLife); //Health bar at maximum
        m_MainTowerCurrentLife = m_MainTowerLife; //Set current bar value at maximum
        if(m_WavesQuantity > 0) StartCoroutine("ActivateNextWave");
        UpdateScoreAndReward(0, 0); 
    }

    private void Update()
    {
        //Enemy waves will respawn from 1 to any custom waves quantity for each level.
        //m_newWave controls next wave respawn. If m_newWave is true and the waves quantity isnt reached yet
        //this statemant will call to the EnemyWaves courutine to instatiate next wave.
        //m_currentWave will be updated till is less or equal to the waves quantity.
        //Is important to update to false the newWave bool, to exit the statement and stop instantiating enemies
        //until the time to the next wave is accomplished.
        if (m_newWave && (m_currentWave < m_WavesQuantity) && m_WavesQuantity != 0)
        {
            Debug.Log(m_currentWave);
            //Couroutines are used to yield functions.
            //EnemyWaves instantiate and control the enemies instances
            StartCoroutine("EnemyWaves");
            m_waveElements.text = "Enemy: " + (m_currentWave+1).ToString() + "/" + m_WavesQuantity.ToString();
            m_currentWave++;
            m_newWave = false; 
        }  

        //If left mouse button is clicked, verify if Raycast ray collide with a game object with "WeaponMat" tag.
        //Verify if raycast2d dont detect any turret placed before instantiate
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider == null)
            {
                RaycastHit hit2;
                Ray ray2 = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray2, out hit2))
                {
                    //if raycast hit && do not hit a player tag (it means it isn't a turret placed yet)
                    if (hit2.collider != null)
                    {
                        if (hit2.collider.gameObject.tag == "WeaponMat")
                        {
                            if (m_Reward >= m_Turret_Reward[m_turretSelection])
                            {
                                //Instance the turret if the spot is free 
                                GameObject clone = Instantiate(m_TurretGO[m_turretSelection], hit2.collider.transform);
                                m_spent += m_Turret_Reward[m_turretSelection];
                                m_Reward -= m_Turret_Reward[m_turretSelection];
                                UpdateScoreAndReward(0, 0);
                                if (debug) Debug.Log("WeaponMat");
                            }
                        }  
                    }
                }
            }
        }

        if (m_Reward < 0) m_Reward = 0; //m_reward cannot be less than 0

        //Right mouse click to erase turret
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null)
            {
                if (hit.collider.gameObject.tag == "Player")
                {
                    if (debug) Debug.Log("Player hit");
                    m_Reward += m_Turret_Reward[m_turretSelection];
                    Destroy(hit.collider.gameObject);
                }
            }
        }

        if(m_currentWave == m_WavesQuantity)
        {
            if(m_EnemySpawnedList.Count < 1)
            {
                if(m_MainTowerCurrentLife > 0) StartCoroutine("ActivateNextWave");
            }  
        }

        if (m_MainTowerCurrentLife <= 0)
        {
            SceneManager.LoadScene("FrontWaveMenu");
        }

    }

    private void LateUpdate()
    { 
        //Some turrets will be available, depending on the current money earned.
        //Butons activate and deactivate depending on the money costs of each turret.
        if (m_Reward < m_Turret_Reward[0])
        {
            
            m_TurretButton01.interactable = m_TurretButton02.interactable = m_TurretButton03.interactable = m_TurretButton04.interactable = false;
        }
        else if (m_Reward >= m_Turret_Reward[0] && m_Reward < m_Turret_Reward[1])
        {
            m_TurretButton01.interactable = true;
            m_TurretButton02.interactable = m_TurretButton03.interactable = m_TurretButton04.interactable = false;
        }
        else if (m_Reward >= m_Turret_Reward[1] && m_Reward < m_Turret_Reward[2])
        {
            m_TurretButton01.interactable = m_TurretButton02.interactable = true;
            m_TurretButton03.interactable = m_TurretButton04.interactable = false;
        }
        else if (m_Reward >= m_Turret_Reward[2] && m_Reward < m_Turret_Reward[3])
        {
            m_TurretButton01.interactable = m_TurretButton02.interactable = m_TurretButton03.interactable = true;
            m_TurretButton04.interactable = false;
        }
        else if (m_Reward >= m_Turret_Reward[3])
        {
            m_TurretButton01.interactable = m_TurretButton02.interactable = m_TurretButton03.interactable = m_TurretButton04.interactable = true;
        }
    }

    IEnumerator EnemyWaves()
    {
        //e is a random integer-forced variable to instantiate
        //a random enemie from the enemies list available in the GameManager.
        int e = Random.Range(0, m_EnemyGO.Length);

        //clone is a GameObject copy from a random enemy game object from the enemies stack.
        GameObject clone = Instantiate(m_EnemyGO[e]);

        //Add new enemy to list
        m_EnemySpawnedList.Add(clone.transform);

        //The floating point r is a variable that takes a random number between the minimum wave time and the maximum wave time
        //to yield the next wave
        float r = Random.Range(m_minWaveTime, m_maxWaveTime);
        yield return new WaitForSeconds(r);

        //The m_newWave variable bool update to true, let this courutine available to be accesed again.
        m_newWave = true;

        if (debug) Debug.Log("New Wave");
    }

    public void EnemyHitTower()
    {
        m_TowerHealthBar.HealtH(m_MainTowerCurrentLife); //Update health bar
    }

    public void UpdateScoreAndReward(float score, float reward)
    {
        m_Score += score; //Update score
        m_Reward += reward; //Update reward to unblock turrets
        m_ScoreText.text = "SCORE: " + m_Score.ToString(); //Update score graphics
        m_RewardText.text = "CURRENT $: " + m_Reward.ToString(); //Update reward graphics
        m_SpentText.text = "$ SPENT: " + m_spent.ToString();
    }

    //Buttons change the turret selection for the next instance invokation
    public void TurretSelector(int s)
    {
        m_turretSelection = s;
    }

    IEnumerator ActivateNextWave()
    {
        level += 1;
        m_WavesQuantity += 5;
        m_currentWave = 0;
        m_currentWave02.text = m_LevelUp.text = "LEVEL " + level;
        m_LevelUp.gameObject.SetActive(true);
        yield return new WaitForSeconds(3.0f);
        m_LevelUp.gameObject.SetActive(false); 
        m_newWave = true;
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("FrontWaveGameplay");
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("FrontWaveMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }


}