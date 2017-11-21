using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using CoreGraphics;

namespace PageViewController.ViewControllers
{
    public class DPageViewController : LazyTabBarController.LazyTabBarController
    {
        public override UIViewController InitTabControllers(int index)
        {
            switch (index)
            {
                case 0:
                    return new A();
                case 1:
                    return new B();
                case 2:
                    return new C();
                case 3:
                    return new D();
                default:
                    return new A();
            }
        }

        public override (string name, string icon) InitTabBarNameAndIcon(int index)
        {
            switch (index)
            {
                case 0:
                    return ("test", "alex_simple_small");
                case 1:
                    return ("test", "ge_icon_challenges@");
                case 2:
                    return ("", "ge_icon_rewards");
                case 3:
                    return ("test", "");
                default:
                    return ("test", "icon_activity");
            }
        }

        public override void InitSelectedTab()
        {
            CurrentIndex = 2;
        }

        public override UIColor SelectedTabBarTintColor => UIColor.Orange;
    }
}