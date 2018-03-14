﻿using Data_Manager2.Classes.DBManager;
using Data_Manager2.Classes.DBTables;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UWP_XMPP_Client.Classes;
using UWP_XMPP_Client.DataTemplates;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XMPP_API.Classes;
using XMPP_API.Classes.Network.XML.Messages;
using XMPP_API.Classes.Network.XML.Messages.XEP_0045.Configuration;

namespace UWP_XMPP_Client.Controls
{
    public sealed partial class MUCManageControl : UserControl
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public ChatTable Chat
        {
            get { return (ChatTable)GetValue(chatProperty); }
            set
            {
                SetValue(chatProperty, value);
                requestRoomInfo();
            }
        }
        public static readonly DependencyProperty chatProperty = DependencyProperty.Register("Chat", typeof(ChatTable), typeof(MUCManageControl), null);

        public XMPPClient Client
        {
            get { return (XMPPClient)GetValue(clientProperty); }
            set
            {
                SetValue(clientProperty, value);
                requestRoomInfo();
            }
        }
        public static readonly DependencyProperty clientProperty = DependencyProperty.Register("Client", typeof(XMPPClient), typeof(MUCManageControl), null);

        public MUCChatInfoTable MUCInfo
        {
            get { return (MUCChatInfoTable)GetValue(mucInfoProperty); }
            set
            {
                SetValue(mucInfoProperty, value);
                requestRoomInfo();
            }
        }
        public static readonly DependencyProperty mucInfoProperty = DependencyProperty.Register("MUCInfo", typeof(MUCChatInfoTable), typeof(MUCManageControl), null);

        private MessageResponseHelper<IQMessage> messageResponseHelper;
        private MessageResponseHelper<IQMessage> saveMessageResponseHelper;

