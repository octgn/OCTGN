using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Octgn.Definitions;
using Octgn.Scripting;
using Octgn.Utils;

namespace Octgn.Play.Gui
{
    public class GroupControl : UserControl
    {
#pragma warning disable 649   // Unassigned variable: it's initialized by MEF

        [Import] protected Engine ScriptEngine;

#pragma warning restore 649

        protected Group group;

        private readonly CollectionContainer _groupMenu = new CollectionContainer();
        private readonly CollectionContainer _cardMenu = new CollectionContainer();

        protected Card ContextCard;
        // don't make this static. Timing with Keep/ReleaseControl and ContextMenu can create bug when field is shared amongst groups.

        protected Group ContextGroup;
        // obviously this is equal to group. But as the control gets unload / reloaded, group gets null/non null in .NET 4.

        // now if a context menu is open when the group is unloaded (e.g. change player tab), group gets null before ReleaseControl gets called => NPE.
        private MenuItem _cardHeader;
        private ActionDef _defaultCardAction, _defaultGroupAction;
        private int _turnAnimationTimestamp, _turnAnimationDelay;
        public static FontFamily groupFont;
        public static int fontsize;

        public Group Group
        {
            get { return group; }
        }

        public GroupControl()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            Program.Game.ComposeParts(this);

            AddHandler(CardControl.CardOverEvent, new CardsEventHandler(OnCardOver));
            AddHandler(CardControl.CardOutEvent, new CardsEventHandler(OnCardOut));
            AddHandler(CardControl.CardDroppedEvent, new CardsEventHandler(OnCardDropped));
            AddHandler(TableControl.TableKeyEvent, new EventHandler<TableKeyEventArgs>(OnKeyShortcut));

            DataContextChanged += delegate
                                      {
                                          group = DataContext as Group;
                                          if (group != null)
                                              GroupChanged();
                                      };

            ContextMenuOpening += delegate(object sender, ContextMenuEventArgs e)
                                      {
                                          e.Handled = true;
                                          ShowContextMenu(null);
                                      };
        }

        internal virtual ItemContainerGenerator GetItemContainerGenerator()
        {
            return null;
        }

        protected virtual void OnCardOver(object sender, CardsEventArgs e)
        {
            e.Handled = e.CanDrop = true;
            e.Handler = this;
            switch (group.Visibility)
            {
                case GroupVisibility.Nobody:
                    e.FaceUp = false;
                    break;
                case GroupVisibility.Everybody:
                    e.FaceUp = true;
                    break;
                case GroupVisibility.Owner:
                    e.FaceUp = group.Owner == Player.LocalPlayer;
                    break;
                case GroupVisibility.Undefined:
                    if (e.ClickedCard.Group != group)
                        e.FaceUp = null;
                    else
                        e.FaceUp = e.ClickedCard.FaceUp;
                    break;
                case GroupVisibility.Custom:
                    e.FaceUp = group.Viewers.Contains(Player.LocalPlayer);
                    break;
                default:
                    throw new NotImplementedException("Unknown visibility : " + group.Visibility);
            }
        }

        protected virtual void OnCardOut(object sender, CardsEventArgs e)
        {
            // Made to be overriden
        }

        protected virtual void OnCardDropped(object sender, CardsEventArgs e)
        {
            // The method cannot be abstract, because abstract classes can't be named in XAML (e.g. TableControl.xaml)
            throw new NotImplementedException("OnCardDropped must be overriden");
        }

        protected virtual void OnKeyShortcut(object sender, TableKeyEventArgs e)
        {
            ActionShortcut[] shortcuts = group.GroupShortcuts;
            ActionShortcut match = shortcuts.FirstOrDefault(shortcut => shortcut.Key.Matches(this, e.KeyEventArgs));
            if (match == null || !@group.TryToManipulate()) return;
            if (match.ActionDef.Execute != null)
                ScriptEngine.ExecuteOnGroup(match.ActionDef.Execute, @group);
            e.Handled = e.KeyEventArgs.Handled = true;
        }

        protected virtual void GroupChanged()
        {
            CreateContextMenus();
        }

        public void ExecuteDefaultAction(Card card)
        {
            if (_defaultCardAction != null)
            {
                if (!card.TryToManipulate()) return;
                group.KeepControl();
                card.KeepControl();
                if (_defaultCardAction.Execute != null)
                    ScriptEngine.ExecuteOnCards(_defaultCardAction.Execute, Selection.ExtendToSelection(card));
                else if (_defaultCardAction.BatchExecute != null)
                    ScriptEngine.ExecuteOnBatch(_defaultCardAction.BatchExecute, Selection.ExtendToSelection(card));
                group.ReleaseControl();
                card.ReleaseControl();
            }
            else
                ExecuteDefaultAction();
        }

