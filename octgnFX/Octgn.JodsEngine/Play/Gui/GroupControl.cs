using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Threading.Tasks;

using Octgn.Scripting;

namespace Octgn.Play.Gui
{
    using System.Reflection;
    using Octgn.Core.DataExtensionMethods;
    using Octgn.DataNew.Entities;

    using log4net;
    using Card = Octgn.Play.Card;
    using Group = Octgn.Play.Group;
    using Player = Octgn.Play.Player;

    public class GroupControl : UserControl
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
#pragma warning disable 649   // Unassigned variable: it's initialized by MEF

        [Import]
        protected Engine ScriptEngine;

#pragma warning restore 649

        protected Group group;

        protected Card ContextCard;
        // don't make this static. Timing with Keep/ReleaseControl and ContextMenu can create bug when field is shared amongst groups.

        protected Group ContextGroup;
        // obviously this is equal to group. But as the control gets unload / reloaded, group gets null/non null in .NET 4.

        private DateTime _lastTurnCall = DateTime.MinValue;
        private long _turnAnimationDelay;
        public static FontFamily groupFont;
        public static int fontsize;

        public Group Group
        {
            get { return group; }
        }


        public GroupControl()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            this.Loaded += OnLoaded;

            AddHandler(CardControl.CardOverEvent, new CardsEventHandler(OnCardOver));
            AddHandler(CardControl.CardOutEvent, new CardsEventHandler(OnCardOut));
            AddHandler(CardControl.CardDroppedEvent, new CardsEventHandler(OnCardDropped));
            AddHandler(TableControl.TableKeyEvent, new EventHandler<TableKeyEventArgs>(OnKeyShortcut));

            Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (Program.GameEngine != null)
                        Program.GameEngine.ComposeParts(this);
                }));
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

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Loaded -= OnLoaded;
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
            if (match.ActionDef.AsAction().Execute != null)
                ScriptEngine.ExecuteOnGroup(match.ActionDef.AsAction().Execute, @group);
            e.Handled = e.KeyEventArgs.Handled = true;
        }

        protected virtual void GroupChanged()
        {
        }

        private GroupAction GetDefaultCardAction()
        {
            return (GroupAction)group.Def.CardActions.FirstOrDefault((IGroupAction baseAction) =>
            {
                var action = baseAction as GroupAction;
                return action != null && action.DefaultAction;
            });
        }

        private GroupAction GetDefaultGroupAction()
        {
            return (GroupAction)group.Def.GroupActions.FirstOrDefault((IGroupAction baseAction) =>
            {
                var action = baseAction as GroupAction;
                return action != null && action.DefaultAction;
            });
        }

        public virtual void ExecuteDefaultAction(Card card)
        {
            if (!ExecuteDefaultCardAction(card)) ExecuteDefaultGroupAction();
        }

        public virtual bool ExecuteDefaultCardAction(Card card)
        {
            var defaultCardAction = GetDefaultCardAction();
            if (defaultCardAction == null || !card.TryToManipulate())
                return false;
            group.KeepControl();
            card.KeepControl();
            if (defaultCardAction.IsBatchExecutable)
                ScriptEngine.ExecuteOnBatch(defaultCardAction.Execute, Selection.ExtendToSelection(card));
            else
                ScriptEngine.ExecuteOnCards(defaultCardAction.Execute, Selection.ExtendToSelection(card));
            group.ReleaseControl();
            card.ReleaseControl();
            return true;
        }

        public virtual bool ExecuteDefaultGroupAction()
        {
            var defaultGroupAction = GetDefaultGroupAction();
            if (defaultGroupAction == null || !@group.TryToManipulate())
                return false;
            @group.KeepControl();
            if (defaultGroupAction.Execute != null)
                ScriptEngine.ExecuteOnGroup(defaultGroupAction.Execute, @group);
            @group.ReleaseControl();
            return true;
        }

        internal long GetTurnAnimationDelay()
        {
            var ticksSinceLastCall = DateTime.Now.Ticks - _lastTurnCall.Ticks;
            var delay = _turnAnimationDelay - ticksSinceLastCall;
            if (delay < 0) delay = 0;
            _turnAnimationDelay = delay + 50;
            _lastTurnCall = DateTime.Now;
            return delay;
        }

        #region Context Menus

        protected virtual bool ShouldShowGroupActions(Card card)
        {
            return true;
        }

        protected Point position;

        internal virtual void ShowContextMenu(Card card)
        {
            if (Player.LocalPlayer.Spectator || Program.GameEngine.IsReplay)
                return;
            // Modify selection
            if (card == null || !card.Selected) Selection.Clear();

            var menuItems = new CompositeCollection();
            ContextGroup = group;
            ContextMenu = new ContextMenu { ItemsSource = menuItems, Tag = card };
            // card has to captured somehow, otherwise it may be overwritten before released in the OnClosed handler, e.g. when rightclicking on a card, then directly right-clicking on another card.
            ContextMenu.Opened += (sender, args) =>
                                      {
                                          ContextGroup.KeepControl();
                                          var c = ((ContextMenu)sender).Tag as Card;
                                          if (c != null) c.KeepControl();
                                      };
            ContextMenu.Closed += (sender, args) =>
                                      {
                                          ContextGroup.ReleaseControl();
                                          var c = ((ContextMenu)sender).Tag as Card;
                                          if (c != null) c.ReleaseControl();
                                      };

            ContextCard = card;
            menuItems.Clear();

            if (card != null)
            {
                //var cardMenuItems = await CreateCardMenuItems(card, group.Definition);
                var cardMenuItems = CreateCardMenuItems(card, group.Definition);
                var container = new CollectionContainer { Collection = cardMenuItems };
                menuItems.Add(container);
            }

            if (ShouldShowGroupActions(card))
            {
                var container = new CollectionContainer { Collection = CreateGroupMenuItems(group.Definition) };
                menuItems.Add(container);
            }
            //else // Group is being shuffled
            //    return;

            ContextMenu.IsOpen = false;
            // Required to trigger the ReleaseControl calls if the ContextMenu was already open
            ContextMenu.UpdateLayout(); // Required if the ContextMenu was already open
            ContextMenu.IsOpen = true;
            ContextMenu.FontFamily = groupFont;
            ContextMenu.FontSize = fontsize;
        }

        protected virtual List<Control> CreateGroupMenuItems(DataNew.Entities.Group def)
        {
            var items = new List<Control> { CreateGroupHeader() };
            if (!group.CanManipulate())
            {
                items.Add(CreateGroupHeader());

                var item = new MenuItem { Header = "Take control" };
                item.Click += delegate { group.TakeControl(); };
                items.Add(item);

                items.Add(new Separator());
                item = CreateLookAtCardsMenuItem();
                if (item != null) items.Add(item);
            }
            else
            {
                var actions = def.GroupActions;
                var nGroupActions = actions.ToArray().Length;
                
                if (nGroupActions > 0)
                    items.AddRange(actions.Select(action => CreateActionMenuItem(action, GroupActionClicked, null)).Where(x => x.Visibility == Visibility.Visible));
                    items.Add(new Separator());

                if (group.Controller != null)
                    items.Add(CreateGroupPassToItem());
                if (group.Visibility != GroupVisibility.Undefined)
                    items.Add(CreateVisibilityItem());
                MenuItem item = CreateLookAtCardsMenuItem();
                if (item != null)
                    items.Add(item);
                if (def.Id == Program.GameEngine.Definition.Table.Id)
                {
                    if (!(items.Last() is Separator))
                        items.Add(new Separator());
                    var noteItem = new MenuItem() { Header = "Create Note" };
                    noteItem.Click += NoteItemOnClick;
                    items.Add(noteItem);
                }

                if (items.Last() is Separator)
                    items.RemoveAt(items.Count - 1);
            }
            return items;
        }

        private void NoteItemOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var a = this as TableControl;
            if (a == null) return;

            a.AddNote(a.ContextMenuNotesMousePosition.X, a.ContextMenuNotesMousePosition.Y);
        }
        
        protected virtual List<Control> CreateCardMenuItems(Card card, DataNew.Entities.Group def)
        {
            var items = new List<Control>();
            if (!card.CanManipulate())
            {
                var item = new MenuItem { Header = card.Name, Background = card.Controller.TransparentBrush };
                item.SetResourceReference(StyleProperty, "MenuHeader");
                items.Add(item);

                item = new MenuItem { Header = "Take control" };
                item.Click += delegate { card.TakeControl(); };
                items.Add(item);
                return items;
            }
            else
            {
                var actions = def.CardActions.ToList();
                var nCardActions = actions.ToArray().Length;

                if (nCardActions > 0 || group.Controller == null)
                {
                    var cardHeader = new MenuItem();
                    cardHeader.SetResourceReference(StyleProperty, "MenuHeader");
                    cardHeader.Header = card.Name;
                    cardHeader.Background = card.Controller.TransparentBrush;
                    items.Add(cardHeader);
                }
                if (nCardActions > 0)
                {
                    items.AddRange(actions.Select(action => CreateActionMenuItem(action, CardActionClicked, card)).Where(x => x.Visibility == Visibility.Visible));
                    if (group.Controller == null)
                        items.Add(new Separator());
                }
                if (group.Controller == null)
                    items.Add(CreateCardPassToItem());
            }
            if (def.Id == Program.GameEngine.Definition.Table.Id)
            {
                var ami = new MenuItem() { Header = card.Anchored ? "Unanchor" : "Anchor" };
                ami.Click += (sender, args) => ContextCard.SetAnchored(false, card.Anchored == false);
                items.Add(ami);
            }
            if (!card.FaceUp)
            {
                var peekItem = new MenuItem { Header = "Peek", InputGestureText = "Ctrl+P" };
                peekItem.Click += delegate { ContextCard.Peek(); };
                items.Add(peekItem);
            }
            return items;
        }

        //private Task<bool> CallActionShowIf(string function, IEnumerable<Card> selection)
        private bool CallActionConditionalExecute(IGroupAction baseAction, IEnumerable<Card> selection)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            if (baseAction.IsGroup)
            {
                ScriptEngine.ExecuteOnGroup(baseAction.ShowExecute, @group, position, (ExecutionResult result) =>
                {
                    bool ret = !System.String.IsNullOrWhiteSpace(result.Error) || result.ReturnValue as bool? == true;
                    taskCompletionSource.SetResult(ret);
                });
            }
            else
            {
                ScriptEngine.ExecuteOnBatch(baseAction.ShowExecute, selection, position, (ExecutionResult result) =>
                {
                    bool ret = !System.String.IsNullOrWhiteSpace(result.Error) || result.ReturnValue as bool? == true;
                    taskCompletionSource.SetResult(ret);
                });
            }
            return taskCompletionSource.Task.Result;
        }

        private string CallActionNameExecute(IGroupAction baseAction, IEnumerable<Card> selection)
        {
            var taskCompletionSource = new TaskCompletionSource<string>();
            if (baseAction.IsGroup)
            {
                ScriptEngine.ExecuteOnGroup(baseAction.HeaderExecute, @group, position, (ExecutionResult result) =>
                {
                    string ret = result.ReturnValue as string;
                    taskCompletionSource.SetResult(ret);
                });
            }
            else
            {
                ScriptEngine.ExecuteOnBatch(baseAction.HeaderExecute, selection, position, (ExecutionResult result) =>
                {
                    string ret = result.ReturnValue as string;
                    taskCompletionSource.SetResult(ret);
                });
            }
            return taskCompletionSource.Task.Result;
        }

        private MenuItem CreateVisibilityItem()
        {
            var item = new MenuItem { Header = "Visibility" };
            var playerItem = new MenuItem { Header = "Nobody", IsCheckable = true };
            playerItem.Click += delegate { group.SetVisibility(false, true); };
            item.Items.Add(playerItem);
            playerItem = new MenuItem { Header = "Everybody", IsCheckable = true };
            playerItem.Click += delegate { group.SetVisibility(true, true); };
            item.Items.Add(playerItem);
            // HACK: this is a quick hack to enable some BlueMoon card scenario. One should find a better solution
            //playerItem = new MenuItem { Header = "Freeze current cards" };
            //playerItem.Click += delegate { group.FreezeCardsVisibility(true); };
            //item.Items.Add(playerItem);
            item.Items.Add(new Separator());
            item.SubmenuOpened += delegate
                                      {
                                          ((MenuItem)item.Items[0]).IsChecked = group.Visibility ==
                                                                                 GroupVisibility.Nobody;
                                          ((MenuItem)item.Items[1]).IsChecked = group.Visibility ==
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
                                                                          var clickedItem = (MenuItem)sender;
                                                                          var player = (Player)clickedItem.Tag;
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
            var passToItem = new MenuItem { Header = "Pass control to" };
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
                                                                                    (Player)((MenuItem)sender).Tag;
                                                                                @group.PassControlTo(p);
                                                                            };
                                                    passToItem.Items.Add(playerItem);
                                                }
                                                if (passToItem.HasItems)
                                                {
                                                }
                                                else
                                                {
                                                    var emptyItem = new MenuItem { Header = "no player", IsEnabled = false };
                                                    passToItem.Items.Add(emptyItem);
                                                }
                                            };
            return passToItem;
        }

        private MenuItem CreateCardPassToItem()
        {
            var passToItem = new MenuItem { Header = "Pass control to" };
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
                                                                                    (Player)((MenuItem)sender).Tag;
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
                                                    var emptyItem = new MenuItem { Header = "no player", IsEnabled = false };
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
            var action = (GroupAction)((MenuItem)sender).Tag;
            if (action.Execute != null)
                ScriptEngine.ExecuteOnGroup(action.Execute, group);
        }

        protected virtual void CardActionClicked(object sender, RoutedEventArgs e)
        {
            var action = (GroupAction)((MenuItem)sender).Tag;
            if (action.IsBatchExecutable)
                ScriptEngine.ExecuteOnBatch(action.Execute, Selection.ExtendToSelection(ContextCard));
            else
                ScriptEngine.ExecuteOnCards(action.Execute, Selection.ExtendToSelection(ContextCard));
        }
        
        private Control CreateActionMenuItem(IGroupAction baseAction, RoutedEventHandler onClick, Card card)
        {
            var selection = card == null ? Enumerable.Empty<Card>() : Selection.ExtendToSelection(card);
            bool showAction = true;
            if (baseAction.ShowExecute != null) showAction = CallActionConditionalExecute(baseAction, selection);
            if (!showAction) return new MenuItem() { Visibility = Visibility.Collapsed };

            //action is a separator
            var separatorAction = baseAction as GroupActionSeparator;
            if (separatorAction != null) return new Separator();

            string newName = baseAction.Name;
            if (baseAction.HeaderExecute != null)
                {
                    var name = CallActionNameExecute(baseAction, selection);
                    if (name != null) newName = name;
                }
            var item = new MenuItem { Header = newName };

            //action is a submenu
            var actionGroupDef = baseAction as GroupActionSubmenu;
            if (actionGroupDef != null)
            {
                foreach (var i in actionGroupDef.Children.Select(subAction => CreateActionMenuItem(subAction, onClick, card)).Where(x => x.Visibility == Visibility.Visible))
                    item.Items.Add(i);
                if (item.Items.Count == 0) return new MenuItem() { Visibility = Visibility.Collapsed };
                return item;
            }

            //action is a proper action
            var action = baseAction as GroupAction;
            item.Tag = action;
            
            if (action != null)
            {
                item.InputGestureText = action.Shortcut;
                if (action.DefaultAction)
                {
                    item.FontWeight = FontWeights.Bold;
                }
            }
            item.Click += onClick;
            return item;
        }

        private MenuItem CreateGroupHeader()
        {
            var header = new MenuItem { Header = group.Name };
            header.SetResourceReference(StyleProperty, "MenuHeader");
            header.Background = @group.Controller != null
                                    ? @group.Controller.TransparentBrush
                                    : new SolidColorBrush(Color.FromArgb(100, 100, 100, 100));
            return header;
        }

        #endregion
    }
}