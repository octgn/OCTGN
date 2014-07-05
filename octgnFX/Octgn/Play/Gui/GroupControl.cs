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

        [Import] protected Engine ScriptEngine;

#pragma warning restore 649

        protected Group group;

        protected Card ContextCard;
        // don't make this static. Timing with Keep/ReleaseControl and ContextMenu can create bug when field is shared amongst groups.

        protected Group ContextGroup;
        // obviously this is equal to group. But as the control gets unload / reloaded, group gets null/non null in .NET 4.

        // Controls visibility of group actions in context menu
        protected bool ShowGroupActionsInContextMenu = true;

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

        public virtual void ExecuteDefaultAction(Card card)
        {
            if (!ExecuteDefaultCardAction(card)) ExecuteDefaultGroupAction();
        }

        protected GroupAction GetDefaultCardAction()
        {
            foreach (var a in group.Def.CardActions) {
                var ga = a as GroupAction;
                if (ga != null&& ga.DefaultAction) {
                    return ga;
                }
            }
            return null;
        }
        public virtual bool ExecuteDefaultCardAction(Card card)
        {
            var defaultCardAction = GetDefaultCardAction();
            if (defaultCardAction == null || !card.TryToManipulate()) 
                return false;
            group.KeepControl();
            card.KeepControl();
            if (defaultCardAction.Execute != null)
                ScriptEngine.ExecuteOnCards(defaultCardAction.Execute, Selection.ExtendToSelection(card));
            else if (defaultCardAction.BatchExecute != null)
                ScriptEngine.ExecuteOnBatch(defaultCardAction.BatchExecute, Selection.ExtendToSelection(card));
            group.ReleaseControl();
            card.ReleaseControl();
            return true;
        }

        protected GroupAction GetDefaultGroupAction()
        {
            foreach (var a in group.Def.GroupActions) {
                var ga = a as GroupAction;
                if (ga != null&& ga.DefaultAction) {
                    return ga;
                }
            }
            return null;
        }
        public virtual bool ExecuteDefaultGroupAction()
        {
            var defaultAction = GetDefaultGroupAction();
            if (defaultAction == null || !@group.TryToManipulate()) 
                return false;
            @group.KeepControl();
            if (defaultAction.Execute != null)
                ScriptEngine.ExecuteOnGroup(defaultAction.Execute, @group);
            @group.ReleaseControl();
            return true;
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

        protected virtual bool ShouldShowGroupActions(Card card) {
            return true;
        }

        internal virtual async void ShowContextMenu(Card card)
        {
            if (Player.LocalPlayer.Spectator)
                return;
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

            if (card != null)
            {
                var cardMenuItems = await CreateCardMenuItems(card, group.Definition);
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
            var items = new List<Control> {CreateGroupHeader()};
            if (!group.CanManipulate())
            {
                items.Add(CreateGroupHeader());

                var item = new MenuItem {Header = "Take control"};
                item.Click += delegate { group.TakeControl(); };
                items.Add(item);

                items.Add(new Separator());
                item = CreateLookAtCardsMenuItem();
                if (item != null) items.Add(item);
            } else {
                var tempActions = def.GroupActions.ToArray();
                int nGroupActions = def.GroupActions == null ? 0 : tempActions.Length;
                for (int i = 0; i < nGroupActions; i++)
                    items.Add(CreateActionMenuItem(tempActions[i], GroupActionClicked));

                if (nGroupActions > 0)
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

        private delegate Task<bool> actionFilter(IGroupAction action);
        protected virtual async Task<List<Control>> CreateCardMenuItems(Card card, DataNew.Entities.Group def)
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
            else {
                var selection = Selection.ExtendToSelection(card);
                actionFilter showCard = async (IGroupAction a) =>
                {
                    if (a.ShowIf != null)
                    {
                      return await CallActionShowIf(a.ShowIf, selection);
                    }
                    return true;
                };
                var visibleActionsTasks = def.CardActions.Select(item => new { Item = item, PredTask = showCard.Invoke(item) }).ToList();
                await TaskEx.WhenAll(visibleActionsTasks.Select(x => x.PredTask));
                var visibleActions = visibleActionsTasks.Where(x => x.PredTask.Result).Select(x => x.Item).ToArray();
                var nCardActions = visibleActions.Length;

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
                    items.AddRange(visibleActions.Select(action => CreateActionMenuItem(action, CardActionClicked)));
                    if (group.Controller == null)
                        items.Add(new Separator());
                }
                if (group.Controller == null)
                    items.Add(CreateCardPassToItem());
            }
            if (!card.FaceUp)
            {
              var peekItem = new MenuItem { Header = "Peek", InputGestureText = "Ctrl+P" };
              peekItem.Click += delegate { ContextCard.Peek(); };
              items.Add(peekItem);
            }

            return items;
        }
        
        private Task<bool> CallActionShowIf(string function, IEnumerable<Card> selection)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            ScriptEngine.ExecuteOnBatch(function, selection, null, (ExecutionResult result) => {
                bool ret = !System.String.IsNullOrWhiteSpace(result.Error) || result.ReturnValue as bool? == true;
                taskCompletionSource.SetResult(ret);
            });
            return taskCompletionSource.Task;
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
            var action = (GroupAction) ((MenuItem) sender).Tag;
            if (action.Execute != null)
                ScriptEngine.ExecuteOnGroup(action.Execute, group);
        }

        protected virtual void CardActionClicked(object sender, RoutedEventArgs e)
        {
            var action = (GroupAction)((MenuItem)sender).Tag;
            if (action.Execute != null)
                ScriptEngine.ExecuteOnCards(action.Execute, Selection.ExtendToSelection(ContextCard));
            else if (action.BatchExecute != null)
                ScriptEngine.ExecuteOnBatch(action.BatchExecute, Selection.ExtendToSelection(ContextCard));
        }

        private Control CreateActionMenuItem(IGroupAction baseAction, RoutedEventHandler onClick)
        {
            var separatorAction = baseAction as GroupActionSeparator;
            if (separatorAction != null) {
                return new Separator();
            }
            var item = new MenuItem {Header = baseAction.Name};

            var actionGroupDef = baseAction as GroupActionGroup;
            if (actionGroupDef != null)
            {
                item.Items.Add(actionGroupDef.Children.Select(subAction => CreateActionMenuItem(subAction, onClick)));
                return item;
            }

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