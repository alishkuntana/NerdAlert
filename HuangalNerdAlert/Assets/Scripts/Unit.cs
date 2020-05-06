using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public bool selected;

    GameMaster gm;

    public int tileSpeed;

    public bool hasMoved;

    public float moveSpeed;

    public int playerNumber;

    public int attackRange;

    List<Unit> enemiesInRange = new List<Unit>();
    public bool hasAttacked;

    public GameObject weaponIcon;

    public int health;

    public int attackDamage;

    public int defenseDamage;

    public int armor;

    public DamageIcon damageIcon;

    public GameObject deathEffect;

    private Animator camAnim;

    private void Start()
    {
        gm = FindObjectOfType<GameMaster>();
        camAnim = Camera.main.GetComponent<Animator>();
    }

    private void OnMouseDown()
    {
        ResetWeaponIcons();

        if (selected == true)
        {
            selected = false;
            gm.selectedUnit = null;
            gm.ResetTiles();
        } else
        {
            if (playerNumber == gm.playerTurn)
            {
                 if (gm.selectedUnit != null)
                {
                    gm.selectedUnit.selected = false;
                }
                gm.ResetTiles();

                gm.selectedUnit = this;
                selected = true;
                

                GetEnemies();
                GetWalkableTiles();
            }

           
        }

        Collider2D col = Physics2D.OverlapCircle(Camera.main.ScreenToWorldPoint(Input.mousePosition), 0.15f);
        if (col != null)
        {
            Unit unit = col.GetComponent<Unit>(); // double check that what we clicked on is a unit
            if (unit != null && gm.selectedUnit != null)
            {
                if (gm.selectedUnit.enemiesInRange.Contains(unit) && !gm.selectedUnit.hasAttacked)
                { // does the currently selected unit have in his list the enemy we just clicked on
                    gm.selectedUnit.Attack(unit);

                }
            }
        }

    }

    void Attack(Unit enemy)
    {
        camAnim.SetTrigger("shake");
        
        hasAttacked = true;

        int enemyDamage = attackDamage - enemy.armor;
        int myDamage = enemy.defenseDamage - armor;

        if (enemyDamage >= 1)
        {
            DamageIcon instance = Instantiate(damageIcon, enemy.transform.position, Quaternion.identity);
            instance.Setup(enemyDamage);
            enemy.health -= enemyDamage;
        }
        if (myDamage >= 1)
        {
            health -= myDamage;
        }

        if (enemy.health <= 0)
        {
            Instantiate(deathEffect, enemy.transform.position, Quaternion.identity);
            Destroy(enemy.gameObject);
            GetWalkableTiles();
        }

        if (health <= 0)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
            gm.ResetTiles();
            Destroy(this.gameObject);
        }

    }

    void GetWalkableTiles()
    {
        if (hasMoved == true)
        {
            return;
        }

        foreach (Tile tile in FindObjectsOfType<Tile>())
        {
            if (Mathf.Abs(transform.position.x - tile.transform.position.x) + Mathf.Abs(transform.position.y - tile.transform.position.y) <= tileSpeed)
            {
                if (tile.IsClear() == true)
                {
                    tile.Highlight();
                }
            }
            
        }

    }

    void GetEnemies()
    {
        enemiesInRange.Clear();

        foreach (Unit unit in FindObjectsOfType<Unit>())
        {
            if (Mathf.Abs(transform.position.x - unit.transform.position.x) + Mathf.Abs(transform.position.y - unit.transform.position.y) <= attackRange)
            {
               if (unit.playerNumber != gm.playerTurn && hasAttacked == false)
               {
                   enemiesInRange.Add(unit);
                   unit.weaponIcon.SetActive(true);
               }
            }
            
        }
    }

    public void ResetWeaponIcons()
    {
        Unit[] enemies = FindObjectsOfType<Unit>();
        foreach (Unit enemy in enemies)
        {
            enemy.weaponIcon.SetActive(false);
        }
    }

    public void Move(Vector2 tilePos)
    {
        gm.ResetTiles();
        StartCoroutine(StartMovement(tilePos));
    }

    IEnumerator StartMovement(Vector2 destination)
    {
        Vector2 position;
        while((Vector2)transform.position != destination)
        {
            position = Vector2.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            transform.position = new Vector3(position.x, position.y, transform.position.z);
            yield return null;
        }
        hasMoved = true;
        ResetWeaponIcons();
        GetEnemies();
    }
}
