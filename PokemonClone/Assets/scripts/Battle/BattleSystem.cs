using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    Start,
    PlayerAction,
    PlayerMove,
    EnemyMove,
    Busy
}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHUD playerHUD;
    [SerializeField] BattleHUD enemyHUD;
    [SerializeField] BattleDialogueBox dialogBox;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;  // if 0-fight, if 1-run
    int currentMove;

    public void StartBattle()
    {
        StartCoroutine( SetUpBattle());
    }
    public IEnumerator SetUpBattle()
    {
        playerUnit.Setup();
        enemyUnit.Setup();
        playerHUD.SetData(playerUnit.pokemon);
        enemyHUD.SetData(enemyUnit.pokemon);

        dialogBox.SetMoveNames(playerUnit.pokemon.moves);

        yield return dialogBox.TypeDialog($"A wild {enemyUnit.pokemon.basePokemon.name} appeared!");           //waits for this coroutine to finish instead of a set time
        

        PlayerAction();
    }
    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogBox.TypeDialog("choose an action"));
        dialogBox.EnableActionSelector(true);
    }

    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }
    IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;
        var move = playerUnit.pokemon.moves[currentMove];
        yield return dialogBox.TypeDialog($"{playerUnit.pokemon.basePokemon.name} used {move.Base.name}");

        var damageDetails = enemyUnit.pokemon.TakeDamage(move, playerUnit.pokemon);
        yield return enemyHUD.UpdateHP();
        yield return ShowDamageDetails(damageDetails);
        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.pokemon.basePokemon.name} Fainted!");
            
            yield return new WaitForSeconds(2f);
            OnBattleOver(true);
        }
        else
        {
            StartCoroutine(EnemyMove());
        }

    }
    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;
        var move = enemyUnit.pokemon.SelectEnemyMove();
        yield return dialogBox.TypeDialog($"{enemyUnit.pokemon.basePokemon.name} used {move.Base.name}");

        var damageDetails = playerUnit.pokemon.TakeDamage(move, enemyUnit.pokemon);
        yield return playerHUD.UpdateHP();
        yield return ShowDamageDetails(damageDetails);
        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.pokemon.basePokemon.name} Fainted!");
            yield return new WaitForSeconds(2f);
            OnBattleOver(false);
        }
        else
        {
            PlayerAction();
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if(damageDetails.Critical > 1f)
        {
            yield return dialogBox.TypeDialog("A critical hit!");
        }
        if (damageDetails.Type > 1f)
        {
            yield return dialogBox.TypeDialog("It's super effective!");
        }
        else if (damageDetails.Type < 1f)
        {
            yield return dialogBox.TypeDialog("It's not very effective...");
        }
    }

    public void HandleUpdate()
    {
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelector();
        }
        else if(state == BattleState.PlayerMove)
        {
            HandleMoveSelection();
        }
    }
    void HandleActionSelector()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 1)
            {
                ++currentAction;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if(currentAction > 0)
            {
                --currentAction;
            }
        }
        dialogBox.UpdateActionSelection(currentAction);
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if(currentAction == 0)
            {
                //fight
                PlayerMove();
            }
            else if(currentAction == 1)
            {
                //run
            }
        }
    }
    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove < playerUnit.pokemon.moves.Count-1)
            {
                ++currentMove;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove > 0)
            {
                --currentMove;
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove < playerUnit.pokemon.moves.Count-2)
            {
                currentMove+=2;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove > 1)
            {
                currentMove-=2;
            }
        }
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.pokemon.moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }
    }

}
