using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using CoreGraphics;

namespace PageViewController.ViewControllers
{
    public class DPageViewController : UIViewController
    {
        UIScrollView containerScrollView;
        nfloat pageWidth = 1f;
        public List<UIViewController> ViewControllers;

        int _currentPage;
        public int CurrentPage
        {
            get=> _currentPage; 
            set
            {
                _currentPage = value;
                if (_currentPage >= ViewControllers.Count)
                {
                    _currentPage = ViewControllers.Count - 1;
                }
                containerScrollView.Delegate = null;
                containerScrollView.ContentOffset = new CGPoint((nfloat)_currentPage * View.Bounds.Size.Width, 0.0);
                containerScrollView.Delegate = new Test(this);
                // Set the fully switched page in order to notify the delegates about it if needed.
                FullySwitchedPage = CurrentPage;
            }
        }

        int _fullySwitchedPage;
        public int FullySwitchedPage
        {
            get=> _fullySwitchedPage; 
            set
            {
                if (value != _fullySwitchedPage)
                {
                    // The page is fully switched.
                    if (_fullySwitchedPage < ViewControllers.Count)
                    {
                        var previousViewController = ViewControllers[_fullySwitchedPage];
                        previousViewController.WillMoveToParentViewController(null);
                        previousViewController.ViewWillDisappear(true);
                        previousViewController.ViewDidDisappear(true);
                        previousViewController.RemoveFromParentViewController();
                    }

                    var currentViewController = ViewControllers[value];
                    AddChildViewController(currentViewController);
                    currentViewController.ViewWillAppear(true);
                    currentViewController.ViewDidAppear(true);
                    currentViewController.DidMoveToParentViewController(this);
                }
                _fullySwitchedPage = value;
            }
        }

        public DPageViewController(List<UIViewController> viewControllers)
        {
            ViewControllers = viewControllers;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            containerScrollView = new UIScrollView(new CGRect(0, 0, View.Frame.Size.Width, View.Frame.Size.Height-70)); //frame: self.view.bounds
            containerScrollView.PagingEnabled = true;
            containerScrollView.AlwaysBounceVertical = false;
            containerScrollView.AlwaysBounceHorizontal = true; // drag with only one view controller
            containerScrollView.ShowsHorizontalScrollIndicator = false;
            containerScrollView.Delegate = new Test(this);
            pageWidth = View.Frame.Size.Width;
            View.AddSubview(containerScrollView);
            LayoutPages();
        }

        public override void ViewDidLayoutSubviews()
        {
            for (var i = 0; i < ViewControllers.Count; i += 1)
            {
                var pageX = (nfloat)i * View.Bounds.Size.Width;
                //ViewControllers[i].View.Frame = new CGRect(pageX, 0.0, View.Bounds.Size.Width, View.Bounds.Size.Height);
            }
            // It is important to set the pageWidth property before the contentSize and contentOffset,
            // in order to use the new width into scrollView delegate methods.
            pageWidth = View.Bounds.Size.Width;
            containerScrollView.ContentSize = new CGSize((nfloat)ViewControllers.Count * View.Bounds.Size.Width, View.Frame.Size.Height - 70);
            containerScrollView.ContentOffset = new CGPoint((nfloat)CurrentPage * View.Bounds.Size.Width, 0.0);
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
                //AddChildViewController(page);
                var nextFrame = new CGRect((nfloat)i * View.Bounds.Size.Width, View.Frame.Y, View.Frame.Size.Width, View.Frame.Size.Height); // Origin.Y
                page.View.Frame = nextFrame;
                containerScrollView.AddSubview(page.View);
                //page.DidMoveToParentViewController(this);
                //if (i == 0)
                    //page.DidMoveToParentViewController(this);
            }

            containerScrollView.ContentSize = new CGSize(View.Bounds.Size.Width * (nfloat)ViewControllers.Count, View.Frame.Size.Height - 70);
        }

        public override void WillMoveToParentViewController(UIViewController parent)
        {
            var page = (int)((containerScrollView.ContentOffset.X - pageWidth / 2.0) / pageWidth) + 1;
            if (CurrentPage != page)
            {
                CurrentPage = page;
            }
        }
        public void InsertPage(UIViewController viewController, int index)
        {
            ViewControllers.Insert(index, viewController);
            LayoutPages();
            CurrentPage = index;
            //self.notifyDelegateDidChangeControllers()
        }

        public void RemovePage(UIViewController viewController)
        {
            for (var i = 0; i < ViewControllers.Count; i += 1)
            {
                if (ViewControllers[i] == viewController)
                {
                    ViewControllers.RemoveAt(i);
                    LayoutPages();
                    //self.notifyDelegateDidChangeControllers()
                }
            }
        }

        class Test : UIScrollViewDelegate
        {
            private DPageViewController _viewController;
            private nfloat pageWidth;
            public Test(DPageViewController viewcontroller)
            {
                _viewController = viewcontroller;
                pageWidth = viewcontroller.pageWidth;
            }

            public override void Scrolled(UIScrollView scrollView)
            {
                System.Diagnostics.Debug.WriteLine(scrollView.ContentOffset.X);

                //// Update the page when more than 50% of the previous/next page is visible
                //int page = (int)Math.Floor((_viewController.containerScrollView.ContentOffset.X - pageWidth / 2) / pageWidth) + 1;

                //if (_viewController.CurrentPage != (int)page)
                //{
                //    //Check the page to avoid "index out of bounds" exception.
                //    if (page >= 0 && (int)page < _viewController.ViewControllers.Count)
                //    {
                //        _viewController.CurrentPage = page;
                //    }
                //}

                //// Check whether the current view controller is fully presented.
                //if ((int)_viewController.containerScrollView.ContentOffset.X % (int)pageWidth == 0)
                //{
                //    _viewController.CurrentPage = (int)page;
                //}
            }
        }
    }
}