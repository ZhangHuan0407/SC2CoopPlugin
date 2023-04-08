using System;
using System.Collections;
using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// 结构体数组
    /// </summary>
    public struct StrcutArray4<T> : IReadOnlyList<T> where T : struct
    {
        public T A, B, C, D;
        public int Length;

        public T this[int index]
        {
            get
            {
                if (index == 0 && Length > 0)
                    return A;
                else if (index == 1 && Length > 1)
                    return B;
                else if (index == 2 && Length > 2)
                    return C;
                else if (index == 3 && Length > 3)
                    return D;
                else
                    throw new IndexOutOfRangeException(index.ToString());
            }
            set
            {
                if (index == 0 && Length > 0)
                {
                    A = value;
                    return;
                }
                else if (index == 1 && Length > 1)
                {
                    B = value;
                    return;
                }
                else if (index == 2 && Length > 2)
                {
                    C = value;
                    return;
                }
                else if (index == 3 && Length > 3)
                {
                    D = value;
                    return;
                }
                throw new IndexOutOfRangeException(index.ToString());
            }
        }

        public int Count => Length;

        public StrcutArray4(int length)
        {
            Length = length;
            if (Length < 0 || Length > 4)
                throw new ArgumentException($"{nameof(Length)}: {Length}");
            A = B = C = D = default;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (Length == 0)
                yield break;
            yield return A;
            if (Length == 1)
                yield break;
            yield return B;
            if (Length == 2)
                yield break;
            yield return C;
            if (Length == 3)
                yield break;
            yield return D;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}