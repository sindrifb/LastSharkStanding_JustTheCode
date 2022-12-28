using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TournamentManager : MonoBehaviour
{
    public List<TournamentContender> Contenders { get; private set; } = new List<TournamentContender>();
    public int ScoreNeeded { get; private set; }

    public class TournamentContender
    {
        public PlayerDataModel DataModel;
        public int Score;
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize(int pScoreNeeded)
    {
        foreach (var player in GameManager.Instance.PlayerDataModels)
        {
            var contender = new TournamentContender();
            contender.DataModel = player;
            contender.Score = 0;
            Contenders.Add(contender);
        }

        ScoreNeeded = pScoreNeeded;
    }
    
    public void AddScore(PlayerDataModel pPlayer)
    {
        var matchWinner = Contenders.FirstOrDefault(a => a.DataModel == pPlayer);
        matchWinner.Score += 1;
    }

    public bool TournamentOver ()
    {
        var leadingPlayer = Contenders.OrderByDescending(a => a.Score).ToList()[0];

        return (leadingPlayer.Score >= ScoreNeeded);
    }
}
