﻿using Data_Manager.Classes;
using Data_Manager.Classes.Events;
using Data_Manager2.Classes.DBTables;
using System.Collections.Generic;
using System.Threading.Tasks;
using XMPP_API.Classes;
using XMPP_API.Classes.Network.XML.Messages.XEP_0045;

namespace Data_Manager2.Classes.DBManager
{
    public class ChatManager : AbstractManager
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public static readonly ChatManager INSTANCE = new ChatManager();

        public delegate void NewChatMessageHandler(ChatManager handler, NewChatMessageEventArgs args);
        public delegate void ChatChangedHandler(ChatManager handler, ChatChangedEventArgs args);
        public delegate void MUCInfoChangedHandler(ChatManager handler, MUCInfoChangedEventArgs args);
        public delegate void ChatMessageChangedHandler(ChatManager handler, ChatMessageChangedEventArgs args);

        public event NewChatMessageHandler NewChatMessage;
        public event ChatChangedHandler ChatChanged;
        public event MUCInfoChangedHandler MUCInfoChanged;
        public event ChatMessageChangedHandler ChatMessageChanged;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 18/11/2017 Created [Fabian Sauter]
        /// </history>
        public ChatManager()
        {
            resetPresences();
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        public bool doesChatExist(string id)
        {
            return getChat(id) != null;
        }

        public MUCChatInfoTable getMUCInfo(string chatId)
        {
            IList<MUCChatInfoTable> list = dB.Query<MUCChatInfoTable>(true, "SELECT * FROM " + DBTableConsts.MUC_CHAT_INFO_TABLE + " WHERE chatId = ?;", chatId);
            if (list != null && list.Count > 0)
            {
                return list[0];
            }
            return null;
        }

        public IList<ChatMessageTable> getAllChatMessagesForChat(string chatId)
        {
            return dB.Query<ChatMessageTable>(true, "SELECT * FROM " + DBTableConsts.CHAT_MESSAGE_TABLE + " WHERE chatId = ? ORDER BY date ASC;", chatId);
        }

        public ChatTable getChat(string id)
        {
            IList<ChatTable> list = dB.Query<ChatTable>(true, "SELECT * FROM " + DBTableConsts.CHAT_TABLE + " WHERE id = ?;", id);
            if (list.Count < 1)
            {
                return null;
            }
            else
            {
                return list[0];
            }
        }

        public List<ChatTable> getAllMUCs(string userAccountId)
        {
            return dB.Query<ChatTable>(true, "SELECT * FROM " + DBTableConsts.CHAT_TABLE + " WHERE userAccountId = ? AND chatType = ?;", userAccountId, ChatType.MUC);
        }

        public void setChat(ChatTable chat, bool delete, bool triggerChatChanged)
        {
            if (chat != null)
            {
                if (delete)
                {
                    dB.Delete(chat);
                }
                else
                {
                    update(chat);
                }
                if (triggerChatChanged)
                {
                    ChatChanged?.Invoke(this, new ChatChangedEventArgs(chat, delete));
                }
            }
        }

        public void setMUCMember(MUCMemberTable member, bool delete)
        {
            if (member != null)
            {
                if (delete)
                {
                    dB.Delete(member);
                }
                else
                {
                    update(member);
                }
            }
        }

        public MUCMemberTable getMUCMember(string chatId, string nickname)
        {
            List<MUCMemberTable> list = dB.Query<MUCMemberTable>(true, "SELECT * FROM " + DBTableConsts.MUC_MEMBER_TABLE + " WHERE id = ?;", MUCMemberTable.generateId(chatId, nickname));
            if(list.Count <= 0)
            {
                return null;
            }
            return list[0];
        }

        public void setMUCChatInfo(MUCChatInfoTable info, bool delete, bool triggerMUCChanged)
        {
            if (info != null)
            {
                if (delete)
                {
                    dB.Delete(info);
                }
                else
                {
                    update(info);
                }

                if (triggerMUCChanged)
                {
                    MUCInfoChanged?.Invoke(this, new MUCInfoChangedEventArgs(info, delete));
                }
            }
        }

        public void setMUCEnterState(string chatId, MUCEnterState state, bool triggerMUCChanged)
        {
            dB.Execute("UPDATE " + DBTableConsts.MUC_CHAT_INFO_TABLE + " SET enterState = ? WHERE chatId = ?", state, chatId);
            if (triggerMUCChanged)
            {
                MUCChatInfoTable info = getMUCInfo(chatId);
                if(info != null)
                {
                    MUCInfoChanged?.Invoke(this, new MUCInfoChangedEventArgs(info, false));
                }
            }
        }

        private List<ChatMessageTable> getAllUnreadMessages(ChatTable chat)
        {
            return dB.Query<ChatMessageTable>(true, "SELECT * FROM " + DBTableConsts.CHAT_MESSAGE_TABLE + " WHERE chatId = ? AND state = ? AND fromUser != ?;", chat.id, MessageState.UNREAD, chat.userAccountId);
        }

        public List<ChatTable> getAllChatsForClient(string userAccountId)
        {
            return dB.Query<ChatTable>(true, "SELECT * FROM " + DBTableConsts.CHAT_TABLE + " WHERE userAccountId LIKE ?;", userAccountId);
        }

        public void setAllNotInRoster(string userAccountId)
        {
            Parallel.ForEach(getAllChatsForClient(userAccountId), (c) =>
            {
                c.inRoster = false;
                update(c);
                ChatChanged?.Invoke(this, new ChatChangedEventArgs(c, false));
            });
        }

        public ChatMessageTable getChatMessageById(string messageId)
        {
            List<ChatMessageTable> list = dB.Query<ChatMessageTable>(true, "SELECT * FROM " + DBTableConsts.CHAT_MESSAGE_TABLE + " WHERE id = ?;", messageId);
            if (list.Count > 0)
            {
                return list[0];
            }
            return null;
        }

        public ChatMessageTable getLastChatMessageForChat(string chatId)
        {
            IList<ChatMessageTable> list = getAllChatMessagesForChat(chatId);
            if (list.Count <= 0)
            {
                return null;
            }
            return list[list.Count - 1];
        }

        public List<MUCMemberTable> getAllMUCMembers(string chatId)
        {
            return dB.Query<MUCMemberTable>(true, "SELECT * FROM " + DBTableConsts.MUC_MEMBER_TABLE + " WHERE chatId = ?;", chatId);
        }

        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        public void updateChatMessageState(string msgId, MessageState state)
        {
            dB.Execute("UPDATE " + DBTableConsts.CHAT_MESSAGE_TABLE + " SET state = ? WHERE id = ?", state, msgId);
            List<ChatMessageTable> list = dB.Query<ChatMessageTable>(true, "SELECT * FROM " + DBTableConsts.CHAT_MESSAGE_TABLE + " WHERE id = ?;", msgId);
            Parallel.ForEach(list, (msg) =>
            {
                ChatMessageChanged?.Invoke(this, new ChatMessageChangedEventArgs(msg));
            });
        }

        public void deleteAllChatMessagesForAccount(ChatTable chat)
        {
            dB.Execute("DELETE FROM " + DBTableConsts.CHAT_MESSAGE_TABLE + " WHERE chatId = ?;", chat.id);
        }

        public void deleteAllMUCMemberforChat(string chatId)
        {
            dB.Execute("DELETE FROM " + DBTableConsts.MUC_MEMBER_TABLE + " WHERE chatId = ?;", chatId);
        }

        public void markAllMessagesAsRead(ChatTable chat)
        {
            List<ChatMessageTable> list = getAllUnreadMessages(chat);
            if (list.Count > 0)
            {
                Parallel.ForEach(list, (msg) =>
                {
                    msg.state = MessageState.READ;
                    update(msg);
                    msg.onChanged();
                    ChatMessageChanged?.Invoke(this, new ChatMessageChangedEventArgs(msg));
                });
            }
        }

        public void setChatMessageEntry(ChatMessageTable message, bool triggerNewChatMessage, bool triggerMessageChanged)
        {
            update(message);
            if (triggerNewChatMessage)
            {
                NewChatMessage?.Invoke(this, new NewChatMessageEventArgs(message));
                if (message.isImage)
                {
                    cacheImage(message);
                }
            }
            if (triggerMessageChanged)
            {
                ChatMessageChanged?.Invoke(this, new ChatMessageChangedEventArgs(message));
            }
        }

        public void resetPresence(string userAccountId)
        {
            Parallel.ForEach(getAllChatsForClient(userAccountId), (c) =>
            {
                if (c.chatType == ChatType.CHAT)
                {
                    c.presence = Presence.Unavailable;
                    update(c);
                    ChatChanged?.Invoke(this, new ChatChangedEventArgs(c, false));
                }
            });
        }

        public void resetMUCEnterState(string userAccountId, bool triggerMUCChanged)
        {
            // Semi join:
            List<MUCChatInfoTable> list = dB.Query<MUCChatInfoTable>(true, "SELECT * FROM " + DBTableConsts.MUC_CHAT_INFO_TABLE + " WHERE EXISTS (SELECT * FROM " + DBTableConsts.CHAT_TABLE + " c JOIN " + DBTableConsts.MUC_CHAT_INFO_TABLE + " i ON c.id = i.chatId WHERE c.userAccountId = ? AND chatType = ?) AND enterState != ?;", userAccountId, ChatType.MUC, MUCEnterState.DISCONNECTED);
            foreach (MUCChatInfoTable info in list)
            {
                setMUCEnterState(info.chatId, MUCEnterState.DISCONNECTED, triggerMUCChanged);
            }
        }

        #endregion

        #region --Misc Methods (Private)--
        private void cacheImage(ChatMessageTable msg)
        {
            ImageManager.INSTANCE.downloadImage(msg);
        }

        private void resetPresences()
        {
            dB.Execute("UPDATE " + DBTableConsts.CHAT_TABLE + " SET presence = ? WHERE chatType = ?;", Presence.Unavailable, ChatType.CHAT);
        }

        #endregion

        #region --Misc Methods (Protected)--
        protected override void createTables()
        {
            dB.CreateTable<ChatTable>();
            dB.CreateTable<ChatMessageTable>();
            dB.CreateTable<MUCChatInfoTable>();
            dB.CreateTable<MUCMemberTable>();
        }

        protected override void dropTables()
        {
            dB.DropTable<ChatTable>();
            dB.DropTable<ChatMessageTable>();
            dB.DropTable<MUCChatInfoTable>();
            dB.DropTable<MUCMemberTable>();
        }

        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}
