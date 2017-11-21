using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UIKit;
using CoreGraphics;
using XibFree;
using MvvmCross.Platform.iOS.Platform;

namespace LazyTabBarController
{
    public abstract class LazyTabBarController : UIViewController
    {
        List<CustomTabBarItem> TabBarList;

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
        private nfloat height => UIScreen.MainScreen.Bounds.Height - TabBarHeight;

        private bool IsIphoneX() => UIScreen.MainScreen.Bounds.Height == 812;

        public abstract UIViewController InitTabControllers(int index);

        public abstract (string name, string icon) InitTabBarNameAndIcon(int index);

        UIScrollView containerScrollView;
        public List<UIViewController> ViewControllers;

        private int? _otherIndex;
        public int? OtherIndex
        {
            get => _otherIndex;
            set => _otherIndex = value;
        }

        public bool DisabledScorlledDelegate { get; set; }

        int _currentIndex = -1;
        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                if (_currentIndex == value)
                    return;

                if (value >= ViewControllers.Count)
                {
                    value = ViewControllers.Count - 1;
                }

                var currentPage = ViewControllers[value];
                if (currentPage == null)
                {
                    currentPage = new UINavigationController(InitTabControllers(value));
                    currentPage.View.Frame = new CGRect((nfloat)value * width, 0, width, height);
                    ViewControllers[value] = currentPage;
                }
                AddChildViewController(currentPage);
                containerScrollView.AddSubview(currentPage.View);
                currentPage.DidMoveToParentViewController(this);

                if (DisabledScorlledDelegate)
                {
                    containerScrollView.ContentOffset = new CGPoint((nfloat)value * width, 0);
                    DisabledScorlledDelegate = false;

                    if (_currentIndex >= 0)
                    {
                        var otherPage = ViewControllers[_currentIndex];
                        otherPage?.WillMoveToParentViewController(null);
                        otherPage?.RemoveFromParentViewController();
                        otherPage?.View.RemoveFromSuperview();
                    }
                }
                for (var i = 0; i < 4; i++)
                {
                    if (i == value)
                        TabBarList[i].Selected = true;
                    else
                        TabBarList[i].Selected = false;
                }

