using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;
using CoreGraphics;
using XibFree;
using Foundation;
using SDWebImage;
using System.Drawing;

namespace LazyTabBarController
{
    [Register("LazyTabBarController")]
    public abstract class LazyTabBarController : UIViewController
    {
        private List<CustomTabBarItem> TabBarList;
        private UIScrollView containerScrollView;
        public List<UIViewController> ViewControllers;
        public event EventHandler<int> PageIndexChanged;

        private int TabBarHeight => IsIphoneX() ? 104 : 70;

        private nfloat Width => UIScreen.MainScreen.Bounds.Width;
        private nfloat Height => UIScreen.MainScreen.Bounds.Height - TabBarHeight;
        /// <summary>
        /// the device is iphonex, iphonexr, iphonexs or iphonexs max
        /// </summary>
        /// <returns></returns>
        private bool IsIphoneX() => UIScreen.MainScreen.Bounds.Height >= 812;

        public abstract UIViewController InitTabControllers(int index);

        public abstract (string name, string icon) InitTabBarNameAndIcon(int index);

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
                {
                    DisabledScorlledDelegate = false;
                    return;
                }

                if (value >= ViewControllers.Count)
                {
                    value = ViewControllers.Count - 1;
                }

                var currentPage = ViewControllers[value];
                if (currentPage == null)
                {
                    currentPage = new UINavigationController(InitTabControllers(value));
                    currentPage.View.Frame = new CGRect((nfloat)value * Width, 0, Width, Height);
                    ViewControllers[value] = currentPage;
                }
                AddChildViewController(currentPage);
                containerScrollView.AddSubview(currentPage.View);
                currentPage.DidMoveToParentViewController(this);

                if (DisabledScorlledDelegate)
                {
                    containerScrollView.ContentOffset = new CGPoint((nfloat)value * Width, 0);
                    DisabledScorlledDelegate = false;

                    if (_currentIndex >= 0)
                    {
                        var otherPage = ViewControllers[_currentIndex];
                        otherPage?.WillMoveToParentViewController(null);
                        otherPage?.RemoveFromParentViewController();
                        otherPage?.View.RemoveFromSuperview();
                    }
                }
                for (var i = 0; i < _count; i++)
                {
                    if (i == value)
                        TabBarList[i].Selected = true;
                    else
                        TabBarList[i].Selected = false;
                }

                _currentIndex = value;
                PageIndexChanged?.Invoke(this, value);
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

        private int _count;

