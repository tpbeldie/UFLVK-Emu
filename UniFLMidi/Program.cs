using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace UniFLMidi
{
    internal static class Program
    {
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UniFLMidi());
        }
    }

    [DesignerCategory("C#")]
    public class UniFLMidi : Form
    {

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, uint pvParam, uint fWinIni);

        public enum KeyColor
        {
            White,
            Black
        }

        public class MidiKeyClap
        {
            public int Index { get; set; }
            public Rectangle Rectangle { get; set; }
            public Keys Key { get; set; }
            public int Note { get; set; }
            public KeyColor KeyType { get; set; }
            public bool CNoteKey { get; set; }
        }

        private readonly GlobalKeyboardHook m_hook;

        private int m_velocity = 127;

        private Rectangle m_pianoRect;

        private Rectangle m_thumbRect = new Rectangle(14, 94 + 80, 60, 3);

        private List<MidiKeyClap> m_midiKeyboard = new List<MidiKeyClap>();

        private readonly int KeysCount = 121;

        private readonly int WhiteKeyWidth = 13;

        private readonly int WhiteKeyHeight = 60;

        private readonly int BlackKeyWidth = 8;

        private readonly int BlackKeyHeight = 40;

        private const int WM_NCLBUTTONDOWN = 0xA1;

        const uint SPI_SETBEEP = 0x0002;

        const uint SPIF_SENDCHANGE = 0x0002;

        private const int HT_CAPTION = 0x2;

        private readonly Font m_font;

        private bool m_debugKeysDrawing = false;

        private int m_rootKey = 60;

        private List<int> PlayingKeys = new List<int>();

        private OutputDevice m_device;

        private int m_downKey = -1;

        private Rectangle m_resetButtonRect = new Rectangle(10, 66, 47, 21);

        private Color m_mainColor = Color.FromArgb(67, 76, 81);

        private Pen m_borderPenDark = new Pen(Color.FromArgb(45, 54, 59));

        private Pen m_borderPenLight = new Pen(Color.FromArgb(77, 86, 91));

        private Brush m_titleTextBrushFocused = new SolidBrush(Color.FromArgb(192, 212, 211));

        private Brush m_titleTextBrushUfocused = new SolidBrush(Color.FromArgb(123, 140, 140));

        private Brush m_containerBrush = new SolidBrush(Color.FromArgb(49, 55, 59));

        private Pen m_containerBorderPen = new Pen(Color.FromArgb(39, 45, 49));

        private Brush m_headerContainerBrush = new SolidBrush(Color.FromArgb(54, 60, 64));

        private Brush m_headerTitleBrush = new SolidBrush(Color.FromArgb(200, 200, 200));

        private Pen m_buttonBorderPen = new Pen(Color.FromArgb(28, 34, 38));

        private Brush m_pianoBrushOne = new SolidBrush(Color.FromArgb(134, 138, 145));

        private Brush m_pianoBrushTwo = new SolidBrush(Color.FromArgb(207, 211, 218));

        private Brush m_pianoBrushThree = new SolidBrush(Color.FromArgb(203, 207, 214));

        private Color m_buttonColor1 = Color.FromArgb(44, 49, 53);

        private Color m_buttonColor2 = Color.FromArgb(33, 38, 42);

        private Brush m_rootWhiteKeyBrush = new SolidBrush(Color.FromArgb(200, 119, 169, 230));

        private Brush m_rootBlackKeyBrush = new SolidBrush(Color.FromArgb(100, 77, 175, 249));

        private Brush m_playingKeyBrush = new SolidBrush(Color.FromArgb(251, 160, 49));

        private Brush m_overlayBrush = new SolidBrush(Color.FromArgb(180, 30, 36, 39));

        private StringFormat m_centerFormatFlag = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        private StringFormat m_farFormatFlag = new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Far };

        private bool m_locked => m_device == null;

        private string m_indexProviderFilePath = "loopMIDI.txt";

        private Bitmap m_bitmap = null;

        private bool m_shown = false;

        private HashSet<Keys> currentlyPressedKeys = new HashSet<Keys>();

        private List<string> m_pianoNotesKeys;

        private Rectangle m_ctrlMinRect;

        private Rectangle m_ctrlClsRect;

        private Point m_mousePosition;

        private bool m_knobDown = false;

        private int m_velKnobValue = 100;

        private int m_velKnobRotation = 0;

        private Rectangle m_velKnobRect;

        private Color m_velKnobColor = Color.FromArgb(170, 170, 170);

        private Color m_velKnobIndicatorColor = Color.FromArgb(100, 255, 255, 255);

        private Color m_velKnobRingcolor = Color.FromArgb(185, 247, 240);

        private bool m_active => IsKeyLocked(Keys.CapsLock);

        private bool m_beep = false;

        private Rectangle m_beepToggleRect;

        private bool m_stealFocus = false;

        private Rectangle m_stealFocusToggleRect;

        private Dictionary<Keys, int> MidiKeysMapping = new Dictionary<Keys, int>() {
            { Keys.Z, 48},
            { Keys.S, 49},
            { Keys.X, 50},
            { Keys.D, 51},
            { Keys.C, 52},
            { Keys.V, 53},
            { Keys.G, 54},
            { Keys.B, 55},
            { Keys.H, 56},
            { Keys.N, 57},
            { Keys.J, 58},
            { Keys.M, 59},
            { Keys.Oemcomma, 60 },
            { Keys.L, 61 },
            { Keys.OemPeriod, 62 },
            { Keys.Oem1, 63 },
            { Keys.OemQuestion, 64 },
            { Keys.Q, 60 },
            { Keys.D2, 61 },
            { Keys.W, 62 },
            { Keys.D3, 63 },
            { Keys.E, 64 },
            { Keys.R, 65 },
            { Keys.D5, 66 },
            { Keys.T, 67 },
            { Keys.D6, 68 },
            { Keys.Y, 69 },
            { Keys.D7, 70 },
            { Keys.U, 71 },
            { Keys.I, 72 },
            { Keys.D9, 73 },
            { Keys.O, 74 },
            { Keys.D0, 75 },
            { Keys.P, 76 },
            { Keys.OemOpenBrackets, 77 },
            { Keys.Oemplus, 78 },
            { Keys.Oem6, 79 }
        };

        public UniFLMidi() {
            if (File.Exists(m_indexProviderFilePath)) {
                try {
                    m_device = new OutputDevice(int.Parse(File.ReadAllText(m_indexProviderFilePath.Trim())));
                } catch { }
            }
            m_velocity = 127;
            m_hook = new GlobalKeyboardHook();
            m_hook.KeyPressed += Hook_KeyPressed;
            m_hook.KeyUp += Hook_KeyUp;
            m_hook.HookKeyboard();
            m_font = new Font("Verdana", 7);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            Size = new Size(568, 192);
            GenerateRectangles();
            GeneratePianoNotes();
            Click += (s, e) => Refresh();
            Text = "UFLVK Emu - Universal FL Studio Virtual Keyboard Emulator by tpbeldie";
            SystemParametersInfo(SPI_SETBEEP, 0, 0, SPIF_SENDCHANGE);
        }

        public int ScrollX {
            get {
                var x = -m_thumbRect.X + 14;
                if (x <= -450) {
                    x = -450;
                }
                return x;
            }
        }

        private void GeneratePianoNotes() {
            m_pianoNotesKeys = new List<string>();
            for (int i = 0; i <= KeysCount - 1; i++) {
                int octave = i / 12;
                int noteInOctave = i % 12;
                string noteName = GetNoteName(noteInOctave);
                string note = $"{noteName}{octave}";
                m_pianoNotesKeys.Add(note);
            }
        }

        private string GetNoteName(int noteInOctave) {
            string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            return noteNames[noteInOctave];
        }

        private void SendMidiSignal(int signal, bool play, int velocity = -1) {
            if (signal < 0) { return; }
            var c = play ? ChannelCommand.NoteOn : ChannelCommand.NoteOff;
            var v = velocity == -1 ? m_velocity : velocity;
            if (v == 127) {
                v = (int)((float)(127 / 100) * m_velKnobValue);
            }
            ChannelMessage message = new ChannelMessage(c, 0, signal, v);
            m_device?.Send(message);
        }

        private void StopAllPlayingKeys() {
            foreach (var key in PlayingKeys) {
                SendMidiSignal(key, false);
            }
            PlayingKeys.Clear();
            Invalidate();
        }

        private int GetCursorVelocity(int keyIndex, Point mousePosition) {
            if (keyIndex < 0) {
                return 0;
            }
            var keyRect = m_midiKeyboard[keyIndex].Rectangle;
            int percentage = (mousePosition.Y - keyRect.Y) * m_velocity / keyRect.Height;
            return percentage;
        }

        private void ChangeRootKey(int newRootKey) {
            PlayingKeys.Clear();
            int rootNoteDiff = m_rootKey - newRootKey;
            var copy = new Dictionary<Keys, int>(MidiKeysMapping);
            foreach (var keyValuePair in copy) {
                MidiKeysMapping[keyValuePair.Key] = keyValuePair.Value + rootNoteDiff;
            }
            m_rootKey = newRootKey;
        }

        private string PressedKeysString() {
            if (currentlyPressedKeys.Count <= 0) {
                return "none";
            }
            return string.Join(", ", currentlyPressedKeys);
        }

        private string ProcessedNotesString() {
            if (PlayingKeys.Count <= 0) {
                return "-";
            }
            // Issue - Index was out of range. 
            // var notes = PlayingKeys.Select(key => m_pianoNotesKeys[key]);
            // return string.Join(", ", notes);
            var notes = PlayingKeys.Where(key => key >= 0 && key < m_pianoNotesKeys.Count)
                                   .Select(key => m_pianoNotesKeys[key]);
            return notes.Count() <= 0 ? "-" : string.Join(", ", notes);
        }

        private void SendBeepBoop() {
            Invalidate();
            if (IsKeyLocked(Keys.CapsLock)) {
                if (m_beep) {
                    Console.Beep(568, 60);
                }
            }
            else {
                StopAllPlayingKeys();
                if (m_beep) { 
                    Console.Beep(667, 100);
                }
            }
        }

        private void GenerateRectangles() {
            m_pianoRect = new Rectangle(10, 94, Width - 18, 74);
            m_ctrlMinRect = new Rectangle(Width - 40, 10, 10, 10);
            m_ctrlClsRect = new Rectangle(Width - 20, 10, 10, 10);
            m_velKnobRect = new Rectangle(Width - 28, 35, 20, 20);
            m_beepToggleRect = new Rectangle(Width - 52, 80, 44, 12);
            m_stealFocusToggleRect = new Rectangle(Width - 124, 80, 70, 12);
            AssignKeys();
            Invalidate();
        }

        private void AssignKeys() {
            Rectangle whiteKeyRect = new Rectangle(m_pianoRect.X, m_pianoRect.Y + 14, WhiteKeyWidth, WhiteKeyHeight);
            Rectangle blackKeyRect = new Rectangle(whiteKeyRect.Right - BlackKeyWidth / 2, m_pianoRect.Y + 14, BlackKeyWidth, BlackKeyHeight);
            const int nLargeGap = 16;
            const int nSmallGap = 6;
            int keyIndex = 0;
            for (int i = 0; i < KeysCount - 1; i++) {
                if (i % 7 == 0) {
                    m_midiKeyboard.Add(new MidiKeyClap() { Index = keyIndex++, Rectangle = whiteKeyRect, KeyType = KeyColor.White, CNoteKey = true });
                    whiteKeyRect.X += WhiteKeyWidth;
                    for (int j = 0; j < 11; j++) {
                        if (j == 0) { blackKeyRect.X = whiteKeyRect.X - BlackKeyWidth / 2; }
                        if (j % 2 == 0 && j <= 2 || j >= 5 && j % 2 != 0) {
                            m_midiKeyboard.Add(new MidiKeyClap() { Index = keyIndex++, Rectangle = blackKeyRect, KeyType = KeyColor.Black });
                            blackKeyRect.X += BlackKeyWidth + ((j == 2 || j == 9) ? nLargeGap : nSmallGap);
                        }
                        else {
                            m_midiKeyboard.Add(new MidiKeyClap() { Index = keyIndex++, Rectangle = whiteKeyRect, KeyType = KeyColor.White });
                            whiteKeyRect.X += WhiteKeyWidth;
                        }
                    }
                }
            }
        }

        private void AssignWhiteKeys() {
            int currentKeyIndex = 0;
            int currentWhiteKeyIndex = 0;
            int currentBlackKeyIndex = 0;
            Rectangle currentkeyRect = new Rectangle(m_pianoRect.X, m_pianoRect.Y + 14, WhiteKeyWidth, WhiteKeyHeight);
            for (int i = 0; i < KeysCount - 1; i++) {
                bool clapIsCKey = i % 7 == 0;
                m_midiKeyboard.Add(new MidiKeyClap() { Index = currentKeyIndex, Rectangle = currentkeyRect, CNoteKey = clapIsCKey, KeyType = KeyColor.White, });
                if (clapIsCKey) {
                    AssignBlackKeys(currentkeyRect.Left - BlackKeyWidth / 2 - 1, ref currentKeyIndex, ref currentBlackKeyIndex, ref currentBlackKeyIndex);
                }
                currentkeyRect.X += WhiteKeyWidth;
                currentKeyIndex++;
                currentWhiteKeyIndex++;
            }
        }

        private void AssignBlackKeys(int l, ref int currentKeyIndex, ref int currentBlackKeyIndex, ref int currentWhiteKeyIndex) {
            const int nLargeGap = 10;
            const int nSmallGap = 6;
            int left = l + BlackKeyWidth + nSmallGap;
            bool drawTwo = true;
            for (int i = 1; i < 6; i++) {
                if (i % 7 == 0) {
                    left += nLargeGap + 3;
                }
                else if (drawTwo && i > 2) {
                    drawTwo = !drawTwo;
                    left += nLargeGap;
                }
                Rectangle blackKeyRect = new Rectangle(left, m_pianoRect.Y + 14, BlackKeyWidth, BlackKeyHeight);
                m_midiKeyboard.Add(new MidiKeyClap() { Index = currentKeyIndex, Rectangle = blackKeyRect, CNoteKey = false, KeyType = KeyColor.Black, });
                left += BlackKeyWidth + nSmallGap;
                currentKeyIndex++;
                currentBlackKeyIndex++;
            }
        }

        public static Bitmap ApplyGaussianBlur(Bitmap bitmap, int radius) {
            Bitmap blurredBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            // Convert the bitmap to a 32-bit ARGB format for easy manipulation.
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            byte[] pixels = new byte[bitmapData.Stride * bitmapData.Height];
            Marshal.Copy(bitmapData.Scan0, pixels, 0, pixels.Length);
            bitmap.UnlockBits(bitmapData);
            // Apply the Gaussian blur algorithm.
            double[,] kernel = CreateGaussianKernel(radius);
            int kernelSize = kernel.GetLength(0);
            int kernelRadius = kernelSize / 2;
            int bytesPerPixel = 4;
            int stride = bitmapData.Stride;
            byte[] blurredPixels = new byte[pixels.Length];
            Array.Copy(pixels, blurredPixels, pixels.Length);
            for (int y = kernelRadius; y < bitmap.Height - kernelRadius; y++) {
                for (int x = kernelRadius; x < bitmap.Width - kernelRadius; x++) {
                    double r = 0, g = 0, b = 0, a = 0;
                    int index = (y * stride) + (x * bytesPerPixel);
                    for (int ky = -kernelRadius; ky <= kernelRadius; ky++) {
                        for (int kx = -kernelRadius; kx <= kernelRadius; kx++) {
                            double kernelValue = kernel[ky + kernelRadius, kx + kernelRadius];
                            int pixelIndex = index + ((ky * stride) + (kx * bytesPerPixel));
                            b += pixels[pixelIndex + 0] * kernelValue;
                            g += pixels[pixelIndex + 1] * kernelValue;
                            r += pixels[pixelIndex + 2] * kernelValue;
                            a += pixels[pixelIndex + 3] * kernelValue;
                        }
                    }
                    blurredPixels[index + 0] = (byte)Math.Round(b);
                    blurredPixels[index + 1] = (byte)Math.Round(g);
                    blurredPixels[index + 2] = (byte)Math.Round(r);
                    blurredPixels[index + 3] = (byte)Math.Round(a);
                }
            }
            // Copy the modified byte array back to the pixel data of the new Bitmap object.
            BitmapData blurredBitmapData = blurredBitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(blurredPixels, 0, blurredBitmapData.Scan0, blurredPixels.Length);
            blurredBitmap.UnlockBits(blurredBitmapData);
            return blurredBitmap;
        }

        private static double[,] CreateGaussianKernel(int radius) {
            int kernelSize = radius * 2 + 1;
            double[,] kernel = new double[kernelSize, kernelSize];
            double sigma = radius / 3.0;
            double sigmaSquaredTimesTwo = 2 * sigma * sigma;
            double kernelSum = 0;
            for (int y = -radius; y <= radius; y++) {
                for (int x = -radius; x <= radius; x++) {
                    double distanceSquared = x * x + y * y;
                    double kernelValue = Math.Exp(-distanceSquared / sigmaSquaredTimesTwo);
                    kernelSum += kernelValue;
                    kernel[y + radius, x + radius] = kernelValue;
                }
            }
            // Normalize the kernel so that the sum of all kernel values equals 1.
            for (int y = 0; y < kernelSize; y++) {
                for (int x = 0; x < kernelSize; x++) {
                    kernel[y, x] /= kernelSum;
                }
            }
            return kernel;
        }

        private void UpdateVelocityKnob(MouseEventArgs e) {
            Point center = new Point(m_velKnobRect.X + m_velKnobRect.Width / 2, m_velKnobRect.Y + m_velKnobRect.Height / 2);
            int dx = e.X - center.X;
            int dy = e.Y - center.Y;
            double angle = Math.Atan2(dy, dx);
            m_velKnobRotation = (int)(angle * 180 / Math.PI);
            if (m_velKnobRotation < 0) {
                m_velKnobRotation += 360;
            }
            m_velKnobValue = (int)(m_velKnobRotation / 360.0 * 101);
            if (m_velKnobValue >= 101) {
                m_velKnobRotation = 0;
                m_velKnobValue = 0;
            }
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (m_knobDown && e.Button == MouseButtons.Left) {
                UpdateVelocityKnob(e);
                return;
            }
            m_mousePosition = e.Location;
            Invalidate(new Rectangle(0, 0, Width, 40));
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            if (m_locked) {
                return;
            }
            m_knobDown = false;
            if (m_downKey != -1) {
                PlayingKeys.Remove(m_downKey);
                SendMidiSignal(m_downKey, false);
                m_downKey = -1;
                Invalidate();
            }
            base.OnMouseUp(e);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            if (m_beepToggleRect.Contains(e.Location)) {
                m_beep = !m_beep;
            }
            if (m_stealFocusToggleRect.Contains(e.Location)) {
                m_stealFocus = !m_stealFocus;
            }
            if (m_velKnobRect.Contains(e.Location)) {
                UpdateVelocityKnob(e);
                m_knobDown = true;
                return;
            }
            if (m_ctrlClsRect.Contains(e.Location)) {
                m_device?.Dispose();
                Environment.Exit(-1);
            }
            if (m_ctrlMinRect.Contains(e.Location)) {
                WindowState = FormWindowState.Minimized;
                m_mousePosition = Point.Empty;
                return;
            }
            if (m_resetButtonRect.Contains(e.Location)) {
                m_thumbRect.X = 14;
                ChangeRootKey(60);
                StopAllPlayingKeys();
            }
            if (!m_pianoRect.Contains(e.Location) || m_locked) {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
            int keyIndex = -1;
            for (int i = 0; i < m_midiKeyboard.Count - 1; i++) {
                var key = m_midiKeyboard[i];
                if (!key.Rectangle.Contains(new Point(e.X - ScrollX, e.Y))) {
                    continue;
                }
                if (key.KeyType == KeyColor.White) {
                    for (int j = Math.Max(0, key.Index - 2); j < Math.Min(m_midiKeyboard.Count, key.Index + 2); j++) {
                        if (j == i) { continue; }
                        var blackKey = m_midiKeyboard[j];
                        if (blackKey.Rectangle.Contains(new Point(e.X - ScrollX, e.Y)) &&
                            blackKey.KeyType == KeyColor.Black &&
                            key.Rectangle.IntersectsWith(blackKey.Rectangle)) {
                            keyIndex = key.Index;
                            break;
                        }
                    }
                }
                keyIndex = i;
                break;
            }
            if (e.Button == MouseButtons.Right) {
                ChangeRootKey(keyIndex);
            }
            else {
                m_downKey = keyIndex;
                PlayingKeys.Add(m_downKey);
                SendMidiSignal(keyIndex, true, GetCursorVelocity(m_downKey, e.Location));
                Invalidate();
            }
            base.OnMouseDown(e);
        }

        private void Hook_KeyUp(object sender, GlobalKeyboardHook.KeyUpEventArgs e) {
            if (m_locked) {
                return;
            }
            if (e.KeyUp == Keys.CapsLock) {
                SendBeepBoop();
            }
            if (!MidiKeysMapping.ContainsKey(e.KeyUp)) {
                return;
            }
            var keyIndex = MidiKeysMapping[e.KeyUp];
            currentlyPressedKeys.Remove(e.KeyUp);
            if (PlayingKeys.Contains(keyIndex)) {
                SendMidiSignal(MidiKeysMapping[e.KeyUp], false);
                PlayingKeys.Remove(keyIndex);
            }
            Invalidate();
        }

        private void Hook_KeyPressed(object sender, GlobalKeyboardHook.KeyPressedEventArgs e) {
            if (m_locked) {
                return;
            }
            if (!m_active) {
                return;
            }
            if (!MidiKeysMapping.ContainsKey(e.KeyPressed)) {
                return;
            }
            var keyIndex = MidiKeysMapping[e.KeyPressed];
            currentlyPressedKeys.Add(e.KeyPressed);
            if (MidiKeysMapping.ContainsKey(e.KeyPressed) && !PlayingKeys.Contains(keyIndex)) {
                if(m_stealFocus) {
                    SetForegroundWindow(Handle);
                }
                SendMidiSignal(MidiKeysMapping[e.KeyPressed], true);
                PlayingKeys.Add(keyIndex);
            }
            Invalidate();
        }

        // Brain fart, lol.
        private void OnPaintBlackKeysOlder(PaintEventArgs e, int l) {
            const int nLargeGap = 10;
            const int nSmallGap = 6;
            int left = l;
            int j = 0;
            bool drawTwo = true;
            for (int i = 1; i < 6; i++) {
                if (drawTwo) {
                    if (j == 2) {
                        drawTwo = false;
                        j = 0;
                        left += nLargeGap;
                    }
                    left += BlackKeyWidth + nSmallGap;
                    j++;
                }
                else {
                    if (j == 3) {
                        drawTwo = true;
                        j = 0;
                        left += nLargeGap;
                        if (i % 7 == 0) {
                            left += 3;
                        }
                    }
                    left += BlackKeyWidth + nSmallGap;
                    j++;
                }
                Rectangle blackKeyRect = new Rectangle(left, m_pianoRect.Y + 14, BlackKeyWidth, BlackKeyHeight);
                OnPaintBlackKey(e, blackKeyRect, int.MinValue);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e) {
            if (m_locked) { return; }
            int limitFar = Width - m_thumbRect.Width - 14;
            int limitNear = 14;
            int scroolBy = 30;
            int scrollX;
            if (e.Delta > 0) {
                scrollX = m_thumbRect.X - scroolBy;
            }
            else {
                scrollX = m_thumbRect.X + scroolBy;
            }
            if (scrollX < limitNear) { scrollX = limitNear; }
            if (scrollX > limitFar) { scrollX = limitFar; }
            m_thumbRect.X = scrollX;
            Invalidate();
        }

        protected override void OnShown(EventArgs e) {
            base.OnShown(e);
            m_shown = true;
        }

        protected override CreateParams CreateParams {
            get {
                CreateParams cp = base.CreateParams;
                // Turn on WS_EX_COMPOSITED
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        protected void OnPaintVelocityKnob(PaintEventArgs e) {
            Point center = new Point(m_velKnobRect.X + m_velKnobRect.Width / 2, m_velKnobRect.Y + m_velKnobRect.Height / 2);
            var alpha = (int)(Math.Max(20, m_velKnobValue) / 255f * 255f);
            using (Brush backgroundBrush = new SolidBrush(Color.FromArgb(alpha, m_velKnobColor))) {
                e.Graphics.FillEllipse(backgroundBrush, m_velKnobRect);
            }
            double indicatorAngle = m_velKnobRotation / 180.0 * Math.PI;
            int indicatorLength = (int)(m_velKnobRect.Width * 0.4);
            Point indicatorStart = center;
            Point indicatorEnd = new Point(
                (int)(center.X + indicatorLength * Math.Cos(indicatorAngle)),
                (int)(center.Y + indicatorLength * Math.Sin(indicatorAngle)));
            using (var indicatorPen = new Pen(m_velKnobIndicatorColor, m_velKnobRect.Width * 0.1f)) {
                e.Graphics.DrawLine(indicatorPen, indicatorStart, indicatorEnd);
            }
            var knobRectSecond = m_velKnobRect;
            knobRectSecond.Inflate(-3, -3);
            using (Pen pen = new Pen(Color.FromArgb(alpha, m_velKnobRingcolor), 2)) {
                e.Graphics.DrawArc(pen, knobRectSecond, 0, m_velKnobValue / 100.0f * 360);
            }
            e.Graphics.DrawString("Global velocity: " + m_velKnobValue + "%", m_font, Brushes.White, new RectangleF(m_velKnobRect.X - 400 - 4, m_velKnobRect.Y + 6, 400, 10), m_farFormatFlag);
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            e.Graphics.Clear(m_mainColor);
            e.Graphics.DrawRectangle(m_borderPenDark, 0, 0, Width - 1, Height - 1);
            e.Graphics.DrawRectangle(m_borderPenLight, 1, 1, Width - 2, Height - 2);
            e.Graphics.DrawString(Text, m_font, Focused ? m_titleTextBrushFocused : m_titleTextBrushUfocused, 7, 9);
            e.Graphics.FillRectangle(m_containerBrush, 4, 30, Width - 8, Height - 30 - 4);
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            e.Graphics.DrawRectangle(m_containerBorderPen, 4, 30, Width - 8, Height - 30 - 4);
            e.Graphics.FillRectangle(m_headerContainerBrush, 5, 31, Width - 9, 30);
            e.Graphics.DrawLine(m_containerBorderPen, 4, 60, Width - 4, 60);
            e.Graphics.DrawString("Root note: " + m_rootKey + $" [{m_pianoNotesKeys[m_rootKey]}] - Current pressed key/s: " + PressedKeysString(), m_font, m_headerTitleBrush, 10, 38);
            using (var resetButtonBrush = new LinearGradientBrush(m_resetButtonRect, m_buttonColor1, m_buttonColor2, LinearGradientMode.Vertical)) {
                e.Graphics.FillRectangle(resetButtonBrush, m_resetButtonRect);
            }
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.DrawRectangle(m_buttonBorderPen, m_resetButtonRect);
            e.Graphics.DrawString("Reset", m_font, m_headerTitleBrush, m_resetButtonRect.X + 8, m_resetButtonRect.Y + 4);
            e.Graphics.FillRectangle(m_pianoBrushOne, 10, 94 + 74, Width - 18, 15);
            e.Graphics.FillRectangle(m_pianoBrushTwo, m_thumbRect);
            e.Graphics.FillRectangle(m_pianoBrushThree, m_pianoRect);
            e.Graphics.DrawString("Playing notes: " + ProcessedNotesString(), m_font, m_headerTitleBrush, m_resetButtonRect.Right + 6, m_resetButtonRect.Y + 4);
            e.Graphics.DrawString(m_active ? "ACTIVE" : "INACTIVE", m_font, m_active ? Brushes.GreenYellow : Brushes.Orange, new Rectangle(0, m_resetButtonRect.Y, Width - 12, 12), m_farFormatFlag);
            e.Graphics.FillEllipse(m_beep ? Brushes.GreenYellow : Brushes.Gray, new Rectangle(Width - 52, 81, 6, 6));
            e.Graphics.DrawString("Beep?", m_font, m_beep ? Brushes.White : Brushes.Gray, new Rectangle(0, m_resetButtonRect.Y, Width - 12, 24), m_farFormatFlag);
            e.Graphics.DrawString("Steal focus?", m_font, m_stealFocus ? Brushes.White : Brushes.Gray, new Rectangle(0, m_resetButtonRect.Y, Width - 56, 24), m_farFormatFlag);
            e.Graphics.FillEllipse(m_stealFocus ? Brushes.GreenYellow : Brushes.Gray, new Rectangle(Width - 124, 81, 6, 6));
            OnPaintVelocityKnob(e);
            e.Graphics.SetClip(m_pianoRect);
            e.Graphics.TranslateTransform(ScrollX, 0);
            var whiteKeys = m_midiKeyboard.Where(i => i.KeyType == KeyColor.White);
            var blackKeys = m_midiKeyboard.Where(i => i.KeyType == KeyColor.Black);
            var activeKey = m_midiKeyboard.Where(i => i.Index == 1);
            /* ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::: */
#if DEBUG && m_debugKeysDrawing
            bool o = false;
            whiteKeys.ToList().ForEach(i => {
                e.Graphics.FillRectangle(i.CNoteKey ? Brushes.BlueViolet : o ? Brushes.Yellow : Brushes.Red, i.Rectangle);
                o = !o;
            });
#endif
            /* ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::: */
            whiteKeys.ToList().ForEach(i => {
                if (i.CNoteKey == true) {
                    using (var brush = new LinearGradientBrush(i.Rectangle, Color.Transparent, Color.FromArgb(186, 190, 197), LinearGradientMode.Vertical)) {
                        e.Graphics.DrawString("C" + whiteKeys.ToList().IndexOf(i) / 7, m_font, Brushes.Black, i.Rectangle.Left - 2, i.Rectangle.Y - 15);
                        e.Graphics.FillRectangle(brush, i.Rectangle);
                    }
                }
                if (i.Index == m_rootKey) {
                    e.Graphics.FillRectangle(m_rootWhiteKeyBrush, i.Rectangle);
                }
                OnPaintActiveKey(e, i.Index, i.Rectangle);
            });
            blackKeys.ToList().ForEach(i => OnPaintBlackKey(e, i.Rectangle, i.Index));
            e.Graphics.ResetClip();
            /* ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::: */
            if (m_device == null || OutputDeviceBase.DeviceCount <= 0 && m_shown) {
                if (m_bitmap == null) { // 
                    m_bitmap = new Bitmap(Width, Height);
                    DrawToBitmap(m_bitmap, new Rectangle(0, 0, Width, Height));
                    m_bitmap = ApplyGaussianBlur(m_bitmap, 4);
                    Invalidate();
                }
                else {
                    e.Graphics.DrawImage(m_bitmap, 0, 0);
                    e.Graphics.FillRectangle(m_overlayBrush, 0, 0, Width, Height);
                    e.Graphics.PixelOffsetMode = PixelOffsetMode.Default;
                    if (OutputDeviceBase.DeviceCount <= 0) {
                        e.Graphics.DrawString("No loopMIDI loopback MIDI ports available.", m_font, m_titleTextBrushFocused, DisplayRectangle, m_centerFormatFlag);
                    }
                    else {
                        e.Graphics.DrawString("In the root folder of the application, please create a text file called 'loopMIDI.txt', " +
                            "in the text file put the index which will represent the loopMIDI loopback MIDI port. After that restart the" +
                            " application and at runtime it will try to connect to the emulated MIDI. If you don't know the index start from " +
                            "0 to ∞ until you get it right. xD \r\n (Sorry for laziness!)",
                            m_font, m_titleTextBrushFocused, new RectangleF(10, 10, Width - 10, Height - 10), m_centerFormatFlag);
                    }
                }
            }
            e.Graphics.ResetTransform();
            e.Graphics.FillEllipse(m_ctrlMinRect.Contains(m_mousePosition) ? Brushes.White : Brushes.Gray, m_ctrlMinRect);
            e.Graphics.FillEllipse(m_ctrlClsRect.Contains(m_mousePosition) ? Brushes.OrangeRed : Brushes.Gray, m_ctrlClsRect);
        }

        protected void OnPaintActiveKey(PaintEventArgs e, int keyIndex, Rectangle keyRect) {
            if (PlayingKeys.Count <= 0) { return; }
            if (PlayingKeys.Contains(keyIndex)) { e.Graphics.FillRectangle(m_playingKeyBrush, keyRect); }
        }

        protected void OnPaintBlackKey(PaintEventArgs e, Rectangle blackKeyRect, int keyIndex) {
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(47, 48, 49)), blackKeyRect);
            using (var brush = new LinearGradientBrush(blackKeyRect, Color.FromArgb(59, 60, 61), Color.FromArgb(90, 90, 90), LinearGradientMode.Vertical)) {
                var pth = new GraphicsPath();
                pth.AddLine(blackKeyRect.X, blackKeyRect.Bottom - 2, blackKeyRect.Right - 2, blackKeyRect.Bottom - 15);
                pth.AddLine(blackKeyRect.Right - 2, blackKeyRect.Top, blackKeyRect.Right - 2, blackKeyRect.Top);
                pth.AddLine(blackKeyRect.X, blackKeyRect.Top, blackKeyRect.X, blackKeyRect.Top);
                pth.CloseFigure();
                e.Graphics.FillPath(brush, pth);
                pth.Dispose();
            }
            var smoothState = e.Graphics.Save();
            e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
            using (var brush = new LinearGradientBrush(new Rectangle(0, blackKeyRect.Y, 2, blackKeyRect.Height - 3), Color.Transparent, Color.FromArgb(144, 145, 146), LinearGradientMode.Vertical)) {
                e.Graphics.DrawLine(new Pen(brush), blackKeyRect.X + 2, blackKeyRect.Y + 1, blackKeyRect.X + 2, blackKeyRect.Y + 1 + blackKeyRect.Height - 5);
            }
            e.Graphics.DrawLine(new Pen(Color.FromArgb(87, 88, 89), 1), blackKeyRect.X, blackKeyRect.Bottom - 4, blackKeyRect.X + blackKeyRect.Width - 2, blackKeyRect.Bottom - 4);
            e.Graphics.Restore(smoothState);
            if (keyIndex == m_rootKey) {
                e.Graphics.FillRectangle(m_rootBlackKeyBrush, blackKeyRect);
            }
            OnPaintActiveKey(e, keyIndex, blackKeyRect);
        }
    }

    public class GlobalKeyboardHook
    {

        private LowLevelKeyboardProc m_proc;

        private IntPtr m_hookID = IntPtr.Zero;

        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x0100;

        private const int WM_KEYUP = 0x0101;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        public event EventHandler<KeyUpEventArgs> KeyUp;

        public GlobalKeyboardHook() {
            m_proc = HookCallback;
        }

        ~GlobalKeyboardHook() {
            UnhookWindowsHookEx(m_hookID);
        }

        public void HookKeyboard() {
            m_hookID = SetWindowsHookEx(WH_KEYBOARD_LL, m_proc, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
        }

        public void UnhookKeyboard() {
            UnhookWindowsHookEx(m_hookID);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN) {
                int vkCode = Marshal.ReadInt32(lParam);
                KeyPressed?.Invoke(this, new KeyPressedEventArgs((Keys)vkCode));
            }
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP) {
                int vkCode = Marshal.ReadInt32(lParam);
                KeyUp?.Invoke(this, new KeyUpEventArgs((Keys)vkCode));
            }
            return CallNextHookEx(m_hookID, nCode, wParam, lParam);
        }

        public class KeyUpEventArgs : EventArgs
        {
            public Keys KeyUp { get; private set; }

            public KeyUpEventArgs(Keys key) {
                KeyUp = key;
            }
        }

        public class KeyPressedEventArgs : EventArgs
        {
            public Keys KeyPressed { get; private set; }

            public KeyPressedEventArgs(Keys key) {
                KeyPressed = key;
            }
        }
    }
}