﻿using System.Threading.Tasks;
using Data_Manager2.Classes;
using Logging;
using UWPX_UI_Context.Classes.DataTemplates;
using UWPX_UI_Context.Classes.DataTemplates.Controls.Chat.MUC;
using UWPX_UI_Context.Classes.DataTemplates.Controls.IoT;
using Windows.UI.Xaml;
using XMPP_API.Classes.Network.XML.Messages;
using XMPP_API.Classes.Network.XML.Messages.Helper;
using XMPP_API.Classes.Network.XML.Messages.XEP_0004;
using XMPP_API.Classes.Network.XML.Messages.XEP_0045;
using XMPP_API.Classes.Network.XML.Messages.XEP_0045.Configuration;

namespace UWPX_UI_Context.Classes.DataContext.Controls.Chat.MUC
{
    public class MucConfigurationControlContext
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public readonly MucConfigurationControlDataTemplate MODEL = new MucConfigurationControlDataTemplate();

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--


        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        private void SetError(string errorMessage)
        {
            MODEL.ErrorMarkdownText = errorMessage;
        }

        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        public void UpdateView(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ChatDataTemplate oldChat)
            {
                oldChat.PropertyChanged -= Chat_PropertyChanged;
            }

            if (e.NewValue is ChatDataTemplate newChat)
            {
                newChat.PropertyChanged -= Chat_PropertyChanged;
                newChat.PropertyChanged += Chat_PropertyChanged;
                OnChatChanged(newChat);
                return;
            }
            MODEL.IsAvailable = false;
        }

        public void Reload(ChatDataTemplate chat)
        {
            if (chat.MucInfo is null || chat.MucInfo.affiliation != MUCAffiliation.OWNER)
            {
                MODEL.IsAvailable = false;
                return;
            }
            MODEL.IsAvailable = true;
            RequestConfiguartion(chat);
        }

        public void Save(ChatDataTemplate chat)
        {
            Task.Run(async () =>
            {
                MODEL.IsLoading = true;
                MODEL.Form.Form.type = DataFormType.SUBMIT;
                MessageResponseHelperResult<IQMessage> result = await chat.Client.MUC_COMMAND_HELPER.saveRoomConfigurationAsync(chat.Chat.chatJabberId, MODEL.Form.Form);
                if (result.STATE != MessageResponseHelperResultState.SUCCESS)
                {
                    SetError("Failed to save room configuration:\n**" + result.STATE + "**");
                    Logger.Warn("Failed to save the room configuration for '" + chat.Chat.chatJabberId + "': " + result.STATE);
                }
                else if (result.RESULT is IQErrorMessage errorMessage)
                {
                    SetError("Failed to save room configuration:\n**" + errorMessage + "**");
                    Logger.Warn("Failed to save the room configuration for '" + chat.Chat.chatJabberId + "': " + errorMessage);
                }
                else
                {
                    SetError("");
                    Logger.Info("Successfully saved the room configuration for '" + chat.Chat.chatJabberId + '\'');
                }
                MODEL.IsLoading = false;
            });
        }

        #endregion

        #region --Misc Methods (Private)--
        private void OnChatChanged(ChatDataTemplate chat)
        {
            if (!(chat.MucInfo is null) && chat.MucInfo.affiliation == MUCAffiliation.OWNER && chat.MucInfo.state == MUCState.ENTERD)
            {
                MODEL.IsAvailable = true;
                RequestConfiguartion(chat);
                return;
            }
            MODEL.IsAvailable = false;
        }

        private void RequestConfiguartion(ChatDataTemplate chat)
        {
            Task.Run(async () =>
            {
                MODEL.IsLoading = true;
                MessageResponseHelperResult<IQMessage> result = await chat.Client.MUC_COMMAND_HELPER.requestRoomConfigurationAsync(chat.Chat.chatJabberId);

                if (result.STATE == MessageResponseHelperResultState.SUCCESS)
                {
                    if (result.RESULT is RoomConfigMessage configMessage)
                    {
                        MODEL.Form = new DataFormDataTemplate(configMessage.ROOM_CONFIG);
                        MODEL.IsLoading = false;
                        MODEL.Success = true;
                        SetError("");
                        return;
                    }
                    else if (result.RESULT is IQErrorMessage errorMessage)
                    {
                        SetError("Failed to request room configuration:\n**" + errorMessage + "**");
                        Logger.Warn("Failed to request the room configuration for '" + chat.Chat.chatJabberId + "': " + errorMessage);
                    }
                    else
                    {
                        SetError("Failed to request room configuration:\n**Unexpected response - " + result.RESULT?.GetType().ToString() + "**");
                        Logger.Warn("Failed to request the room configuration for '" + chat.Chat.chatJabberId + "': Unexpected response - " + result.RESULT?.GetType().ToString());
                    }
                }
                else
                {
                    SetError("Failed to request room configuration:\n**" + result.STATE + "**");
                    Logger.Warn("Failed to request the room configuration for '" + chat.Chat.chatJabberId + "': " + result.STATE);
                }
                MODEL.IsLoading = false;
                MODEL.Success = false;
            });
        }

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--
        private void Chat_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is ChatDataTemplate chat)
            {
                OnChatChanged(chat);
            }
        }

        #endregion
    }
}