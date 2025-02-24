using Arad.Portal.DataLayer.Entities.Abstractions;

using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Entities.General.SliderModule;

/// <summary>
/// this entity stores all images with effects which belongs to slider module
/// an slider can be added in admin each slider contains list of slides which can be Image or video (uploading video not implemented yet)
/// and each slide contains list of layer a layer can be a button or a text in front of the image or video
/// </summary>
public class Slider : BaseEntity
{
    public string Title { get; set; }

    public List<Slide> Slides { get; set; } = new List<Slide>();
}

public class Slide
{
    public string Id { get; set; }
    public string ImageUrl { get; set; }
    public string ColoredBackground { get; set; }
    public string VideoUrl { get; set; }
    public ImageFit ImageFit { get; set; }
    public TransActionType TransActionType { get; set; }
    public string Link { get; set; }
    public Target Target { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime? StartDate { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime? ExpireDate { get; set; }
    public bool IsActive { get; set; }
    public int IsDeleted { get; set; }
    public int IntervalTime { get; set; }
    public string Title { get; set; }
    public string Alt { get; set; }
    public List<Layer> Layers { get; set; } = new List<Layer>();
}


public class Layer
{
    public string Id { get; set; }
    public LayerType Type { get; set; }
    public string Content { get; set; }
    public string Link { get; set; }
    public Target Target { get; set; }
    public Position Position { get; set; }
    public TransActionType TransActionType { get; set; }
    public Style Styles { get; set; }
    public Attribute Attributes { get; set; }
    public int IsDeleted { get; set; }
    public bool IsActive { get; set; }
}

public enum LayerType
{
    Button,
    Text
}

public class Position
{
    public string Top { get; set; }
    public string Bottom { get; set; }
    public string Right { get; set; }
    public string Left { get; set; }
}

public class Attribute
{
    public string Class { get; set; }
    public string Id { get; set; }
    public string Alt { get; set; }
    public string Title { get; set; }
}

public class Style
{
    public string Width { get; set; }
    public string Height { get; set; }
    public string Top { get; set; }
    public string PaddingTop { get; set; }
    public string PaddingRight { get; set; }
    public string PaddingLeft { get; set; }
    public string PaddingBottom { get; set; }
    public string BorderTop { get; set; }
    public string BorderBottom { get; set; }
    public string BorderRight { get; set; }
    public string BorderLeft { get; set; }
    public string FontFamily { get; set; }
    public string FontSize { get; set; }
    public string LineHeight { get; set; }
    public string Color { get; set; }
    public string BackgroundColor { get; set; }
    public string RoundedCorners { get; set; }
    public string WordWrap { get; set; }
    public string CustomCss { get; set; }

    /// <summary>
    /// animation-delay in seconds
    /// </summary>
    public string AnimationDelay { get; set; }
}


public enum ImageFit
{
    None,//The image is not resized
    Fill,//This is default. The image is resized to fill the given dimension. If necessary, the image will be stretched or squished to fit
    Contain, //The image keeps its aspect ratio, but is resized to fit within the given dimension
    Cover,//The image keeps its aspect ratio and fills the given dimension. The image will be clipped to fit
    /// <summary>
    /// scale-down
    /// </summary>
    Scaledown //the image is scaled down to the smallest version of none or contain
}
/// <summary>
/// this enum extracted from all available effects in animate.css cause we use this plugin for animation 
/// </summary>
public enum TransActionType
{
    None = 1,
    Bounce,
    Flash,
    Pulse,
    RubberBand,
    Shake,
    HeadShake,
    Swing,
    Tada,
    Wobble,
    Jello,
    HeartBeat,
    BounceIn,
    BounceInDown,
    BounceInLeft,
    BounceInRight,
    BounceInUp,
    BounceOut,
    BounceOutDown,
    BounceOutLeft,
    BounceOutRight,
    BounceOutUp,
    FadeIn,
    FadeInDown,
    FadeInDownBig,
    FadeInLeft,
    FadeInLeftBig,
    FadeInRight,
    FadeInRightBig,
    FadeInUp,
    FadeInUpBig,
    FadeOut,
    FadeOutDown,
    FadeOutDownBig,
    FadeOutLeft,
    FadeOutLeftBig,
    FadeOutRight,
    FadeOutRightBig,
    FadeOutUp,
    FadeOutUpBig,
    BackInDown,
    BackInUp,
    BackInRight,
    BackInLeft,
    BackOutDown,
    BackOutUp,
    BackOutRight,
    BackOutLeft,
    FlipInX,
    FlipInY,
    FlipOutX,
    FlipOutY,
    LightSpeedInRight,
    LightSpeedInLeft,
    LightSpeedOutRight,
    LightSpeedOutLeft,
    RotateIn,
    RotateInDownLeft,
    RotateInDownRight,
    RotateInUpLeft,
    RotateInUpRight,
    RotateOut,
    RotateOutDownLeft,
    RotateOutDownRight,
    RotateOutUpLeft,
    RotateOutUpRight,
    Hinge,
    JackInTheBox,
    RollIn,
    RollOut,
    ZoomIn,
    ZoomInDown,
    ZoomInLeft,
    ZoomInRight,
    ZoomInUp,
    ZoomOut,
    ZoomOutDown,
    ZoomOutLeft,
    ZoomOutRight,
    ZoomOutUp,
    SlideInDown,
    SlideInLeft,
    SlideInRight,
    SlideInUp,
    SlideOutDown,
    SlideOutLeft,
    SlideOutRight,
    SlideOutUp
}

public enum Target
{
    Blank,
    Parent
}