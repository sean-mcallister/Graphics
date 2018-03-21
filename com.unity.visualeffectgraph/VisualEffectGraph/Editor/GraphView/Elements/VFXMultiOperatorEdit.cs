using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Experimental.UIElements.StyleSheets;
using UnityEngine.Experimental.VFX;
using UnityEditor.VFX.UIElements;

using VFXEditableOperator = UnityEditor.VFX.VFXOperatorMultiplyNew;

namespace UnityEditor.VFX.UI
{
    class VFXMultiOperatorEdit : VFXReorderableList, IControlledElement<VFXOperatorController>
    {
        VFXOperatorController m_Controller;
        Controller IControlledElement.controller
        {
            get { return m_Controller; }
        }
        public VFXOperatorController controller
        {
            get { return m_Controller; }
            set
            {
                if (m_Controller != value)
                {
                    if (m_Controller != null)
                    {
                        m_Controller.UnregisterHandler(this);
                    }
                    m_Controller = value;
                    if (m_Controller != null)
                    {
                        m_Controller.RegisterHandler(this);
                    }
                }
            }
        }

        VFXEditableOperator model
        {
            get
            {
                if (controller == null)
                    return null;

                return controller.model as VFXEditableOperator;
            }
        }


        protected override void ElementMoved(int movedIndex, int targetIndex)
        {
            base.ElementMoved(movedIndex, targetIndex);
            model.OperandMoved(movedIndex, targetIndex);
        }

        public VFXMultiOperatorEdit()
        {
            RegisterCallback<ControllerChangedEvent>(OnChange);
        }

        public override void OnAdd()
        {
            model.AddOperand();
        }

        public override void OnRemove(int index)
        {
            model.RemoveOperand(index);
        }

        int m_CurrentIndex = -1;
        void OnTypeMenu(Label button, int index)
        {
            VFXEditableOperator op = model;
            GenericMenu menu = new GenericMenu();
            int selectedIndex = -1;
            VFXValueType selectedType = op.GetOperandType(index);
            int cpt = 0;
            foreach (var type in op.validTypes)
            {
                if (selectedType == type)
                    selectedIndex = cpt++;
                menu.AddItem(EditorGUIUtility.TrTextContent(type.ToString()), selectedType == type, OnChangeType, type);
            }
            m_CurrentIndex = index;
            menu.DropDown(button.worldBound);
        }

        void OnChangeType(object type)
        {
            VFXEditableOperator op = model;

            op.SetOperandType(m_CurrentIndex, (VFXValueType)type);
        }

        void OnChangeLabel(string value, int index)
        {
            if (!m_SelfChanging)
            {
                VFXEditableOperator op = model;

                if (value != op.GetOperandName(index)) // test mandatory because TextField might send ChangeEvent anytime
                    op.SetOperandName(index, value);
            }
        }

        void OnChange(ControllerChangedEvent e)
        {
            if (e.controller == controller)
            {
                SelfChange();
            }
        }

        bool m_SelfChanging;

        void SelfChange()
        {
            m_SelfChanging = true;
            VFXEditableOperator op = model;
            int count = op.operandCount;

            bool sizeChanged = false;

            while (itemCount < count)
            {
                AddItem(new OperandInfo(this, op, itemCount));
                sizeChanged = true;
            }
            while (itemCount > count)
            {
                RemoveItemAt(itemCount - 1);
                sizeChanged = true;
            }

            for (int i = 0; i < count; ++i)
            {
                OperandInfo operand = ItemAt(i) as OperandInfo;
                operand.index = i; // The operand might have been changed by the drag
                operand.Set(op);
            }

            VFXOperatorUI opUI = GetFirstAncestorOfType<VFXOperatorUI>();
            if (opUI != null)
            {
                opUI.ForceRefreshLayout();
            }
            m_SelfChanging = false;
        }

        class OperandInfo : VisualElement
        {
            VFXStringField field;
            Label type;
            VFXMultiOperatorEdit m_Owner;

            public int index;

            public OperandInfo(VFXMultiOperatorEdit owner, VFXEditableOperator op, int index)
            {
                this.AddStyleSheetPathWithSkinVariant("VFXControls");
                m_Owner = owner;
                field = new VFXStringField("name");
                field.OnValueChanged = () => owner.OnChangeLabel(field.value, index);
                type = new Label();
                this.index = index;
                type.AddToClassList("PopupButton");
                type.AddManipulator(new DownClickable(() => owner.OnTypeMenu(type, index)));
                Set(op);

                Add(field);
                Add(type);
            }

            public void Set(VFXEditableOperator op)
            {
                field.value = op.GetOperandName(index);
                type.text = op.GetOperandType(index).ToString();
            }
        }
    }
}
