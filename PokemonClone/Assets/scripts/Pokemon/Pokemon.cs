﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pokemon
{
    public PokemonBase basePokemon { get; set; }            //called properties, it allows for us to grab the data from other scripts
    public int level { get; set; } 
    public int Hp { get; set; }
    public List<Move> moves { get; set; }

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        basePokemon = pBase;
        level = pLevel;
        Hp = MaxHp;

        moves = new List<Move>();
        foreach(var move in basePokemon.LearnableMoves)     //loops through each move and adds the learnable ones
        {
            if(move.Level <= level)
            {
                moves.Add(new Move(move.Base));
                if(moves.Count >= 4)
                {
                    break;
                }
            }
        }
    }
    public int MaxHp
    {
        get { return Mathf.FloorToInt((basePokemon.Attack * level) / 100f) + 10; }
    }                                       //generates the stat values
    public int Attack
    {
        get { return Mathf.FloorToInt((basePokemon.Attack * level) / 100f) + 5; }
    }
    public int Defense
    {
        get { return Mathf.FloorToInt((basePokemon.Attack * level) / 100f) + 5; }
    }
    public int SpAttack
    {
        get { return Mathf.FloorToInt((basePokemon.Attack * level) / 100f) + 5; }
    }
    public int SpDefense
    {
        get { return Mathf.FloorToInt((basePokemon.Attack * level) / 100f) + 5; }
    }
    public int Speed
    {
        get { return Mathf.FloorToInt((basePokemon.Attack * level) / 100f) + 5; }
    }
}
