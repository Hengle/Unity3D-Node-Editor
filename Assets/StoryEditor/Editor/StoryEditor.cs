using UnityEngine;
using System.Collections;
using UnityEditor;



namespace StoryEditorContext
{
    public class StoryEditor : EditorWindow
    {
        private const float kNodeInfoWWP = 0.3f;
        private const float kNodeInfoWHP = 0.5f;
        private const float kToolbarHeight = 40;

        private const int kNodeWidth = 50;
        private const int kNodeHeight = 30;

        private const string kBgTexturePath = "Assets/StoryEditor/Texture/background.png";
        private const float kTitleHeight = 21;

        private Texture2D _girdTex = null;
        
        private Vector2 _mousePos;
        private bool _isCanScrollWindow = false;

        private Matrix4x4 _noZoomMatrix;
        private Vector2 _zoomPivotPos;
        
        // editor serialize data
        private StoryCanvas _canvas = null;

        [MenuItem("Window/Story Editor")]
        static void CreateEditor()
        {
            StoryEditor se= (StoryEditor)EditorWindow.GetWindow(typeof(StoryEditor));
            se.minSize = new Vector2(800, 600);
            
        }
        
        void OnEnable()
        {

            InitToolRes();

            if (_canvas == null)
            {
                Rect rootRect = new Rect(position.width / 2, position.height / 2, kNodeWidth, kNodeHeight);
                _canvas = new StoryCanvas(rootRect);
                
            }

            _zoomPivotPos = new Vector2(position.width / 2, position.height / 2);
                
        }

        void InitToolRes()
        {
            if (_girdTex == null)
            {
                _girdTex = AssetDatabase.LoadAssetAtPath(kBgTexturePath, typeof(Texture2D)) as Texture2D;
            }
            
        }

        void OnGUI()
        {
            
            HandleInputEvents();

            DrawCenterWindow();
            DrawToolBar();
            DrawNodeInfoWindow();
            DrawPlayInfoWidnow();
            
            
            
        }

        void HandleInputEvents()
        {
            // 鼠标的原点过title的左上角，而GUI的就是在Widnow的左上角，
            // 这里就是让event.mousePos的原点移到Widnow的左上角,对齐GUI的Rect的原点
            Rect totalRect = new Rect(0, -kTitleHeight, position.width, position.height);
            GUI.BeginGroup(totalRect);

            HandleRightClickMenu();
            HandleScrollWindow();
            HandleZoomWindow();



            GUI.EndGroup();

            

        }

        void HandleRightClickMenu()
        {
            Event e = Event.current;
            _mousePos = e.mousePosition;

            // 空白区域右键
            bool isCanDo = (e.type == EventType.MouseDown) && (e.button == 1) && IsInBlankArea(_mousePos);
            if (isCanDo)
            {
                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < ActionMenuInfo.elementList.Count; i++)
                {
                    ActionMenuElement element = ActionMenuInfo.elementList[i];
                    menu.AddItem(new GUIContent(element.path), false, RightClickMenuCallback, i);
                }
                menu.ShowAsContext();
                e.Use();
            }
        }

        void RightClickMenuCallback(object obj)
        {
            //Debug.Log("Selected: " + obj);

            ActionMenuElement element = ActionMenuInfo.elementList[(int)obj];
            Rect nodeRect = new Rect(_mousePos.x, _mousePos.y, kNodeWidth, kNodeHeight);
            //Zoom
            //nodeRect = ScaleRect(nodeRect,1.0f / _canvas.zoom, _zoomPivotPos);
            Rect tempRect = ScaleRect(nodeRect, 1.0f / _canvas.zoom, _zoomPivotPos);
            Vector2 pos = tempRect.position;
            nodeRect.position = pos;
            
            
            Node node = _canvas.Create(nodeRect);
            node.actionName = element.actionName;

        }

        void HandleScrollWindow()
        {
            Event e = Event.current;
            _mousePos = e.mousePosition;
            // 空白区域左键
            bool isLeftMouseDown = (e.type == EventType.MouseDown) && (e.button == 0) && IsInBlankArea(_mousePos);
            if (isLeftMouseDown)
            {
                _isCanScrollWindow = true;
                e.delta = Vector2.zero;
            }

            if (_isCanScrollWindow)
            {
                _canvas.scrollOffset += e.delta / 2;

                for (int i = 0; i < _canvas.nodeList.Count; i++)
                {
                    Node node = _canvas.nodeList[i];
                    node.rect.position += e.delta / 2;
                }
                Repaint();
            }

            bool isLeftMouseUp = (e.type == EventType.MouseUp) && (e.button == 0);
            if (isLeftMouseUp)
            {
                _isCanScrollWindow = false;
            }


        }

        void HandleZoomWindow()
        {
            Event e = Event.current;
            _mousePos = e.mousePosition;
            // 滚轮
            bool isCanZoom = (e.type == EventType.ScrollWheel);
            if (isCanZoom)
            {
                _canvas.zoom += e.delta.y / 50.0f;
                _canvas.zoom = Mathf.Clamp(_canvas.zoom, 0.5f, 2.0f);
                
                Debug.Log(_canvas.zoom);
                Repaint();
            }

        }


        void DrawCenterWindow()
        {
            BeginZoomCenterWindow();
            DrawGirdBackground();
            DrawNode();
            EndZoomCenterWidnow();
        }

