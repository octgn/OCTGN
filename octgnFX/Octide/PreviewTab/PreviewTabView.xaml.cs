using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using Octide.Messages;
using System.Windows.Data;
using Octgn.Play;
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System;
using System.Drawing;
using Point = System.Windows.Point;
using Octide.ItemModel;
using Octide.ViewModel;

namespace Octide.Views
{
    public partial class PreviewTabView : UserControl
    {

        private bool mouseDown = false;
        private Point mouseDownOffset = new Point();
        private Canvas DragItem = null;
        HitType MouseHitType = HitType.None;


        public PreviewTabView()
        {
            InitializeComponent();
        }
        
        private enum HitType
        {
            None, Body, TL, TR, BR, BL, L, R, T, B
        };

        private HitType SetHitType(Rect rect, Point point)
        {
            if (point.X < 0) return HitType.None;
            if (point.X > rect.Width) return HitType.None;
            if (point.Y < 0) return HitType.None;
            if (point.Y > rect.Height) return HitType.None;

            const double GAP = 10;
            if (point.X < GAP)
            {
                // Left edge.
                if (point.Y < GAP) return HitType.TL;
                if (rect.Height - point.Y < GAP) return HitType.BL;
                return HitType.L;
    }
            else if (rect.Width - point.X < GAP)
            {
                // Right edge.
                if (point.Y < GAP) return HitType.TR;
                if (rect.Height - point.Y < GAP) return HitType.BR;
                return HitType.R;
            }
            if (point.Y < GAP) return HitType.T;
            if (rect.Height - point.Y < GAP) return HitType.B;
            return HitType.Body;
        }


        private void SetMouseCursor()
        {
            // See what cursor we should display.
            Cursor desired_cursor = Cursors.Arrow;
            switch (MouseHitType)
            {
                case HitType.None:
                    desired_cursor = Cursors.Arrow;
                    break;
                case HitType.Body:
                    desired_cursor = Cursors.ScrollAll;
                    break;
                case HitType.TL:
                case HitType.BR:
                    desired_cursor = Cursors.SizeNWSE;
                    break;
                case HitType.BL:
                case HitType.TR:
                    desired_cursor = Cursors.SizeNESW;
                    break;
                case HitType.T:
                case HitType.B:
                    desired_cursor = Cursors.SizeNS;
                    break;
                case HitType.L:
                case HitType.R:
                    desired_cursor = Cursors.SizeWE;
                    break;
            }
            
            if (Cursor != desired_cursor) Cursor = desired_cursor;
        }

        private Rect RectTransform(Rect rect, Point point)
        {
            switch (MouseHitType)
            {
                case HitType.Body:
                    rect.X = point.X - mouseDownOffset.X;
                    rect.Y = point.Y - mouseDownOffset.Y;
                    break;
                case HitType.B:
                    rect.Height = Math.Max(5, point.Y - rect.Y);
                    break;
                case HitType.T:
                    rect.Height = Math.Max(5, rect.Y + rect.Height - (point.Y - mouseDownOffset.Y));
                    rect.Y = point.Y - mouseDownOffset.Y;
                    break;
                case HitType.R:
                    rect.Width = Math.Max(5, point.X - rect.X);
                    break;
                case HitType.L:
                    rect.Width = Math.Max(5, rect.X + rect.Width - (point.X - mouseDownOffset.X));
                    rect.X = point.X - mouseDownOffset.X;
                    break;
                case HitType.TL:
                    rect.Height = Math.Max(5, rect.Y + rect.Height - (point.Y - mouseDownOffset.Y));
                    rect.Y = point.Y - mouseDownOffset.Y;
                    rect.Width = Math.Max(5, rect.X + rect.Width - (point.X - mouseDownOffset.X));
                    rect.X = point.X - mouseDownOffset.X;
                    break;
                case HitType.TR:
                    rect.Height = Math.Max(5, rect.Y + rect.Height - (point.Y - mouseDownOffset.Y));
                    rect.Y = point.Y - mouseDownOffset.Y;
                    rect.Width = Math.Max(5, point.X - rect.X);
                    break;
                case HitType.BL:
                    rect.Height = point.Y - rect.Y;
                    rect.Width = rect.X + rect.Width - (point.X - mouseDownOffset.X);
                    rect.X = point.X - mouseDownOffset.X;
                    break;
                case HitType.BR:
                    rect.Height = Math.Max(5, point.Y - rect.Y);
                    rect.Width = Math.Max(5, point.X - rect.X);
                    break;
            }
            return rect;
        }

