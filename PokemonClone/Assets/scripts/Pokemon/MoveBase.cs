using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string name;
    [TextArea]
    [SerializeField] string description;
    [SerializeField] PokemonType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int pp;
    [SerializeField] MoveCatagory catagory;
    [SerializeField] MoveEffects effects;
    [SerializeField] MoveTarget target;
    //[SerializeField] bool isSpecial;

    

    public string Name
    {
        get { return name; }
    }
    public string Description
    {
        get { return description; }
    }
    public PokemonType Type
    {
        get { return type; }
    }
    public int Power
    {
        get { return power; }
    }
    public int Accuracy
    {
        get { return accuracy; }
    }
    public int Pp
    {
        get { return pp; }
    }

    public MoveCatagory Catagory{
        get {return catagory; }
    }
    public MoveEffects Effects{
        get {return effects;}
    }
    public MoveTarget Target{
        get { return target;}
    }
}
[System.Serializable]
public class MoveEffects{
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;
    public List<StatBoost> Boosts{
        get { return boosts; }
    }
    public ConditionID Status{
        get{ return status; }
    }
}
[System.Serializable]
public class StatBoost{
    public Stat stat;
    public int boost;
}
public enum MoveCatagory{
    Physical, Special, Status
}
public enum MoveTarget{
    Foe, Self
}