        void DrawNode()
        {
            // 加了之后会令Node一直以Vector2.zero为轴心缩放，但是想要的效果是要以屏幕的中点为轴心缩放
            //Rect totalRect = new Rect(0, kTitleHeight, position.width, position.height);
            ////totalRect = ScaleRect(totalRect, 1.0f / _canvas.zoom, Vector2.zero);
            //totalRect = ScaleRect(totalRect,  _canvas.zoom, _zoomPivotPos);
            //GUI.BeginGroup(totalRect);

            BeginWindows();

            if (_canvas != null)
            {
                int nodeCount = _canvas.nodeList.Count;
                for (int i = 0; i < nodeCount; i++)
                {
                    Node node = _canvas.nodeList[i];
                    // will modify node.rect when the node move
                    node.rect = GUILayout.Window(node.Id, node.rect, DrawNodeWindow, node.name);
                    node.DrawKnob();
                }
            }

            EndWindows();

            //GUI.EndGroup();

        }
        void DrawNodeWindow(int id)
        {
            _canvas.nodeList[id].DrawWindow();
            GUI.DragWindow();
        }

        void DrawGirdBackground()
        {
            Event e = Event.current;
            // update scrollWindow background
            if (e.type == EventType.Repaint)
            {
                if (_girdTex != null)
                {
                    
                    Rect totalRect = new Rect(0, kTitleHeight, position.width, position.height);
                    totalRect = ScaleRect(totalRect, 1.0f / _canvas.zoom, _zoomPivotPos);

                    GUI.BeginGroup(totalRect);

                    Vector2 bgSize = new Vector2(_girdTex.width, _girdTex.height);
                    Vector2 beginPos = new Vector2(_canvas.scrollOffset.x % bgSize.x - bgSize.x,
                        _canvas.scrollOffset.y % bgSize.y - bgSize.y);

                    Rect unitRect = new Rect(beginPos, bgSize);

                    // todo : must need zero to scaleRect
                    unitRect = ScaleRect(unitRect, _canvas.zoom, Vector2.zero);
                    //unitRect = ScaleRect(unitRect, _canvas.zoom, _zoomPivotPos);
                    Vector2 totalSize = totalRect.size + new Vector2(Mathf.Abs(unitRect.x), Mathf.Abs(unitRect.y));

                    int tileX = Mathf.CeilToInt(totalSize.x / unitRect.width);
                    int tileY = Mathf.CeilToInt(totalSize.y / unitRect.height);
                    for (int i = 0; i < tileX; i++)
                    {
                        for (int j = 0; j < tileY; j++)
                        {
                            Rect rect = new Rect(unitRect.position + new Vector2(i * unitRect.width, j * unitRect.height), 
                                unitRect.size);
                            GUI.DrawTexture(rect, _girdTex);
                        }
                    }
                    
                    GUI.EndGroup();
                     
                }
            }
        }

        void BeginZoomCenterWindow()
        {
            GUI.EndGroup();
            _noZoomMatrix = GUI.matrix;
            Vector2 scale = new Vector2(_canvas.zoom, _canvas.zoom);
            GUIUtility.ScaleAroundPivot(scale, _zoomPivotPos);
            
        }
        void EndZoomCenterWidnow()
        {
            GUI.matrix = _noZoomMatrix;
            
            GUI.BeginGroup(new Rect(0, kTitleHeight, Screen.width, Screen.height));
        }

        void DrawToolBar()
        {
            GUILayout.BeginArea(ToolbarRect, GUI.skin.button);
            GUILayout.EndArea();
        }

        void DrawNodeInfoWindow(){
            GUILayout.BeginArea(NodeInfoWindowRect, GUI.skin.button);
            GUILayout.EndArea();
        }

        void DrawPlayInfoWidnow()
        {
            GUILayout.BeginArea(PlayInfoWindowRect, GUI.skin.button);
            GUILayout.EndArea();
        }



        public Rect NodeInfoWindowRect
        {
            // position 就是editorWindow的位置与大小
            get
            {
                return new Rect(position.width * (1 - kNodeInfoWWP),
                kToolbarHeight,
                position.width * kNodeInfoWWP,
                position.height * kNodeInfoWHP);
            }
        }

        public Rect PlayInfoWindowRect
        {
            get
            {
                return new Rect(
                    position.width * (1 - kNodeInfoWWP),
                    kToolbarHeight + position.height * kNodeInfoWHP,
                    position.width * kNodeInfoWWP,
                    position.height - (kToolbarHeight + position.height * kNodeInfoWHP)
                    );
            }
        }
        public Rect ToolbarRect
        {
            get { return new Rect(0, 0, position.width, kToolbarHeight); }
        }

        public bool IsInBlankArea(Vector2 mousePos)
        {
            bool isHitNode = IsHitNode(mousePos);
            return !NodeInfoWindowRect.Contains(mousePos) &&
                !PlayInfoWindowRect.Contains(mousePos) && !ToolbarRect.Contains(mousePos) && !isHitNode;
        }


        public bool IsHitNode(Vector2 pos)
        {
            if (GetNodeFromPosition(pos) == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public Node GetNodeFromPosition(Vector2 pos)
        {
            Debug.Log(pos);
            for (int i = 0; i < _canvas.nodeList.Count; i++)
            {
                Node node = _canvas.nodeList[i];
                Rect nodeTotalRect = node.TotalRect;
                // zoom
                // 可以选择不缩放Rect，而去缩放鼠标的点
                nodeTotalRect = ScaleRect(nodeTotalRect, _canvas.zoom, _zoomPivotPos);
                Debug.Log(nodeTotalRect);
                if (nodeTotalRect.Contains(pos))
                {
                    return node;
                }
            }
            return null;
        }

        public Rect ScaleRect(Rect rect, float scale, Vector2 pivotPoint)
        {

            Rect result = rect;
            // 以锚点为中心
            result.x -= pivotPoint.x;
            result.y -= pivotPoint.y;
            result.xMin *= scale;
            result.xMax *= scale;
            result.yMin *= scale;
            result.yMax *= scale;
            result.x += pivotPoint.x;
            result.y += pivotPoint.y;
            return result;
        }
    }


}


