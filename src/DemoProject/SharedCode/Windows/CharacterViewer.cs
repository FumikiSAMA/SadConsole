﻿using System;
using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Controls;
using SadConsole.Effects;

namespace StarterProject.Windows
{
    internal class CharacterViewer : Window
    {

        #region Control Declares
        private readonly SadConsole.Controls.ScrollBar _charScrollBar;
        private readonly Button _closeButton;
        #endregion

        private Rectangle trackedRegion = new Rectangle(1, 1, 16, 16);
        private SadConsole.Input.MouseConsoleState lastMouseState = null;
        private readonly SadConsole.Effects.Recolor highlightedEffect;
        private readonly SadConsole.Effects.Fade unhighlightEffect;
        private int fontRowOffset = 0;
        private ColoredString selectedGlyphString;
        private int hoverGlyph;
        private bool isHover;

        public event EventHandler ColorsChanged;
        public int SelectedCharacterIndex = 0;

        public Color Foreground = Color.White;
        public Color Background = Color.Black;

        #region Constructors
        public CharacterViewer()
            : base(27, 20)
        {
            //DefaultShowLocation = StartupLocation.CenterScreen;
            //Fill(Color.White, Color.Black, 0, null);
            Title = (char)198 + "Character" + (char)198;
            TitleAlignment = HorizontalAlignment.Left;
            //SetTitle(" Characters ", HorizontalAlignment.Center, Color.Blue, Color.LightGray);
            CloseOnEscKey = true;
            UsePixelPositioning = true;

            // CHARACTER SCROLL
            _charScrollBar = new ScrollBar(Orientation.Vertical, 16)
            {
                Position = new Point(17, 1),
                Name = "ScrollBar",
                Maximum = Font.Rows - 16,
                Value = 0
            };
            _charScrollBar.ValueChanged += new EventHandler(_charScrollBar_ValueChanged);
            _charScrollBar.IsEnabled = Font.Rows > 16;

            // Add all controls
            Add(_charScrollBar);

            _closeButton = new Button(6, 1) { Text = "Ok", Position = new Point(19, 1) }; Add(_closeButton); _closeButton.Click += (sender, e) => { DialogResult = true; Hide(); };

            // Effects
            highlightedEffect = new Recolor
            {
                Foreground = Color.Blue,
                Background = Color.DarkGray
            };

            unhighlightEffect = new SadConsole.Effects.Fade()
            {
                FadeBackground = true,
                FadeForeground = true,
                DestinationForeground = new ColorGradient(highlightedEffect.Foreground, Color.White),
                DestinationBackground = new ColorGradient(highlightedEffect.Background, Color.Black),
                FadeDuration = 0.3d,
                RemoveOnFinished = true,
                UseCellBackground = false,
                UseCellForeground = false,
                CloneOnApply = true,
                Permanent = true
            };

            Theme = SadConsole.Themes.Library.Default.WindowTheme.Clone();
            Theme.BorderLineStyle = CellSurface.ConnectedLineThick;

            // The frame will have been drawn by the base class, so redraw and our close button will be put on top of it
            //Invalidate();
        }

        #endregion


        private void _charScrollBar_ValueChanged(object sender, EventArgs e)
        {
            fontRowOffset = ((SadConsole.Controls.ScrollBar)sender).Value;

            int charIndex = fontRowOffset * 16;
            for (int y = 1; y < 17; y++)
            {
                for (int x = 1; x < 17; x++)
                {
                    SetGlyph(x, y, charIndex);
                    charIndex++;
                }
            }
        }

        protected override void Invalidate()
        {
            base.Invalidate();

            // Draw the border between char sheet area and the text area
            SetGlyph(0, Height - 3, 204);
            SetGlyph(Width - 1, Height - 3, 185);
            for (int i = 1; i < Width - 1; i++)
            {
                SetGlyph(i, Height - 3, 205);
            }
            //SetCharacter(this.Width - 1, 0, 256);

            int charIndex = 0;
            for (int y = 1; y < 17; y++)
            {
                for (int x = 1; x < 17; x++)
                {
                    SetGlyph(x, y, charIndex);
                    charIndex++;
                }
            }

            if (isHover)
                DrawHoverItemString();
            else
                DrawSelectedItemString();
        }

        protected override void OnMouseLeftClicked(SadConsole.Input.MouseConsoleState state)
        {
            if (state.Cell != null && trackedRegion.Contains(state.ConsoleCellPosition.X, state.ConsoleCellPosition.Y))
            {
                SelectedCharacterIndex = state.Cell.Glyph;
            }
            else if (state.ConsoleCellPosition.X == Width - 1 && state.ConsoleCellPosition.Y == 0)
            {
                Hide();
            }

            base.OnMouseLeftClicked(state);
        }

