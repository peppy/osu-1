// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Skinning
{
    public class SkinCheckboxRow : SkinSettingsRow
    {
        private readonly SkinSetting lookup;

        public SkinCheckboxRow(string title, SkinSetting lookup, Skin[] sources)
            : base(title, sources)
        {
            this.lookup = lookup;
        }

        protected override Drawable CreateCellContent(Skin skin) => new CheckboxCell(skin, lookup);

        private class CheckboxCell : CompositeDrawable
        {
            // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
            private readonly Bindable<bool?> bindable;

            public CheckboxCell(Skin skin, SkinSetting lookup)
            {
                RelativeSizeAxes = Axes.Both;

                Bindable<bool> isOverriding = new Bindable<bool>();
                Bindable<bool> checkboxValue = new Bindable<bool>();

                CenteredCheckbox checkbox;

                InternalChild = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        checkbox = new CenteredCheckbox { Current = checkboxValue, },
                    }
                };

                // in some cases this is a read-only value.
                if (!(skin.Configuration is DatabasedSkinConfiguration bindableConfiguration))
                {
                    if (skin.Configuration.CursorExpand.HasValue)
                    {
                        isOverriding.Value = true;
                        checkboxValue.Value = skin.Configuration.CursorExpand.Value;
                    }
                    else
                    {
                        checkbox.Hide();
                    }

                    isOverriding.Disabled = true;
                    checkboxValue.Disabled = true;
                    return;
                }

                OsuButton overrideText;

                AddRangeInternal(new Drawable[]
                {
                    new RemoveOverrideButton { Action = () => isOverriding.Value = false },
                    overrideText = new OverrideButton
                    {
                        Alpha = 0,
                        Action = () => isOverriding.Value = true,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                });

                bindable = bindableConfiguration.GetBindable<bool?>(lookup);

                if (bindable.Value.HasValue)
                {
                    isOverriding.Value = true;
                    checkboxValue.Value = bindable.Value ?? default;
                }
                else
                {
                    // if override is not enabled, use the closest previously overridden value.
                    // checkboxValue.Value = lastUsableValue;
                }

                isOverriding.BindValueChanged(e =>
                {
                    checkboxValue.Disabled = !e.NewValue;
                    overrideText.Alpha = !e.NewValue ? 1 : 0;
                }, true);

                checkboxValue.ValueChanged += e => bindable.Value = e.NewValue;
                isOverriding.ValueChanged += e => bindable.Value = e.NewValue ? checkboxValue.Value : (bool?)null;

                //if (isOverriding.Value)
                //    lastUsableValue = checkboxValue.Value;
            }

            private class OverrideButton : OsuButton
            {
                public OverrideButton()
                {
                    Text = "override";
                    Width = SkinningPanel.CELL_SIZE;
                }
            }

            private class RemoveOverrideButton : ClickableContainer, IHasTooltip
            {
                private Color4 buttonColour;

                private bool hovering;

                public RemoveOverrideButton()
                {
                    RelativeSizeAxes = Axes.Y;
                    Width = 10;
                    Alpha = 0f;
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colour)
                {
                    buttonColour = colour.Red;

                    Child = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        CornerRadius = 3,
                        Masking = true,
                        Colour = buttonColour,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Colour = buttonColour.Opacity(0.1f),
                            Type = EdgeEffectType.Glow,
                            Radius = 2,
                        },
                        Size = new Vector2(0.5f, 0.8f),
                        Child = new Box { RelativeSizeAxes = Axes.Both },
                    };
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();
                    UpdateState();
                }

                public string TooltipText => "Remove override";

                protected override bool OnMouseDown(MouseDownEvent e) => true;

                protected override bool OnMouseUp(MouseUpEvent e) => true;

                protected override bool OnHover(HoverEvent e)
                {
                    hovering = true;
                    UpdateState();
                    return true;
                }

                protected override void OnHoverLost(HoverLostEvent e)
                {
                    hovering = false;
                    UpdateState();
                }

                public void UpdateState()
                {
                    this.FadeTo(hovering ? 1f : 0.65f, 200, Easing.OutQuint);
                }
            }

            private class CenteredCheckbox : OsuCheckbox
            {
                public CenteredCheckbox()
                {
                    Nub.Origin = Anchor.Centre;
                    Nub.Anchor = Anchor.Centre;
                    Nub.Margin = new MarginPadding();

                    AutoSizeAxes = Axes.None;
                    RelativeSizeAxes = Axes.Both;
                }
            }
        }
    }
}
