﻿using System;
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

        UIViewController[] viewControllers = new UIViewController[] {
            new UINavigationController(new A()),
            new UINavigationController(new B()),
            new UINavigationController(new C()),
            new UINavigationController(new D()) };

        UIScrollView containerScrollView;
        nfloat pageWidth = UIScreen.MainScreen.Bounds.Width;
        public List<UIViewController> ViewControllers;

        int _currentPage=-1;
        public int CurrentPage
        {
            get=> _currentPage; 
            set
            {
                _currentPage = value;
                System.Diagnostics.Debug.WriteLine($"currentPage:{value}");
                if (_currentPage >= ViewControllers.Count)
                {
                    _currentPage = ViewControllers.Count - 1;
                }
                FullySwitchedPage = CurrentPage;
            }
        }

        int _fullySwitchedPage=-1;
        public int FullySwitchedPage
        {
            get=> _fullySwitchedPage; 
            set
            {
                if (value != _fullySwitchedPage)
                {
                    if (_fullySwitchedPage < ViewControllers.Count&&_fullySwitchedPage>0)
                    {
                        var previousViewController = ViewControllers[_fullySwitchedPage];
                        previousViewController.WillMoveToParentViewController(null);
                        previousViewController.RemoveFromParentViewController();
                        previousViewController.View.RemoveFromSuperview();
                    }
                }
                _fullySwitchedPage = value;
            }
        }

        private bool? _left;
        public bool Left
        {
            get => _left.Value;
            set
            {
                if (_left == value)
                    return;
                _left = value;

                if (value)
                {
                    if (CurrentPage >= ViewControllers.Count)
                        return;
                    var currentViewController = ViewControllers[CurrentPage + 1];
                    AddChildViewController(currentViewController);
                    containerScrollView.AddSubview(currentViewController.View);
                    currentViewController.DidMoveToParentViewController(this);
                }
                else
                {
                    if (CurrentPage <= 0)
                        return;
                    var currentViewController = ViewControllers[CurrentPage - 1];
                    AddChildViewController(currentViewController);
                    containerScrollView.AddSubview(currentViewController.View);
                    currentViewController.DidMoveToParentViewController(this);
                }
            }
        }

        public DPageViewController()
        {
            ViewControllers = viewControllers.ToList();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            containerScrollView = new UIScrollView(new CGRect(0, 0, View.Frame.Size.Width, View.Frame.Size.Height)); //frame: self.view.bounds
            containerScrollView.Bounces = false;
            containerScrollView.PagingEnabled = true;
            containerScrollView.AlwaysBounceVertical = false;
            containerScrollView.AlwaysBounceHorizontal = false; // drag with only one view controller
            containerScrollView.ShowsHorizontalScrollIndicator = false;
            containerScrollView.Delegate = new Test(this);
            View.AddSubview(containerScrollView);
            LayoutPages();
        }

        public override void ViewDidLayoutSubviews()
        {
            //for (var i = 0; i < ViewControllers.Count; i += 1)
            //{
            //    var pageX = (nfloat)i * View.Bounds.Size.Width;
            //    ViewControllers[i].View.Frame = new CGRect(pageX, 0.0, View.Bounds.Size.Width, View.Bounds.Size.Height);
            //}

            //pageWidth = View.Bounds.Size.Width;
            //containerScrollView.ContentSize = new CGSize((nfloat)ViewControllers.Count * View.Bounds.Size.Width, View.Frame.Size.Height);
            //containerScrollView.ContentOffset = new CGPoint((nfloat)CurrentPage * View.Bounds.Size.Width, 0.0);
            LayoutPages();
            containerScrollView.ContentOffset = new CGPoint((nfloat)CurrentPage * View.Bounds.Size.Width, 0.0);
        }

        public override void WillMoveToParentViewController(UIViewController parent)
        {
            CurrentPage = 0;
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
                var nextFrame = new CGRect((nfloat)i * View.Bounds.Size.Width, View.Frame.Y, View.Frame.Size.Width, View.Frame.Size.Height); // Origin.Y
                page.View.Frame = nextFrame;
            }

            containerScrollView.ContentSize = new CGSize(View.Bounds.Size.Width * (nfloat)ViewControllers.Count, View.Frame.Size.Height);
        }
        public void InsertPage(UIViewController viewController, int index)
        {
            ViewControllers.Insert(index, viewController);
            LayoutPages();
            CurrentPage = index;
        }

        public void RemovePage(UIViewController viewController)
        {
            for (var i = 0; i < ViewControllers.Count; i += 1)
            {
                if (ViewControllers[i] == viewController)
                {
                    ViewControllers.RemoveAt(i);
                    LayoutPages();
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
                double t = _viewController.containerScrollView.ContentOffset.X / pageWidth;
                int page = (int)Math.Round(t);
                bool left = t - page > 0;

                if (_viewController.CurrentPage != (int)page)
                {
                    if (page >= 0 && (int)page < _viewController.ViewControllers.Count)
                    {
                        _viewController.CurrentPage = page;
                    }
                }

                if ((int)_viewController.containerScrollView.ContentOffset.X % (int)pageWidth == 0)
                {
                    _viewController.CurrentPage = (int)page;
                }
                System.Diagnostics.Debug.WriteLine($"offset:{scrollView.ContentOffset.X} , scrolledPage:{page} ,left:{left}");

                _viewController.Left = left;
            }

            public override void ScrollAnimationEnded(UIScrollView scrollView)
            {
            }
        }
    }
}