        private CustomObservableCollection<MUCInfoOptionTemplate> options;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 07/02/2018 Created [Fabian Sauter]
        /// </history>
        public MUCManageControl()
        {
            this.options = new CustomObservableCollection<MUCInfoOptionTemplate>();
            this.messageResponseHelper = null;
            this.saveMessageResponseHelper = null;
            this.InitializeComponent();
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--


        #endregion

        #region --Misc Methods (Private)--
        private void requestRoomInfo()
        {
            if (messageResponseHelper != null || Client == null || Chat == null || MUCInfo == null)
            {
                return;
            }

            loading_grid.Visibility = Visibility.Visible;
            info_grid.Visibility = Visibility.Collapsed;
            timeout_stckpnl.Visibility = Visibility.Collapsed;

            string chatJID = Chat.chatJabberId;
            string chatID = Chat.id;
            string nickname = MUCInfo.nickname;

            Task.Run(async () =>
            {
                MUCOccupantTable member = MUCDBManager.INSTANCE.getMUCOccupant(chatID, nickname);
                if (member != null && MUCInfo.state == Data_Manager2.Classes.MUCState.ENTERD)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => messageResponseHelper = Client.MUC_COMMAND_HELPER.requestRoomConfiguration(Chat.chatJabberId, member.affiliation, onNewMessage, onTimeout));
                }
                else
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        timeout_stckpnl.Visibility = Visibility.Visible;
                        loading_grid.Visibility = Visibility.Collapsed;
                        reload_btn.IsEnabled = true;

                        notificationBanner_ian.Show("Failed to request information!\nIt seems like you are no member or have not entered this room. Please reconnect and/or retry.");
                    });
                }
            });
        }

        private bool onNewMessage(IQMessage iq)
        {
            if (iq is RoomInfoMessage)
            {
                RoomInfoMessage responseMessage = iq as RoomInfoMessage;

                // Add controls and update viability:
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    options.Clear();
                    roomConfigType_tbx.Text = "Configuration level: " + Utils.mucAffiliationToString(responseMessage.configType);
                    foreach (MUCInfoField o in responseMessage.roomConfig.options)
                    {
                        if (o.type != MUCInfoFieldType.HIDDEN)
                        {
                            options.Add(new MUCInfoOptionTemplate() { option = o });
                        }
                    }
                    reload_btn.IsEnabled = true;
                    notificationBanner_ian.Dismiss();
                    timeout_stckpnl.Visibility = Visibility.Collapsed;
                    loading_grid.Visibility = Visibility.Collapsed;
                    info_grid.Visibility = Visibility.Visible;
                }).AsTask();
                return true;
            }
            else if (iq is IQErrorMessage)
            {
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    options.Clear();
                    retry_btn.IsEnabled = true;
                    info_grid.Visibility = Visibility.Collapsed;
                    loading_grid.Visibility = Visibility.Collapsed;
                    timeout_stckpnl.Visibility = Visibility.Visible;

                    notificationBanner_ian.Show("Failed to request configuration! Server responded:\n" + (iq as IQErrorMessage).ERROR_MESSAGE ?? "null");
                }).AsTask();
            }
            return false;
        }

        private void onTimeout()
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                retry_btn.IsEnabled = true;
                notificationBanner_ian.Dismiss();
                info_grid.Visibility = Visibility.Collapsed;
                loading_grid.Visibility = Visibility.Collapsed;
                timeout_stckpnl.Visibility = Visibility.Visible;
            }).AsTask();
        }

        private void save()
        {
            if (Client == null || Chat == null)
            {
                return;
            }

            save_prgr.Visibility = Visibility.Visible;
            save_prgr.IsActive = true;
            save_btn.IsEnabled = false;

            List<MUCInfoField> list = new List<MUCInfoField>();
            foreach (MUCInfoOptionTemplate t in options)
            {
                list.Add(t.option);
            }

            string chatId = Chat.id;
            string nickname = MUCInfo.nickname;
            Task.Run(async () =>
            {
                MUCOccupantTable member = MUCDBManager.INSTANCE.getMUCOccupant(chatId, nickname);
                if (member == null)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => notificationBanner_ian.Show("Failed to save!\nSeams like you are no member of the room any more. Please rejoin the room and try again."));
                    return;
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    saveMessageResponseHelper = Client.MUC_COMMAND_HELPER.saveRoomConfiguration(Chat.chatJabberId, new RoomConfiguration(list), member.affiliation, onSaveMessage, onSaveTimeout);
                });
            });
        }

        private bool onSaveMessage(IQMessage msg)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (msg is IQErrorMessage)
                {
                    notificationBanner_ian.Show("Failed to save! Server responded:\n" + (msg as IQErrorMessage).ERROR_MESSAGE ?? "null");
                }
                else
                {
                    switch (msg.getMessageType())
                    {
                        case IQMessage.RESULT:
                            notificationBanner_ian.Show("Successfully saved room configuration.", 5000);
                            break;
                        case IQMessage.ERROR:
                        default:
                            notificationBanner_ian.Show("Failed to save! Unknown error. Please retry.");
                            break;
                    }
                }

                save_prgr.Visibility = Visibility.Collapsed;
                save_prgr.IsActive = false;
                save_btn.IsEnabled = true;
            }).AsTask();
            return true;
        }

        private void onSaveTimeout()
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                notificationBanner_ian.Show("Failed to save!\nServer did not respond in time.");

                save_prgr.Visibility = Visibility.Collapsed;
                save_prgr.IsActive = false;
                save_btn.IsEnabled = true;
            }).AsTask();
        }

        private void reload()
        {
            reload_btn.IsEnabled = false;
            retry_btn.IsEnabled = false;
            messageResponseHelper?.Dispose();
            messageResponseHelper = null;
            requestRoomInfo();
        }

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--
        private void retry_btn_Click(object sender, RoutedEventArgs e)
        {
            reload();
        }

        private void reload_btn_Click(object sender, RoutedEventArgs e)
        {
            reload();
        }

        private void save_btn_Click(object sender, RoutedEventArgs e)
        {
            save();
        }

        #endregion
    }
}
