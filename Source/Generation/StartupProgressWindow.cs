using UnityEngine;
using Verse;

namespace RandomStartMod
{
    // A real opaque window keeps native input routing intact while all setup
    // pages continue to receive their normal drawing and initialization calls.
    internal sealed class StartupProgressWindow : Window
    {
        private readonly NativeStartupController controller;

        internal StartupProgressWindow(NativeStartupController controller)
        {
            this.controller = controller;
            layer = WindowLayer.Super;
            doWindowBackground = false;
            drawShadow = false;
            absorbInputAroundWindow = true;
            preventCameraMotion = true;
            preventDrawTutor = true;
            focusWhenOpened = true;
            forceCatchAcceptAndCancelEventEvenIfUnfocused = true;
            closeOnAccept = false;
            closeOnCancel = false;
            closeOnClickedOutside = false;
            soundAppear = null;
            soundClose = null;
        }

        public override Vector2 InitialSize => new Vector2(UI.screenWidth, UI.screenHeight);
        // The project's publicized core references expose these virtual members.
        public override float Margin => 0f;
        public override void SetInitialSizeAndPosition() => windowRect = new Rect(0f, 0f, UI.screenWidth, UI.screenHeight);
        public override void Notify_ResolutionChanged() => SetInitialSizeAndPosition();

        public override void WindowUpdate()
        {
            base.WindowUpdate();
            // Session cleanup can run during queued generation. Remove a stale
            // cover here on the UI thread rather than touching windows there.
            if (Current.ProgramState != ProgramState.Entry
                || !ReferenceEquals(RandomStartData.State?.NativeStartup, controller)) Close(false);
        }

        public override void OnAcceptKeyPressed() { }
        public override void OnCancelKeyPressed() => controller.ContinueManually();

        public override void DoWindowContents(Rect inRect)
        {
            Color color = GUI.color;
            GameFont font = Text.Font;
            TextAnchor anchor = Text.Anchor;
            try
            {
                GUI.color = Color.white;
                Widgets.DrawBoxSolid(inRect, new Color(0.08f, 0.09f, 0.11f));
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(new Rect(20f, inRect.height / 2f - 60f, inRect.width - 40f, 60f),
                    "RandomStartMod.PreparingRandomStart".Translate());
                Text.Font = GameFont.Small;
                float buttonWidth = Mathf.Min(280f, inRect.width - 40f);
                if (Widgets.ButtonText(new Rect((inRect.width - buttonWidth) / 2f, inRect.height / 2f + 20f, buttonWidth, 40f),
                    "RandomStartMod.ContinueSetupManually".Translate())) controller.ContinueManually();
            }
            finally
            {
                GUI.color = color;
                Text.Font = font;
                Text.Anchor = anchor;
            }
        }
    }
}
