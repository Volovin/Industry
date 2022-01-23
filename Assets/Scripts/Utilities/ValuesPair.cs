using System;

namespace Industry.Utilities
{
    public struct ValuesPair<T> where T : struct
    {
        public ValuesPair(T a, T b)
        {
            A = a;
            B = b;
        }

        public T A
        {
            get; set;
        }

        public T B
        {
            get; set;
        }

        public override int GetHashCode()
        {
            return A.GetHashCode() ^ B.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ValuesPair<T>) || obj == null)
                return false;

            var castedObj = (ValuesPair<T>)obj;

            return A.Equals(castedObj.A) && B.Equals(castedObj.B);
        }

        public override string ToString()
        {
            return A.ToString() + " ; " + B.ToString();
        }
    }
}
