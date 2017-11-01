using System;

using UIKit;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using ImageIO;
using System.Linq;
using VideoToolbox;
using CoreGraphics;
using XibFree;
using System.Drawing;
using Foundation;

namespace PageViewController.ViewControllers
{
    public class ViewController : UIViewController
    {
        private DPageViewController pageViewController;
        private int _tabBarHeight = 70;

        private CustomTabBarItem _challengesTabBar;
        private CustomTabBarItem _rewardsTabBar;
        private CustomTabBarItem _axtivityTabBar;
        private CustomTabBarItem _moreTabBar;

        UIViewController[] viewControllers = new UIViewController[] {
            new UINavigationController(new A(){ pageIndex=0}),
            new UINavigationController(new B(){ pageIndex=1}),
            new UINavigationController(new C(){ pageIndex=2}),
            new UINavigationController(new D(){ pageIndex=3}) };

        public ViewController()
        {
            if (IsIphoneX()) _tabBarHeight += 34;
        }

        private bool IsIphoneX()
        {
            return UIScreen.MainScreen.Bounds.Height == 812;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            foreach(var viewcontroller in viewControllers)
            {
                viewcontroller.View.Frame=new CGRect(0, 0, this.View.Frame.Width, this.View.Frame.Size.Height+40- _tabBarHeight);
            }

            //pageViewController = new UIPageViewController(UIPageViewControllerTransitionStyle.Scroll, UIPageViewControllerNavigationOrientation.Horizontal)
            //{
            //    DataSource = new PageViewControllerDataSource(this)
            //};
            pageViewController = new DPageViewController(viewControllers.ToList());

            //pageViewController.SetViewControllers(new UIViewController[] {viewControllers[0] }, UIPageViewControllerNavigationDirection.Forward, false, null);
            pageViewController.View.Frame = new CGRect(0, 0, this.View.Frame.Width, this.View.Frame.Size.Height+40- _tabBarHeight);
          
            
            AddChildViewController(this.pageViewController);
            View.AddSubview(this.pageViewController.View);
            pageViewController.DidMoveToParentViewController(this);
            //pageViewController.DidFinishAnimating += PageViewController_DidFinishAnimating;
            //pageViewController.WillTransition += PageViewController_WillTransition;

            foreach(var view in pageViewController.View.Subviews)
            {
                if(view is UIScrollView scrollview)
                {
                    scrollview.Delegate = new Test(this);
                }
            }


            var layout = new LinearLayout(Orientation.Horizontal)
            {
                Layer = new CoreAnimation.CALayer
                {
                    BorderColor = UIColor.Gray.CGColor,
                    BorderWidth = 0.5f
                },
                LayoutParameters = new LayoutParameters(AutoSize.FillParent, AutoSize.FillParent),
                Padding = new UIEdgeInsets(0, 0, IsIphoneX() ? 34 : 0, 0),
                SubViews = new View[]
                {
                    _challengesTabBar= new CustomTabBarItem("settings_challenges","Challenges",()=> {
                        if(GetIndex()==0)
                            return;
                        DockIndex = 0;
                        //pageViewController.SetViewControllers(new UIViewController[] {viewControllers[0] },GetIndex()>0?UIPageViewControllerNavigationDirection.Reverse:UIPageViewControllerNavigationDirection.Forward, false, null);
                    }),
                    _rewardsTabBar= new CustomTabBarItem("settings_rewards","Rewards",()=> {
                        if(GetIndex()==1)
                            return;
                        DockIndex = 1;
                        //pageViewController.SetViewControllers(new UIViewController[] {viewControllers[1] },GetIndex()>1?UIPageViewControllerNavigationDirection.Reverse:UIPageViewControllerNavigationDirection.Forward, false, null);
                    }),
                    _axtivityTabBar= new CustomTabBarItem("settings_activity","Activity",()=> {
                        if(GetIndex()==2)
                            return;
                        DockIndex = 2;
                        //pageViewController.SetViewControllers(new UIViewController[] {viewControllers[2] },GetIndex()>2?UIPageViewControllerNavigationDirection.Reverse:UIPageViewControllerNavigationDirection.Forward, false, null);
                    }),
                    _moreTabBar= new CustomTabBarItem("threelines","More",()=> {
                        if(GetIndex()==3)
                            return;
                        DockIndex = 3;

                        //pageViewController.SetViewControllers(new UIViewController[] {viewControllers[3] }, GetIndex()>3?UIPageViewControllerNavigationDirection.Reverse:UIPageViewControllerNavigationDirection.Forward, false, null);
                    }),
                }
            };

            var customTabbar = new UILayoutHost(layout, new CGRect(
                0, UIScreen.MainScreen.Bounds.Height - _tabBarHeight, UIScreen.MainScreen.Bounds.Width, _tabBarHeight))
            { BackgroundColor = UIColor.White };
            this.View.Add(customTabbar);
            View.BackgroundColor = UIColor.White;

            DockIndex = 0;
        }

