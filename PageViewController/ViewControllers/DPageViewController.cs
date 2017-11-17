using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using CoreGraphics;
using XibFree;

namespace PageViewController.ViewControllers
{
    public class DPageViewController : UIViewController
    {
        private CustomTabBarItem _challengesTabBar;
        private CustomTabBarItem _rewardsTabBar;
        private CustomTabBarItem _axtivityTabBar;
        private CustomTabBarItem _moreTabBar;

        private int TabBarHeight
        {
            get
            {
                if (IsIphoneX())
                    return 104;
                return 70;
            }
        }

        nfloat width = UIScreen.MainScreen.Bounds.Width;
        private nfloat height
        {
            get
            {
                if (IsIphoneX())
                    return UIScreen.MainScreen.Bounds.Height - TabBarHeight - 40;
                return UIScreen.MainScreen.Bounds.Height - TabBarHeight - 20;
            }
        }

        private bool IsIphoneX()
        {
            return UIScreen.MainScreen.Bounds.Height == 812;
        }

        public virtual UIViewController InitTabControllers(int index)
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

        UIScrollView containerScrollView;
        public List<UIViewController> ViewControllers;

        private int? _otherIndex;
        public int? OtherIndex
        {
            get => _otherIndex;
            set => _otherIndex = value;
        }

        int _currentIndex = -1;
        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                if (_currentIndex == value)
                    return;
                _currentIndex = value;
                //System.Diagnostics.Debug.WriteLine($"currentPage:{value}");
                if (_currentIndex >= ViewControllers.Count)
                {
                    _currentIndex = ViewControllers.Count - 1;
                }

