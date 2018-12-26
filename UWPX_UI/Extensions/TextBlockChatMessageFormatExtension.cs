﻿using Data_Manager2.Classes;
using NeoSmart.Unicode;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UWPX_UI_Context.Classes;
using Windows.ApplicationModel.Contacts;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace UWPX_UI.Extensions
{
    /// <summary>
    /// Based on: https://stackoverflow.com/questions/38208741/auto-detect-url-phone-number-email-in-textblock
    /// </summary>
    public sealed class TextBlockChatMessageFormatExtension
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        /// <summary>
        /// The default prefix to use to convert a relative URI to an absolute URI
        /// The Windows RunTime is only working with absolute URI
        /// </summary>
        private const string RelativeUriDefaultPrefix = "https://";
        private const string URL_REGEX_PATTERN = @"(?i)\b((?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))";
        private const string EMAIL_REGEX_PATTERN = @"(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))";
        private const string PHONE_REGEX_PATTERN = @"\+?(\d{2,}[\-\(\)\. ]?){2,}\d\b";

        private static readonly Regex URL_REGEX = new Regex(URL_REGEX_PATTERN, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));
        private static readonly Regex EMAIL_REGEX = new Regex(EMAIL_REGEX_PATTERN, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));
        private static readonly Regex PHONE_REGEX = new Regex(PHONE_REGEX_PATTERN, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));

        public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.Register("FormattedText", typeof(string), typeof(TextBlockChatMessageFormatExtension), new PropertyMetadata("", OnFormattedTextChanged));

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--


        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        public static string GetFormattedText(DependencyObject obj)
        {
            return (string)obj.GetValue(FormattedTextProperty);
        }

        public static void SetFormattedText(DependencyObject obj, string value)
        {
            obj.SetValue(FormattedTextProperty, value);
        }

        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--


        #endregion

        #region --Misc Methods (Private)--
        /// <summary>
        /// This method will extract a fragment of the raw text string, create a Run element with the fragment and
        /// add it to the textblock inlines collection
        /// </summary>
        /// <param name="textBlock">the textblock where to add the run element</param>
        /// <param name="rawText">the raw text where the fragment will be extracted</param>
        /// <param name="startPosition">the start position to extract the fragment</param>
        /// <param name="endPosition">the end position to extract the fragment</param>
        private static void createRunElement(TextBlock textBlock, string rawText, int startPosition, int endPosition)
        {
            var fragment = rawText.Substring(startPosition, endPosition - startPosition);
            textBlock.Inlines.Add(new Run { Text = fragment });
        }

        /// <summary>
        /// Create an URL element with the provided match result from the URL regex
        /// It will create the Hyperlink element that will contain the URL and add it to the provided textblock
        /// </summary>
        /// <param name="textBlock">the textblock where to add the hyperlink</param>
        /// <param name="urlMatch">the match for the URL to use to create the hyperlink element</param>
        /// <returns>the newest position on the source string for the parsing</returns>
        private static int createUrlElement(TextBlock textBlock, Match urlMatch)
        {
            if (Uri.TryCreate(urlMatch.Value, UriKind.RelativeOrAbsolute, out Uri targetUri))
            {
                var link = new Hyperlink();
                link.Inlines.Add(new Run { Text = urlMatch.Value });

                if (targetUri.IsAbsoluteUri)
                {
                    link.NavigateUri = targetUri;
                }
                else
                {
                    link.NavigateUri = new Uri(RelativeUriDefaultPrefix + targetUri.OriginalString);
                }

                textBlock.Inlines.Add(link);
            }
            else
            {
                textBlock.Inlines.Add(new Run { Text = urlMatch.Value });
            }

            return urlMatch.Index + urlMatch.Length;
        }

        /// <summary>
        /// Create a hyperlink element with the provided match result from the regex that will open the contact application
        /// with the provided contact information (it should be a phone number or an email address
        /// This is used only if the email address / phone number is not prefixed with the mailto: / tel: scheme
        /// It will create the Hyperlink element that will contain the email/phone number hyperlink and add it to the provided textblock.
        /// Clicking on the link will open the contact application
        /// </summary>
        /// <param name="textBlock">the textblock where to add the hyperlink</param>
        /// <param name="emailMatch">the match for the email to use to create the hyperlink element. Set to null if not available but at least one of emailMatch and phoneMatch must be not null.</param>
        /// <param name="phoneMatch">the match for the phone number to create the hyperlink element. Set to null if not available but at least one of emailMatch and phoneMatch must be not null.</param>
        /// <returns>the newest position on the source string for the parsing</returns>
        private static int createContactElement(TextBlock textBlock, Match emailMatch, Match phoneMatch)
        {
            var currentMatch = emailMatch ?? phoneMatch;

            var link = new Hyperlink();
            link.Inlines.Add(new Run { Text = currentMatch.Value });
            link.Click += (s, a) =>
            {
                var contact = new Contact();
                if (emailMatch != null)
                {
                    contact.Emails.Add(new ContactEmail { Address = emailMatch.Value });
                }
                if (phoneMatch != null)
                {
                    contact.Phones.Add(new ContactPhone { Number = new string(phoneMatch.Value.Where(c => char.IsDigit(c) || c == '+').ToArray()) });
                }

                ContactManager.ShowFullContactCard(contact, new FullContactCardOptions());
            };

            textBlock.Inlines.Add(link);
            return currentMatch.Index + currentMatch.Length;
        }

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--
        private static async void OnFormattedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock && e.NewValue is string text && !string.IsNullOrWhiteSpace(text))
            {
                // Check if advanced chat message processing is disabled:
                if (Settings.getSettingBoolean(SettingsConsts.DISABLE_ADVANCED_CHAT_MESSAGE_PROCESSING))
                {
                    textBlock.Inlines.Add(new Run { Text = text });
                    return;
                }

                // Clear all inlines:
                textBlock.Inlines.Clear();

                bool isEmoji = await Task.Run(() => Emoji.IsEmoji(text.TrimEnd(UiUtils.TRIM_CHARS).TrimStart(UiUtils.TRIM_CHARS)));
                if (isEmoji)
                {
                    textBlock.Inlines.Add(new Run
                    {
                        Text = text,
                        FontSize = 50
                    });
                }
                else
                {
                    var lastPosition = 0;
                    var matches = new Match[3] { Match.Empty, Match.Empty, Match.Empty };

                    do
                    {
                        await Task.Run(() =>
                        {
                            try
                            {
                                matches[0] = URL_REGEX.Match(text, lastPosition);
                                matches[1] = EMAIL_REGEX.Match(text, lastPosition);
                                matches[2] = PHONE_REGEX.Match(text, lastPosition);
                            }
                            catch (RegexMatchTimeoutException)
                            {
                            }
                        });

                        var firstMatch = matches.Where(x => x != null && x.Success).OrderBy(x => x.Index).FirstOrDefault();
                        if (firstMatch == matches[0])
                        {
                            // the first match is an URL:
                            createRunElement(textBlock, text, lastPosition, firstMatch.Index);
                            lastPosition = createUrlElement(textBlock, firstMatch);
                        }
                        else if (firstMatch == matches[1])
                        {
                            // the first match is an email:
                            createRunElement(textBlock, text, lastPosition, firstMatch.Index);
                            lastPosition = createContactElement(textBlock, firstMatch, null);
                        }
                        else if (firstMatch == matches[2])
                        {
                            // the first match is a phone number:
                            createRunElement(textBlock, text, lastPosition, firstMatch.Index);
                            lastPosition = createContactElement(textBlock, null, firstMatch);
                        }
                        else
                        {
                            // no match, we add the whole text:
                            textBlock.Inlines.Add(new Run { Text = text.Substring(lastPosition) });
                            lastPosition = text.Length;
                        }

                    } while (lastPosition < text.Length);
                }
            }
        }

        #endregion
    }
}
