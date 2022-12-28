using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScoreCard : MonoBehaviour
{
    [SerializeField]
    private List<Image> MatchScoreImages;
    [SerializeField]
    private Image PlayerImage;
    [SerializeField]
    private GameObject TournamentScoreParent;
    [SerializeField]
    private List<Image> TournamentScoreImages;
    [SerializeField]
    private List<Image> GrayedOutTournamentScoreImages;
    [SerializeField]
    private Image BorderImage;
    public int PlayerID { get; private set; }
    private int MatchScoreCounter;
    private int TournamentScoreCounter;
    public Animator Animator { get; private set; }
    public GameObject Crown;
    public Image WinnerBorder;

    public void Initialize(PlayerDataModel pPlayerDM)
    {
        Animator = GetComponent<Animator>();
        Crown.SetActive(false);
        PlayerID = pPlayerDM.PlayerID;
        PlayerImage.sprite = pPlayerDM.PlayerSprite;
        BorderImage.color = pPlayerDM.PlayerColor;
        WinnerBorder.color = pPlayerDM.PlayerColor;
        WinnerBorder.gameObject.SetActive(false);
        foreach (var image in MatchScoreImages)
        {
            image.color = pPlayerDM.PlayerColor;
            image.gameObject.SetActive(false);
        }
        TournamentScoreParent.SetActive(false);
    }

    public void Initialize(PlayerDataModel pPlayerDM, TournamentManager pTournamentManager, bool pIsLeading)
    {
        Initialize(pPlayerDM);

        int TournamentMaxScore = pTournamentManager.ScoreNeeded;
        int TournamentPlayerScore = pTournamentManager.Contenders.FirstOrDefault(a => a.DataModel == pPlayerDM).Score;

        float UIWidth = TournamentScoreParent.GetComponent<RectTransform>().rect.width;
        for (int i = 0; i < TournamentScoreImages.Count; i++)
        {
            if (i < TournamentMaxScore)
            {
                float xPos = ((UIWidth / (TournamentMaxScore + 1)) * (i + 1));
                TournamentScoreImages[i].transform.localPosition = new Vector3(xPos - (UIWidth / 2), 0, 0);
                GrayedOutTournamentScoreImages[i].transform.localPosition = new Vector3(xPos - (UIWidth / 2), 0, 0);

                GrayedOutTournamentScoreImages[i].gameObject.SetActive(true);
                TournamentScoreImages[i].color = pPlayerDM.PlayerColor;

                if (i < TournamentPlayerScore)
                {
                    TournamentScoreImages[i].gameObject.SetActive(true);
                    TournamentScoreCounter++;
                }
                else
                {
                    TournamentScoreImages[i].gameObject.SetActive(false);
                }
            }
            else
            {
                TournamentScoreImages[i].gameObject.SetActive(false);
                GrayedOutTournamentScoreImages[i].gameObject.SetActive(false);
            }
        }
        TournamentScoreParent.SetActive(true);
        Crown.SetActive(pIsLeading);
    }

    public void GiveMatchScore()
    {
        Animator?.SetTrigger("MatchScore");
        MatchScoreImages[MatchScoreCounter].gameObject.SetActive(true);
        MatchScoreCounter++;
    }

    public void GiveTournamentScore()
    {
        TournamentScoreImages[TournamentScoreCounter].gameObject.SetActive(true);
        TournamentScoreCounter++;
    }
}
