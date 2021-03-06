#region Copyright & License Information
/*
 * Copyright 2007-2014 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Graphics
{
	public struct SelectionBarsRenderable : IRenderable
	{
		readonly WPos pos;
		readonly Actor actor;

		public SelectionBarsRenderable(Actor actor)
			: this(actor.CenterPosition, actor) { }

		public SelectionBarsRenderable(WPos pos, Actor actor)
		{
			this.pos = pos;
			this.actor = actor;
		}

		public WPos Pos { get { return pos; } }

		public float Scale { get { return 1f; } }
		public PaletteReference Palette { get { return null; } }
		public int ZOffset { get { return 0; } }
		public bool IsDecoration { get { return true; } }

		public IRenderable WithScale(float newScale) { return this; }
		public IRenderable WithPalette(PaletteReference newPalette) { return this; }
		public IRenderable WithZOffset(int newOffset) { return this; }
		public IRenderable OffsetBy(WVec vec) { return new SelectionBarsRenderable(pos + vec, actor); }
		public IRenderable AsDecoration() { return this; }

		void DrawExtraBars(WorldRenderer wr, float2 start, float2 end)
		{
			foreach (var extraBar in actor.TraitsImplementing<ISelectionBar>())
			{
				var value = extraBar.GetValue();
				if (value != 0)
				{
					start.Y += (int)(4 / wr.Viewport.Zoom);
					end.Y += (int)(4 / wr.Viewport.Zoom);
					DrawSelectionBar(wr, start, end, extraBar.GetValue(), extraBar.GetColor());
				}
			}
		}

		void DrawSelectionBar(WorldRenderer wr, float2 start, float2 end, float value, Color barColor)
		{
			var c = Color.FromArgb(128, 30, 30, 30);
			var c2 = Color.FromArgb(128, 10, 10, 10);
			var p = new float2(0, -4 / wr.Viewport.Zoom);
			var q = new float2(0, -3 / wr.Viewport.Zoom);
			var r = new float2(0, -2 / wr.Viewport.Zoom);

			var barColor2 = Color.FromArgb(255, barColor.R / 2, barColor.G / 2, barColor.B / 2);

			var z = float2.Lerp(start, end, value);
			var wlr = Game.Renderer.WorldLineRenderer;
			wlr.DrawLine(start + p, end + p, c, c);
			wlr.DrawLine(start + q, end + q, c2, c2);
			wlr.DrawLine(start + r, end + r, c, c);

			wlr.DrawLine(start + p, z + p, barColor2, barColor2);
			wlr.DrawLine(start + q, z + q, barColor, barColor);
			wlr.DrawLine(start + r, z + r, barColor2, barColor2);
		}

		Color GetHealthColor(Health health)
		{
			if (Game.Settings.Game.TeamHealthColors)
			{
				var isAlly = actor.Owner.IsAlliedWith(actor.World.LocalPlayer)
					|| (actor.EffectiveOwner != null && actor.EffectiveOwner.Disguised
					&& actor.World.LocalPlayer.IsAlliedWith(actor.EffectiveOwner.Owner));
				return isAlly ? Color.LimeGreen : actor.Owner.NonCombatant ? Color.Tan : Color.Red;
			}
			else
				return health.DamageState == DamageState.Critical ? Color.Red :
					health.DamageState == DamageState.Heavy ? Color.Yellow : Color.LimeGreen;
		}

		void DrawHealthBar(WorldRenderer wr, Health health, float2 start, float2 end)
		{
			if (health == null || health.IsDead)
				return;

			var c = Color.FromArgb(128, 30, 30, 30);
			var c2 = Color.FromArgb(128, 10, 10, 10);
			var p = new float2(0, -4 / wr.Viewport.Zoom);
			var q = new float2(0, -3 / wr.Viewport.Zoom);
			var r = new float2(0, -2 / wr.Viewport.Zoom);

			var healthColor = GetHealthColor(health);
			var healthColor2 = Color.FromArgb(
				255,
				healthColor.R / 2,
				healthColor.G / 2,
				healthColor.B / 2);

			var z = float2.Lerp(start, end, (float)health.HP / health.MaxHP);

			var wlr = Game.Renderer.WorldLineRenderer;
			wlr.DrawLine(start + p, end + p, c, c);
			wlr.DrawLine(start + q, end + q, c2, c2);
			wlr.DrawLine(start + r, end + r, c, c);

			wlr.DrawLine(start + p, z + p, healthColor2, healthColor2);
			wlr.DrawLine(start + q, z + q, healthColor, healthColor);
			wlr.DrawLine(start + r, z + r, healthColor2, healthColor2);

			if (health.DisplayHp != health.HP)
			{
				var deltaColor = Color.OrangeRed;
				var deltaColor2 = Color.FromArgb(
					255,
					deltaColor.R / 2,
					deltaColor.G / 2,
					deltaColor.B / 2);
				var zz = float2.Lerp(start, end, (float)health.DisplayHp / health.MaxHP);

				wlr.DrawLine(z + p, zz + p, deltaColor2, deltaColor2);
				wlr.DrawLine(z + q, zz + q, deltaColor, deltaColor);
				wlr.DrawLine(z + r, zz + r, deltaColor2, deltaColor2);
			}
		}

		public void BeforeRender(WorldRenderer wr) { }
		public void Render(WorldRenderer wr)
		{
			if (!actor.IsInWorld || actor.IsDead)
				return;

			var health = actor.TraitOrDefault<Health>();

			var screenPos = wr.ScreenPxPosition(pos);
			var bounds = actor.Bounds;
			bounds.Offset(screenPos.X, screenPos.Y);

			var start = new float2(bounds.Left + 1, bounds.Top);
			var end = new float2(bounds.Right - 1, bounds.Top);

			DrawHealthBar(wr, health, start, end);
			DrawExtraBars(wr, start, end);
		}

		public void RenderDebugGeometry(WorldRenderer wr) { }
	}
}