        public void ExecuteDefaultAction()
        {
            if (_defaultGroupAction == null) return;
            if (!@group.TryToManipulate()) return;
            @group.KeepControl();
            if (_defaultGroupAction.Execute != null)
                ScriptEngine.ExecuteOnGroup(_defaultGroupAction.Execute, @group);
            @group.ReleaseControl();
        }

        internal int GetTurnAnimationDelay()
        {
            int currentTimestamp = Environment.TickCount;
            int delay = _turnAnimationDelay - (currentTimestamp - _turnAnimationTimestamp);
            if (delay < 0) delay = 0;
            _turnAnimationDelay = delay + 50;
            _turnAnimationTimestamp = currentTimestamp;
            return delay;
        }

        #region Context Menus

        internal virtual void ShowContextMenu(Card card, bool showGroupActions = true)
        {
            // Modify selection
            if (card == null || !card.Selected) Selection.Clear();

            var menuItems = new CompositeCollection();            
            ContextGroup = group;
            ContextMenu = new ContextMenu {ItemsSource = menuItems, Tag = card};
            // card has to captured somehow, otherwise it may be overwritten before released in the OnClosed handler, e.g. when rightclicking on a card, then directly right-clicking on another card.
            ContextMenu.Opened += (sender, args) =>
                                      {
                                          ContextGroup.KeepControl();
                                          var c = ((ContextMenu) sender).Tag as Card;
                                          if (c != null) c.KeepControl();
                                      };
            ContextMenu.Closed += (sender, args) =>
                                      {
                                          ContextGroup.ReleaseControl();
                                          var c = ((ContextMenu) sender).Tag as Card;
                                          if (c != null) c.ReleaseControl();
                                      };

            ContextCard = card;
            menuItems.Clear();

            if (group.CanManipulate())
            {
                if (card != null)
                {
                    if (card.CanManipulate())
                    {
                        if (_cardHeader != null)
                        {
                            _cardHeader.Header = card.Name;
                            _cardHeader.Background = card.Controller.TransparentBrush;
                            menuItems.Add(_cardMenu);
                        }
                    }
                    else
                    {
                        var item = new MenuItem {Header = card.Name, Background = card.Controller.TransparentBrush};
                        item.SetResourceReference(StyleProperty, "MenuHeader");
                        menuItems.Add(item);

                        item = new MenuItem {Header = "Take control"};
                        item.Click += delegate { card.TakeControl(); };
                        menuItems.Add(item);
                    }

                    if (!card.FaceUp)
                    {
                        var peekItem = new MenuItem {Header = "Peek", InputGestureText = "Ctrl+P"};
                        peekItem.Click += delegate { ContextCard.Peek(); };
                        if (menuItems.Count == 0)
                        {
                            var item = new MenuItem {Header = card.Name, Background = card.Owner.TransparentBrush};
                            item.SetResourceReference(StyleProperty, "MenuHeader");
                            menuItems.Add(item);
                        }
                        menuItems.Add(peekItem);
                    }
                }

                if (showGroupActions)
                    menuItems.Add(_groupMenu);
            }
            else if (!group.WantToShuffle)
            {
                menuItems.Add(CreateGroupHeader());

                var item = new MenuItem {Header = "Take control"};
                item.Click += delegate { group.TakeControl(); };
                menuItems.Add(item);

                menuItems.Add(new Separator());
                item = CreateLookAtCardsMenuItem();
                if (item != null) menuItems.Add(item);
            }
            else // Group is being shuffled
                return;

            ContextMenu.IsOpen = false;
            // Required to trigger the ReleaseControl calls if the ContextMenu was already open
            ContextMenu.UpdateLayout(); // Required if the ContextMenu was already open
            ContextMenu.IsOpen = true;
            ContextMenu.FontFamily = groupFont;
            ContextMenu.FontSize = fontsize;
        }

        private void CreateContextMenus()
        {
            _cardHeader = null;
            _defaultGroupAction = _defaultCardAction = null;

            GroupDef def = group.Definition;

            // Create the card actions
            List<Control> cardItems = CreateCardMenuItems(def);
            _cardMenu.Collection = cardItems;

            // Create the group actions
            List<Control> groupItems = CreateGroupMenuItems(def);
            _groupMenu.Collection = groupItems;
        }

