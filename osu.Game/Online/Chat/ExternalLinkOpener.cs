﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Overlays;
using osu.Game.Overlays.Chat;

namespace osu.Game.Online.Chat
{
    public class ExternalLinkOpener : Component
    {
        private GameHost host;
        private DialogOverlay dialogOverlay;
        private Bindable<bool> warnAboutOpeningExternal;

        [BackgroundDependencyLoader(true)]
        private void load(GameHost host, DialogOverlay dialogOverlay, OsuConfigManager config)
        {
            this.host = host;
            this.dialogOverlay = dialogOverlay;
            warnAboutOpeningExternal = config.GetBindable<bool>(OsuSetting.WarnAboutOpeningExternalLink);
        }

        public void OpenUrlExternally(string url)
        {
            void externalAction() => host.OpenUrlExternally(url);

            if (warnAboutOpeningExternal)
                dialogOverlay.Push(new ExternalLinkDialog(url, externalAction));
            else
                externalAction();
        }
    }
}
