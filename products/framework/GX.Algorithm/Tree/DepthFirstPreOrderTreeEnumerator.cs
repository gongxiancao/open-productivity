using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.Algorithm.Tree
{
    public interface ITreeNode
    {
        IEnumerable<ITreeNode> Children { get; }
    }

    public class DepthFirstPreOrderTreeNodeWrapper<TNode>
    {
        public DepthFirstPreOrderTreeNodeWrapper<TNode> Next { get; set; }
        public TNode Node { get; set; }
    }

    public class DepthFirstPreOrderTreeEnumerator<TNode> : IEnumerator<TNode> where TNode : ITreeNode
    {
        DepthFirstPreOrderTreeNodeWrapper<TNode> _root;
        DepthFirstPreOrderTreeNodeWrapper<TNode> _current;
        public DepthFirstPreOrderTreeEnumerator(TNode root)
        {
            this._root = new DepthFirstPreOrderTreeNodeWrapper<TNode> { Node = root };
        }

        public TNode Current
        {
            get { return _current.Node; }
        }

        public void Dispose()
        {
        }

        object System.Collections.IEnumerator.Current
        {
            get { return _current.Node; }
        }

        public bool MoveNext()
        {
            if (Current.Children.Count() > 0)
            {
                DepthFirstPreOrderTreeNodeWrapper<TNode> firstChild = null;
                DepthFirstPreOrderTreeNodeWrapper<TNode> prevNode = null;
                foreach (TNode child in Current.Children)
                {
                    var wrapper = new DepthFirstPreOrderTreeNodeWrapper<TNode> { Node = child };
                    if (prevNode != null)
                    {
                        prevNode.Next = wrapper;
                    }
                    else
                    {
                        firstChild = wrapper;
                    }
                    prevNode = wrapper;
                }
                prevNode.Next = _current.Next;
                _current = firstChild;
            }
            else
            {
                _current = _current.Next;
            }
            return _current != null;
        }

        public void Reset()
        {
            _current = _root;
        }
    }
}
