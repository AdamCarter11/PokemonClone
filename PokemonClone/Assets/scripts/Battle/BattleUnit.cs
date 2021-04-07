using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BattleUnit : MonoBehaviour
{
    [SerializeField] PokemonBase pokemonBase;
    [SerializeField] int level;
    [SerializeField] bool isPlayerUnit;
    public Pokemon pokemon { get; set; }

    public void Setup()                                                           //the function that generates a pokemon and selects which sprite (based on player or enemy)
    {
       pokemon = new Pokemon(pokemonBase, level);
        if (isPlayerUnit)
        {
            GetComponent<Image>().sprite = pokemon.basePokemon.BackSprite;
        }
        else
        {
            GetComponent<Image>().sprite = pokemon.basePokemon.FrontSprite;
        }
    }
}