        int nextVCIndex = 0;
        private void PageViewController_WillTransition(object sender, UIPageViewControllerTransitionEventArgs e) 
            => nextVCIndex = ((BaseView)((UINavigationController)e.PendingViewControllers[0]).ViewControllers[0]).pageIndex;

        private void PageViewController_DidFinishAnimating(object sender, UIPageViewFinishedAnimationEventArgs e)
        {
            if (!e.Completed)
                return;
            if(e.Finished)
            DockIndex = nextVCIndex;
        }

        private int _dockIndex;
        public int DockIndex
        {
            get => _dockIndex;
            set
            {
                _dockIndex = value;
                if (_challengesTabBar == null)
                    return;
                switch (value)
                {
                    case 0:
                        _challengesTabBar.Selected = true;
                        _rewardsTabBar.Selected = false;
                        _axtivityTabBar.Selected = false;
                        _moreTabBar.Selected = false;
                        break;
                    case 1:
                        _challengesTabBar.Selected = false;
                        _rewardsTabBar.Selected = true;
                        _axtivityTabBar.Selected = false;
                        _moreTabBar.Selected = false;
                        break;
                    case 2:
                        _challengesTabBar.Selected = false;
                        _rewardsTabBar.Selected = false;
                        _axtivityTabBar.Selected = true;
                        _moreTabBar.Selected = false;
                        break;
                    case 3:
                        _challengesTabBar.Selected = false;
                        _rewardsTabBar.Selected = false;
                        _axtivityTabBar.Selected = false;
                        _moreTabBar.Selected = true;
                        break;
                    default:
                        break;
                }
                pageViewController.CurrentPage = value;
            }
        }

        private int GetIndex()
        {
            return pageViewController.CurrentPage;
            //nint s = pageViewController.DataSource.GetPresentationIndex(pageViewController);
            //return ((pageViewController.ViewControllers.First() as UINavigationController).ViewControllers[0] as BaseView).pageIndex;
        }

        public UIViewController ViewControllerAtIndex(int index)
        {
            var vc = (viewControllers.ElementAt(index) as UINavigationController).ViewControllers[0] as BaseView;
            vc.pageIndex = index;
            return viewControllers.ElementAt(index);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            this.NavigationController.NavigationBarHidden = true;
        }

        private class PageViewControllerDataSource : UIPageViewControllerDataSource
        {
            private ViewController _parentViewController;
            
            public PageViewControllerDataSource(UIViewController parentViewController)
            {
                _parentViewController = parentViewController as ViewController;
            }
         
            public override UIViewController GetPreviousViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
            {
                var vc = (referenceViewController as UINavigationController).ViewControllers[0] as BaseView;
                var index = vc.pageIndex;
                if (index == 0)
                {
                    return null;
                }
                else
                {
                    index--;
                    return _parentViewController.ViewControllerAtIndex(index);
                }
            }

            public override UIViewController GetNextViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
            {
                var vc = (referenceViewController as UINavigationController).ViewControllers[0] as BaseView;
                var index = vc.pageIndex;

                index++;
                if (index == 4)
                {
                    return null;
                }
                else
                {
                    return _parentViewController.ViewControllerAtIndex(index);
                }
            }

            public override nint GetPresentationCount(UIPageViewController pageViewController)
            {
                return 4;
            }

            public override nint GetPresentationIndex(UIPageViewController pageViewController)
            {
                return 1;
            }
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

        class Test : UIScrollViewDelegate
        {
            private ViewController _viewController;
            public Test(ViewController viewcontroller)
            {
                _viewController = viewcontroller;
            }

            public override void Scrolled(UIScrollView scrollView)
            {
                System.Diagnostics.Debug.WriteLine(scrollView.ContentOffset.X);
                //scrollView.Bounces = false;
                if(_viewController.DockIndex==0)
                {
                    if (scrollView.ContentOffset.X < scrollView.Bounds.Width)
                        scrollView.ContentOffset = new CGPoint(scrollView.Bounds.Width, scrollView.ContentOffset.Y);
                }
                else if (_viewController.DockIndex == 3)
				{
					if (scrollView.ContentOffset.X > scrollView.Bounds.Width)
						scrollView.ContentOffset = new CGPoint(scrollView.Bounds.Width, scrollView.ContentOffset.Y);
				}
            }

            //there is a bug, prefer to this
			//https://stackoverflow.com/questions/21798218/disable-uipageviewcontroller-bounce
			public override void WillEndDragging(UIScrollView scrollView, CGPoint velocity, ref CGPoint targetContentOffset)
            {
				if (_viewController.DockIndex == 0)
				{
					if (scrollView.ContentOffset.X < scrollView.Bounds.Width)
						scrollView.ContentOffset = new CGPoint(scrollView.Bounds.Width, scrollView.ContentOffset.Y);
				}
				else if (_viewController.DockIndex == 3)
				{
					if (scrollView.ContentOffset.X > scrollView.Bounds.Width)
						scrollView.ContentOffset = new CGPoint(scrollView.Bounds.Width, scrollView.ContentOffset.Y);
				}
            }
        }
    }
}

