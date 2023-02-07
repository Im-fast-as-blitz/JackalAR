public class Helpers
{
    public class PairCardInt
    {
        public Card CardPair;
        public int Amount;

        public PairCardInt(Card otherCard, int amount)
        {
            CardPair = otherCard;
            Amount = amount;
        }
    };
    
    public class IntVector2
    {
        public int x;
        public int y;

        public IntVector2(int first, int second)
        {
            x = first;
            y = second;
        }
    };
}
