using System;
using System.Drawing;

using CoreFoundation;
using UIKit;
using Foundation;

namespace PageViewController.ViewControllers
{

    [Register("B")]
    public class B : UIViewController
    {
        public B()
        {
        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Perform any additional setup after loading the view

            Title = "B";
            View.BackgroundColor = UIColor.Gray;
        }

        public override void ViewWillAppear(bool animated)
        {
            System.Diagnostics.Debug.WriteLine($"ViewWillAppear{Title}");
            base.ViewWillAppear(animated);
            if (this.NavigationController == null)
                return;
            this.NavigationController.NavigationBarHidden = false;

        }

        public override void ViewWillDisappear(bool animated)
        {
            System.Diagnostics.Debug.WriteLine($"ViewWillDisappear{Title}");
            base.ViewWillDisappear(animated);
        }

        public override void WillMoveToParentViewController(UIViewController parent)
        {
            base.WillMoveToParentViewController(parent);
        }
    }
}