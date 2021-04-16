﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;
    private void Start()
    {
        foreach(var pokemon in pokemons)
        {
            pokemon.Init();
        }
    }
    public Pokemon GetHealthyPokemon()
    {
        return pokemons.Where(x => x.Hp > 0).FirstOrDefault();  //returns first pokemon in the party that is not fainted
    }
}