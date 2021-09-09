using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    Pokemon _pokemon;

    [SerializeField] Color highlightedColor;

    public void SetData(Pokemon pokemon)                    //what makes the HUD display the proper data
    {
        _pokemon = pokemon;
        nameText.text = pokemon.basePokemon.name;
        levelText.text = "Lvl: " + pokemon.Level;
        hpBar.SetHP((float)pokemon.Hp / pokemon.MaxHp);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            nameText.color = highlightedColor;
        }
        else
        {
            nameText.color = Color.black;        
        }
    }
}
