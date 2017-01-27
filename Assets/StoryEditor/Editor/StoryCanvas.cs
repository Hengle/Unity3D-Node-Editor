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

        
    }
}


