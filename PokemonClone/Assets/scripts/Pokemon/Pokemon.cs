using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;
    public PokemonBase basePokemon
    {
        get { return _base; }
    }
    public int Level
    {
        get { return level; }
    }
    //public PokemonBase basePokemon { get; set; }            //called properties, it allows for us to grab the data from other scripts
    //public int level { get; set; } 
    public int Hp { get; set; }
    public List<Move> moves { get; set; }

    public void Init()
    {
        //basePokemon = pBase;
        //level = pLevel;
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
    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        float critical = 1f;
        if(Random.value * 100f <= 6.25f)
        {
            critical = 1.5f;
        }
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.basePokemon.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.basePokemon.Type2);

        var damageDetails = new DamageDetails()
        {
            Type = type,
            Critical = critical,
            Fainted = false
        };

        float attack = (move.Base.IsSpecial) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.IsSpecial) ? SpDefense : Defense;

        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        Hp -= damage;
        if(Hp <= 0)
        {
            Hp = 0;
            damageDetails.Fainted = true;
        }
        return damageDetails;
    }
    public Move SelectEnemyMove()
    {
        int r = Random.Range(0, moves.Count);
        return moves[r];
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float Type { get; set; }
}
