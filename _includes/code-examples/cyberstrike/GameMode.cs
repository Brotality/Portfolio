using UnityEngine;
using System.Collections;
using System;

/*
	Base GameMode class which all game modes extend.
	Allows me to guarentee certain methods accross
	all game modes.
*/
public abstract class GameMode : MonoBehaviour {

	public enum GameType{
		FFA = 0, TDM = 1, CTF = 2, LTS = 3
	};

	public static int numOfGameTypes(){
		return Enum.GetNames(typeof(GameType)).Length;
	}

	protected int scoreToWin = 5;
	protected bool bIsTeamBased;
	protected GameControl gameControl;
	public GameType gameType {get; protected set;}
	public bool bCanRespawn {get; protected set;}

	void Awake () {
		gameControl = GetComponent<GameControl>();
		init ();
	}

	protected abstract void init ();
	public abstract void createTeams ();
	public abstract void drawUI ();
	public abstract void drawWinner();
	public abstract void resetGameMode();
	//public abstract void checkWinState ();

	public void applyDamageToFrom(int damage, NetworkPlayer	receiver, NetworkPlayer sender, Vector3 normal){
		Player r = gameControl.findPlayer(receiver), s = gameControl.findPlayer(sender);
		if(r.playerData.alive && ((bIsTeamBased) ? !s.playerData.team.ToString().Equals(r.playerData.team.ToString()) : true)){
			Debug.Log("Applying damage to: " + receiver + " from " + sender);
			r.applyDamage(s, damage);
			if(receiver == Network.player)
				gameControl.client.Client_UpdatePlayerHealth(r.playerData.health);
			else
				networkView.RPC("Client_UpdatePlayerHealth", receiver, r.playerData.health);
			if(!r.playerData.alive)
				playerKilled(r, s, normal);
		}
	}

	protected virtual void playerKilled(Player r, Player s, Vector3 normal){
		updatePlayerScores(r, s);
		gameControl.server.killPlayer(r, normal);
	}

	protected virtual void updatePlayerScores(Player r, Player s){
		Player k = r.getMaxDamageDealer();
		k.playerData.kills++;
		networkView.RPC("Client_UpdatePlayerScore", RPCMode.All, k.networkPlayer, k.playerData.kills, k.playerData.assists, k.playerData.deaths);
		networkView.RPC("Client_UpdatePlayerScore", RPCMode.All, r.networkPlayer, r.playerData.kills, r.playerData.assists, r.playerData.deaths);
		foreach(Player p in r.getDamageDealers()){
			if(!p.Equals(k))
				networkView.RPC("Client_UpdatePlayerScore", RPCMode.All, p.networkPlayer, p.playerData.kills, p.playerData.assists, p.playerData.deaths);
		}
	}

	public virtual void playerDied(){
		gameControl.menuNavigator.playerDied(bCanRespawn);
	}
}
