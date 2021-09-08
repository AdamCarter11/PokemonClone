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
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;  // if 0-fight, if 1-run
    int currentMove;

    PokemonParty playerParty;
    Pokemon wildPokemon;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine( SetUpBattle());
    }
    public IEnumerator SetUpBattle()
    {
        playerUnit.Setup(playerParty.GetHealthyPokemon());
        enemyUnit.Setup(wildPokemon);
        playerHUD.SetData(playerUnit.pokemon);
        enemyHUD.SetData(enemyUnit.pokemon);

        partyScreen.Init();
            
        dialogBox.SetMoveNames(playerUnit.pokemon.moves);

        yield return dialogBox.TypeDialog($"A wild {enemyUnit.pokemon.basePokemon.name} appeared!");           //waits for this coroutine to finish instead of a set time
        

        PlayerAction();
    }
    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        dialogBox.SetDialog("choose an action");
        dialogBox.EnableActionSelector(true);
    }
    void OpenPartyScreen()
    {
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
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
        move.Pp--;
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
        move.Pp--;
        yield return dialogBox.TypeDialog($"{enemyUnit.pokemon.basePokemon.name} used {move.Base.name}");

        var damageDetails = playerUnit.pokemon.TakeDamage(move, enemyUnit.pokemon);
        yield return playerHUD.UpdateHP();
        yield return ShowDamageDetails(damageDetails);
        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.pokemon.basePokemon.name} Fainted!");
            yield return new WaitForSeconds(2f);
            var nextPokemon = playerParty.GetHealthyPokemon();
            if(nextPokemon != null)
            {
                //will need to reset which move is selected for the player in case the enemy attacked first and defeated the current player pokemon
                playerUnit.Setup(nextPokemon);
                playerHUD.SetData(nextPokemon);

                dialogBox.SetMoveNames(nextPokemon.moves);

                yield return dialogBox.TypeDialog($"Go: {nextPokemon.basePokemon.name} !");           //waits for this coroutine to finish instead of a set time

                PlayerAction();
            }
            else
            {
                OnBattleOver(false);
            }
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
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentAction += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentAction -= 2;
        }
        currentAction = Mathf.Clamp(currentAction, 0, 3); //clamps current action between 0 and 3 (4 moves)
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
                //bag
            }
            else if (currentAction == 2)
            {
                //pokemon select
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                //run
            }
        }
    }
    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMove -= 2;
        }
        currentMove = Mathf.Clamp(currentMove, 0, 3); //clamps current action between 0 and 3 (4 moves)
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.pokemon.moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            PlayerAction();
        }
    }

}
