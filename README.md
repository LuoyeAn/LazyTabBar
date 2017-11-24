# LazyTabBar
[![Build status](https://build.appcenter.ms/v0.1/apps/824f3e37-9102-412f-89a6-caae9ab0c3a4/branches/CustomPageViewController/badge)](https://appcenter.ms)
[![NuGet](https://img.shields.io/nuget/v/LazyTabBar.svg)](https://www.nuget.org/packages/LazyTabBar)
[![NuGet Pre Release](https://img.shields.io/nuget/vpre/LazyTabBar.svg)](https://www.nuget.org/packages/LazyTabBar)
[![CodeFactor](https://www.codefactor.io/repository/github/tingtingan/LazyTabBar/badge)](https://www.codefactor.io/repository/github/tingtingan/LazyTabBar)
[![BCH compliance](https://bettercodehub.com/edge/badge/TingtingAn/LazyTabBar?branch=CustomPageViewController)](https://bettercodehub.com/)

A LazyTabBarController that implements swiping left/right to switch controllers and lazying to load controllers.

# How to use it?

        private class LTabBarController : LazyTabBarController.LazyTabBarController
        {
            public LTabBarController():base(5)
            {
            }
            public override UIViewController InitTabControllers(int index)
            {
                switch (index)
                {
                    case 0:
                        return new AController();
                    case 1:
                        return new BController();
                    case 2:
                        return new CController();
                    case 3:
                        return new DController();
                    case 4:
                        return new EController();
                    default:
                        return new AController();
                }
            }

            public override (string name, string icon) InitTabBarNameAndIcon(int index)
            {
                switch (index)
                {
                    case 0:
                        return ("Challenges"., "settings_challenges");
                    case 1:
                        return ("Rewards", "settings_rewards");
                    case 2:
                        return (AppDelegate.Tenant.CoachName, "setting_add");
                    case 3:
                        return ("Activity", "settings_activity");
                    case 4:
                        return ("More", "threelines");
                    default:
                        return ("More", "threelines");
                }
            }

            public override void InitSelectedTab()
            {
                CurrentIndex = 2;
            }

            public override UIColor SelectedTabBarTintColor => CommonHelper.TabbarSelectedColor;
            public override UIColor UnSelectedTabBarTintColor => CommonHelper.RewardsText;

        }
