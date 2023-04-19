using System;
using System.Collections.Generic;

namespace Game
{
    public struct OpRecordEntry<T> where T : class
    {
        public string Debug;
        public Action<T> Redo;
        public Action<T> Undo;
    }

    public class OpRecord<T> where T : class
    {
        private LinkedList<OpRecordEntry<T>> m_OperatingRecordList;
        private LinkedListNode<OpRecordEntry<T>> m_LatestOperatingRecordNode;
        private readonly int RecordLimit;

        public OpRecord(int recordLimit = 50)
        {
            m_OperatingRecordList = new LinkedList<OpRecordEntry<T>>();
            RecordLimit = recordLimit;
        }

        public void Undo(T t)
        {
            if (m_LatestOperatingRecordNode != null)
            {
                m_LatestOperatingRecordNode.Value.Undo(t);
                m_LatestOperatingRecordNode = m_LatestOperatingRecordNode.Previous;
            }
        }
        public void Redo(T t)
        {
            if (m_LatestOperatingRecordNode == null && m_OperatingRecordList.Count > 0)
            {
                m_LatestOperatingRecordNode = m_OperatingRecordList.First;
                m_LatestOperatingRecordNode.Value.Redo(t);
            }
            else if (m_LatestOperatingRecordNode?.Next != null)
            {
                m_LatestOperatingRecordNode = m_LatestOperatingRecordNode.Next;
                m_LatestOperatingRecordNode.Value.Redo(t);
            }
        }
        /// <summary>
        /// 丢弃未执行的所有 Redo 操作记录，从当前位置分叉记录
        /// </summary>
        public void AppendRecord(OpRecordEntry<T> operatingRecord)
        {
            if (operatingRecord.Redo is null)
                throw new ArgumentNullException(nameof(OpRecordEntry<T>.Redo));
            if (operatingRecord.Undo is null)
                throw new ArgumentNullException(nameof(OpRecordEntry<T>.Undo));

            LinkedListNode<OpRecordEntry<T>> node = m_OperatingRecordList.Last;
            while (m_OperatingRecordList.Count > 0 &&
                node != m_LatestOperatingRecordNode)
            {
                LinkedListNode<OpRecordEntry<T>> previousNode = node.Previous;
                m_OperatingRecordList.Remove(node);
                node = previousNode;
            }
            m_OperatingRecordList.AddLast(operatingRecord);
            if (m_OperatingRecordList.Count > RecordLimit)
            {
                var first = m_OperatingRecordList.First;
                if (m_LatestOperatingRecordNode == first)
                    m_LatestOperatingRecordNode = null;
                m_OperatingRecordList.Remove(first);
            }
        }
    }
}