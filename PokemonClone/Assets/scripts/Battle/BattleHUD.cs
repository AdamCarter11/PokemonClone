using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    Pokemon _pokemon;

    public void SetData(Pokemon pokemon)                    //what makes the HUD display the proper data
    {
        _pokemon = pokemon;
        nameText.text = pokemon.basePokemon.name;
        levelText.text = "Lvl: " + pokemon.Level;
        hpBar.SetHP((float)pokemon.Hp/pokemon.MaxHp);
    }
    public IEnumerator UpdateHP()
    {
        if(_pokemon.HpChanged){
            yield return hpBar.SetHpSmooth((float)_pokemon.Hp / _pokemon.MaxHp);
            _pokemon.HpChanged = false;
        }
    }
}
