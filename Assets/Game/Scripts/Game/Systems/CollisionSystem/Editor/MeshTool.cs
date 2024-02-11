using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Systems.CollisionSystem
{
    public class MeshTool : EditorTool
    {
        private GUIContent toolContent;
        public override GUIContent toolbarIcon => toolContent ?? (toolContent = new GUIContent
        {
            image = (Texture)EditorGUIUtility.Load("GameEditor/Level.png"),
            text = "Mesh Tool",
            tooltip = "Mesh Tool"
        });
        private VisualElement toolRootElement;

        public override void OnActivated()
        {
            //Create the UI
            toolRootElement = new VisualElement();
            // toolRootElement.style.width = 200;
            // var backgroundColor = EditorGUIUtility.isProSkin
            //     ? new Color(0.21f, 0.21f, 0.21f, 0.8f)
            //     : new Color(0.8f, 0.8f, 0.8f, 0.8f);
            // toolRootElement.style.backgroundColor = backgroundColor;
            // toolRootElement.style.marginLeft = 10f;
            // toolRootElement.style.marginBottom = 10f;
            // toolRootElement.style.paddingTop = 5f;
            // toolRootElement.style.paddingRight = 5f;
            // toolRootElement.style.paddingLeft = 5f;
            // toolRootElement.style.paddingBottom = 5f;
        }
        
        public override void OnWillBeDeactivated()
        {
            toolRootElement?.RemoveFromHierarchy();
            SceneView.beforeSceneGui -= BeforeSceneGUI;
        }

        public override void OnToolGUI( EditorWindow window )
        {
            //If we're not in the scene view, we're not the active tool, we don't have a placeable object, exit.
            if (!(window is SceneView))
                return;

            if (!ToolManager.IsActiveTool(this))
                return;
            
            //Force the window to repaint.
            window.Repaint();
        }
        
        private void BeforeSceneGUI(SceneView sceneView)
        {
            if (!ToolManager.IsActiveTool(this))
                return;
        }
    }
}