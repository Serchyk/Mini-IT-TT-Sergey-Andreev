// Copyright (c) 2015 - 2021 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

//.........................
//.....Generated Class.....
//.........................
//.......Do not edit.......
//.........................

using System.Collections.Generic;
// ReSharper disable All
namespace Doozy.Runtime.UIManager.Components
{
    public partial class UIButton
    {
        public static IEnumerable<UIButton> GetButtons(UIButtonId.Action id) => GetButtons(nameof(UIButtonId.Action), id.ToString());
        public static bool SelectButton(UIButtonId.Action id) => SelectButton(nameof(UIButtonId.Action), id.ToString());

        public static IEnumerable<UIButton> GetButtons(UIButtonId.Direction id) => GetButtons(nameof(UIButtonId.Direction), id.ToString());
        public static bool SelectButton(UIButtonId.Direction id) => SelectButton(nameof(UIButtonId.Direction), id.ToString());

        public static IEnumerable<UIButton> GetButtons(UIButtonId.Generic id) => GetButtons(nameof(UIButtonId.Generic), id.ToString());
        public static bool SelectButton(UIButtonId.Generic id) => SelectButton(nameof(UIButtonId.Generic), id.ToString());

        public static IEnumerable<UIButton> GetButtons(UIButtonId.Media id) => GetButtons(nameof(UIButtonId.Media), id.ToString());
        public static bool SelectButton(UIButtonId.Media id) => SelectButton(nameof(UIButtonId.Media), id.ToString());

        public static IEnumerable<UIButton> GetButtons(UIButtonId.Navigation id) => GetButtons(nameof(UIButtonId.Navigation), id.ToString());
        public static bool SelectButton(UIButtonId.Navigation id) => SelectButton(nameof(UIButtonId.Navigation), id.ToString());

        public static IEnumerable<UIButton> GetButtons(UIButtonId.Shop id) => GetButtons(nameof(UIButtonId.Shop), id.ToString());
        public static bool SelectButton(UIButtonId.Shop id) => SelectButton(nameof(UIButtonId.Shop), id.ToString());
    }
}

namespace Doozy.Runtime.UIManager
{
    public partial class UIButtonId
    {
        public enum Action
        {
            AddToCart,
            AddToFavorites,
            Calendar,
            Camera,
            Close,
            Gamepad,
            Gift,
            Language,
            Map,
            Minus,
            Plus,
            Redo,
            Refresh,
            Reply,
            ReportBug,
            Search,
            SendEmail,
            Share,
            Star,
            Tag,
            Undo
        }

        public enum Direction
        {
            Down,
            Left,
            Right,
            Up
        }

        public enum Generic
        {
            Add,
            Back,
            Cancel,
            Clear,
            Close,
            Delete,
            Disable,
            Enable,
            Help,
            Load,
            No,
            Ok,
            Pause,
            Play,
            Refresh,
            Remove,
            Restore,
            Resume,
            Save,
            Send,
            Settings,
            Start,
            Stats,
            Stop,
            Yes
        }

        public enum Media
        {
            Backward,
            BackwardStep,
            Forward,
            ForwardStep,
            Pause,
            Play,
            PlayPause,
            Repeat,
            Stop
        }

        public enum Navigation
        {
            Back,
            Cancel,
            Next,
            Ok
        }

        public enum Shop
        {
            Buy,
            Close,
            Open,
            Sell
        }    
    }
}