        private void BoardMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragItem = sender as Canvas;
            BoardItemViewModel board = DragItem.DataContext as BoardItemViewModel;
            ViewModelLocator.PreviewTabViewModel.Selection = board;
            mouseDownOffset = e.GetPosition(DragItem);
            MouseHitType = SetHitType(new Rect(board.XPos, board.YPos, board.Width, board.Height), mouseDownOffset);
            if (MouseHitType == HitType.None) return;
            SetMouseCursor();
            mouseDown = true;
            boardView.CaptureMouse();
        }

        private void BoardViewMove(object sender, MouseEventArgs e)
        {
            if (!mouseDown)
            {
                Canvas itemCanvas = (sender as Canvas).Children[0] as Canvas;
                BoardItemViewModel board = itemCanvas.DataContext as BoardItemViewModel;
                MouseHitType = SetHitType(new Rect(board.XPos, board.YPos, board.Width, board.Height), e.GetPosition(itemCanvas));
                SetMouseCursor();
                return;
            }
            if (DragItem.DataContext is BoardItemViewModel)
            {
                base.OnMouseMove(e);
                e.Handled = true;

                var vm = DragItem.DataContext as BoardItemViewModel;
                Rect newrect = RectTransform(new Rect(vm.XPos, vm.YPos, vm.Width, vm.Height), e.GetPosition(boardView));
                vm.XPos = Convert.ToInt32(newrect.X);
                vm.YPos = Convert.ToInt32(newrect.Y);
                vm.Width = Convert.ToInt32(newrect.Width);
                vm.Height = Convert.ToInt32(newrect.Height);
                vm.RefreshValues();
            }
        }
        
        private void CardMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragItem = sender as Canvas;
            CardViewModel card = DragItem.DataContext as CardViewModel;
            if (e.ClickCount == 2)
            {
                card.IsBack = !card.IsBack;
                return;
            }
            ViewModelLocator.PreviewTabViewModel.Selection = card.Size;
            mouseDownOffset = e.GetPosition(DragItem);
            MouseHitType = SetHitType(new Rect(card.X, card.Y, card.CardWidth, card.CardHeight), mouseDownOffset);
            if (MouseHitType == HitType.None) return;
            SetMouseCursor();
            mouseDown = true;
            cardsView.CaptureMouse();
            
        }

        private void CardsViewMouseMove(object sender, MouseEventArgs e)
        {
            if (!mouseDown) return;
            if (DragItem.DataContext is CardViewModel)
            {
                base.OnMouseMove(e);
                e.Handled = true;

                var vm = DragItem.DataContext as CardViewModel;
                Rect rect = RectTransform(new Rect(vm.X, vm.Y, vm.CardWidth, vm.CardHeight), e.GetPosition(boardView));
                vm.X = Convert.ToInt32(rect.X);
                vm.Y = Convert.ToInt32(rect.Y);
                vm.CardWidth = Convert.ToInt32(rect.Width);
                vm.CardHeight = Convert.ToInt32(rect.Height);
                vm.RefreshValues();
            }
        }
        
        private void CardMouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown) return;
            CardViewModel card = (sender as Canvas).DataContext as CardViewModel;
            MouseHitType = SetHitType(new Rect(card.X, card.Y, card.CardWidth, card.CardHeight), e.GetPosition(sender as Canvas));
            SetMouseCursor();
            e.Handled = true;
            return;
        }

        private void ControlMouseUp(object sender, MouseButtonEventArgs e)
        {
            mouseDown = false;
            DragItem = null;
            cardsView.ReleaseMouseCapture();
            boardView.ReleaseMouseCapture();
        }

        private void CardMouseUp(object sender, MouseButtonEventArgs e)
        {
        }
        
        private void BoardMouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void CreateActionsMenu(object sender, MouseButtonEventArgs e)
        {
            var data = ((Border)sender).DataContext;
            var menu = new ContentControl
            {
                Content = ViewModelLocator.ActionMenuViewModel
            };

            if (data is CardViewModel)
            {
                ViewModelLocator.ActionMenuViewModel.Group = ViewModelLocator.PreviewTabViewModel.TableGroup;
                ViewModelLocator.ActionMenuViewModel.Card = (CardViewModel)data;
            }
            else
            {
                ViewModelLocator.ActionMenuViewModel.Group = (GroupItemViewModel)data;
                ViewModelLocator.ActionMenuViewModel.Card = null;
            }
      
            ActionMenuPopup.Child = menu;
            ActionMenuPopup.IsOpen = true;
            e.Handled = true;
        }

        private void ResetCursor(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }
        
    }
}