using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyFileManger;

public class SpaceBackground
{
    private readonly Canvas canvas;
    private readonly SpaceRenderer renderer;

    public SpaceBackground(Canvas targetCanvas)
    {
        canvas = targetCanvas;

        canvas.Background =
            new SolidColorBrush(
                Color.FromRgb(5, 1, 8));

        renderer = new SpaceRenderer();

        canvas.Children.Clear();

        canvas.Children.Add(renderer);

        Panel.SetZIndex(renderer, 0);

        canvas.SizeChanged += Canvas_SizeChanged;

        renderer.Start();

        renderer.Resize(
            canvas.ActualWidth,
            canvas.ActualHeight);
    }

    private void Canvas_SizeChanged(
        object sender,
        SizeChangedEventArgs e)
    {
        renderer.Resize(
            e.NewSize.Width,
            e.NewSize.Height);
    }

    public void Stop()
    {
        renderer.Stop();

        canvas.SizeChanged -=
            Canvas_SizeChanged;

        canvas.Children.Remove(renderer);
    }
}


// =============================================================
// SPACE RENDERER
// =============================================================

internal sealed class SpaceRenderer : FrameworkElement
{
    private readonly Random random = new();

    private readonly List<Star> stars = new();

    private readonly List<Meteor> meteors = new();

    private bool running;

    private DateTime lastFrame;

    private double meteorSpawnTimer;

    private double width;

    private double height;


    // =========================================================
    // STAR SETTINGS
    // =========================================================

    private const int STAR_COUNT = 500;

    private const double STAR_MIN_SIZE = 0.65;

    private const double STAR_MAX_SIZE = 1.75;

    private const double STAR_SPEED_MIN = 7.0;

    private const double STAR_SPEED_MAX = 16.0;


    // =========================================================
    // METEOR SETTINGS
    // =========================================================

    private const int MAX_METEORS = 4;

    private const double METEOR_SPEED = 820.0;

    private const double METEOR_LENGTH = 500.0;

    private const double METEOR_WIDTH = 7.0;

    private const double METEOR_HEAD_SIZE = 7.0;


    // =========================================================
    // CONSTANTS
    // =========================================================

    private const double DIAGONAL =
        0.70710678118;


    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    public SpaceRenderer()
    {
        IsHitTestVisible = false;

        SnapsToDevicePixels = false;

        UseLayoutRounding = false;

        HorizontalAlignment =
            HorizontalAlignment.Stretch;

        VerticalAlignment =
            VerticalAlignment.Stretch;
    }


    // =========================================================
    // START
    // =========================================================

    public void Start()
    {
        if (running)
            return;

        running = true;

        lastFrame =
            DateTime.UtcNow;

        CompositionTarget.Rendering +=
            OnRendering;
    }


    // =========================================================
    // STOP
    // =========================================================

    public void Stop()
    {
        if (!running)
            return;

        running = false;

        CompositionTarget.Rendering -=
            OnRendering;
    }


    // =========================================================
    // RESIZE
    // =========================================================

    public void Resize(
        double newWidth,
        double newHeight)
    {
        if (newWidth <= 0 ||
            newHeight <= 0)
            return;

        bool firstSize =
            width <= 0 ||
            height <= 0;

        bool changed =
            Math.Abs(width - newWidth) > 1 ||
            Math.Abs(height - newHeight) > 1;

        width = newWidth;

        height = newHeight;

        if (firstSize)
        {
            CreateStars();

            InvalidateVisual();

            return;
        }

        if (!changed)
            return;


        // =====================================================
        // INSTANTLY REDISTRIBUTE STARS
        // =====================================================

        foreach (Star star in stars)
        {
            star.X =
                random.NextDouble() *
                width;

            star.Y =
                random.NextDouble() *
                height;
        }


        // =====================================================
        // GUARANTEE STAR COUNT
        // =====================================================

        while (stars.Count < STAR_COUNT)
        {
            stars.Add(CreateStar());
        }


        // =====================================================
        // REMOVE METEORS DURING RESIZE
        // =====================================================
        //
        // This prevents old meteors from suddenly appearing
        // in strange locations after maximizing.
        //

        meteors.Clear();

        meteorSpawnTimer = 0;


        InvalidateVisual();
    }