        protected virtual List<Control> CreateGroupMenuItems(GroupDef def)
        {
            int nGroupActions = def.GroupActions == null ? 0 : def.GroupActions.Length;
            var items = new List<Control> {CreateGroupHeader()};
            for (int i = 0; i < nGroupActions; i++)
                if (def.GroupActions != null) items.Add(CreateGroupMenuItem(def.GroupActions[i]));

            if (nGroupActions > 0)
                items.Add(new Separator());

            if (group.Controller != null)
                items.Add(CreateGroupPassToItem());
            if (group.Visibility != GroupVisibility.Undefined)
                items.Add(CreateVisibilityItem());
            MenuItem item = CreateLookAtCardsMenuItem();
            if (item != null)
                items.Add(item);

            if (items.Last() is Separator)
                items.RemoveAt(items.Count - 1);

            return items;
        }

        protected virtual List<Control> CreateCardMenuItems(GroupDef def)
        {
            int nCardActions = def.CardActions == null ? 0 : def.CardActions.Length;
            var items = new List<Control>();

            if (nCardActions > 0 || group.Controller == null)
            {
                _cardHeader = new MenuItem();
                _cardHeader.SetResourceReference(StyleProperty, "MenuHeader");
                items.Add(_cardHeader);
            }
            if (nCardActions > 0)
            {
                for (int i = 0; i < nCardActions; i++)
                    if (def.CardActions != null) items.Add(CreateCardMenuItem(def.CardActions[i]));
                if (group.Controller == null)
                    items.Add(new Separator());
            }
            if (group.Controller == null)
                items.Add(CreateCardPassToItem());

            return items;
        }

        private MenuItem CreateVisibilityItem()
        {
            var item = new MenuItem {Header = "Visibility"};
            var playerItem = new MenuItem {Header = "Nobody", IsCheckable = true};
            playerItem.Click += delegate { group.SetVisibility(false, true); };
            item.Items.Add(playerItem);
            playerItem = new MenuItem {Header = "Everybody", IsCheckable = true};
            playerItem.Click += delegate { group.SetVisibility(true, true); };
            item.Items.Add(playerItem);
            // HACK: this is a quick hack to enable some BlueMoon card scenario. One should find a better solution
            //playerItem = new MenuItem { Header = "Freeze current cards" };
            //playerItem.Click += delegate { group.FreezeCardsVisibility(true); };
            //item.Items.Add(playerItem);
            item.Items.Add(new Separator());
            item.SubmenuOpened += delegate
                                      {
                                          ((MenuItem) item.Items[0]).IsChecked = group.Visibility ==
                                                                                 GroupVisibility.Nobody;
                                          ((MenuItem) item.Items[1]).IsChecked = group.Visibility ==
                                                                                 GroupVisibility.Everybody;
                                          //((MenuItem)item.Items[2]).IsChecked = group.Visibility == GroupVisibility.Undefined;
                                          //while (item.Items.Count > 4) item.Items.RemoveAt(item.Items.Count - 1);
                                          while (item.Items.Count > 3) item.Items.RemoveAt(item.Items.Count - 1);
                                          foreach (Player p in Player.AllExceptGlobal)
                                          {
                                              playerItem = new MenuItem
                                                               {
                                                                   Header = p.Name,
                                                                   Tag = p,
                                                                   IsCheckable = true,
                                                                   IsChecked =
                                                                       group.Visibility == GroupVisibility.Custom &&
                                                                       group.Viewers.Contains(p),
                                                               };
                                              playerItem.Click += delegate(object sender, RoutedEventArgs e)
                                                                      {
                                                                          var clickedItem = (MenuItem) sender;
                                                                          var player = (Player) clickedItem.Tag;
                                                                          if (clickedItem.IsChecked)
                                                                              group.AddViewer(player, true);
                                                                          else
                                                                              group.RemoveViewer(player, true);
                                                                      };
                                              item.Items.Add(playerItem);
                                          }
                                      };
            return item;
        }

        private MenuItem CreateGroupPassToItem()
        {
            var passToItem = new MenuItem {Header = "Pass control to"};
            passToItem.Items.Add(new MenuItem()); // dummy item
            passToItem.SubmenuOpened += delegate
                                            {
                                                passToItem.Items.Clear();
                                                foreach (MenuItem playerItem in from player in Player.AllExceptGlobal
                                                                                where player != Player.LocalPlayer
                                                                                select new MenuItem
                                                                                           {
                                                                                               Header = player.Name,
                                                                                               Tag = player
                                                                                           })
                                                {
                                                    playerItem.Click += delegate(object sender, RoutedEventArgs e)
                                                                            {
                                                                                var p =
                                                                                    (Player) ((MenuItem) sender).Tag;
                                                                                @group.PassControlTo(p);
                                                                            };
                                                    passToItem.Items.Add(playerItem);
                                                }
                                                if (passToItem.HasItems)
                                                {
                                                }
                                                else
                                                {
                                                    var emptyItem = new MenuItem
                                                                        {Header = "no player", IsEnabled = false};
                                                    passToItem.Items.Add(emptyItem);
                                                }
                                            };
            return passToItem;
        }