                _currentIndex = value;
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
            }
        }

        public LazyTabBarController()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ViewControllers = new List<UIViewController> { null, null, null, null };
            TabBarList = new List<CustomTabBarItem>();

            DisabledScorlledDelegate = true;

            var tabbarLayout = new LinearLayout(Orientation.Horizontal)
            {
                Layer = new CoreAnimation.CALayer
                {
                    BorderColor = UIColor.Gray.CGColor,
                    BorderWidth = 0.5f
                },
                LayoutParameters = new LayoutParameters(AutoSize.FillParent, TabBarHeight),
                Padding = new UIEdgeInsets(0, 0, IsIphoneX() ? 34 : 0, 0)
            };

            for (var i = 0; i < 4; i++)
            {
                var nameAndIcon = InitTabBarNameAndIcon(i);
                var tabBar = GetTabBarItem(nameAndIcon.icon, nameAndIcon.name, (index) =>
                 {
                     DisabledScorlledDelegate = true;
                     CurrentIndex = index;
                 }, i);
                TabBarList.Add(tabBar);
                tabbarLayout.AddSubView(tabBar);
            }


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
                            containerScrollView.Delegate = new ScrollViewDelegate(this);
                            if(!new MvxIosMajorVersionChecker(11).IsVersionOrHigher)
                            {
                                 containerScrollView.ContentInsetAdjustmentBehavior= UIScrollViewContentInsetAdjustmentBehavior.Never;
                            }
                            else
                            {
                                 this.AutomaticallyAdjustsScrollViewInsets=false;
                            }
                        }
                    },
                    tabbarLayout
                }
            };

            this.View = new UILayoutHost(layout) { BackgroundColor = UIColor.White };

            LayoutPages();

            InitSelectedTab();
        }

        public virtual void InitSelectedTab()
        {
            CurrentIndex = 0;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            this.NavigationController.NavigationBarHidden = true;
            this.NavigationController.NavigationBar.TintColor = UIColor.White.ColorWithAlpha(0);
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
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

        public virtual UIColor SelectedTabBarTintColor => UIColor.Green;
        public virtual UIColor UnSelectedTabBarTintColor => UIColor.Gray;

        private CustomTabBarItem GetTabBarItem(string imageName, string title, Action<int> action, int index)
        {
            var customTabBarItem = new CustomTabBarItem(imageName, title, action, index)
            {
                SelectedColor = SelectedTabBarTintColor,
                UnSelectedColor = UnSelectedTabBarTintColor
            };
            return customTabBarItem;
        }

        private class CustomTabBarItem : NativeView
        {
            private UIImageView _bottomImage;
            private UIImageView _image;
            private UILabel _titleLabel;
            private string _imageName;
            private int _index;
            private UIColor _selectedColor = UIColor.Red;
            public UIColor SelectedColor
            {
                get => _selectedColor;
                set
                {
                    _selectedColor = value;
                    SetColor();
                }
            }
            private UIColor _unSelectedColor = UIColor.Gray;
            public UIColor UnSelectedColor
            {
                get => _unSelectedColor;
                set
                {
                    _unSelectedColor = value;
                    SetColor();
                }
            }
            public CustomTabBarItem(string imageName, string title, Action<int> action, int index)
            {
                _imageName = imageName;
                _index = index;
                LayoutParameters = new LayoutParameters(AutoSize.FillParent, 70);
                View = new UILayoutHost(new LinearLayout(Orientation.Vertical)
                {
                    LayoutParameters = new LayoutParameters(AutoSize.FillParent, AutoSize.FillParent),
                    SubViews = new View[]
                    {
                        new NativeView
                        {
                            View=new UIView(),//{BackgroundColor=UIColor.Red},
                            LayoutParameters=new LayoutParameters(AutoSize.FillParent,AutoSize.FillParent),
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
                                    LayoutParameters=new LayoutParameters(AutoSize.WrapContent,AutoSize.WrapContent)
                                    {
                                        MaxWidth=25,
                                        MaxHeight=25
                                    },
                                    //LayoutParameters=new LayoutParameters(25,25)
                                    //{
                                    //},
                                    Init = view =>
                                    {
                                        var imageView=view.As<UIImageView>();
                                        imageView.Image = UIImage.FromBundle(imageName);
                                    },
                                    Gone=string.IsNullOrEmpty(imageName)
                                },
                            }
                        },
                        new NativeView
                        {
                            View=new UIView(),//{BackgroundColor=UIColor.Gray},
                            LayoutParameters=new LayoutParameters(AutoSize.FillParent,5),
                            Gone=string.IsNullOrEmpty(title)||string.IsNullOrEmpty(imageName)
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
                            View=new UIView(),//{BackgroundColor=UIColor.Green},
                            LayoutParameters=new LayoutParameters(AutoSize.FillParent,AutoSize.FillParent)
                            {
                                MinHeight=7
                            }
                        },
                        new NativeView
                        {
                            View=_bottomImage=new UIImageView {Image=UIImage.FromBundle("trigon") },
                            LayoutParameters=new LayoutParameters(AutoSize.WrapContent,AutoSize.WrapContent)
                            {
                                Gravity=Gravity.CenterHorizontal,
                                MarginBottom=-7f,
                                MarginTop=-7f
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
                        action?.Invoke(_index);
                    });
                };
            }

            private void SetColor()
            {
                if (Selected)
                {
                    if (!string.IsNullOrEmpty(_imageName))
                        _image.Image = CreateColoredImage(SelectedColor, UIImage.FromBundle(_imageName));
                    _bottomImage.Image = CreateColoredImage(SelectedColor, UIImage.FromBundle("trigon"));
                    _titleLabel.TextColor = SelectedColor;
                }
                else
                {
                    if (!string.IsNullOrEmpty(_imageName))
                        _image.Image = CreateColoredImage(UnSelectedColor, UIImage.FromBundle(_imageName));
                    _bottomImage.Image = CreateColoredImage(UnSelectedColor, UIImage.FromBundle("trigon"));
                    _titleLabel.TextColor = UnSelectedColor;
                }
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
                    }
                    else
                    {
                        _bottomImage.GetNativeView().Gone = true;
                    }
                    SetColor();
                    _bottomImage.GetLayoutHost().SetNeedsLayout();
                }
            }

            private UIImage CreateColoredImage(UIColor color, UIImage mask)
            {
                var rect = new CGRect(CGPoint.Empty, mask.Size);
                UIGraphics.BeginImageContextWithOptions(mask.Size, false, mask.CurrentScale);
                CGContext context = UIGraphics.GetCurrentContext();
                mask.DrawAsPatternInRect(rect);
                context.SetFillColor(color.CGColor);
                context.SetBlendMode(CGBlendMode.SourceAtop);
                context.FillRect(rect);
                UIImage result = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();
                return result;
            }
        }

        private class ScrollViewDelegate : UIScrollViewDelegate
        {
            private LazyTabBarController _viewController;
            private nfloat pageWidth;
            public ScrollViewDelegate(LazyTabBarController viewcontroller)
            {
                _viewController = viewcontroller;
                pageWidth = viewcontroller.width;
            }

            nfloat offset = 0;
            public override void Scrolled(UIScrollView scrollView)
            {
                if (_viewController.DisabledScorlledDelegate)
                    return;
                var moveToLeft = offset - scrollView.ContentOffset.X < 0;
                offset = scrollView.ContentOffset.X;
                double t = offset / pageWidth;
                int page = (int)Math.Round(t);

                if (_viewController.CurrentIndex != (int)page)
                {
                    if (page >= 0 && (int)page < _viewController.ViewControllers.Count)
                    {
                        _viewController.CurrentIndex = page;
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
        }
    }
}
