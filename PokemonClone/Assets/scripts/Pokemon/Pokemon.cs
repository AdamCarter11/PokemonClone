using System.Collections;
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
    public bool TakeDamage(Move move, Pokemon attacker)
    {
        float modifiers = Random.Range(0.85f, 1f);
        float a = (2 * attacker.level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attacker.Attack / Defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        Hp -= damage;
        if(Hp <= 0)
        {
            Hp = 0;
            return true;
        }
        return false;
    }
    public Move SelectEnemyMove()
    {
        int r = Random.Range(0, moves.Count);
        return moves[r];
    }
}
