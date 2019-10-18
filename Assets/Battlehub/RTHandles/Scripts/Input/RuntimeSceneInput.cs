﻿using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTHandles
{
    public class RuntimeSceneInput : RuntimeSelectionInput
    {
        public KeyCode FocusKey = KeyCode.F;
        public KeyCode SnapToGridKey = KeyCode.G;
        public KeyCode RotateKey = KeyCode.LeftAlt;
        public KeyCode RotateKey2 = KeyCode.RightAlt;
        public KeyCode RotateKey3 = KeyCode.AltGr;
        
        public float RotateXSensitivity = 5.0f;
        public float RotateYSensitivity = 5.0f;
        public float MoveZSensitivity = 1.0f;
        public float FreeMoveSensitivity = 0.25f;
        public float FreeRotateSensitivity = 7.5f;
        
        private bool m_rotate;
        private bool m_pan;
        private bool m_freeMove;
        private bool m_isActive;

        protected RuntimeSceneComponent SceneComponent
        {
            get { return (RuntimeSceneComponent)m_component; }
        }

        protected virtual bool AllowRotateAction()
        {
            IInput input = m_component.Editor.Input;
            return input.GetPointer(0);
        }

        protected virtual bool RotateAction()
        {
            IInput input = m_component.Editor.Input;
            return input.GetKey(RotateKey) ||
                input.GetKey(RotateKey2) ||
                input.GetKey(RotateKey3);
        }

        protected virtual bool PanAction()
        {
            IInput input = m_component.Editor.Input;
            RuntimeTools tools = m_component.Editor.Tools;
            return input.GetPointer(2) || input.GetPointer(0) && tools.Current == RuntimeTool.View && tools.ActiveTool == null;
        }

        protected virtual bool FreeMoveAction()
        {
            IInput input = m_component.Editor.Input;
            RuntimeTools tools = m_component.Editor.Tools;
            return input.GetPointer(1);
        }

        protected virtual bool FocusAction()
        {
            IInput input = m_component.Editor.Input;
            return input.GetKeyDown(FocusKey);
        }

        protected virtual bool SnapToGridAction()
        {
            IInput input = m_component.Editor.Input;
            return input.GetKeyDown(SnapToGridKey) && input.GetKey(ModifierKey);
        }

        protected virtual Vector2 RotateAxes()
        {
            IInput input = m_component.Editor.Input;
            float deltaX = input.GetAxis(InputAxis.X);
            float deltaY = input.GetAxis(InputAxis.Y);
            return new Vector2(deltaX, deltaY);
        }

        protected virtual float ZoomAxis()
        {
            IInput input = m_component.Editor.Input;
            float deltaZ = input.GetAxis(InputAxis.Z);
            return deltaZ;
        }

        protected virtual Vector3 MoveAxes()
        {
            IInput input = m_component.Editor.Input;
            float deltaX = input.GetAxis(InputAxis.Horizontal);
            float deltaY = input.GetAxis(InputAxis.Vertical);
            float deltaZ = 0;
            if (input.GetKey(KeyCode.Q))
            {
                deltaZ = 0.5f;
            }
            else if (input.GetKey(KeyCode.E))
            {
                deltaZ = -0.5f;
            }
            return new Vector3(deltaX, deltaY, deltaZ);
        }

        protected override void Start()
        {
            base.Start();
            m_component.Editor.ActiveWindowChanged += Editor_ActiveWindowChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(m_component != null && m_component.Editor != null)
            {
                m_component.Editor.ActiveWindowChanged -= Editor_ActiveWindowChanged;
            }
        }

        private void Editor_ActiveWindowChanged(RuntimeWindow deactivatedWindow)
        {
            if(m_component != null)
            {
                if(m_isActive)
                {
                    SceneComponent.UpdateCursorState(false, false, false, false);
                    m_pan = false;
                    m_rotate = false;
                    m_freeMove = false;
                }
                m_isActive = m_component.IsWindowActive;
            }
        }

        protected override void LateUpdate()
        {
            if (!m_component.IsWindowActive)
            {
                return;
            }

            bool isPointerOverAndSelected = m_component.Window.IsPointerOver;// && m_component.IsUISelected;

            IInput input = m_component.Editor.Input;
            RuntimeTools tools = m_component.Editor.Tools;

            bool canRotate = AllowRotateAction();
            bool rotate = RotateAction();
            bool pan = PanAction();
            bool freeMove = FreeMoveAction();

            if(pan && tools.Current != RuntimeTool.View)
            {
                rotate = false;
            }

            bool beginRotate = m_rotate != rotate && rotate;
            if(beginRotate && !isPointerOverAndSelected)
            {
                rotate = false;
                beginRotate = false;
            }
            bool endRotate = m_rotate != rotate && !rotate;
            m_rotate = rotate;
            
            bool beginPan = m_pan != pan && pan;
            if(beginPan && !isPointerOverAndSelected)
            {
                pan = false;
            }
            bool endPan = m_pan != pan && !pan;
            m_pan = pan;

            bool beginFreeMove = m_freeMove != freeMove && freeMove;
            if(beginFreeMove && !isPointerOverAndSelected)
            {
                freeMove = false;
            }
            bool endFreeMove = m_freeMove != freeMove && !freeMove;
            m_freeMove = freeMove;

            Vector3 pointerPosition = input.GetPointerXY(0);
            tools.IsViewing = m_rotate || m_pan || m_freeMove;

            if (beginPan || endPan || beginRotate || endRotate || beginFreeMove || endFreeMove)
            {
                SceneComponent.UpdateCursorState(true, m_pan, m_rotate, m_freeMove);
            } 
            if (m_freeMove)
            {
                SceneComponent.FreeMove(RotateAxes() * FreeRotateSensitivity, MoveAxes() * FreeMoveSensitivity, ZoomAxis());
            }
            else if (m_rotate)
            {
                if(canRotate)
                {
                    Vector2 orbitAxes = RotateAxes();  
                    SceneComponent.Orbit(orbitAxes.x * RotateXSensitivity, orbitAxes.y * RotateYSensitivity, 0);
                }
            }
            else if(m_pan)
            {
                if (beginPan)
                {
                    SceneComponent.BeginPan(pointerPosition);
                }
                SceneComponent.Pan(pointerPosition);
            }
            else
            {
                SceneComponent.FreeMove(Vector2.zero, Vector3.zero, 0);

                if (isPointerOverAndSelected)
                {
                    SceneComponent.Zoom(ZoomAxis() * MoveZSensitivity);
                  
                    if (SelectAction())
                    {
                        SelectGO();
                    }

                    if (SnapToGridAction())
                    {
                        SceneComponent.SnapToGrid();
                    }

                    if (FocusAction())
                    {
                        SceneComponent.Focus();
                    }

                    if(SelectAllAction())
                    {
                        SceneComponent.SelectAll();
                    }
                }   
            }
        }
    }

}

