using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;

    public void SetHP(float hpNormalized)                               //sets the hp bar proportinal to the maxHP
    {
        health.transform.localScale = new Vector3(hpNormalized, 1f);
    }
}
