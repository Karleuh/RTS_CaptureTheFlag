using System;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }
	private float startTime;

	[SerializeField]
	private Menu menu;

	public float GameTime => Time.time - this.startTime;
	public bool IsGameStarted { get; private set; }


	private void Awake()
	{
		GameManager.Instance = this;
	}

	public void StartGame()
	{
		this.startTime = Time.time;
		this.IsGameStarted = true;
	}

	public void GameOver(bool win)
	{
		this.menu.OnWin(win);
		this.IsGameStarted = false;
	}

	private void ChooseStartingArea()
	{

	}
}
