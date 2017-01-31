using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
	Extension of GameMode which puts the players into teams
	and keeps track of special team based information.
*/
public class TeamBased : GameMode {

	protected Dictionary<string, int> teamScores;
	protected string winningTeam;
	protected Team[] teams;
	protected int[] teamNumbers;

	protected override void init(){
		bIsTeamBased = true;
		bCanRespawn = true;
		teams = new Team[]{new Team(Team.TeamColor.BLUE), new Team(Team.TeamColor.RED)};
	}

	public override void createTeams(){
		teamNumbers = new int[teams.Length];
		foreach(Player p in gameControl.players)
			networkView.RPC("Client_SetPlayerTeam", RPCMode.All, p.networkPlayer, requestTeam().asInt());
	}

	public Team requestTeam(){
		int rand = Random.Range(0, 2);
		if(teamNumbers[rand] < gameControl.numberOfPlayers()/2){
			teamNumbers[rand]++;
			return teams[rand];
		} else {
			int opRand = (rand == 0) ? 1 : 0; //((0 xor rand) xor 1)
			teamNumbers[opRand]++;
			return teams[opRand];
		}
		//return (rand == 0) ? ((teamNumbers[rand] < gameControl.numberOfPlayers()/2) ? blue : red) : (redTeam.Count < gameControl.numberOfPlayers()/2) ? red : blue; 
	}

	protected void updateTeamScores(Team t){
		teamScores[t.ToString()]++;
		networkView.RPC("syncTeamScores", RPCMode.Others, teamScores["RED"], teamScores["BLUE"]);
		if (teamScores[t.ToString()] >= scoreToWin) {
			winningTeam = t.ToString();
			bCanRespawn = false;
			gameControl.roundOver();
			gameControl.gameFinished();
		}
	}

	public override void drawUI(){
		GUI.Label (new Rect(Screen.width/2 - 100, 0, 200, 25), "Team Scores");
		GUI.Label (new Rect(Screen.width/2 - 100, 30, 200, 25), "RED | BLUE");
		GUI.Label (new Rect(Screen.width/2 - 100, 60, 200, 25), teamScores["RED"] + " | " + teamScores["BLUE"]);
	}
	
	public override void drawWinner(){
		GUI.Label (new Rect(Screen.width/2 - 100, Screen.height/2 - 12.5f, 200, 25), winningTeam + " Team Wins!");
	}
	
	public override void resetGameMode(){
		teamScores = new Dictionary<string, int> ();
		teamScores.Add(Team.TeamColor.RED.ToString(), 0);
		teamScores.Add(Team.TeamColor.BLUE.ToString(), 0);
	}

	[RPC]
	public void syncTeamScores(int red, int blue){
		teamScores["RED"] = red;
		teamScores["BLUE"] = blue;
		winningTeam = (teamScores["RED"] > teamScores["BLUE"]) ? "RED" : "BLUE";
	}

}