    // =========================================================
    // CREATE STARS
    // =========================================================

    private void CreateStars()
    {
        stars.Clear();

        stars.Capacity = STAR_COUNT;

        for (int i = 0;
             i < STAR_COUNT;
             i++)
        {
            stars.Add(CreateStar());
        }
    }


    // =========================================================
    // CREATE STAR
    // =========================================================

    private Star CreateStar()
    {
        return new Star
        {
            X =
                random.NextDouble() *
                Math.Max(width, 1),

            Y =
                random.NextDouble() *
                Math.Max(height, 1),

            Size =
                STAR_MIN_SIZE +
                random.NextDouble() *
                (
                    STAR_MAX_SIZE -
                    STAR_MIN_SIZE
                ),

            Speed =
                STAR_SPEED_MIN +
                random.NextDouble() *
                (
                    STAR_SPEED_MAX -
                    STAR_SPEED_MIN
                ),

            Alpha =
                (byte)random.Next(
                    130,
                    235)
        };
    }


    // =========================================================
    // SPAWN METEOR
    // =========================================================

    private void SpawnMeteor()
    {
        if (meteors.Count >= MAX_METEORS)
            return;

        if (width <= 0 ||
            height <= 0)
            return;


        // Spawn slightly ABOVE the screen.

        double x =
            random.NextDouble() *
            (width + 500) -
            250;

        double y =
            -random.NextDouble() *
            300;


        meteors.Add(
            new Meteor
            {
                X = x,
                Y = y
            });
    }


    // =========================================================
    // ANIMATION
    // =========================================================

    private void OnRendering(
        object? sender,
        EventArgs e)
    {
        if (!running)
            return;

        if (width <= 0 ||
            height <= 0)
            return;


        DateTime now =
            DateTime.UtcNow;


        double delta =
            (
                now -
                lastFrame
            ).TotalSeconds;


        lastFrame = now;


        // =====================================================
        // FRAME PROTECTION
        // =====================================================

        if (delta <= 0)
            return;

        if (delta > 0.033)
            delta = 0.033;


        // =====================================================
        // MOVE STARS
        // =====================================================

        foreach (Star star in stars)
        {
            star.X -=
                star.Speed *
                delta;


            // Wrap around.

            if (star.X <
                -star.Size)
            {
                star.X =
                    width +
                    star.Size;

                star.Y =
                    random.NextDouble() *
                    height;
            }
        }


        // =====================================================
        // MOVE METEORS
        // =====================================================

        double meteorMovement =
            METEOR_SPEED *
            delta *
            DIAGONAL;


        for (int i =
                 meteors.Count - 1;
             i >= 0;
             i--)
        {
            Meteor meteor =
                meteors[i];


            // EXACT 45 DEGREE
            // DOWN + RIGHT

            meteor.X +=
                meteorMovement;

            meteor.Y +=
                meteorMovement;


            if (meteor.X >
                    width +
                    METEOR_LENGTH ||
                meteor.Y >
                    height +
                    METEOR_LENGTH)
            {
                meteors.RemoveAt(i);
            }
        }


        // =====================================================
        // METEOR SPAWN
        // =====================================================

        meteorSpawnTimer += delta;

        if (meteorSpawnTimer >= 1.15)
        {
            meteorSpawnTimer = 0;

            SpawnMeteor();
        }


        // =====================================================
        // REDRAW
        // =====================================================

        InvalidateVisual();
    }


    // =========================================================
    // DRAW
    // =========================================================

