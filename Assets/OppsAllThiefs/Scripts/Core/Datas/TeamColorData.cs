using UnityEngine;

[CreateAssetMenu(fileName = "TeamColorData", menuName = "ScriptableObjects/TeamColorData")]
public class TeamColorData : ScriptableObject
{
    [SerializeField] private Color[] teamColor;

    public Color? GetTeamColor(int teamIndex)
    {
        if (teamIndex < 0)
        {
            return null;
        }
        else if (teamIndex >= teamColor.Length)
        {
            return Random.ColorHSV(0f, 1f, 1f, 1f, .5f, 1f);
        }
        else
        {
            return teamColor[teamIndex];
        }
    }
}
