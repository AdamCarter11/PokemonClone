using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BattleUnit : MonoBehaviour
{
    //[SerializeField] PokemonBase pokemonBase;
    //[SerializeField] int level;
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHUD hud;

    //so we can access this data in other functions
    public bool IsPlayerUnit
    {
        get { return isPlayerUnit; }
    }
    public BattleHUD Hud
    {
        get { return hud; }
    }
    public Pokemon pokemon { get; set; }

    public void Setup(Pokemon pokemonUnit)                                                           //the function that generates a pokemon and selects which sprite (based on player or enemy)
    {
       pokemon = pokemonUnit;
        if (isPlayerUnit)
        {
            GetComponent<Image>().sprite = pokemon.basePokemon.BackSprite;
        }
        else
        {
            GetComponent<Image>().sprite = pokemon.basePokemon.FrontSprite;
        }
        hud.SetData(pokemon);
    }
}
