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
    public IEnumerator SetHpSmooth(float newHp)
    {
        float currHp = health.transform.localScale.x;
        float changeAmount = currHp - newHp;
        while(currHp-newHp > Mathf.Epsilon)
        {
            currHp -= changeAmount * Time.deltaTime;
            health.transform.localScale = new Vector3(currHp, 1f);
            yield return null;
        }
        health.transform.localScale = new Vector3(newHp, 1f);
    }
}
