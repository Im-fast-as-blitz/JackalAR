using UnityEngine;

static public class Helpers
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
        public int z;

        public IntVector2(int first, int second)
        {
            x = first;
            z = second;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, 0, z);
        }
        
        public static  IntVector2 operator+(IntVector2 lhs, IntVector2 rhs)
        {
            return new IntVector2(lhs.x + rhs.x, lhs.z + rhs.z);
        }   
    };
    
    public enum Teams
    {
        White = 0,
        Red = 1,
        Black = 2,
        Yellow = 3
    }
}
