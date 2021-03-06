﻿using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace DebugOS
{
    public class SmartContextBehaviour
    {
        private Timer timer;
        private ContentControl parent;
        private Popup popup;


        public bool IsContextPopupOpen { get; private set; }


        public double UpdateInterval
        {
            get { return timer.Interval; }
            set { timer.Interval = value; }
        }

        public List<ISmartContextHandler> Handlers { get; private set; }

        public SmartContextBehaviour(ContentControl parent)
        {
            if (parent == null) throw new ArgumentNullException("target");

            this.parent = parent;

            this.popup = new Popup() { Child = new SmartContext() };
            this.popup.MouseLeave += (s, v) => this.CloseContextPopup();

            this.popup.IsVisibleChanged += App.OnPopupVisChange;

            this.timer = new Timer(1500) { AutoReset = true };
            this.timer.Elapsed += OnTimerTick;
            this.timer.Start();

            this.Handlers = new List<ISmartContextHandler>();
        }

        HitTestResultBehavior ProcessHitTest(HitTestResult result)
        {
            UIElement obj = result.VisualHit as UIElement;

            if (obj == null || !obj.IsVisible)
            {
                return HitTestResultBehavior.Continue;
            }
            else
            {
                this.parent.Dispatcher.BeginInvoke((Action)delegate
                {

                    string content = null;

                    // Get control string content
                    if (obj is TextBlock)
                    {
                        content = ((TextBlock)obj).Text;
                    }
                    else if (obj is ContentControl)
                    {
                        var ctrl = (ContentControl)obj;

                        if (ctrl.Content is string)
                        {
                            content = ctrl.Content as string;
                        }
                    }

                    // Display the context popup
                    if (content != null)
                    {
                        this.ShowContextPopup(content, obj);
                    }
                });

                // We're done here
                return HitTestResultBehavior.Stop;
            }

        }

        void OnTimerTick(object sender, ElapsedEventArgs e)
        {
            // When popups are open, just return
            if (App.IsPopupOpen) return;

            this.parent.Dispatcher.Invoke((Action)delegate
            {
                // Hit test
                VisualTreeHelper.HitTest(this.parent, null, ProcessHitTest,
                    new PointHitTestParameters(Mouse.GetPosition(this.parent)));
            });
        }

        public void ShowContextPopup(string content, UIElement target)
        {
            if (target == null) throw new ArgumentNullException("target");

            this.parent.Dispatcher.Invoke((Action)delegate
            {
                var menu = (SmartContext)this.popup.Child;
                menu.LayoutRoot.Children.Clear();

                // Populate the smart popup with items
                foreach (ISmartContextHandler handler in this.Handlers)
                {
                    var items = handler.GetContextualUI(content, target);
                    if (items != null)
                    {
                        foreach (var item in items) {
                            menu.LayoutRoot.Children.Add(item);
                        }
                    }
                }

                // Don't show an empty popup
                if (menu.LayoutRoot.Children.Count == 0) return;

                this.popup.PlacementTarget = target;
                this.popup.Tag             = target;
                this.popup.IsOpen          = true;
                this.IsContextPopupOpen    = true;

                // Move to the mouse cursor
                this.popup.HorizontalOffset = Mouse.GetPosition(target).X;

                target.MouseLeave += this.OnTargetMouseLeave;
                this.timer.Stop();
            });
        }

        public void CloseContextPopup()
        {
            this.CloseContextPopup(null, null);
        }
        private void CloseContextPopup(object s, EventArgs e)
        {
            this.parent.Dispatcher.Invoke((Action)delegate
            {
                this.popup.IsOpen       = false;
                this.IsContextPopupOpen = false;

                if (this.popup.Tag != null)
                {
                    ((UIElement)this.popup.Tag).MouseLeave -= this.OnTargetMouseLeave;
                }
                this.timer.Start();
            });
        }

        private void OnTargetMouseLeave(object sender, MouseEventArgs e)
        {
            if (!this.popup.IsMouseOver)
            {
                this.CloseContextPopup();
            }
        }
    }
}