    protected override void OnRender(
        DrawingContext dc)
    {
        base.OnRender(dc);

        if (width <= 0 ||
            height <= 0)
            return;


        // =====================================================
        // BACKGROUND
        // =====================================================

        dc.DrawRectangle(
            BackgroundBrush,
            null,
            new Rect(
                0,
                0,
                width,
                height));


        // =====================================================
        // SUBTLE PINK SPACE HAZE
        // =====================================================

        dc.DrawRectangle(
            HazeBrush,
            null,
            new Rect(
                0,
                0,
                width,
                height));


        // =====================================================
        // STARS
        // =====================================================

        foreach (Star star in stars)
        {
            Color color =
                Color.FromArgb(
                    star.Alpha,
                    255,
                    205,
                    245);


            dc.DrawEllipse(
                new SolidColorBrush(color),
                null,
                new Point(
                    star.X,
                    star.Y),
                star.Size,
                star.Size);
        }


        // =====================================================
        // METEORS
        // =====================================================

        foreach (Meteor meteor in meteors)
        {
            DrawMeteor(
                dc,
                meteor);
        }
    }


    // =========================================================
    // STATIC BACKGROUND BRUSH
    // =========================================================

    private static readonly Brush BackgroundBrush =
        CreateBackgroundBrush();


    private static Brush CreateBackgroundBrush()
    {
        SolidColorBrush brush =
            new SolidColorBrush(
                Color.FromRgb(
                    5,
                    1,
                    8));

        brush.Freeze();

        return brush;
    }


    // =========================================================
    // STATIC HAZE BRUSH
    // =========================================================

    private static readonly Brush HazeBrush =
        CreateHazeBrush();


    private static Brush CreateHazeBrush()
    {
        RadialGradientBrush brush =
            new RadialGradientBrush();

        brush.GradientStops.Add(
            new GradientStop(
                Color.FromArgb(
                    30,
                    255,
                    20,
                    190),
                0));

        brush.GradientStops.Add(
            new GradientStop(
                Color.FromArgb(
                    9,
                    190,
                    10,
                    140),
                0.45));

        brush.GradientStops.Add(
            new GradientStop(
                Color.FromArgb(
                    0,
                    0,
                    0,
                    0),
                1));

        brush.Freeze();

        return brush;
    }


    // =========================================================
    // DRAW METEOR
    // =========================================================

