using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.SliderModule
{
    public class Slider : BaseEntity
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string SliderId { get; set; }

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
        public DateTime? StartDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public bool IsActive { get; set; }
        public int IsDeleted { get; set; }
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
        /// animation-delay
        /// </summary>
        public string AnimationDelay { get; set; }
    }


    public enum ImageFit
    {
        none,//The image is not resized
        fill,//This is default. The image is resized to fill the given dimension. If necessary, the image will be stretched or squished to fit
        contain, //The image keeps its aspect ratio, but is resized to fit within the given dimension
        cover,//The image keeps its aspect ratio and fills the given dimension. The image will be clipped to fit
        /// <summary>
        /// scale-down
        /// </summary>
        scaledown //the image is scaled down to the smallest version of none or contain
    }

    public enum TransActionType
    {
        none = 1,
        bounce,
        flash,
        pulse,
        rubberBand,
        shake,
        headShake,
        swing,
        tada,
        wobble,
        jello,
        heartBeat,
        bounceIn,
        bounceInDown,
        bounceInLeft,
        bounceInRight,
        bounceInUp,
        bounceOut,
        bounceOutDown,
        bounceOutLeft,
        bounceOutRight,
        bounceOutUp,
        fadeIn,
        fadeInDown,
        fadeInDownBig,
        fadeInLeft,
        fadeInLeftBig,
        fadeInRight,
        fadeInRightBig,
        fadeInUp,
        fadeInUpBig,
        fadeOut,
        fadeOutDown,
        fadeOutDownBig,
        fadeOutLeft,
        fadeOutLeftBig,
        fadeOutRight,
        fadeOutRightBig,
        fadeOutUp,
        fadeOutUpBig,
        flipInX,
        flipInY,
        flipOutX,
        flipOutY,
        lightSpeedIn,
        lightSpeedOut,
        rotateIn,
        rotateInDownLeft,
        rotateInDownRight,
        rotateInUpLeft,
        rotateInUpRight,
        rotateOut,
        rotateOutDownLeft,
        rotateOutDownRight,
        rotateOutUpLeft,
        rotateOutUpRight,
        hinge,
        rollIn,
        rollOut,
        zoomIn,
        zoomInDown,
        zoomInLeft,
        zoomInRight,
        zoomInUp,
        zoomOut,
        zoomOutDown,
        zoomOutLeft,
        zoomOutRight,
        zoomOutUp,
        slideInDown,
        slideInLeft,
        slideInRight,
        slideInUp,
        slideOutDown,
        slideOutLeft,
        slideOutRight,
        slideOutUp
    }

    public enum Target
    {
        Blank,
        Parent
    }


}
