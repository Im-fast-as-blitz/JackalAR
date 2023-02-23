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

        public IntVector2(IntVector2 other)
        {
            x = other.x;
            z = other.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, 0, z);
        }

        public static IntVector2 operator +(IntVector2 lhs, IntVector2 rhs)
        {
            return new IntVector2(lhs.x + rhs.x, lhs.z + rhs.z);
        }
    };

    public enum Teams
    {
        White,
        Red,
        Black,
        Yellow
    }
}
