using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace StoryEditorContext{

    [System.Serializable]
    public class StoryCanvas {
        public List<Node> nodeList = new List<Node>();
        public Node root = null;

        public Vector2 scrollOffset = Vector2.zero;
        public float zoom = 1;
        public StoryCanvas(Rect rootRect)
        {
            nodeList.Clear();
            CreateRoot(rootRect);
            
        }

        private void CreateRoot(Rect r)
        {
            root = Create(r);
            root.type = NodeType.Root;
            root.name = "Root";
        }

        public Node Create(Rect r)
        {
            int id = nodeList.Count;
            Node node = new Node(r);
            node.Id = id;
            nodeList.Add(node);
            return node;
        }

        public void NodeAddChild(Node parent, Node child)
        {
            Debug.Assert(parent != null && child != null);
            if (!parent.childList.Contains(child.Id))
            {
                parent.childList.Add(child.Id);
                NodeSetParent(child, parent);
            }
            
            
        }

        public void NodeSetParent(Node child, Node parent)
        {
            Debug.Assert(parent != null && child != null);
            if (child.parentId != parent.Id)
            {
                if (child.parentId != -1)
                {
                    Node preParent = nodeList[child.parentId];
                    preParent.childList.Remove(child.Id);
                }
                child.parentId = parent.Id;
                NodeAddChild(parent, child);
            }
        }


        public void NodeRemoveChild(Node parent, Node child)
        {
            Debug.Assert(parent != null && child != null);
            if (parent.childList.Contains(child.Id))
            {
                parent.childList.Remove(child.Id);
                NodeRemoveParent(child, parent);
            }
        }

        public void NodeRemoveParent(Node child, Node parent)
        {
            Debug.Assert(parent != null && child != null);
            if (child.parentId == parent.Id)
            {
                child.parentId = -1;
                NodeRemoveChild(parent, child);
            }
        }
    }
}


