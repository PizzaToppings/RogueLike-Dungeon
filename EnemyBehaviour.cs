using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBehaviour : MonoBehaviour {

    public int maxHP;                           // Max health of the enemy

    public GameObject bleedParticles;           // Bleeding particles, if they're hit

    [SerializeField]
    private GameObject analyticsGO;             // Used for storing data/ balancing

    public GameObject healthSliderPrefab;       // the healthsliderprefab, for spawning a healthbar in the UI
    private Slider healthSlider;                // the healthslider, showing the health of the enemy

    // Use this for initialization
    void Start()
    {
        GameObject healthSliderGO = Instantiate(healthSliderPrefab, Camera.main.transform.GetChild(1).transform);
        healthSlider = healthSliderGO.GetComponent<Slider>();
        healthSlider.maxValue = maxHP;
        healthSlider.value = maxHP;
    }

    // Update is called once per frame
    void Update()
    {
        healthSlider.gameObject.transform.position = transform.position + new Vector3(0, 0.75f, 0);

        if (healthSlider.value <= 0)
        {
            analyticsGO.GetComponent<AnalyticsManager>().enemiesKilled += 1;
            Destroy(gameObject);
        }

    }

    // when the enemy dies, it will also remove the healthslider
    private void OnDestroy()
    {
        Destroy(healthSlider.gameObject);
    }

    // take damage if the enemy is hit by a weapon. Enemy will also be knocked back
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "WarriorSword")
        {
            healthSlider.value -= 10;

            Instantiate(bleedParticles, gameObject.transform);

            KnockBack(other.gameObject, 6f);
        }
            
        if (other.tag == "PaladinSword")
        {
            healthSlider.value -= 5;

            Instantiate(bleedParticles, gameObject.transform);

            KnockBack(other.gameObject, 4f);
        }
            
        if (other.tag == "Arrow")
        {
            healthSlider.value -= 10;

            Instantiate(bleedParticles, gameObject.transform);

            Destroy(other.gameObject);

            KnockBack(other.gameObject, 2f);
        }

        if (other.tag == "Explosion")
        {
            healthSlider.value -= 5;

            Instantiate(bleedParticles, gameObject.transform);
        }

        if (healthSlider.value < maxHP)
            healthSlider.gameObject.SetActive(true);
    }

    // kock the enemy back
    private void KnockBack(GameObject playerWeapon, float magnitude)
    {
        Vector3 knockbackDir = (transform.position - playerWeapon.transform.position).normalized;

        transform.Translate(knockbackDir);
    }
}
