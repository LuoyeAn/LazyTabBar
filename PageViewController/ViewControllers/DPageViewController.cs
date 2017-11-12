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
        nfloat width = UIScreen.MainScreen.Bounds.Width;
        private nfloat height
        {
            get
            {
                if(IsIphoneX())
                    return UIScreen.MainScreen.Bounds.Height - 104-20;
                return UIScreen.MainScreen.Bounds.Height - 70-20;
                return View.Bounds.Height-70;
            }
        }

        private bool IsIphoneX()
        {
            return UIScreen.MainScreen.Bounds.Height == 812;
        }

        UIViewController[] viewControllers = new UIViewController[] {
            new UINavigationController(new A()),
            new UINavigationController(new B()),
            new UINavigationController(new C()),
            new UINavigationController(new D()) };

        UIScrollView containerScrollView;
        public List<UIViewController> ViewControllers;

        private int _otherPage;
        public int OtherPage
        {
            get => _otherPage;
            set
            {
                _otherPage = value;
            }
        }

        int _currentPage=-1;
        public int CurrentPage
        {
            get=> _currentPage; 
            set
            {
                if (_currentPage == value)
                    return;
                _currentPage = value;
                System.Diagnostics.Debug.WriteLine($"currentPage:{value}");
                if (_currentPage >= ViewControllers.Count)
                {
                    _currentPage = ViewControllers.Count - 1;
                }

                if (_currentPage < ViewControllers.Count && _currentPage > 0)
                {
                    var previousViewController = ViewControllers[OtherPage];
                    previousViewController.WillMoveToParentViewController(null);
                    previousViewController.RemoveFromParentViewController();
                    previousViewController.View.RemoveFromSuperview();
                }
                var currentViewController = ViewControllers[_currentPage];
                AddChildViewController(currentViewController);
                containerScrollView.AddSubview(currentViewController.View);
                currentViewController.DidMoveToParentViewController(this);
            }
        }

        private bool _movingToCurrentPage;
        public bool MovingToCurrentPage
        {
            get => _movingToCurrentPage;
            set
            {
                if (_movingToCurrentPage == value)
                    return;
                _movingToCurrentPage = value;

                System.Diagnostics.Debug.WriteLine($"MovingToCurrent:{value},CurrentPage:{CurrentPage},OtherPage:{OtherPage}");

                //if (value)
                //{
                //    if (CurrentPage >= ViewControllers.Count)
                //        return;
                //    var currentViewController = ViewControllers[CurrentPage + 1];
                //    AddChildViewController(currentViewController);
                //    containerScrollView.AddSubview(currentViewController.View);
                //    currentViewController.DidMoveToParentViewController(this);
                //}
                //else
                //{
                //    if (CurrentPage <= 0)
                //        return;
                //    var currentViewController = ViewControllers[CurrentPage - 1];
                //    AddChildViewController(currentViewController);
                //    containerScrollView.AddSubview(currentViewController.View);
                //    currentViewController.DidMoveToParentViewController(this);
                //}
            }
        }

        public DPageViewController()
        {
            ViewControllers = viewControllers.ToList();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var layout = new LinearLayout()
            {
                SubViews=new View[]
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
                            containerScrollView.AlwaysBounceHorizontal = false; // drag with only one view controller
                            containerScrollView.ShowsHorizontalScrollIndicator = false;
                            containerScrollView.Delegate = new Test(this);
                        }
                    }
                }
            };

            this.View = new UILayoutHost(layout);

            LayoutPages();
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
                var nextFrame = new CGRect((nfloat)i * width, 0, width, height); // Origin.Y
                System.Diagnostics.Debug.WriteLine(nextFrame);
                page.View.Frame = nextFrame;
            }
            containerScrollView.Frame = new CGRect(0, 0, width, height); //frame: self.view.bounds
            containerScrollView.ContentSize = new CGSize(width * (nfloat)ViewControllers.Count, height);
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

                if (_viewController.CurrentPage != (int)page)
                {
                    if (page >= 0 && (int)page < _viewController.ViewControllers.Count)
                    {
                        _viewController.CurrentPage = page;
                        System.Diagnostics.Debug.WriteLine($"offset:{scrollView.ContentOffset.X} , scrolledPage:{page} ,moveToLeft:{moveToLeft}");
                    }
                }

                if (t-page>0)
                    _viewController.OtherPage = _viewController.CurrentPage + 1;
                else
                    _viewController.OtherPage = _viewController.CurrentPage - 1;


                var result = _viewController.CurrentPage * pageWidth - offset;

                _viewController.MovingToCurrentPage = moveToLeft&&(result>=0)||(!moveToLeft)&&(result<0);
            }

            public override void ScrollAnimationEnded(UIScrollView scrollView)
            {
            }
        }
    }
}