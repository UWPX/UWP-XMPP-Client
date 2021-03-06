﻿using System.Linq;
using Manager.Classes;
using Shared.Classes;
using Shared.Classes.Collections;
using XMPP_API.Classes.Network;

namespace UWPX_UI_Context.Classes.DataTemplates.Dialogs
{
    public sealed class AccountSelectionControlDataTemplate: AbstractDataTemplate
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public readonly CustomObservableCollection<ClientDataTemplate> CLIENTS = new CustomObservableCollection<ClientDataTemplate>(true);

        private ClientDataTemplate _SelectedItem;
        public ClientDataTemplate SelectedItem
        {
            get => _SelectedItem;
            set => SetProperty(ref _SelectedItem, value);
        }

        private bool _HasClients;
        public bool HasClients
        {
            get => _HasClients;
            set => SetProperty(ref _HasClients, value);
        }

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        public AccountSelectionControlDataTemplate()
        {
            HasClients = false;
            CLIENTS.CollectionChanged += CLIENTS_CollectionChanged;
            LoadClients();
            SetSelectedItem();
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        private void SetSelectedItem()
        {
            foreach (ClientDataTemplate client in CLIENTS)
            {
                if (client.ConnectionState == ConnectionState.CONNECTED)
                {
                    SelectedItem = client;
                    return;
                }
            }

            if (SelectedItem is null && CLIENTS.Count > 0)
            {
                SelectedItem = CLIENTS[0];
            }
        }

        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--


        #endregion

        #region --Misc Methods (Private)--
        private void LoadClients()
        {
            CLIENTS.Clear();
            CLIENTS.AddRange(ConnectionHandler.INSTANCE.GetClients().Select((x) => new ClientDataTemplate(x.client)));
        }

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--
        private void CLIENTS_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            HasClients = CLIENTS.Count > 0;
        }

        #endregion
    }
}
