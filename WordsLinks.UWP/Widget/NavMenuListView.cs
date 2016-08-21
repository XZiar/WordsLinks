using Main.Util;
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using WordsLinks.UWP.ViewModel;

namespace WordsLinks.UWP.Widget
{
    class NavMenuListView : ListView
    {
        private SplitView host;

        /// <summary>
        /// Occurs when an item has been selected
        /// </summary>
        public event EventHandler<NavPageItem> PageSelected;

        public NavMenuListView()
        {
            SelectionMode = ListViewSelectionMode.Single;
            SingleSelectionFollowsFocus = false;
            IsItemClickEnabled = true;
            ItemClick += ItemClickedHandler;

            // Locate the hosting SplitView control
            
            this.Loaded += (s, a) =>
            {
                var parent = VisualTreeHelper.GetParent(this);
                while (parent != null && !(parent is SplitView))
                    parent = VisualTreeHelper.GetParent(parent);

                if (parent != null)
                {
                    host = parent as SplitView;

                    host.RegisterPropertyChangedCallback(SplitView.IsPaneOpenProperty, (sender, args) =>
                    {
                        OnPaneToggled();
                    });
                    host.RegisterPropertyChangedCallback(SplitView.DisplayModeProperty, (sender, args) =>
                    {
                        OnPaneToggled();
                    });

                    // Call once to ensure we're in the correct state
                    OnPaneToggled();
                }
            };
        }

        private void OnPaneToggled()
        {
            if (host.IsPaneOpen)
            {
                ItemsPanelRoot.ClearValue(WidthProperty);
                ItemsPanelRoot.ClearValue(HorizontalAlignmentProperty);
            }
            else if (host.DisplayMode == SplitViewDisplayMode.CompactInline ||
                host.DisplayMode == SplitViewDisplayMode.CompactOverlay)
            {
                ItemsPanelRoot.SetValue(WidthProperty, host.CompactPaneLength);
                ItemsPanelRoot.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Left);
            }
        }

        private void ItemClickedHandler(object sender, ItemClickEventArgs args)
        {
            // Triggered when the item is selected using something other than a keyboard
            NavPageItem npi = args.ClickedItem as NavPageItem;
            ListViewItem lvi = ContainerFromItem(npi) as ListViewItem;
            if (lvi == null)
                return;
            int index = IndexFromContainer(lvi);
            try
            {
                foreach (NavPageItem i in Items)
                    i.IsSelected = (i == npi);
            }
            catch(Exception e)
            {
                e.CopeWith("Single Select");
            }
            PageSelected?.Invoke(this, npi);

            host.IsPaneOpen = false;

            lvi.Focus(FocusState.Programmatic);
        }
    }
}
