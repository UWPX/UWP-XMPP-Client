﻿using Data_Manager2.Classes;
using UWP_XMPP_Client.Classes;
using UWP_XMPP_Client.DataTemplates;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UWP_XMPP_Client.Pages.SettingsPages
{
    public sealed partial class PersonalizeSettingsPage : Page
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        private CustomObservableCollection<BackgroundImageTemplate> backgroundImages;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 04/09/2017 Created [Fabian Sauter]
        /// </history>
        public PersonalizeSettingsPage()
        {
            this.InitializeComponent();
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += AbstractBackRequestPage_BackRequested;
            backgroundImages = BackgroundImageCache.backgroundImages;
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--


        #endregion

        #region --Misc Methods (Private)--


        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--
        private void AbstractBackRequestPage_BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                return;
            }
            if (rootFrame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }

        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(e.ClickedItem is BackgroundImageTemplate)
            {
                BackgroundImageTemplate img = e.ClickedItem as BackgroundImageTemplate;
                img.selected = true;
                if(BackgroundImageCache.selectedImage != null)
                {
                    BackgroundImageCache.selectedImage.selected = false;
                }
                BackgroundImageCache.selectedImage = img;
                Settings.setSetting(SettingsConsts.CHAT_BACKGROUND_IMAGE_NAME, img.name);

            }
        }
        #endregion
    }
}
