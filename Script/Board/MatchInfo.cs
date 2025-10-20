using UnityEngine;

namespace CandyProject
{
    public enum MatchType
    {
        None,
        Three,
        FourHorizontal,
        FourVertical,
        Five,
        boomWrapped,
    }
    public class MatchInfo
    {
        public Vector2Int center;
        public MatchType matchType;
        public MatchInfo(Vector2Int center, MatchType matchType)
        {
            this.center = center;
            this.matchType = matchType;
        }

    }
}
