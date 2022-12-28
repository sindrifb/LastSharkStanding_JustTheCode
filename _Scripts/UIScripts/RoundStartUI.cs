using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;

public class RoundStartUI : MonoBehaviour
{
    [SerializeField]
    private GameObject MatchEndUI;
    public GameObject WinnerFlare;
    public GameObject ScoreBoard;
    public GameObject TextOnTop;
    public Image TimeToNextRound;
    [SerializeField]
    private GameObject PlayerScoreCardPrefab;
    private List<PlayerScoreCard> PlayerScoreCardScripts = new List<PlayerScoreCard>();
    private TournamentManager TournamentManager;

    public Animator ScoreboardAnimator { get; private set; }

    private List<PlayerDataModel> PlayerDataModels;
    private Dictionary<int, Sprite> Numbers = new Dictionary<int, Sprite>();
    private Dictionary<int, Image> PlayerScores = new Dictionary<int, Image>();

    public void InitializeUI()
    {
        TournamentManager = FindObjectOfType<TournamentManager>();
        ScoreboardAnimator = gameObject.GetComponent<Animator>();
        PlayerDataModels = GameManager.Instance.GetPlayerDataModels();
        var sortedPDMList = PlayerDataModels.OrderBy(a => a.PlayerID).ToList();
        float distanceBetween = 150; //unnecessary variable just here for clarity
    
        for (int i = 0; i < sortedPDMList.Count(); i++)
        {
            var scoreCard = Instantiate(PlayerScoreCardPrefab, ScoreBoard.transform);
            RectTransform rectTransform = scoreCard.GetComponent<RectTransform>();
            float xPos = (-((distanceBetween + rectTransform.rect.width) / 2) * (sortedPDMList.Count() - 1)) + ((distanceBetween + rectTransform.rect.width) * i);
            scoreCard.transform.localPosition = new Vector3(xPos, 0, 0);
            var scoreCardScript = scoreCard.GetComponent<PlayerScoreCard>();
            PlayerScoreCardScripts.Add(scoreCardScript);
            if (GameManager.Instance.Tournament)
            {
                var tContendersDescending = TournamentManager.Contenders.OrderByDescending(a => a.Score).ToList();
                bool isLeading = (tContendersDescending[0].DataModel == sortedPDMList[i] && tContendersDescending[0].Score != 0) || (tContendersDescending.FirstOrDefault(a => a.DataModel == sortedPDMList[i]).Score == tContendersDescending[0].Score && tContendersDescending[0].Score != 0); 
 
                scoreCardScript.Initialize(sortedPDMList[i], TournamentManager, isLeading);
            }
            else
            {
                scoreCardScript.Initialize(sortedPDMList[i]);
            }
        }
        LeaderBoardSetActive(false);
        StartCoroutine(StartCountDown(true));
    }

    public void LeaderBoardSetActive(bool pValue)
    {
        ScoreboardAnimator.SetBool("ScoreBoardActive", pValue);
    }

