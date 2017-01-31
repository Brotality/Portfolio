using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LTS : TeamBased {

	protected Dictionary<string, int> teamPlayesAlive;

	protected override void init(){
		base.init();
		gameType = GameType.LTS;
		bCanRespawn = false;
	}

	public override void createTeams(){
		base.createTeams();
		teamPlayesAlive = new Dictionary<string, int>();
		foreach(Team t in teams)
			teamPlayesAlive.Add(t.ToString(), 0);
		foreach(Player p in gameControl.players)
			teamPlayesAlive[p.playerData.team.ToString()]++;
	}

	protected override void updatePlayerScores(Player r, Player s){
		base.updatePlayerScores(r, s);
		teamPlayesAlive[r.playerData.team.ToString()]--;
		if(teamPlayesAlive[r.playerData.team.ToString()] <= 0){
			updateTeamScores(s.playerData.team);
			gameControl.roundOver();
			networkView.RPC("allowPlayersToSpawn", RPCMode.All);
		}
	}

	[RPC]
	public void allowPlayersToSpawn(){
		Debug.Log("round over, allowing respawn");
		gameControl.menuNavigator.playerDied(true);
	}

}
