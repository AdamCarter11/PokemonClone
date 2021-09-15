using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//used to hold the different states the game can be in
public enum BattleState
{
    Start,
    ActionSelection,
    MoveSelection,
    PerformMove,
    Busy,
    PartyScreen,
    BattleOver
}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogueBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;  // if 0-fight, if 1-run
    int currentMove;
    int currentMember;

    PokemonParty playerParty;
    Pokemon wildPokemon;

    //sets the variables and triggers the SetUpBattle function
    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine( SetUpBattle());
    }

    //gets the first healthy pokemon in the players party, gets a random enemy pokemon for that area
    //initilizes the party screen and displays the players moves
    public IEnumerator SetUpBattle()
    {
        playerUnit.Setup(playerParty.GetHealthyPokemon());
        enemyUnit.Setup(wildPokemon);

        partyScreen.Init();
            
        dialogBox.SetMoveNames(playerUnit.pokemon.moves);

        yield return dialogBox.TypeDialog($"A wild {enemyUnit.pokemon.basePokemon.name} appeared!");           //waits for this coroutine to finish instead of a set time
        

        ChooseFirstTurn();
    }

    void ChooseFirstTurn(){
        if(playerUnit.pokemon.Speed >= enemyUnit.pokemon.Speed){
            ActionSelection();
        }
        else{
            StartCoroutine(EnemyMove());
        }
    }
    //triggers battle over state depending on who has won
    //needed so that the player/enemy move functions don't run coroutines if battle is over
    void BattleOver(bool outcome)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p=>p.OnBattleOver());
        OnBattleOver(outcome);
    }

    //displays message and then triggers the actionselector
    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("choose an action");
        dialogBox.EnableActionSelector(true);
    }

    //used to display players party screen to swap pokemon during battle
    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    //displays moves and lets you chose one
    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    //most of the code is done in the RuneMove function
    //saves which move the user selected and passes it to RunMove
    //checks if you can keep playing (player isn't out of pokemon)
    IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove;
        var move = playerUnit.pokemon.moves[currentMove];
        yield return RunMove(playerUnit, enemyUnit, move);

        //why we need the battle over function
        if(state == BattleState.PerformMove)
        {
            StartCoroutine(EnemyMove());
        }
    }

    //selects an enemy move and passes it to RunMove (like PlayerMove function)
    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;
        var move = enemyUnit.pokemon.SelectEnemyMove();
        yield return RunMove(enemyUnit, playerUnit, move);
        if (state == BattleState.PerformMove)
        {
            ActionSelection();
        }
    }

    //lowers that moves pp and displays what move is being used
    //updates the targets health and UI
    //checks if the target pokemon has fainted
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        move.Pp--;
        yield return dialogBox.TypeDialog($"{sourceUnit.pokemon.basePokemon.name} used {move.Base.name}");
        //attack animation: sourceUnit.PlayAttackAnimation();

        if(move.Base.Catagory == MoveCatagory.Status){
            yield return RunMoveEffects(move, sourceUnit.pokemon, targetUnit.pokemon);
        }
        else{
            var damageDetails = targetUnit.pokemon.TakeDamage(move, sourceUnit.pokemon);
            yield return targetUnit.Hud.UpdateHP();
            yield return ShowDamageDetails(damageDetails);
        }
        
        if (targetUnit.pokemon.Hp <= 0)
        {
            yield return dialogBox.TypeDialog($"{targetUnit.pokemon.basePokemon.name} Fainted!");

            yield return new WaitForSeconds(2f);


            CheckForBattleOver(targetUnit);
        }

        //used for status conditions (damage after the turn)
        sourceUnit.pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.pokemon);
        yield return sourceUnit.Hud.UpdateHP();
        if (sourceUnit.pokemon.Hp <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.pokemon.basePokemon.name} Fainted!");

            yield return new WaitForSeconds(2f);


            CheckForBattleOver(sourceUnit);
        }
    }
    IEnumerator RunMoveEffects(Move move, Pokemon source, Pokemon target){
        var effects = move.Base.Effects;
        if(effects.Boosts != null){
            if(move.Base.Target == MoveTarget.Self){
                source.ApplyBoosts(effects.Boosts);
            }
            else{
                target.ApplyBoosts(effects.Boosts);
            }
        }
        if(effects.Status != ConditionID.none){
            //move causes a status condition
            target.SetStatus(effects.Status);
        }
        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }
    IEnumerator ShowStatusChanges(Pokemon pokemon){
        while(pokemon.StatusChanges.Count > 0){
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }
    //Checks to see if the player pokemon fainted and is also out of healthy pokemon
    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                OpenPartyScreen();
            }
            else
            {
                BattleOver(false);  //player is out of pokemon
            }
        }
        else
        {
            BattleOver(true);
        }
    }

    //Displays crits and type effectivness 
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
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelector();
        }
        else if(state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
    }

    //the first chunk of if/ifelse/else statements are used for letting the player select an action
    //we also clamp the action so it can't go out of bounds
    //based on what is selected, we then open the correct UI screen
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
                MoveSelection();
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

    //almost the same as above, but for picking moves
    //we also have the option of backing out with the key: X
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
        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.pokemon.moves.Count-1); //clamps current action between 0 and 3 (4 moves)
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.pokemon.moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    //similar as the above 2 functions, 
    //but now we also are checking to see if the player has selected a valid pokemon 
    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMember += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMember -= 2;
        }
        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count-1); //clamps current action between 0 and 3 (4 moves)

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMember = playerParty.Pokemons[currentMember];
            if (selectedMember.Hp <= 0)
            {
                partyScreen.SetMessageText("This pokemon has already fainted!");
                return;
            }
            if(selectedMember == playerUnit.pokemon)
            {
                partyScreen.SetMessageText("This pokemon is already out!");
                return;
            }
            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
            StartCoroutine(SwitchPokemon(selectedMember));
        }
        else if(Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }

    //function to control when we swap pokemon
    //check to see if we still have HP and if so, call the pokemon back
    //then we setup the next pokemon and swap it in
    //we end the function by calling the EnemyMove coroutine (as swapping uses the players move)
    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        bool currentPokemonFainted = true;
        if (playerUnit.pokemon.Hp > 0)
        {
            currentPokemonFainted = false;
            yield return dialogBox.TypeDialog($"Return {playerUnit.pokemon.basePokemon.name}");
            //playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        //swapping pokemon
        playerUnit.Setup(newPokemon);
        dialogBox.SetMoveNames(newPokemon.moves);
        yield return dialogBox.TypeDialog($"Go: {newPokemon.basePokemon.name} !");

        if(currentPokemonFainted){
            ChooseFirstTurn();
        }else{
            StartCoroutine(EnemyMove());
        }
    }
}