    public IEnumerator EndRound()
    {
        LeaderBoardSetActive(true);
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.Scoreboard_Woosh);
        StartCoroutine(StartCountDown());
        yield return new WaitForSeconds(1f);
        PlayerScoreCardScripts.FirstOrDefault(a => a.PlayerID == GameManager.Instance.RoundWinnerPlayerID).GiveMatchScore();
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.GainScore);
        //SetPlayerScores();
    }

    public IEnumerator EndMatch()
    {
        var roundWinnerScoreCard = PlayerScoreCardScripts.FirstOrDefault(a => a.PlayerID == GameManager.Instance.RoundWinnerPlayerID);
        Color winnerColor = GameManager.Instance.PlayerDataModels.FirstOrDefault(a => a.PlayerID == roundWinnerScoreCard.PlayerID).PlayerColor; ;
        LeaderBoardSetActive(true);
        yield return new WaitForSeconds(1f);

        roundWinnerScoreCard.GiveMatchScore();
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.GainScore);
        yield return new WaitForSeconds(0.5f);

        if (GameManager.Instance.Tournament)
        {
            roundWinnerScoreCard.GiveTournamentScore();
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.GainTrophy);
            var tContendersDescending = TournamentManager.Contenders.OrderByDescending(a => a.Score).ToList();
            foreach (var playerScoreCard in PlayerScoreCardScripts)
            {
                bool isLeading = tContendersDescending[0].DataModel.PlayerID == playerScoreCard.PlayerID || tContendersDescending.FirstOrDefault(a => a.DataModel.PlayerID == playerScoreCard.PlayerID).Score == tContendersDescending[0].Score;
                playerScoreCard.Crown.SetActive(isLeading);
                //AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.Scoreboard_BorderPop);
            }

            yield return new WaitForSeconds(0.5f);
        }
        
        roundWinnerScoreCard.WinnerBorder.gameObject.SetActive(true);
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.Scoreboard_BorderPop);
        yield return new WaitForSeconds(0.5f);

        if (GameManager.Instance.Tournament && TournamentManager.TournamentOver())
        {
            StartCoroutine(EndTournament(roundWinnerScoreCard, winnerColor));
            //StopCoroutine(EndMatch());
            yield break;
        }

        ScoreboardAnimator.SetTrigger("MatchEnd");
        yield return new WaitForSeconds(0.6f);
        WinnerFlare.GetComponent<Image>().color = winnerColor;
        WinnerFlare.transform.position = roundWinnerScoreCard.transform.position;
        WinnerFlare.gameObject.SetActive(true);

        MatchEndUI.SetActive(true);
    }

    private IEnumerator EndTournament(PlayerScoreCard pWinnerScoreCard, Color pWinnerColor)
    {
        //make non-winning player cards disappear
        foreach (var scoreCard in PlayerScoreCardScripts.Where(a => a != pWinnerScoreCard))
        {
            scoreCard.Animator.SetTrigger("TournamentLoser");
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        //move winning player card to the middle of the screen
        Vector2 targetPos = new Vector2(0, 90);
        float timeToReachTarget = 0.5f;
        var scoreCardRectTransform = pWinnerScoreCard.GetComponent<RectTransform>();
        var startPos = scoreCardRectTransform.anchoredPosition;
        float t = 0;
        while (scoreCardRectTransform.anchoredPosition != targetPos)
        {
            yield return null;
            t += Time.deltaTime / timeToReachTarget;
            scoreCardRectTransform.anchoredPosition = Vector3.Lerp(startPos, targetPos, t);
        }
        //scale up winning player card
        pWinnerScoreCard.Animator.SetTrigger("TournamentWinner");
        yield return new WaitForSeconds(0.2f);

        //put correct color and position on the winner flare and activate
        WinnerFlare.GetComponent<Image>().color = pWinnerColor;
        WinnerFlare.transform.position = pWinnerScoreCard.transform.position;
        WinnerFlare.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);

        //set match end UI active
        MatchEndUI.SetActive(true);
    }

    public IEnumerator StartCountDown(bool pFirstSpawn = false)
    {
        float timer = 0;
        bool hasRun = false;
        if (!pFirstSpawn)
        {
            ScoreboardAnimator.SetTrigger("BetweenRoundsTimer");
            while (timer < 5)
            {
                yield return new WaitForEndOfFrame();
                timer += Time.deltaTime;

                if (timer > 3 && !hasRun)
                {
                    GameManager.Instance.SpawnPlayers();
                    LeaderBoardSetActive(false);
                    AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.Scoreboard_Woosh);
                    hasRun = true;
                }
            }
        }
        else
        {
            ScoreboardAnimator.SetTrigger("FirstRoundTimer");
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.RoundStartIn3);
            GameManager.Instance.SpawnPlayers();
            LeaderBoardSetActive(false);
            hasRun = true;
            yield return new WaitForSeconds(3);
        }

        GameManager.Instance.StartRound();        
    }
}