        private MenuItem CreateCardPassToItem()
        {
            var passToItem = new MenuItem {Header = "Pass control to"};
            passToItem.Items.Add(new MenuItem()); // dummy item
            passToItem.SubmenuOpened += delegate
                                            {
                                                passToItem.Items.Clear();
                                                foreach (MenuItem playerItem in from player in Player.AllExceptGlobal
                                                                                where player != Player.LocalPlayer
                                                                                select new MenuItem
                                                                                           {
                                                                                               Header = player.Name,
                                                                                               Tag = player
                                                                                           })
                                                {
                                                    playerItem.Click += delegate(object sender, RoutedEventArgs e)
                                                                            {
                                                                                var p =
                                                                                    (Player) ((MenuItem) sender).Tag;
                                                                                Selection.Do(
                                                                                    c => c.PassControlTo(p),
                                                                                    ContextCard);
                                                                            };
                                                    passToItem.Items.Add(playerItem);
                                                }
                                                if (passToItem.HasItems)
                                                {
                                                }
                                                else
                                                {
                                                    var emptyItem = new MenuItem
                                                                        {Header = "no player", IsEnabled = false};
                                                    passToItem.Items.Add(emptyItem);
                                                }
                                            };
            return passToItem;
        }

        protected virtual MenuItem CreateLookAtCardsMenuItem()
        {
            // Do nothing: only piles define this...
            return null;
        }

        protected virtual void GroupActionClicked(object sender, RoutedEventArgs e)
        {
            var action = (ActionDef) ((MenuItem) sender).Tag;
            if (action.Execute != null)
                ScriptEngine.ExecuteOnGroup(action.Execute, group);
        }

        protected virtual void CardActionClicked(object sender, RoutedEventArgs e)
        {
            var action = (ActionDef) ((MenuItem) sender).Tag;
            if (action.Execute != null)
                ScriptEngine.ExecuteOnCards(action.Execute, Selection.ExtendToSelection(ContextCard));
            else if (action.BatchExecute != null)
                ScriptEngine.ExecuteOnBatch(action.BatchExecute, Selection.ExtendToSelection(ContextCard));
        }

        private MenuItem CreateGroupMenuItem(BaseActionDef baseAction)
        {
            var item = new MenuItem {Header = baseAction.Name};

            var actionGroupDef = baseAction as ActionGroupDef;
            if (actionGroupDef != null)
            {
                foreach (MenuItem subItem in actionGroupDef.Children.Select(CreateGroupMenuItem))
                    item.Items.Add(subItem);
                return item;
            }

            var action = baseAction as ActionDef;
            item.Tag = action;
            if (action != null)
            {
                item.InputGestureText = action.Shortcut;
                if (action.DefaultAction)
                {
                    item.FontWeight = FontWeights.Bold;
                    _defaultGroupAction = action;
                }
            }
            item.Click += GroupActionClicked;
            return item;
        }

        private MenuItem CreateCardMenuItem(BaseActionDef baseAction)
        {
            var item = new MenuItem {Header = baseAction.Name};

            var actionGroupDef = baseAction as ActionGroupDef;
            if (actionGroupDef != null)
            {
                foreach (MenuItem subItem in actionGroupDef.Children.Select(CreateCardMenuItem))
                    item.Items.Add(subItem);
                return item;
            }

            var action = baseAction as ActionDef;
            item.Tag = action;
            if (action != null)
            {
                item.InputGestureText = action.Shortcut;
                if (action.DefaultAction)
                {
                    item.FontWeight = FontWeights.Bold;
                    _defaultCardAction = action;
                }
            }
            item.Click += CardActionClicked;
            return item;
        }

        private MenuItem CreateGroupHeader()
        {
            var header = new MenuItem {Header = group.Name};
            header.SetResourceReference(StyleProperty, "MenuHeader");
            header.Background = @group.Controller != null
                                    ? @group.Controller.TransparentBrush
                                    : new SolidColorBrush(Color.FromArgb(100, 100, 100, 100));
            return header;
        }

        #endregion
    }
}