using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CakeGame.Models;
using CakeGame.Controllers;
using CakeGame.Views;
using CakeGame.Helpers;

namespace CakeGame
{
    public partial class GameForm : Form
    {
        private readonly CakeModel cakeModel;
        private readonly GameModel gameModel;
        private readonly GameController controller;
        private readonly GameView view;
        private readonly TimerManager timerManager;

        public GameForm()
        {
            Text = "CakeGame - Собери торт!";
            ClientSize = new Size(Constants.FORM_W, Constants.FORM_H);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.FromArgb(0xf8, 0xe8, 0xf5);
            DoubleBuffered = true;

            cakeModel = new CakeModel();
            gameModel = new GameModel(cakeModel);
            controller = new GameController(gameModel, cakeModel);
            view = new GameView(gameModel, controller);
            timerManager = new TimerManager(controller);

            gameModel.OnGameStateChanged += () => Invalidate();
            controller.OnStateChanged += () => Invalidate();

            timerManager.StartAll(this);

            MouseClick += OnMouseClick;
            MouseMove += OnMouseMove;

            gameModel.NewGame();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            controller.TrashHover = new Rectangle(Constants.TRASH_X, Constants.TRASH_Y, Constants.TRASH_W, Constants.TRASH_H).Contains(e.Location);
            Invalidate();
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (gameModel.ShowResult)
            {
                int bx = Constants.FORM_W / 2 - 230, by = Constants.FORM_H / 2 - 160;
                int bw = 460, bh = 320;
                var okButton = new Rectangle(bx + bw / 2 - 60, by + bh - 52, 120, 34);
                if (okButton.Contains(e.Location))
                {
                    gameModel.ShowResult = false;  

                    if (gameModel.Lives > 0)
                        gameModel.NextRound();
                    else
                        gameModel.NewGame();
                    Invalidate();
                }
                return;
            }

            if (new Rectangle(Constants.FORM_W - 118, 8, 108, 28).Contains(e.Location))
            {
                controller.NewGameFlash = true;
                gameModel.NewGame();
                var t = new Timer { Interval = 150 };
                t.Tick += (ts, te) => { controller.NewGameFlash = false; t.Stop(); Invalidate(); };
                t.Start();
                return;
            }

            if (new Rectangle(Constants.FORM_W - 238, 8, 112, 28).Contains(e.Location))
            {
                controller.CompareFlash = true;
                gameModel.Compare();

                if (gameModel.ResultPercent != 100)
                {
                    controller.SetWrongFlash(18);
                    controller.SetShake(12);
                }

                var t = new Timer { Interval = 150 };
                t.Tick += (ts, te) => { controller.CompareFlash = false; t.Stop(); Invalidate(); };
                t.Start();
                return;
            }

            if (new Rectangle(Constants.TRASH_X, Constants.TRASH_Y, Constants.TRASH_W, Constants.TRASH_H).Contains(e.Location))
            {
                controller.ClearTrash();
                return;
            }

            for (int i = 0; i < Constants.DispRects.Length; i++)
            {
                if (Constants.DispRects[i].Contains(e.Location))
                {
                    controller.TriggerDispenser(i);
                    return;
                }
            }

            for (int i = 0; i < Constants.CREAM_COLORS.Length; i++)
            {
                int x = Constants.PALETTE_X + i * (Constants.PALETTE_W + 5);
                int y = Constants.PALETTE_Y;
                if (new Rectangle(x, y, Constants.PALETTE_W, Constants.PALETTE_H).Contains(e.Location))
                {
                    gameModel.SelectedCreamColor = i;
                    Invalidate();
                    return;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                int x = Constants.SPONGE_ICONS_X;
                int y = Constants.SPONGE_ICONS_Y + i * Constants.SPONGE_ICON_SPACING;
                if (new Rectangle(x, y, Constants.SPONGE_ICON_SIZE, Constants.SPONGE_ICON_SIZE).Contains(e.Location))
                {
                    gameModel.SelectedSponge = i;
                    controller.AddLayerSponge();
                    return;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            if (controller.ShakeTimer > 0)
            {
                int dx = (controller.ShakeTimer % 4 < 2) ? 4 : -4;
                g.TranslateTransform(dx, 0);
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            controller.UpdateWrongFlash();
            controller.UpdateShake();

            view.Render(g);

            if (controller.ShakeTimer > 0)
                g.ResetTransform();
        }
    }
}