    private static void DrawMeteor(
        DrawingContext dc,
        Meteor meteor)
    {
        Vector direction =
            new Vector(
                DIAGONAL,
                DIAGONAL);


        Vector perpendicular =
            new Vector(
                -DIAGONAL,
                DIAGONAL);


        Point head =
            new Point(
                meteor.X,
                meteor.Y);


        Point tail =
            head -
            direction *
            METEOR_LENGTH;


        // =====================================================
        // OUTER PINK TAIL
        // =====================================================

        double outerWidth =
            METEOR_WIDTH;


        Vector outerOffset =
            perpendicular *
            outerWidth;


        Point outerHeadLeft =
            head +
            outerOffset;


        Point outerHeadRight =
            head -
            outerOffset;


        // Tail becomes extremely thin.

        Vector tailOffset =
            perpendicular *
            0.35;


        Point outerTailLeft =
            tail +
            tailOffset;


        Point outerTailRight =
            tail -
            tailOffset;


        StreamGeometry geometry =
            new StreamGeometry();


        using (
            StreamGeometryContext context =
            geometry.Open())
        {
            context.BeginFigure(
                outerTailLeft,
                true,
                true);


            context.LineTo(
                outerHeadLeft,
                true,
                false);


            context.LineTo(
                outerHeadRight,
                true,
                false);


            context.LineTo(
                outerTailRight,
                true,
                false);
        }


        geometry.Freeze();


        // =====================================================
        // PINK → WHITE → PINK
        // ACROSS THE WIDTH
        // =====================================================

        LinearGradientBrush outerBrush =
            new LinearGradientBrush
            {
                MappingMode =
                    BrushMappingMode.Absolute,

                StartPoint =
                    outerHeadLeft,

                EndPoint =
                    outerHeadRight
            };


        outerBrush.GradientStops.Add(
            new GradientStop(
                Color.FromArgb(
                    0,
                    255,
                    15,
                    220),
                0));


        outerBrush.GradientStops.Add(
            new GradientStop(
                Color.FromArgb(
                    255,
                    255,
                    25,
                    220),
                0.20));


        outerBrush.GradientStops.Add(
            new GradientStop(
                Colors.White,
                0.50));


        outerBrush.GradientStops.Add(
            new GradientStop(
                Color.FromArgb(
                    255,
                    255,
                    25,
                    220),
                0.80));


        outerBrush.GradientStops.Add(
            new GradientStop(
                Color.FromArgb(
                    0,
                    255,
                    15,
                    220),
                1));


        outerBrush.Freeze();


        dc.DrawGeometry(
            outerBrush,
            null,
            geometry);


        // =====================================================
        // WHITE INNER CORE
        // =====================================================

        double innerWidth =
            METEOR_WIDTH * 0.42;


        Vector innerOffset =
            perpendicular *
            innerWidth;


        Point innerHeadLeft =
            head +
            innerOffset;


        Point innerHeadRight =
            head -
            innerOffset;


        Point innerTail =
            head -
            direction *
            (
                METEOR_LENGTH *
                0.92
            );


        StreamGeometry core =
            new StreamGeometry();


        using (
            StreamGeometryContext context =
            core.Open())
        {
            context.BeginFigure(
                innerTail,
                true,
                true);


            context.LineTo(
                innerHeadLeft,
                true,
                false);


            context.LineTo(
                innerHeadRight,
                true,
                false);


            context.LineTo(
                innerTail,
                true,
                false);
        }


        core.Freeze();


        LinearGradientBrush coreBrush =
            new LinearGradientBrush
            {
                MappingMode =
                    BrushMappingMode.Absolute,

                StartPoint =
                    innerHeadLeft,

                EndPoint =
                    innerHeadRight
            };


        coreBrush.GradientStops.Add(
            new GradientStop(
                Color.FromArgb(
                    0,
                    255,
                    255,
                    255),
                0));


        coreBrush.GradientStops.Add(
            new GradientStop(
                Colors.White,
                0.30));


        coreBrush.GradientStops.Add(
            new GradientStop(
                Colors.White,
                0.70));


        coreBrush.GradientStops.Add(
            new GradientStop(
                Color.FromArgb(
                    0,
                    255,
                    255,
                    255),
                1));


        coreBrush.Freeze();


        dc.DrawGeometry(
            coreBrush,
            null,
            core);


        // =====================================================
        // COMET HEAD
        // =====================================================

        RadialGradientBrush headBrush =
            new RadialGradientBrush();


        headBrush.GradientStops.Add(
            new GradientStop(
                Colors.White,
                0));


        headBrush.GradientStops.Add(
            new GradientStop(
                Colors.White,
                0.35));


        headBrush.GradientStops.Add(
            new GradientStop(
                Color.FromRgb(
                    255,
                    55,
                    225),
                0.72));


        headBrush.GradientStops.Add(
            new GradientStop(
                Color.FromArgb(
                    0,
                    255,
                    40,
                    225),
                1));


        headBrush.Freeze();


        dc.DrawEllipse(
            headBrush,
            null,
            head,
            METEOR_HEAD_SIZE,
            METEOR_HEAD_SIZE);
    }


    // =========================================================
    // STAR
    // =========================================================

    private sealed class Star
    {
        public double X;

        public double Y;

        public double Size;

        public double Speed;

        public byte Alpha;
    }


    // =========================================================
    // METEOR
    // =========================================================

    private sealed class Meteor
    {
        public double X;

        public double Y;
    }
}