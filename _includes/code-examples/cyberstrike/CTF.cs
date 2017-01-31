using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CTF : TeamBased {

	private Dictionary<string, GameObject> flags;

	protected override void init(){
		base.init();
		gameType = GameType.CTF;
	}

	protected override void playerKilled(Player r, Player s, Vector3 normal){
		if(r.flag != null)
			r.flag.networkView.RPC("detach", RPCMode.All);
		base.playerKilled(r, s, normal);
	}

	private void spawnFlags(){
		flags = new Dictionary<string, GameObject>();
		foreach(Team t in teams)
			spawnTeamFlag(t);
	}

	private void spawnTeamFlag(Team t){
		GameObject spawn = GameObject.FindWithTag("FlagSpawn" + t);
		GameObject flag = (GameObject) Network.Instantiate(Resources.Load("GameModes/Flag"), spawn.transform.position, spawn.transform.rotation, 1);
		flags.Add(t.ToString(), flag);
		flag.GetComponent<Flag>().init(t, gameControl);
		spawn.GetComponent<FlagCaptureZone>().init(t);
	}

	public void flagCaptured(Player scoringPlayer, Flag capturedFlag){
		flags.Remove(capturedFlag.team.ToString());
		spawnTeamFlag(capturedFlag.team);
		Network.RemoveRPCs(capturedFlag.networkView.viewID);
		Network.Destroy(capturedFlag.gameObject);
		updateTeamScores(scoringPlayer.playerData.team);
	}

	protected override void updatePlayerScores(Player r, Player s){
		base.updatePlayerScores(r, s);
		updateTeamScores(s.playerData.team);
	}
	
	public override void resetGameMode(){
		base.resetGameMode();
		if(Network.isServer)
			spawnFlags();
	}

}
