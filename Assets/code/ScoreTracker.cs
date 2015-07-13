using UnityEngine;
using System.Collections;

public class ScoreTracker : MonoBehaviour
{

	private int bestScore;
	private int currentScore;
	private UnityEngine.UI.Text current;
	private UnityEngine.UI.Text best;



	void Start ()
	{
		bestScore = PlayerPrefs.GetInt ("bestScore");
		current = GameObject.FindGameObjectWithTag ("current").GetComponent<UnityEngine.UI.Text> ();
		best = GameObject.FindGameObjectWithTag ("best").GetComponent<UnityEngine.UI.Text> ();
	}

	void Update ()
	{
		current.text = "Current moves : " + GetCurrentScore ();
		best.text = "Best moves : " + GetBestScore ();
	}

	public int GetCurrentScore ()
	{
		return this.currentScore;
	}

	public int GetBestScore ()
	{
		return this.bestScore;
	}

	public void SetBestScore (int score)
	{
		this.bestScore = score;
	}
	
	public void SetCurrentScore (int score)
	{
		this.currentScore = score;
	}

	public void IncrementCurrentScore ()
	{
		this.currentScore++;
	}

	public void Save ()
	{
		if (currentScore > bestScore) {
			PlayerPrefs.SetInt ("bestScore", currentScore);
		} else {
			PlayerPrefs.SetInt ("bestScore", bestScore);
		}
		PlayerPrefs.Save ();
	}
}
