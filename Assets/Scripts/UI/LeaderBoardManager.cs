using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    [System.Serializable]
    public class PlayerData
    {
        public Sprite avatar;
        public string username;
        public int score;
    }

    public PlayerData[] players;
    public GameObject rowPrefab;
    public Transform contentParent;

    void Start()
    {
        PopulateLeaderboard();
    }

    void PopulateLeaderboard()
    {
        // Skora göre sýrala (yüksekten düþüðe)
        System.Array.Sort(players, (x, y) => y.score.CompareTo(x.score));

        for (int i = 0; i < players.Length; i++)
        {
            GameObject rowObj = Instantiate(rowPrefab, contentParent);
            LeaderboardRow row = rowObj.GetComponent<LeaderboardRow>();

            // Sýra numarasý
            row.rankText.text = (i + 1).ToString();

            // Kullanýcý bilgileri
            row.avatarImage.sprite = players[i].avatar;
            row.usernameText.text = players[i].username;
            row.scoreText.text = players[i].score.ToString();
        }
    }
}
