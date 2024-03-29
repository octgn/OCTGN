﻿// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octide.ItemModel;
using Octide.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Point = System.Windows.Point;

namespace Octide.Views
{
    public partial class PreviewTabView : UserControl
    {

        private bool mouseDown = false;
        private Point mouseDownOffset = new Point();
        private FrameworkElement DragItem = null;
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
            DragItem = sender as FrameworkElement;
            BoardItemModel board = DragItem.DataContext as BoardItemModel;
            ViewModelLocator.PreviewTabViewModel.Selection = board;
            mouseDownOffset = e.GetPosition(DragItem);
            MouseHitType = SetHitType(new Rect(board.XPos, board.YPos, board.Width, board.Height), mouseDownOffset);
            if (MouseHitType == HitType.None) return;
            SetMouseCursor();
            mouseDown = true;
            boardView.CaptureMouse();
            e.Handled = true;
        }

        private void BoardMove(object sender, MouseEventArgs e)
        {
            FrameworkElement obj = sender as FrameworkElement;
            if (!mouseDown)
            {
                BoardItemModel board = obj.DataContext as BoardItemModel;
                MouseHitType = SetHitType(new Rect(board.XPos, board.YPos, board.Width, board.Height), e.GetPosition(obj));
                SetMouseCursor();
                return;
            }
            if (DragItem.DataContext is BoardItemModel)
            {
                base.OnMouseMove(e);
                e.Handled = true;

                var vm = DragItem.DataContext as BoardItemModel;

                Rect newrect = RectTransform(new Rect(vm.XPos, vm.YPos, vm.Width, vm.Height), e.GetPosition(TableCanvas));
                vm.XPos = Convert.ToInt32(newrect.X);
                vm.YPos = Convert.ToInt32(newrect.Y);
                vm.Width = Convert.ToInt32(newrect.Width);
                vm.Height = Convert.ToInt32(newrect.Height);
                vm.RefreshValues();
            }
        }

        private void CardMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragItem = sender as FrameworkElement;
            SampleCardItemModel card = DragItem.DataContext as SampleCardItemModel;
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
            DragItem.CaptureMouse();
            e.Handled = true;

        }

        private void CardMouseMove(object sender, MouseEventArgs e)
        {
            FrameworkElement obj = sender as FrameworkElement;
            if (!mouseDown)
            {
                SampleCardItemModel card = obj.DataContext as SampleCardItemModel;
                MouseHitType = SetHitType(new Rect(card.X, card.Y, card.CardWidth, card.CardHeight), e.GetPosition(obj));
                SetMouseCursor();
                return;
            }
            if (DragItem.DataContext is SampleCardItemModel)
            {
                base.OnMouseMove(e);
                e.Handled = true;

                var vm = DragItem.DataContext as SampleCardItemModel;
                Rect rect = RectTransform(new Rect(vm.X, vm.Y, vm.CardWidth, vm.CardHeight), e.GetPosition(TableCanvas));
                vm.X = Convert.ToInt32(rect.X);
                vm.Y = Convert.ToInt32(rect.Y);
                vm.CardWidth = Convert.ToInt32(rect.Width);
                vm.CardHeight = Convert.ToInt32(rect.Height);
                vm.RefreshValues();
            }
        }

        private void ControlMouseUp(object sender, MouseButtonEventArgs e)
        {
            mouseDown = false;
            DragItem?.ReleaseMouseCapture();
            DragItem = null;
        }

        private void CardMouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void BoardMouseUp(object sender, MouseButtonEventArgs e)
        {

        }
        private void CreateTableActionsMenu(object sender, MouseButtonEventArgs e)
        {
            ActionMenuPopup.DataContext = ViewModelLocator.PreviewTabViewModel.Table;
            GroupActionsPanel.Visibility = Visibility.Visible;
            CardActionsPanel.Visibility = Visibility.Collapsed;
            ActionMenuPopup.IsOpen = true;
            e.Handled = true;
        }
        private void CreateCardActionsMenu(object sender, MouseButtonEventArgs e)
        {
            ActionMenuPopup.DataContext = ViewModelLocator.PreviewTabViewModel.Table;
            GroupActionsPanel.Visibility = Visibility.Collapsed;
            CardActionsPanel.Visibility = Visibility.Visible;
            ActionMenuPopup.IsOpen = true;
            e.Handled = true;
        }
        private void CreatePileActionsMenu(object sender, MouseButtonEventArgs e)
        {
            ActionMenuPopup.DataContext = ((FrameworkElement)sender).DataContext;
            ActionMenuPopup.Tag = "group";
            GroupActionsPanel.Visibility = Visibility.Visible;
            CardActionsPanel.Visibility = Visibility.Visible;
            ActionMenuPopup.IsOpen = true;
            e.Handled = true;
        }

        private void ResetCursor(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        private void ClickGroupAction(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ViewModelLocator.PreviewTabViewModel.Selection = e.NewValue;
        }

        private void ClickCardAction(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ViewModelLocator.PreviewTabViewModel.Selection = e.NewValue;
        }
    }
}