﻿namespace XMPP_API.Classes.Network.XML.Messages.XEP_0048_1_0
{
    // https://xmpp.org/extensions/attic/xep-0048-1.0.html
    public class RequestBookmarksMessage : IQMessage
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--


        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 11/12/2017 Created [Fabian Sauter]
        /// </history>
        public RequestBookmarksMessage(string from) : base(from, null, GET, getRandomId(), Consts.XML_XEP_0048_1_0_BOOKMARKS_REQUEST)
        {
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


        #endregion
    }
}