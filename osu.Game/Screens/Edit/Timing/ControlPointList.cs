// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Timing
{
    public partial class ControlPointList : CompositeDrawable
    {
        private OsuButton deleteButton = null!;
        private RoundedButton addButton = null!;

        [Resolved]
        private EditorClock clock { get; set; } = null!;

        [Resolved]
        protected EditorBeatmap Beatmap { get; private set; } = null!;

        [Resolved]
        private Bindable<ControlPointGroup?> selectedGroup { get; set; } = null!;

        [Cached]
        private OverlayColourProvider overlayColourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.Both;

            const float margins = 10;
            InternalChildren = new Drawable[]
            {
                new ControlPointTable
                {
                    RelativeSizeAxes = Axes.Both,
                    Groups = { BindTarget = Beatmap.ControlPointInfo.Groups, },
                },
                new Container
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Height = 50,
                            RelativeSizeAxes = Axes.X,
                            Anchor = Anchor.CentreLeft,
                            Colour = overlayColourProvider.Background2,
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.BottomLeft,
                            Padding = new MarginPadding { Left = margins, Bottom = margins },
                            Children = new Drawable[]
                            {
                                new RoundedButton
                                {
                                    Text = "Select closest to current time",
                                    Action = goToCurrentGroup,
                                    Size = new Vector2(220, 30),
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                },
                            }
                        },
                        new FillFlowContainer
                        {
                            Direction = FillDirection.Horizontal,
                            Anchor = Anchor.BottomRight,
                            Spacing = new Vector2(5),
                            Padding = new MarginPadding { Right = margins, Bottom = margins },
                            Children = new Drawable[]
                            {
                                deleteButton = new RoundedButton
                                {
                                    Text = "-",
                                    Size = new Vector2(30, 30),
                                    Action = delete,
                                    Anchor = Anchor.BottomRight,
                                    Origin = Anchor.BottomRight,
                                    BackgroundColour = colours.Red3,
                                },
                                addButton = new RoundedButton
                                {
                                    Action = addNew,
                                    Size = new Vector2(160, 30),
                                    Anchor = Anchor.BottomRight,
                                    Origin = Anchor.BottomRight,
                                },
                            }
                        },
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedGroup.BindValueChanged(selected =>
            {
                deleteButton.Enabled.Value = selected.NewValue != null;

                addButton.Text = selected.NewValue != null
                    ? "+ Clone to current time"
                    : "+ Add at current time";
            }, true);
        }

        protected override bool OnClick(ClickEvent e)
        {
            selectedGroup.Value = null;
            return true;
        }

        protected override void Update()
        {
            base.Update();

            addButton.Enabled.Value = clock.CurrentTimeAccurate != selectedGroup.Value?.Time;
        }

        private void goToCurrentGroup()
        {
            double accurateTime = clock.CurrentTimeAccurate;

            var activeTimingPoint = Beatmap.ControlPointInfo.TimingPointAt(accurateTime);
            var activeEffectPoint = Beatmap.ControlPointInfo.EffectPointAt(accurateTime);

            double latestActiveTime = Math.Max(activeTimingPoint.Time, activeEffectPoint.Time);
            selectedGroup.Value = Beatmap.ControlPointInfo.GroupAt(latestActiveTime);
        }

        private void delete()
        {
            if (selectedGroup.Value == null)
                return;

            Beatmap.ControlPointInfo.RemoveGroup(selectedGroup.Value);

            selectedGroup.Value = Beatmap.ControlPointInfo.Groups.FirstOrDefault(g => g.Time >= clock.CurrentTime);
        }

        private void addNew()
        {
            bool isFirstControlPoint = !Beatmap.ControlPointInfo.TimingPoints.Any();

            var group = Beatmap.ControlPointInfo.GroupAt(clock.CurrentTime, true);

            if (isFirstControlPoint)
                group.Add(new TimingControlPoint());
            else
            {
                // Try and create matching types from the currently selected control point.
                var selected = selectedGroup.Value;

                if (selected != null && !ReferenceEquals(selected, group))
                {
                    foreach (var controlPoint in selected.ControlPoints)
                    {
                        group.Add(controlPoint.DeepClone());
                    }
                }
            }

            selectedGroup.Value = group;
        }
    }
}
