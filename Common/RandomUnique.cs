using System;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    public class RandomUnique
    {
        private const int SmallCountBarrier = 50;

        private bool _isRangeSmall;

        public int Min { get; private set; }
        public int Max { get; private set; }

        private int _collisionCount;

        private int _count;

        // Small range
        private List<int> _numbers;

        // Big range
        private HashSet<int> _hashSet;

        // Random
        private readonly Random _random;

        public RandomUnique(int min, int max)
        {
            if (min < 0)
                throw new ArgumentException("Min must be greater than or equal to 0");
            if (max <= 0)
                throw new ArgumentException("Max must be greater than 0");
            if (min >= max)
                throw new ArgumentException("Min must be small than Max");

            Min = min;
            Max = max;

            _random = new Random();

            Initialize();
        }

        public RandomUnique(int seed, int min, int max)
        {
            if (min < 0)
                throw new ArgumentException("Min must be greater than or equal to 0");
            if (max <= 0)
                throw new ArgumentException("Max must be greater than 0");
            if (min >= max)
                throw new ArgumentException("Min must be small than Max");

            Min = min;
            Max = max;

            _random = new Random(seed);

            Initialize();
        }

        public void Reset()
        {
            _count = 0;
            if (_isRangeSmall)
                Shuffle(_random, _numbers);
            else
            {
                _collisionCount = 0;
                _hashSet.Clear();
            }
        }

        public int Next()
        {
            return _isRangeSmall ? NextSmallRange() : NextBigRange();
        }
        private void Initialize()
        {
            if (Max - Min > SmallCountBarrier)
                InitializeBigRange();
            else
                InitializeSmallRange();
        }

        private void InitializeSmallRange()
        {
            _numbers = Enumerable.Range(Min, Max - Min).ToList();
            Shuffle(_random, _numbers);
            _count = 0;

            _isRangeSmall = true;
        }

        private void InitializeBigRange()
        {
            _hashSet = new HashSet<int>();
            _count = 0;

            _isRangeSmall = false;
        }

        private int NextSmallRange()
        {
            if (_count == _numbers.Count + 1)
                throw new Exception("Cannot generate a new random number, end of random cycle.");
            return _numbers[_count++];
        }

        private int NextBigRange()
        {
            if (_count > (Max-Min) / 2)
                throw new Exception("Cannot generate a new random number efficiently.");
            // Get random
            int r = _random.Next(Min, Max);
            if (_hashSet.Contains(r)) // if already used, cycle until finding a free entry
            {
                while (true)
                {
                    _collisionCount++;
                    if (r == Max)
                        r = Min;
                    else
                        r++;
                    if (!_hashSet.Contains(r))
                        break; // found
                }
            }
            _count++;
            _hashSet.Add(r);
            return r;
        }

        private static void Shuffle<T>(Random random, IList<T> array)
        {
            for (int n = array.Count; n > 1; )
            {
                int k = random.Next(n);
                --n;
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }
    }
}
