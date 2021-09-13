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
    public Dictionary<Stat,int> Stats{get; private set;}
    public Dictionary<Stat,int> StatBoosts{get; private set;} //Stat is the key, int represents the value
    //in the case of the StatBoosts dictionary, the int can be between -6 to +6 (incriments of .5)
    public Queue<string> StatusChanges{ get; private set;} = new Queue<string>();
    public void Init()
    {
        //basePokemon = pBase;
        //level = pLevel;

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
        CalculateStats();
        Hp = MaxHp;
        ResetStatBoosts();
    }

    void CalculateStats(){
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((basePokemon.Attack * level) / 100f) + 10);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((basePokemon.Defense * level) / 100f) + 10);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((basePokemon.SpAttack * level) / 100f) + 10);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((basePokemon.SpDefense * level) / 100f) + 10);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((basePokemon.Speed * level) / 100f) + 10);

        MaxHp = Mathf.FloorToInt((basePokemon.Attack * level) / 100f) + 10;
    }

    void ResetStatBoosts(){
         StatBoosts = new Dictionary<Stat, int>(){
            {Stat.Attack, 0},
            {Stat.SpAttack, 0},
            {Stat.Defense, 0},
            {Stat.SpDefense, 0},
            {Stat.Speed, 0}
        };
    }

    int GetStat(Stat stat){
        int statVal = Stats[stat];

        //Stat boosts, etc.
        int boost = StatBoosts[stat];
        var boostValues = new float[]{1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f};

        if(boost >= 0){
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        }
        else{
            statVal = Mathf.FloorToInt(statVal/boostValues[-boost]);
        }

        return statVal;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts){
        foreach(var statBoost in statBoosts){
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost,-6,6);

            if(boost > 0){
                StatusChanges.Enqueue($"{basePokemon.name}'s {stat} rose!");
            }else{
                StatusChanges.Enqueue($"{basePokemon.name}'s {stat} fell!");
            }

            //Debug.Log($"{stat} has been boosted to {StatBoosts[stat]}");
        }
    }

    public int MaxHp
    {
        get; private set;
    }                                       //generates the stat values
    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }
    public int Defense
    {
        get { return GetStat(Stat.Defense); }
    }
    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }
    public int SpDefense
    {
        get { return GetStat(Stat.SpDefense); }
    }
    public int Speed
    {
        get { return GetStat(Stat.Speed); }
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

        float attack = (move.Base.Catagory == MoveCatagory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Catagory == MoveCatagory.Special) ? SpDefense : Defense;

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
    public void OnBattleOver(){
        ResetStatBoosts();
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float Type { get; set; }
}
