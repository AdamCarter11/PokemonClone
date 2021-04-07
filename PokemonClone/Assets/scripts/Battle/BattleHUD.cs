using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;

    public void SetData(Pokemon pokemon)                    //what makes the HUD display the proper data
    {
        nameText.text = pokemon.basePokemon.name;
        levelText.text = "Lvl: " + pokemon.level;
        hpBar.SetHP((float)pokemon.Hp/pokemon.MaxHp);
    }
}
