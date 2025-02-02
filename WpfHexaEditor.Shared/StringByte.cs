﻿//////////////////////////////////////////////
// Apache 2.0  - 2016-2019
// Author : Derek Tremblay (derektremblay666@gmail.com)
// Contributor: Janus Tida
//////////////////////////////////////////////

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WpfHexaEditor.Core;
using WpfHexaEditor.Core.Bytes;
using WpfHexaEditor.Core.CharacterTable;

namespace WpfHexaEditor
{
    internal class StringByte : BaseByte
    {
        #region Global class variables

        private bool _tblShowMte = true;

        #endregion Global variable

        #region Contructor

        public StringByte(HexEditor parent) : base(parent)
        {

        }

        #endregion Contructor

        #region Properties

        /// <summary>
        /// Next Byte of this instance (used for TBL/MTE decoding)
        /// </summary>
        public byte? ByteNext { get; set; }

        #endregion Properties

        #region Characters tables

        /// <summary>
        /// Show or not Multi Title Enconding (MTE) are loaded in TBL file
        /// </summary>
        public bool TblShowMte
        {
            get => _tblShowMte;
            set
            {
                _tblShowMte = value;
                UpdateTextRenderFromByte();
            }
        }

        /// <summary>
        /// Type of caracter table are used un hexacontrol.
        /// For now, somes character table can be readonly but will change in future
        /// </summary>
        public CharacterTableType TypeOfCharacterTable { get; set; }

        #endregion Characters tables

        #region Methods

        /// <summary>
        /// Update the render of text derived bytecontrol from byte property
        /// </summary>
        public override void UpdateTextRenderFromByte()
        {
            if (Byte != null)
            {
                Text = ByteConverters.ByteToChar(Byte.Value).ToString();
            }
            else
                Text = string.Empty;
        }

        /// <summary>
        /// Update Background,foreground and font property
        /// </summary>
        public override void UpdateVisual()
        {
            if (IsSelected)
            {
                FontWeight = _parent.FontWeight;
                Foreground = _parent.ForegroundContrast;

                Background = FirstSelected ? _parent.SelectionFirstColor : _parent.SelectionSecondColor;
            }
            else if (IsHighLight)
            {
                FontWeight = _parent.FontWeight;
                Foreground = _parent.Foreground;
                Background = _parent.HighLightColor;
            }
            else if (Action != ByteAction.Nothing)
            {
                switch (Action)
                {
                    case ByteAction.Modified:
                        FontWeight = FontWeights.Bold;
                        Background = _parent.ByteModifiedColor;
                        Foreground = _parent.Foreground;
                        break;

                    case ByteAction.Deleted:
                        FontWeight = FontWeights.Bold;
                        Background = _parent.ByteDeletedColor;
                        Foreground = _parent.Foreground;
                        break;
                }
            }
            else
            {
                #region TBL COLORING
                Background = Brushes.Transparent;
                FontWeight = _parent.FontWeight;
                Foreground = _parent.Foreground;
                #endregion
            }

            UpdateAutoHighLiteSelectionByteVisual();

            InvalidateVisual();
        }

        /// <summary>
        /// Render the control
        /// </summary>
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            
            #region Update width of control 
            //It's 8-10 time more fastest to update width on render for TBL string
            switch (TypeOfCharacterTable)
            {
                case CharacterTableType.Ascii:
                    Width = 12;
                    break;
            }
            #endregion
        }

        /// <summary>
        /// Clear control
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            ByteNext = null;
        }

        #endregion Methods

        #region Events delegate

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Byte == null) return;

            if (KeyValidation(e)) return;

            //MODIFY ASCII...
            if (!ReadOnlyMode)
            {
                var isok = false;

                if (Keyboard.GetKeyStates(Key.CapsLock) == KeyStates.Toggled)
                {
                    if (Keyboard.Modifiers != ModifierKeys.Shift && Keyboard.Modifiers != ModifierKeys.Control && 
                        e.Key != Key.RightShift && e.Key != Key.LeftShift)
                    {
                        Text = KeyValidator.GetCharFromKey(e.Key).ToString();
                        isok = true;
                    }
                    else if (Keyboard.Modifiers == ModifierKeys.Shift && Keyboard.Modifiers != ModifierKeys.Control && 
                             e.Key != Key.RightShift && e.Key != Key.LeftShift)
                    {
                        isok = true;
                        Text = KeyValidator.GetCharFromKey(e.Key).ToString().ToLower();
                    }
                }
                else
                {
                    if (Keyboard.Modifiers != ModifierKeys.Shift && Keyboard.Modifiers != ModifierKeys.Control &&
                        e.Key != Key.RightShift && e.Key != Key.LeftShift)
                    {
                        Text = KeyValidator.GetCharFromKey(e.Key).ToString().ToLower();
                        isok = true;
                    }
                    else if (Keyboard.Modifiers == ModifierKeys.Shift && Keyboard.Modifiers != ModifierKeys.Control && 
                             e.Key != Key.RightShift && e.Key != Key.LeftShift)
                    {
                        isok = true;
                        Text = KeyValidator.GetCharFromKey(e.Key).ToString();
                    }
                }

                //Move focus event
                if (isok)
                {
                    Action = ByteAction.Modified;
                    Byte = ByteConverters.CharToByte(Text[0]);

                    //Insert byte at end of file
                    if (_parent.Length == BytePositionInFile + 1)
                    {
                        byte[] byteToAppend = { 0 };
                        _parent.AppendByte(byteToAppend);
                    }

                    OnMoveNext(new EventArgs());
                }
            }

            base.OnKeyDown(e);
        }

        #endregion Events delegate

        #region Caret events

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            //TODO: complete caret implemention ....

            _parent.SetCaretMode(CaretMode.Overwrite);

            if (ReadOnlyMode || Byte == null)
                _parent.HideCaret();
            else
                _parent.MoveCaret(TransformToAncestor(_parent).Transform(new Point(0, 0)));

            base.OnGotFocus(e);
        }
        #endregion
    }
}