        protected override void OnMouseMove(SadConsole.Input.MouseConsoleState state)
        {
            if (state.Cell != null && trackedRegion.Contains(state.ConsoleCellPosition.X, state.ConsoleCellPosition.Y))
            {
                isHover = true;
                hoverGlyph = state.Cell.Glyph;

                // Set the special effect on the current known character and clear it on the last known
                if (lastMouseState == null)
                {
                }
                else if (lastMouseState.ConsoleCellPosition != state.ConsoleCellPosition)
                {
                    SetEffect(lastMouseState.ConsoleCellPosition.X, lastMouseState.ConsoleCellPosition.Y,
                    unhighlightEffect
                    );
                }

                SetEffect(state.ConsoleCellPosition.X, state.ConsoleCellPosition.Y, highlightedEffect);
                lastMouseState = state.Clone();
            }
            else
            {
                isHover = false;
                
                // Clear the special effect on the last known character
                if (lastMouseState != null)
                {
                    SetEffect(lastMouseState.ConsoleCellPosition.X, lastMouseState.ConsoleCellPosition.Y, unhighlightEffect);
                    lastMouseState = null;
                }
            }

            base.OnMouseMove(state);

            IsDirty = true;
        }

        private void DrawHoverItemString()
        {
            // Draw the character index and value in the status area
            string[] items = new string[] { "Index: ", hoverGlyph.ToString() + " ", ((char)hoverGlyph).ToString() };

            items[2] = items[2].PadRight(Width - 2 - (items[0].Length + items[1].Length));

            ColoredString text = items[0].CreateColored(Color.LightBlue, Theme.BorderStyle.Background, null) +
                       items[1].CreateColored(Color.LightCoral, Color.Black, null) +
                       items[2].CreateColored(Color.LightCyan, Color.Black, null);

            text.IgnoreBackground = true;
            text.IgnoreEffect = true;

            Print(1, Height - 2, text);
        }

        private void DrawSelectedItemString()
        {
            // Clear the information area and redraw
            Print(1, Height - 2, "".PadRight(Width - 2));

            //string[] items = new string[] { "Current Index:", SelectedCharacterIndex.ToString() + " ", ((char)SelectedCharacterIndex).ToString() };
            string[] items = new string[] { "Selected: ", ((char)SelectedCharacterIndex).ToString(), " (", SelectedCharacterIndex.ToString(), ")" };
            items[4] = items[4].PadRight(Width - 2 - (items[0].Length + items[1].Length + items[2].Length + items[3].Length));

            ColoredString text = items[0].CreateColored(Color.LightBlue, Theme.BorderStyle.Background, null) +
                       items[1].CreateColored(Color.LightCoral, Theme.BorderStyle.Background, null) +
                       items[2].CreateColored(Color.LightCyan, Theme.BorderStyle.Background, null) +
                       items[3].CreateColored(Color.LightCoral, Theme.BorderStyle.Background, null) +
                       items[4].CreateColored(Color.LightCyan, Theme.BorderStyle.Background, null);

            text.IgnoreBackground = true;
            text.IgnoreEffect = true;

            Print(1, Height - 2, text);
        }

        protected override void OnMouseExit(SadConsole.Input.MouseConsoleState state)
        {
            if (lastMouseState != null)
            {
                //                _lastInfo.Cell.Effect = null;
            }

            DrawSelectedItemString();

            base.OnMouseExit(state);
        }

        protected override void OnMouseEnter(SadConsole.Input.MouseConsoleState state)
        {
            if (lastMouseState != null)
            {
                //                _lastInfo.Cell.Effect = null;
            }

            base.OnMouseEnter(state);
        }

        public void UpdateCharSheetColors()
        {
            for (int y = 1; y < 17; y++)
            {
                for (int x = 1; x < 17; x++)
                {
                    SetForeground(x, y, Foreground);
                    SetBackground(x, y, Background);
                }
            }

            ColorsChanged?.Invoke(this, EventArgs.Empty);
        }

        public override void Show(bool modal)
        {
            if (IsVisible)
            {
                return;
            }

            Center();

            base.Show(modal);


            UpdateCharSheetColors();


            string[] items = new string[] { "Current Index:", SelectedCharacterIndex.ToString() + " ", ((char)SelectedCharacterIndex).ToString() };

            items[2] = items[2].PadRight(Width - 2 - (items[0].Length + items[1].Length));

            ColoredString text = items[0].CreateColored(Color.LightBlue, Color.Black, null) +
                       items[1].CreateColored(Color.LightCoral, Color.Black, null) +
                       items[2].CreateColored(Color.LightCyan, Color.Black, null);

            text.IgnoreBackground = true;
            text.IgnoreEffect = true;

            Print(1, Height - 2, text);
        }

    }
}
