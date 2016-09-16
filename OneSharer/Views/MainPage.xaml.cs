using System;
using OneSharer.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using Windows.Networking.Connectivity;
using OneSharer.Services;
using Windows.UI.Xaml.Input;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Xaml.Hosting;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI;
using Windows.Foundation;

namespace OneSharer.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage(Rect imageBounds)
        {
            InitializeComponent();
            NavigationCacheMode =NavigationCacheMode.Enabled;
            ProgRing.IsActive = false;
            LengthWarningText.Visibility = Visibility.Collapsed;

            SurfaceLoader.Initialize(ElementCompositionPreview.GetElementVisual(this).Compositor);

           
            //ShowCustomSplashScreen(imageBounds);     // Show the custome splash screen

            this.Loaded += ShareView_Loaded;
        }

        private async void ShareView_Loaded(object sender, RoutedEventArgs e)
        {
            // Now that loading is complete, dismiss the custom splash screen
            HideCustomSplashScreen();

            if (IsInternet())
            {
                FbUserNameText.Text = await MainPageViewModel.GetFbName();
                fbNameProgressBar.Visibility = Visibility.Collapsed;
                TwitterUserNameText.Text = await MainPageViewModel.GetTwitterName();
                TwNameProgressBar.Visibility = Visibility.Collapsed;
            }
            else
            {
                NotifyUserText.Text = "No Internet Connection";
            }
        }

        private bool IsInternet()
        {
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            bool HasInternetAccess = (connectionProfile != null &&
                                      connectionProfile.GetNetworkConnectivityLevel() ==
                                      NetworkConnectivityLevel.InternetAccess);
            if (HasInternetAccess)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            EllipseStoryboard.Begin();
        }

        private async void fbEllipse_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var outPut = await AuthServcie.AuthenticateToFacebookAsync();

        }

        private async void TwitterEllipse_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var outPut = await AuthServcie.AuthenticateToTwitterAsync();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsInternet())
            {
                NotifyUserText.Text = "";
                if (fbCheck.IsChecked == false && twitterCheck.IsChecked == false)
                {
                    NotifyUserText.Text = "No social network is selected";
                }

                else
                {
                    NotifyUserText.Text = "";
                    if (rootPivot.SelectedIndex.Equals(0))     //User wants to post Status
                    {
                        if (String.IsNullOrWhiteSpace(StatusTextBox.Text))
                        {
                            NotifyUserText.Text = "Empty posts don't look too good ;)";
                        }
                        else
                        {
                            SendButton.IsEnabled = false;
                            ProgRing.IsActive = true;
                            NotifyUserText.Text = await PostingUtility.Send((bool)fbCheck.IsChecked, (bool)twitterCheck.IsChecked, StatusTextBox.Text);
                            ProgRing.IsActive = false;
                            SendButton.IsEnabled = true;
                        }
                    }
                    else if (rootPivot.SelectedIndex.Equals(1))   //user wants to upload Photo
                    {

                        if (string.IsNullOrWhiteSpace(PhotoOutput.Text))
                        {
                            NotifyUserText.Text = "Please pick a photo to upload";
                        }
                        else
                        {

                            SendButton.IsEnabled = false;
                            ProgRing.IsActive = true;
                            NotifyUserText.Text = await ImagePostingUtility.SendImage((bool)fbCheck.IsChecked, (bool)twitterCheck.IsChecked, CaptionText.Text);
                            ProgRing.IsActive = false;
                            SendButton.IsEnabled = true;
                        }
                    }
                }
            }
            else
            {
                NotifyUserText.Text = "No Internet Connection";
            }
        }

        private void StatusTextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            var textBlock = (TextBox)sender;
            CharacterCountText.Text = 140 - (textBlock.Text).Length + " characters left for Twitter";
        }

        private void StatusTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBlock = (TextBox)sender;
            if ((textBlock.Text).Length > 140)
            {
                LengthWarningText.Visibility = Visibility.Visible;
            }
            else
            {
                LengthWarningText.Visibility = Visibility.Collapsed;
            }
        }

        private async void PhotoButton_Click(object sender, RoutedEventArgs e)
        {
            PhotoOutput.Text = "";
            NotifyUserText.Text = "";

            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                // Application now has read/write access to the picked file
                PhotoOutput.Text = file.Name;
                ImagePostingUtility.file = file;

            }
            else
            {
                PhotoOutput.Text = "";
            }
        }

        //private async void ShowCustomSplashScreen(Rect imageBounds)
        //{
        //    Compositor compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
        //    Vector2 windowSize = new Vector2((float)Window.Current.Bounds.Width, (float)Window.Current.Bounds.Height);


        //    //
        //    // Create a container visual to hold the color fill background and image visuals.
        //    // Configure this visual to scale from the center.
        //    //

        //    ContainerVisual container = compositor.CreateContainerVisual();
        //    container.Size = windowSize;
        //    container.CenterPoint = new Vector3(windowSize.X, windowSize.Y, 0) * .5f;
        //    ElementCompositionPreview.SetElementChildVisual(this, container);


        //    //
        //    // Create the colorfill sprite for the background, set the color to the same as app theme
        //    //

        //    SpriteVisual backgroundSprite = compositor.CreateSpriteVisual();
        //    backgroundSprite.Size = windowSize;
        //    backgroundSprite.Brush = compositor.CreateColorBrush(Color.FromArgb(1, 0, 178, 240));
        //    container.Children.InsertAtBottom(backgroundSprite);


        //    //
        //    // Create the image sprite containing the splash screen image.  Size and position this to
        //    // exactly cover the Splash screen image so it will be a seamless transition between the two
        //    //

        //    CompositionDrawingSurface surface = await SurfaceLoader.LoadFromUri(new Uri("ms-appx:///Assets/SplashScreen.scale-400.png"));
        //    SpriteVisual imageSprite = compositor.CreateSpriteVisual();
        //    imageSprite.Brush = compositor.CreateSurfaceBrush(surface);
        //    imageSprite.Offset = new Vector3((float)imageBounds.X, (float)imageBounds.Y, 0f);
        //    imageSprite.Size = new Vector2((float)imageBounds.Width, (float)imageBounds.Height);
        //    container.Children.InsertAtTop(imageSprite);
        //}

        private void HideCustomSplashScreen()
        {
            ContainerVisual container = (ContainerVisual)ElementCompositionPreview.GetElementChildVisual(this);
            Compositor compositor = container.Compositor;

            // Setup some constants for scaling and animating
            const float ScaleFactor = 20f;
            TimeSpan duration = TimeSpan.FromMilliseconds(1200);
            LinearEasingFunction linearEase = compositor.CreateLinearEasingFunction();
            CubicBezierEasingFunction easeInOut = compositor.CreateCubicBezierEasingFunction(new Vector2(.38f, 0f), new Vector2(.45f, 1f));

            // Create the fade animation which will target the opacity of the outgoing splash screen
            ScalarKeyFrameAnimation fadeOutAnimation = compositor.CreateScalarKeyFrameAnimation();
            fadeOutAnimation.InsertKeyFrame(1, 0);
            fadeOutAnimation.Duration = duration;

            // Create the scale up animation for the grid
            Vector2KeyFrameAnimation scaleUpGridAnimation = compositor.CreateVector2KeyFrameAnimation();
            scaleUpGridAnimation.InsertKeyFrame(0.1f, new Vector2(1 / ScaleFactor, 1 / ScaleFactor));
            scaleUpGridAnimation.InsertKeyFrame(1, new Vector2(1, 1));
            scaleUpGridAnimation.Duration = duration;

            // Create the scale up animation for the Splash screen visuals
            Vector2KeyFrameAnimation scaleUpSplashAnimation = compositor.CreateVector2KeyFrameAnimation();
            scaleUpSplashAnimation.InsertKeyFrame(0, new Vector2(1, 1));
            scaleUpSplashAnimation.InsertKeyFrame(1, new Vector2(ScaleFactor, ScaleFactor));
            scaleUpSplashAnimation.Duration = duration;

            // Configure the grid visual to scale from the center
            Visual gridVisual = ElementCompositionPreview.GetElementVisual(rootGrid);
            gridVisual.Size = new Vector2((float)rootGrid.ActualWidth, (float)rootGrid.ActualHeight);
            gridVisual.CenterPoint = new Vector3(gridVisual.Size.X, gridVisual.Size.Y, 0) * .5f;


            //
            // Create a scoped batch for the animations.  When the batch completes, we can dispose of the
            // splash screen visuals which will no longer be visible.
            //

            CompositionScopedBatch batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

            container.StartAnimation("Opacity", fadeOutAnimation);
            container.StartAnimation("Scale.XY", scaleUpSplashAnimation);
            gridVisual.StartAnimation("Scale.XY", scaleUpGridAnimation);

            batch.Completed += Batch_Completed;
            batch.End();
        }

        private void Batch_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            // Now that the animations are complete, dispose of the custom Splash Screen visuals
            ElementCompositionPreview.SetElementChildVisual(this, null);
        }
    }
}
