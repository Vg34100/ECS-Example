namespace ECS_Base.Mechanics.Progression.Components
{
    /// <summary>
    /// Tracks player progression counts.
    /// </summary>
    public struct CurrencyComponent
    {
        public int Coins;
        public int Stars;
        public int Rupees;
        public int Score;

        public CurrencyComponent(int coins = 0, int stars = 0, int rupees = 0, int score = 0)
        {
            Coins = coins;
            Stars = stars;
            Rupees = rupees;
            Score = score;
        }
    }
}
