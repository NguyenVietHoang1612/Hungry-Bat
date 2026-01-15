using UnityEngine;

namespace CandyProject
{
    public class BackGroundTiles
    {
        private Gem gem;

        public BackGroundTiles(Gem gem)
        {
            this.gem = gem;
        }


        public Gem GetGem()
        {
            return gem;
        }
    }
}