                var currentPage = ViewControllers[_currentIndex];
                if (currentPage == null)
                {
                    currentPage = new UINavigationController(InitTabControllers(_currentIndex));
                    currentPage.View.Frame = new CGRect((nfloat)_currentIndex * width, 0, width, height);
                    ViewControllers[_currentIndex] = currentPage;
                }
                AddChildViewController(currentPage);
                containerScrollView.AddSubview(currentPage.View);
                currentPage.DidMoveToParentViewController(this);
            }
        }

        private bool? _movingToCurrentPage;
        public bool? MovingToCurrentPage
        {
            get => _movingToCurrentPage;
            set
            {
                if (_movingToCurrentPage == value)
                    return;
                _movingToCurrentPage = value;

                if (!value.HasValue)
                {
                    if (OtherIndex.HasValue)
                    {
                        var otherPage = ViewControllers[OtherIndex.Value];
                        otherPage?.WillMoveToParentViewController(null);
                        otherPage?.RemoveFromParentViewController();
                        otherPage?.View.RemoveFromSuperview();
                    }
                }
                else
                {
                    if (OtherIndex.HasValue)
                    {
                        var otherPage = ViewControllers[OtherIndex.Value];
                        if (otherPage != null && otherPage.ParentViewController == null)
                        {
                            AddChildViewController(otherPage);
                            containerScrollView.AddSubview(otherPage.View);
                            otherPage.DidMoveToParentViewController(this);
                        }
                    }

                    var currentPage = ViewControllers[CurrentIndex];
                    if (currentPage != null && currentPage.ParentViewController == null)
                    {
                        AddChildViewController(currentPage);
                        containerScrollView.AddSubview(currentPage.View);
                        currentPage.DidMoveToParentViewController(this);
                    }
                }

                //System.Diagnostics.Debug.WriteLine($"MovingToCurrent:{value},CurrentPage:{CurrentPage},OtherPage:{OtherPage}");
            }
        }

        public DPageViewController()
        {
            ViewControllers = new List<UIViewController>();
            //ViewControllers.Add(new UINavigationController(InitTabControllers(0)));
            //ViewControllers.Add(new UINavigationController(InitTabControllers(1)));
            //ViewControllers.Add(new UINavigationController(InitTabControllers(2)));
            //ViewControllers.Add(new UINavigationController(InitTabControllers(3)));
            ViewControllers.Add(null);
            ViewControllers.Add(null);
            ViewControllers.Add(null);
            ViewControllers.Add(null);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var tabbarLayout = new LinearLayout(Orientation.Horizontal)
            {
                Layer = new CoreAnimation.CALayer
                {
                    BorderColor = UIColor.Gray.CGColor,
                    BorderWidth = 0.5f
                },
                LayoutParameters = new LayoutParameters(AutoSize.FillParent, TabBarHeight),
                Padding = new UIEdgeInsets(0, 0, IsIphoneX() ? 34 : 0, 0),
                SubViews = new View[]
                {
                    _challengesTabBar= new CustomTabBarItem("settings_challenges","Challenges",()=> {
                    }),
                    _rewardsTabBar= new CustomTabBarItem("settings_rewards","Rewards",()=> {

                    }),
                    _axtivityTabBar= new CustomTabBarItem("settings_activity","Activity",()=> {

                    }),
                    _moreTabBar= new CustomTabBarItem("threelines","More",()=> {

                    }),
                }
            };


            var layout = new LinearLayout(Orientation.Vertical)
            {
                SubViews = new View[]
                {
                    new NativeView
                    {
                        View=containerScrollView=new UIScrollView(),
                        LayoutParameters=new LayoutParameters(AutoSize.FillParent,AutoSize.FillParent),
                        Init=view=>
                        {
                            containerScrollView.Bounces = false;
                            containerScrollView.PagingEnabled = true;
                            containerScrollView.AlwaysBounceVertical = false;
                            containerScrollView.AlwaysBounceHorizontal = false;
                            containerScrollView.ShowsHorizontalScrollIndicator = false;
                            containerScrollView.Delegate = new Test(this);
                        }
                    },
                    tabbarLayout
                }
            };

            this.View = new UILayoutHost(layout) { BackgroundColor = UIColor.White };


            LayoutPages();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            this.NavigationController.NavigationBarHidden = true;
        }

        void LayoutPages()
        {
            foreach (var pageView in containerScrollView.Subviews)
            {
                pageView.RemoveFromSuperview();
            }

            for (var i = 0; i < ViewControllers.Count; i++)
            {
                var page = ViewControllers[i];
                if (page == null)
                    break;
                var nextFrame = new CGRect((nfloat)i * width, 0, width, height); 
                page.View.Frame = nextFrame;
            }
            containerScrollView.Frame = new CGRect(0, 0, width, height); 
            containerScrollView.ContentSize = new CGSize(width * (nfloat)ViewControllers.Count, height);
        }

        class CustomTabBarItem : NativeView
        {
            private UIImageView _bottomImage;
            private UIImageView _image;
            private UILabel _titleLabel;
            private string _imageName;
            public CustomTabBarItem(string imageName, string title, Action action)
            {
                _imageName = imageName;
                View = new UILayoutHost(new LinearLayout(Orientation.Vertical)
                {
                    LayoutParameters = new LayoutParameters(AutoSize.FillParent, AutoSize.FillParent),
                    SubViews = new View[]
                    {
                    new NativeView
                    {
                        View=new UIView(),
                        LayoutParameters=new LayoutParameters(AutoSize.FillParent,5)
                    },
                    new NativeView
                    {
                        View=new UIView(),
                        LayoutParameters=new LayoutParameters(AutoSize.FillParent,AutoSize.FillParent,2)
                    },
                    new LinearLayout(Orientation.Horizontal)
                    {
                        LayoutParameters=new LayoutParameters(AutoSize.WrapContent,AutoSize.WrapContent)
                        {
                            Gravity=Gravity.CenterHorizontal
                        },
                        SubViews=new View[]
                        {
                            new NativeView
                            {
                                View=_image=new UIImageView(),
                                LayoutParameters=new LayoutParameters(40,40)
                                {
                                },
                                Init = view =>
                                {
                                    var imageView=view.As<UIImageView>();
                                    imageView.Image = UIImage.FromBundle(imageName);
                                }
                            },
                        }
                    },
                    new NativeView
                    {
                        View=new UIView(),
                        LayoutParameters=new LayoutParameters(AutoSize.FillParent,AutoSize.FillParent)
                    },
                    new NativeView
                    {
                        View=_titleLabel=new UILabel()
                        {
                            Text=title,
                            TextAlignment= UITextAlignment.Center,
                            TextColor=UIColor.Gray,
                        },
                        LayoutParameters=new LayoutParameters(AutoSize.FillParent,AutoSize.WrapContent)
                        {
                        },
                        Gone=string.IsNullOrEmpty(title)
                    },
                    new NativeView
                    {
                        View=new UIView(),
                        LayoutParameters=new LayoutParameters(AutoSize.FillParent,8)
                    },
                    new NativeView
                    {
                        View=_bottomImage=new UIImageView {Image=UIImage.FromBundle("trigon") },
                        LayoutParameters=new LayoutParameters(AutoSize.WrapContent,AutoSize.WrapContent)
                        {
                            Gravity=Gravity.BottomCenter,
                            MarginTop=-7,
                            MarginBottom=-7f
                        },
                        Gone=true
                    },
                    }
                });
                Init = view =>
                {
                    var uiView = view.As<UILayoutHost>();
                    var gesture = new UITapGestureRecognizer();
                    uiView.AccessibilityIdentifier = imageName;
                    gesture.NumberOfTapsRequired = 1;
                    uiView.AddGestureRecognizer(gesture);
                    gesture.AddTarget(() =>
                    {
                        action?.Invoke();
                    });
                };
            }

            private bool _selected;
            public bool Selected
            {
                get => _selected;
                set
                {
                    _selected = value;
                    if (value)
                    {
                        _bottomImage.GetNativeView().Gone = false;
                        _image.Image = UIImage.FromBundle(_imageName);

                        _titleLabel.TextColor = UIColor.Red;
                    }
                    else
                    {
                        _bottomImage.GetNativeView().Gone = true;
                        _image.Image = UIImage.FromBundle(_imageName);
                        _titleLabel.TextColor = UIColor.Gray;
                    }
                    View.SetNeedsLayout();
                }
            }
        }

        private class Test : UIScrollViewDelegate
        {
            private DPageViewController _viewController;
            private nfloat pageWidth;
            public Test(DPageViewController viewcontroller)
            {
                _viewController = viewcontroller;
                pageWidth = viewcontroller.width;
            }

            nfloat offset = 0;
            public override void Scrolled(UIScrollView scrollView)
            {
                var moveToLeft = offset - scrollView.ContentOffset.X < 0;
                offset = scrollView.ContentOffset.X;
                double t = offset / pageWidth;
                int page = (int)Math.Round(t);

                if (_viewController.CurrentIndex != (int)page)
                {
                    if (page >= 0 && (int)page < _viewController.ViewControllers.Count)
                    {
                        _viewController.CurrentIndex = page;
                        //System.Diagnostics.Debug.WriteLine($"offset:{scrollView.ContentOffset.X} , scrolledPage:{page} ,moveToLeft:{moveToLeft}");
                    }
                }

                if (t - page > 0)
                    _viewController.OtherIndex = _viewController.CurrentIndex + 1;
                else if (t - page < 0)
                    _viewController.OtherIndex = _viewController.CurrentIndex - 1;


                var result = _viewController.CurrentIndex * pageWidth - offset;

                if (result == 0)
                    _viewController.MovingToCurrentPage = null;
                else
                    _viewController.MovingToCurrentPage = moveToLeft && (result > 0) || (!moveToLeft) && (result < 0);
            }

            public override void ScrollAnimationEnded(UIScrollView scrollView)
            {
            }
        }
    }
}