        public LazyTabBarController(int count)
        {
            _count = count;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ViewControllers = new List<UIViewController>();
            for (var i = 0; i < _count; i++)
            {
                ViewControllers.Add(null);
            }
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

            for (var i = 0; i < _count; i++)
            {
                var (name, icon) = InitTabBarNameAndIcon(i);
                var tabBar = GetTabBarItem(icon, name, (index) =>
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
                            if (UIDevice.CurrentDevice.CheckSystemVersion(11,0))
                            {
                                containerScrollView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
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

        public virtual void InitSelectedTab() => CurrentIndex = 0;

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            this.NavigationController.SetNavigationBarHidden(true, true);
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
                var nextFrame = new CGRect((nfloat)i * Width, 0, Width, Height);
                page.View.Frame = nextFrame;
            }
            containerScrollView.Frame = new CGRect(0, 0, Width, Height);
            containerScrollView.ContentSize = new CGSize(Width * (nfloat)ViewControllers.Count, Height);
        }

        public virtual UIColor SelectedTabBarTintColor(int index) => UIColor.Green;
        public virtual UIColor UnSelectedTabBarTintColor(int index) => UIColor.Gray;
        public virtual UIFont TabBarFont(int index) => new UILabel().Font;
        public virtual int DisHcolorIndex => -1;
        public virtual bool ShowTriangle => true;
        public virtual UIColor ArrowFixedColor => null;
        public virtual bool IsArrowFixedColor => true;
        public Action<UIImageView> SetBottomImageViewAction { private get; set; }


        private CustomTabBarItem GetTabBarItem(string imageName, string title, Action<int> action, int index)
        {
            var customTabBarItem = new CustomTabBarItem(new TabBarParamsModel {
                ImageName = imageName,
                Title = title,
                Action = action,
                Index = index,
                TitleFont = TabBarFont(index),
                TabBarImageView = SetImageForTabBar(index),
                IsArrowFixedColor = this.IsArrowFixedColor,
                ArrowFixedColor = this.ArrowFixedColor
            })
            {
                SelectedColor = SelectedTabBarTintColor(index),
                UnSelectedColor = UnSelectedTabBarTintColor(index),
                DisHColor = index == DisHcolorIndex,
                ShowTriangle = ShowTriangle
            };
            customTabBarItem.SetBottom(SetBottomImageViewAction);
            return customTabBarItem;
        }
        /// <summary>
        /// custom imageview For Tabbar
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual UIImageView SetImageForTabBar(int index) => null;

        public Action<int> ChangeDotCommand
            => new Action<int>((t) =>
            {
                var tabBarItem = TabBarList.ElementAtOrDefault(2);
                if (tabBarItem == null)
                    return;
                tabBarItem.SetDot(t);
            });

        public Action<int> ChangeCurrentIndexCommand
            => new Action<int>((t) =>
            {
                containerScrollView.SetContentOffset(new CGPoint(Width * t, 0), true);
            });

        private class CustomTabBarItem : NativeView
        {
            private UIImageView _bottomImage;
            private UIImageView _image;
            private UILabel _titleLabel;
            private UILabel _dotLabel;
            private string _imageName;
            private int _index;
            public bool DisHColor { get; set; }
            public bool ShowTriangle { get; set; }
            public bool IsArrowFixedColor { get; set; }
            public CustomTabBarItem(TabBarParamsModel model)
            {
                _imageName = model.ImageName;
                _index = model.Index;
                IsArrowFixedColor = model.IsArrowFixedColor;
                LayoutParameters = new LayoutParameters(AutoSize.FillParent, 70);
                View = new UILayoutHost(new LinearLayout(Orientation.Vertical)
                {
                    LayoutParameters = new LayoutParameters(AutoSize.FillParent, AutoSize.FillParent),
                    SubViews = new View[]
                    {
                        new NativeView
                        {
                            View=new UIView(),
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
                                    View=_image=model.TabBarImageView??new UIImageView(),
                                    LayoutParameters=new LayoutParameters(AutoSize.WrapContent,AutoSize.WrapContent)
                                    {
                                        MaxWidth=35,
                                        MaxHeight=35
                                    },
                                    Init = view =>
                                    {
                                        var imageView=view.As<UIImageView>();
                                        imageView.Image = UIImage.FromBundle(model.ImageName);
                                    },
                                    Gone=string.IsNullOrEmpty(model.ImageName)
                                },
                                new NativeView
                                {
                                    View=_dotLabel=new UILabel
                                    {
                                        TextColor=UIColor.White,
                                        BackgroundColor=SelectedColor,
                                        Font= UIFont.FromName(model.TitleFont.Name,12f),
                                    },
                                    LayoutParameters=new LayoutParameters
                                    {
                                        Width=0,
                                        Height=AutoSize.WrapContent,
                                        Gravity= Gravity.TopRight,
                                    },
                                    Init=view=>
                                    {
                                        var label=view.As<UILabel>();
                                        label.Layer.CornerRadius=6f;
                                        label.ClipsToBounds=true;
                                    }
                                }
                            }
                        },
                        new NativeView
                        {
                            View=new UIView(),
                            LayoutParameters=new LayoutParameters(AutoSize.FillParent,5),
                            Gone=string.IsNullOrEmpty(model.Title)||string.IsNullOrEmpty(model.ImageName)
                        },
                        new NativeView
                        {
                            View=_titleLabel=new UILabel()
                            {
                                Text=model.Title,
                                TextAlignment= UITextAlignment.Center,
                                TextColor=UIColor.Gray,
                                Font=model.TitleFont
                            },
                            LayoutParameters=new LayoutParameters(AutoSize.FillParent,AutoSize.WrapContent)
                            {
                            },
                            Gone=string.IsNullOrEmpty(model.Title)
                        },
                        new NativeView
                        {
                            View=new UIView(),
                            LayoutParameters=new LayoutParameters(AutoSize.FillParent,7)
                            {
                                //MinHeight=7
                            }
                        },
                        new NativeView
                        {
                            View=_bottomImage=new UIImageView(),
                            LayoutParameters=new LayoutParameters(AutoSize.WrapContent,AutoSize.WrapContent)
                            {
                                Gravity=Gravity.CenterHorizontal,
                                MarginBottom=-7f,
                                MarginTop=-7f
                            },
                            Init = view =>
                            {
                                var imageView=view.As<UIImageView>();
                                if(model.IsArrowFixedColor)
                                    imageView.Image= CreateColoredImage(model.ArrowFixedColor, UIImage.FromBundle("trigon"));
                                else
                                    imageView.Image=UIImage.FromBundle("trigon");
                            },
                            Gone=true
                        },
                    }
                });
                Init = view =>
                {
                    var uiView = view.As<UILayoutHost>();
                    var gesture = new UITapGestureRecognizer();
                    uiView.AccessibilityIdentifier = model.ImageName;
                    gesture.NumberOfTapsRequired = 1;
                    uiView.AddGestureRecognizer(gesture);
                    gesture.AddTarget(() =>
                    {
                        model.Action?.Invoke(_index);
                    });
                };

                SetDot(0);
            }

            public void SetBottom(Action<UIImageView> action)
            {
                if (_bottomImage == null)
                    return;
                action?.Invoke(_bottomImage);
            }

            public void SetDot(int dotCount)
            {
                if (_dotLabel == null)
                    return;
                if (dotCount < 0)
                    return;
                if (dotCount == 0)
                {
                    _dotLabel.GetNativeView().Gone = true;
                    _dotLabel.GetLayoutHost().SetNeedsLayout();
                    return;
                }

                var size = GetSize(dotCount);
                _image.GetNativeView().LayoutParameters.MarginLeft = size.Width / 2 - 5;
                _image.GetNativeView().LayoutParameters.MarginRight = size.Width / 2 - 5;
                _dotLabel.Text = GetString(dotCount);
                _dotLabel.Layer.CornerRadius = size.Height / 2;
                _dotLabel.GetNativeView().LayoutParameters.Width = size.Width;
                _dotLabel.GetNativeView().LayoutParameters.MarginLeft = -size.Width;
                _dotLabel.GetNativeView().Gone = false;
                _dotLabel.GetLayoutHost().SetNeedsLayout();
            }

            private CGSize GetSize(int dotCount)
            {
                if (dotCount <= 0)
                    return Size.Empty;
                return GetSizeFromString(GetString(dotCount), _dotLabel.Font);
            }

            private CGSize GetSizeFromString(string text, UIFont font)
            {
                if (string.IsNullOrEmpty(text))
                    return CGSize.Empty;
                NSString nsString = new NSString(text);
                UIStringAttributes attribs = new UIStringAttributes { Font = font };
                CGSize size = nsString.GetSizeUsingAttributes(attribs);
                return size;
            }

            private string GetString(int dotCount) => string.Format("  {0}  ", dotCount < 99 ? dotCount.ToString() : "99+");

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

            private void SetColor()
            {
                if (Selected)
                {
                    if (!string.IsNullOrEmpty(_imageName) && _imageName.StartsWith("http"))
                    {
                        _image.SetImage(NSUrl.FromString(_imageName));
                        _image.GetNativeView().LayoutParameters.Width = 35;
                        _image.GetNativeView().LayoutParameters.Height = 35;
                        _image.GetLayoutHost().SetNeedsLayout();
                    }
                    else if (!string.IsNullOrEmpty(_imageName))
                    {
                        if (DisHColor)
                            _image.Image = UIImage.FromBundle(_imageName);
                        else
                            _image.Image = CreateColoredImage(SelectedColor, UIImage.FromBundle(_imageName));
                    }
                    if(!IsArrowFixedColor)
                        _bottomImage.Image = CreateColoredImage(SelectedColor, UIImage.FromBundle("trigon"));
                    _titleLabel.TextColor = SelectedColor;
                }
                else
                {
                    if (!string.IsNullOrEmpty(_imageName) && _imageName.StartsWith("http"))
                    {
                        _image.SetImage(NSUrl.FromString(_imageName));
                        _image.GetNativeView().LayoutParameters.Width = 35;
                        _image.GetNativeView().LayoutParameters.Height = 35;
                        _image.GetLayoutHost().SetNeedsLayout();
                    }
                    else if (!string.IsNullOrEmpty(_imageName))
                    {
                        if (DisHColor)
                            _image.Image = UIImage.FromBundle(_imageName);
                        else
                            _image.Image = CreateColoredImage(UnSelectedColor, UIImage.FromBundle(_imageName));
                    }
                    if (!IsArrowFixedColor)
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
                    _bottomImage.GetNativeView().Gone = !ShowTriangle || !value;
                    SetColor();
                    _bottomImage.GetLayoutHost().SetNeedsLayout();
                }
            }

            private UIImage CreateColoredImage(UIColor color, UIImage mask)
            {
                if (mask == null)
                    throw new Exception($"LazyTabBar exception: cannot find the image named:{_imageName}");
                if (color == null)
                    return mask;
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

        private class TabBarParamsModel
        {
            public string ImageName { get; set; }
            public string Title { get; set; }
            public int Index { get; set; }
            public UIFont TitleFont { get; set; }
            public bool IsArrowFixedColor { get; set; }
            public UIColor ArrowFixedColor { get; set; }
            public Action<int> Action { get; set; }
            public UIImageView TabBarImageView { get; set; }
        }

        private class ScrollViewDelegate : UIScrollViewDelegate
        {
            private LazyTabBarController _viewController;
            private nfloat pageWidth;
            public ScrollViewDelegate(LazyTabBarController viewcontroller)
            {
                _viewController = viewcontroller;
                pageWidth = viewcontroller.Width;
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
