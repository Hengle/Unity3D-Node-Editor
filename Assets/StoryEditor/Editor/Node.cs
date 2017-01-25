using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace StoryEditorContext
{
    [System.Serializable]
    public enum NodeType
    {
        Root,
        Common
    }


    [System.Serializable]
    public class Node
    {
        private const float KNOB_SIZE = 20;
        private const string INPUT_KNOB_PATH = "Assets/StoryEditor/Texture/In_Knob.png";
        private const string OUTPUT_KNOB_PATH = "Assets/StoryEditor/Texture/Out_Knob.png";

        private static Texture2D _inoutKnobTex = null;
        private static Texture2D _outputKnobTex = null;

        public int Id = -1;
        public int parentId = -1;
        public List<int> childList = new List<int>();
        public Rect rect;
        public string name;
        public string actionName;

        public NodeType type = NodeType.Common;

        private Rect _inputKnobRect;
        private Rect _outputKnobRect;

        static Node()
        {
            if (_inoutKnobTex == null)
            {
                _inoutKnobTex = AssetDatabase.LoadAssetAtPath(INPUT_KNOB_PATH, typeof(Texture2D)) as Texture2D;
            }

            if (_outputKnobTex == null)
            {
                _outputKnobTex = AssetDatabase.LoadAssetAtPath(OUTPUT_KNOB_PATH, typeof(Texture2D)) as Texture2D;
            }
        }  
        public Node(Rect r)
        {
            rect = r;
        }

        public void DrawWindow()
        {
            if (type == NodeType.Root)
            {
                EditorGUILayout.LabelField("Root");
            }
            else
            {
                EditorGUILayout.LabelField(actionName);
            }
            

        }

        public void DrawKnob()
        {
            GUI.DrawTexture(InputKnobRect, _inoutKnobTex);
            GUI.DrawTexture(OutputKnobRect, _outputKnobTex);
        }

        public Rect InputKnobRect
        {
            get
            {
                return new Rect(
                    rect.x - KNOB_SIZE,
                    rect.y + (rect.height - KNOB_SIZE) / 2,
                    KNOB_SIZE, KNOB_SIZE
                    );
            }
        }

        public Rect OutputKnobRect
        {
            get
            {
                return new Rect(
                    rect.x + rect.width,
                    rect.y + (rect.height - KNOB_SIZE) / 2,
                    KNOB_SIZE, KNOB_SIZE
                    );
            }
        }

        public Rect TotalRect
        {
            get
            {
                return new Rect(
                    rect.x - KNOB_SIZE, 
                    rect.y, 
                    rect.width + KNOB_SIZE * 2, 
                    rect.height
                    );
            }
        }
    }
}

