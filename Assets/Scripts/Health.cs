using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    Animator anim;

    [SerializeField] public float maxHealth;
    public float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
        anim = gameObject.GetComponent<Animator>();
        anim.SetFloat("Health", currentHealth);
    }
    private void Update()
    {
        if(currentHealth == 0)
        {
            dead();
        }
    }

    private void ResetHealth()
    {
        currentHealth = maxHealth;
    }

    public void hit()
    {
        currentHealth -= 1;

        anim.SetTrigger("HitTrigger");
        anim.SetFloat("Health", currentHealth);

    }

    public void dead()
    {

    }



}
