using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OCTIDE
{
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    using MetroFramework.Forms;

    using WeifenLuo.WinFormsUI.Docking;

    public class MetroDock : MetroForm, IDockContent
    {
        private static readonly object DockStateChangedEvent = new object();

        private DockContentHandler m_dockHandler;

        private string m_tabText;

        [Browsable(false)]
        public DockContentHandler DockHandler
        {
            get
            {
                return this.m_dockHandler;
            }
        }

        [DefaultValue(true)]
        public bool AllowEndUserDocking
        {
            get
            {
                return this.DockHandler.AllowEndUserDocking;
            }
            set
            {
                this.DockHandler.AllowEndUserDocking = value;
            }
        }

        [DefaultValue(
            DockAreas.Float | DockAreas.DockLeft | DockAreas.DockRight | DockAreas.DockTop | DockAreas.DockBottom
            | DockAreas.Document)]
        public DockAreas DockAreas
        {
            get
            {
                return this.DockHandler.DockAreas;
            }
            set
            {
                this.DockHandler.DockAreas = value;
            }
        }

        [DefaultValue(0.25)]
        public double AutoHidePortion
        {
            get
            {
                return this.DockHandler.AutoHidePortion;
            }
            set
            {
                this.DockHandler.AutoHidePortion = value;
            }
        }

        [Localizable(true)]
        [DefaultValue(null)]
        public string TabText
        {
            get
            {
                return this.m_tabText;
            }
            set
            {
                this.DockHandler.TabText = this.m_tabText = value;
            }
        }

        [DefaultValue(true)]
        public bool CloseButton
        {
            get
            {
                return this.DockHandler.CloseButton;
            }
            set
            {
                this.DockHandler.CloseButton = value;
            }
        }

        [DefaultValue(true)]
        public bool CloseButtonVisible
        {
            get
            {
                return this.DockHandler.CloseButtonVisible;
            }
            set
            {
                this.DockHandler.CloseButtonVisible = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DockPanel DockPanel
        {
            get
            {
                return this.DockHandler.DockPanel;
            }
            set
            {
                this.DockHandler.DockPanel = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DockState DockState
        {
            get
            {
                return this.DockHandler.DockState;
            }
            set
            {
                this.DockHandler.DockState = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public DockPane Pane
        {
            get
            {
                return this.DockHandler.Pane;
            }
            set
            {
                this.DockHandler.Pane = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsHidden
        {
            get
            {
                return this.DockHandler.IsHidden;
            }
            set
            {
                this.DockHandler.IsHidden = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DockState VisibleState
        {
            get
            {
                return this.DockHandler.VisibleState;
            }
            set
            {
                this.DockHandler.VisibleState = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool IsFloat
        {
            get
            {
                return this.DockHandler.IsFloat;
            }
            set
            {
                this.DockHandler.IsFloat = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DockPane PanelPane
        {
            get
            {
                return this.DockHandler.PanelPane;
            }
            set
            {
                this.DockHandler.PanelPane = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DockPane FloatPane
        {
            get
            {
                return this.DockHandler.FloatPane;
            }
            set
            {
                this.DockHandler.FloatPane = value;
            }
        }

        [DefaultValue(false)]
        public bool HideOnClose
        {
            get
            {
                return this.DockHandler.HideOnClose;
            }
            set
            {
                this.DockHandler.HideOnClose = value;
            }
        }

        [DefaultValue(DockState.Unknown)]
        public DockState ShowHint
        {
            get
            {
                return this.DockHandler.ShowHint;
            }
            set
            {
                this.DockHandler.ShowHint = value;
            }
        }

        [Browsable(false)]
        public bool IsActivated
        {
            get
            {
                return this.DockHandler.IsActivated;
            }
        }

        [DefaultValue(null)]
        public ContextMenu TabPageContextMenu
        {
            get
            {
                return this.DockHandler.TabPageContextMenu;
            }
            set
            {
                this.DockHandler.TabPageContextMenu = value;
            }
        }

        [DefaultValue(null)]
        public ContextMenuStrip TabPageContextMenuStrip
        {
            get
            {
                return this.DockHandler.TabPageContextMenuStrip;
            }
            set
            {
                this.DockHandler.TabPageContextMenuStrip = value;
            }
        }

        [DefaultValue(null)]
        [Localizable(true)]
        [Category("Appearance")]
        public string ToolTipText
        {
            get
            {
                return this.DockHandler.ToolTipText;
            }
            set
            {
                this.DockHandler.ToolTipText = value;
            }
        }

        public event EventHandler DockStateChanged
        {
            add
            {
                this.Events.AddHandler(DockStateChangedEvent, (Delegate)value);
            }
            remove
            {
                this.Events.RemoveHandler(DockStateChangedEvent, (Delegate)value);
            }
        }

        static MetroDock()
        {
        }

        public MetroDock()
        {
            this.m_dockHandler = new DockContentHandler((Form)this, new GetPersistStringCallback(this.GetPersistString));
            this.m_dockHandler.DockStateChanged += new EventHandler(this.DockHandler_DockStateChanged);
            this.ParentChanged += new EventHandler(this.DockContent_ParentChanged);
            this.TopLevel = true;
        }

        private void DockContent_ParentChanged(object Sender, EventArgs e)
        {
            if (this.Parent == null) return;
            this.Font = this.Parent.Font;
        }

        private bool ShouldSerializeTabText()
        {
            return this.m_tabText != null;
        }

        protected virtual string GetPersistString()
        {
            return this.GetType().ToString();
        }

        public bool IsDockStateValid(DockState dockState)
        {
            return this.DockHandler.IsDockStateValid(dockState);
        }

        public new void Activate()
        {
            this.DockHandler.Activate();
        }

        public new void Hide()
        {
            this.DockHandler.Hide();
        }

        public new void Show()
        {
            this.DockHandler.Show();
        }

        public void Show(DockPanel dockPanel)
        {
            this.DockHandler.Show(dockPanel);
        }

        public void Show(DockPanel dockPanel, DockState dockState)
        {
            this.DockHandler.Show(dockPanel, dockState);
        }

        public void Show(DockPanel dockPanel, Rectangle floatWindowBounds)
        {
            this.DockHandler.Show(dockPanel, floatWindowBounds);
        }

        public void Show(DockPane pane, IDockContent beforeContent)
        {
            this.DockHandler.Show(pane, beforeContent);
        }

        public void Show(DockPane previousPane, DockAlignment alignment, double proportion)
        {
            this.DockHandler.Show(previousPane, alignment, proportion);
        }

        public void FloatAt(Rectangle floatWindowBounds)
        {
            this.DockHandler.FloatAt(floatWindowBounds);
        }

        public void DockTo(DockPane paneTo, DockStyle dockStyle, int contentIndex)
        {
            this.DockHandler.DockTo(paneTo, dockStyle, contentIndex);
        }

        public void DockTo(DockPanel panel, DockStyle dockStyle)
        {
            this.DockHandler.DockTo(panel, dockStyle);
        }

        void IDockContent.OnActivated(EventArgs e)
        {
            this.OnActivated(e);
        }

        void IDockContent.OnDeactivate(EventArgs e)
        {
            this.OnDeactivate(e);
        }

        private void DockHandler_DockStateChanged(object sender, EventArgs e)
        {
            this.OnDockStateChanged(e);
        }

        protected virtual void OnDockStateChanged(EventArgs e)
        {
            EventHandler eventHandler = (EventHandler)this.Events[DockStateChangedEvent];
            if (eventHandler == null) return;
            eventHandler((object)this, e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (this.DockHandler != null && this.DockPanel != null && this.DockPanel.SupportDeeplyNestedContent && this.IsHandleCreated)
                this.BeginInvoke(new Action<Form>((sen)=> base.OnSizeChanged(e)));
            else base.OnSizeChanged(e);
        }
    }
}
