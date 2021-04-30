// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Skinning.Editor
{
    public class SkinSelectionHandler : SelectionHandler<ISkinnableComponent>
    {
        public override bool HandleFlip(Direction direction)
        {
            var quad = GetSurroundingQuad(SelectedBlueprints.Select(b => b.ScreenSpaceSelectionPoint).ToArray());
            var centre = quad.Centre;

            foreach (var blueprint in SelectedBlueprints)
            {
                var drawable = (Drawable)blueprint.Item;

                var blueprintSelectionPointInParent = drawable.Parent.ToLocalSpace(blueprint.ScreenSpaceSelectionPoint);
                var centreInParent = drawable.Parent.ToLocalSpace(centre);

                // the offset of this blueprint from the centre of selection in drawable parent coords.
                // using the selection point for simplicity (each selected component may have different origins etc. - this standardises the calculation).
                var offset = blueprintSelectionPointInParent - centreInParent;

                // need to apply double the offset to the original drawable position.
                var flippedLocation = drawable.Position - offset * 2;

                switch (direction)
                {
                    case Direction.Horizontal:
                        drawable.Scale = new Vector2(-drawable.Scale.X, drawable.Scale.Y);

                        if (drawable.Origin.HasFlagFast(Anchor.x2))
                            drawable.Origin &= ~Anchor.x2;
                        else if (!drawable.Origin.HasFlagFast(Anchor.x1))
                            drawable.Origin |= Anchor.x2;

                        drawable.Position = new Vector2(flippedLocation.X, drawable.Position.Y);
                        break;

                    case Direction.Vertical:
                        drawable.Scale = new Vector2(drawable.Scale.X, -drawable.Scale.Y);

                        if (drawable.Origin.HasFlagFast(Anchor.y2))
                            drawable.Origin &= ~Anchor.y2;
                        else if (!drawable.Origin.HasFlagFast(Anchor.y1))
                            drawable.Origin |= Anchor.y2;

                        drawable.Position = new Vector2(drawable.Position.X, flippedLocation.Y);
                        break;
                }
            }

            return true;
        }

        protected override void DeleteItems(IEnumerable<ISkinnableComponent> items)
        {
            foreach (var i in items)
            {
                ((Drawable)i).Expire();
                SelectedItems.Remove(i);
            }
        }

        protected override IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint<ISkinnableComponent>> selection)
        {
            yield return new OsuMenuItem("Anchor")
            {
                Items = createAnchorItems().ToArray()
            };

            foreach (var item in base.GetContextMenuItemsForSelection(selection))
                yield return item;

            IEnumerable<AnchorMenuItem> createAnchorItems()
            {
                var displayableAnchors = new[]
                {
                    Anchor.TopLeft,
                    Anchor.TopCentre,
                    Anchor.TopRight,
                    Anchor.CentreLeft,
                    Anchor.Centre,
                    Anchor.CentreRight,
                    Anchor.BottomLeft,
                    Anchor.BottomCentre,
                    Anchor.BottomRight,
                };

                return displayableAnchors.Select(a =>
                {
                    var countExisting = selection.Count(b => ((Drawable)b.Item).Anchor == a);
                    var countTotal = selection.Count();

                    TernaryState state;

                    if (countExisting == countTotal)
                        state = TernaryState.True;
                    else if (countExisting > 0)
                        state = TernaryState.Indeterminate;
                    else
                        state = TernaryState.False;

                    return new AnchorMenuItem(a, selection, _ => applyAnchor(a))
                    {
                        State = { Value = state }
                    };
                });
            }
        }

        private void applyAnchor(Anchor anchor)
        {
            foreach (var item in SelectedItems)
                ((Drawable)item).Anchor = anchor;
        }

        protected override void OnSelectionChanged()
        {
            base.OnSelectionChanged();

            SelectionBox.CanRotate = true;
            SelectionBox.CanScaleX = true;
            SelectionBox.CanScaleY = true;
            SelectionBox.CanReverse = false;
        }

        public override bool HandleRotation(float angle)
        {
            // TODO: this doesn't correctly account for origin/anchor specs being different in a multi-selection.
            foreach (var c in SelectedBlueprints)
                ((Drawable)c.Item).Rotation += angle;

            return base.HandleRotation(angle);
        }

        public override bool HandleScale(Vector2 scale, Anchor anchor)
        {
            adjustScaleFromAnchor(ref scale, anchor);

            foreach (var c in SelectedBlueprints)
                ((Drawable)c.Item).Scale += scale * 0.01f;

            return true;
        }

        public override bool HandleMovement(MoveSelectionEvent<ISkinnableComponent> moveEvent)
        {
            foreach (var c in SelectedBlueprints)
            {
                Drawable drawable = (Drawable)c.Item;
                drawable.Position += drawable.ScreenSpaceDeltaToParentSpace(moveEvent.ScreenSpaceDelta);
            }

            return true;
        }

        private static void adjustScaleFromAnchor(ref Vector2 scale, Anchor reference)
        {
            // cancel out scale in axes we don't care about (based on which drag handle was used).
            if ((reference & Anchor.x1) > 0) scale.X = 0;
            if ((reference & Anchor.y1) > 0) scale.Y = 0;

            // reverse the scale direction if dragging from top or left.
            if ((reference & Anchor.x0) > 0) scale.X = -scale.X;
            if ((reference & Anchor.y0) > 0) scale.Y = -scale.Y;
        }

        public class AnchorMenuItem : TernaryStateMenuItem
        {
            public AnchorMenuItem(Anchor anchor, IEnumerable<SelectionBlueprint<ISkinnableComponent>> selection, Action<TernaryState> action)
                : base(anchor.ToString(), getNextState, MenuItemType.Standard, action)
            {
            }

            private void updateState(TernaryState obj)
            {
                throw new NotImplementedException();
            }

            private static TernaryState getNextState(TernaryState state) => TernaryState.True;
        }
    }
}
