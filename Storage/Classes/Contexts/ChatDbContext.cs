﻿using System.Linq;
using Microsoft.EntityFrameworkCore;
using Storage.Classes.Models.Account;
using Storage.Classes.Models.Chat;

namespace Storage.Classes.Contexts
{
    public class ChatDbContext: AbstractDbContext
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public DbSet<ChatModel> Chats { get; set; }
        public DbSet<ChatMessageModel> ChatMessages { get; set; }

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--


        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        public static void ResetMucSateForAccount(AccountModel account)
        {
            using (ChatDbContext ctx = new ChatDbContext())
            {
                ctx.Chats.Where(c => string.Equals(c.accountBareJid, account.bareJid)).Select(c => c.muc).ToList().ForEach(m => m.state = MucState.DISCONNECTED);
                ctx.SaveChanges();
            }
        }

        #endregion

        #region --Misc Methods (Private)--


        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}
